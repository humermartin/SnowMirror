using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using MirrorRepository.Base;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.REST;
using MirrorRepository.SnowTableApi;
using MirrorRepository.Model.ESB;
using System.Data;
using Org.BouncyCastle.Crypto;
using Azure;
using MirrorRepository.Helpers;
using Microsoft.EntityFrameworkCore.Internal;
using MirrorRepository.Model;
using MirrorRepository.Model.InterfaceMonitoring;
using System.Text.RegularExpressions;
using MirrorRepository.Model.Kafka;
using System.Diagnostics.PerformanceData;
using System.Threading;

namespace MirrorRepository.Processor
{
    public class KafkaTask : SyncTaskBase<EsbClient>, SyncTask
    {
        
        public KafkaTask()
        {
            MyTag = "KafkaTask";
        }

        protected override (bool, Data.SnowDbSyncMgnt.Synchronization, KeyValuePair<SnowDictEntry, List<SnowDictEntry>> snowDictEntry, 
            MonitoringSettings, InterfaceMonitoringResponse) ValidateAndInit()
        {
            //KafkaSyncTable PoolSize = 1; // is not designed for correct Paging of "where kafka_synchronized is null"!!
            (bool, Data.SnowDbSyncMgnt.Synchronization, KeyValuePair<SnowDictEntry, List<SnowDictEntry>> snowDictEntry, MonitoringSettings, InterfaceMonitoringResponse) value = base.ValidateAndInit();
            if (value.Item1)
            {
                DropKafkaSyncTable(Table.Name);
                CreateKafkaSyncTable(Table.Name);
            }
            return value;
        }

        protected override int CountEntries()
        {
            string tableName = Table.Name;
            return CountEntries(tableName);
        }

        public override WriteReport ExecutePage(EsbClient client, QueueEntry myPage, SyncProcess syncProcEntry, DbConnection cnx)
        {
            Log.Debug(MyTag + ": execPage: " + myPage + " on " + Table);
            string tableName = GetKafkaSyncTableName(Table.Name);
            // PreKafkaSyncTable: // Offset: always 0 because "order by sys_id" - works only singe-threaded yet!!
            // Now: Offset on KafkaSyncTable by pages
            var sysIds = GetIds(tableName, myPage.Page*PageSize, PageSize); 
            if (sysIds.Count == 0) // Task finished!
            {
                Log.Info(string.Format(MyTag + ": empty response for page={0}, breaking..", myPage));
                return null;
            }
            return WriteTuples(client, cnx, Table.Name, sysIds, myPage);
        }

        public WriteReport WriteTuples(EsbClient client, DbConnection cnx, string tableName, List<string> sysIds, QueueEntry myPage)
        {
            var report = new WriteReport() { Found = sysIds.Count };
            try
            {
                if (sysIds.Count == 0)
                    return report;

                int tupleCount = sysIds.Count;
                int count = 0;
                while (sysIds.Count > 0) {
                    var sysIdBlock = sysIds.Take(sysIds.Count > KafkaBlockSize ? KafkaBlockSize : sysIds.Count).ToList();
                    count += WriteTupleArray(client, cnx, tableName, sysIdBlock);
                    sysIds = sysIds.Except(sysIdBlock).ToList();
                }

                report.Tuples = tupleCount;
                report.Written = count;
                report.Inserted = count;
                report.Updated = count;
                return report;
            }
            catch (Exception e)
            {
                Log.Info(MyTag + ": cannot write cnx=" + cnx.State + ", table=" + tableName, e);
                throw;
            }
        }

        public int WriteTupleArray(EsbClient client, DbConnection cnx, string tableName, List<string> sysIds)
        {
            if (cnx.State != ConnectionState.Open) cnx.Open();

            using (var tx = cnx.BeginTransaction())
            {
                try
                {
                    KafkaDataEvent kde = new KafkaDataEvent() { tableName = tableName };
                    var data = new List<Dictionary<string, object>>();
                    sysIds.ForEach(sysId =>
                    {
                        var tuples = GetValues(tableName, sysId, cnx, tx);
                        data.Add(tuples);
                    });
                    kde.data = data.ToArray();

                    IncidentUpdatedRequest req = new IncidentUpdatedRequest();
                    req.body = Client.Serialize(kde);
                    Client.Write(EsbClient.Esb_Service_Paths.IncidentUpdated, req, sysIds.First(), data.Count);
                    int res = 0;
                    try
                    {
                        sysIds.ForEach(sysId =>
                        {
                            res += SetSynchronized(tableName, sysId, cnx, tx);
                        });
                        tx.Commit();
                        return res;
                    }
                    catch (Exception e)
                    {
                        Log.Warn(MyTag + ": FAILED " + tableName + "[" + sysIds + "]" + " : " + e);
                    }
                }
                catch (Exception e)
                {
                    Log.Info(MyTag + ": cannot send " + tableName + "[" + sysIds + "]" + " : " + e);
                }
            }
            return 0;
        }

        public int WriteTuple(EsbClient client, DbConnection cnx, string tableName, string sysId)
        {
            if (cnx.State != ConnectionState.Open) cnx.Open();

            using (var tx = cnx.BeginTransaction()) { 
                try
                {
                    var tuples = GetValues(tableName, sysId, cnx, tx);

                    IncidentUpdatedRequest req = new IncidentUpdatedRequest();
                    req.body = Client.Serialize(tuples);
                    Client.Write(EsbClient.Esb_Service_Paths.IncidentUpdated, req, sysId);
                    try
                    {
                        int res = SetSynchronized(tableName, sysId, cnx, tx);
                        tx.Commit();
                        return res;
                    }
                    catch (Exception e)
                    {
                        Log.Warn(MyTag + ": FAILED " + tableName + "[" + sysId + "]" + " : " + e);
                    }
                }
                catch (Exception e)
                {
                    Log.Info(MyTag + ": cannot send " + tableName + "[" + sysId + "]" + " : " + e);
                }
            }
            return 0;
        }

        protected int CountEntries(string tableName)
        {
            var sql = new StringBuilder();
            try
            {
                using (var cnx = Context.New().Database.GetDbConnection())
                {
                    if (cnx.State != ConnectionState.Open) cnx.Open();

                    sql = new StringBuilder("SELECT count(*) FROM [" + tableName + "] " +
                        " WHERE " + SnowBase.KAFKA_SYNCHRONIZED + " IS NULL");

                    var cmd = cnx.CreateCommand();
                    cmd.CommandText = sql.ToString();
                    cmd.CommandTimeout = 600;

                    var res = 0;
                    using (var read = cmd.ExecuteReader())
                    {
                        read.Read();
                        res = read.GetInt32(0);
                    }

                    Log.Debug(MyTag + ": found " + res + " entries in " + tableName);
                    return res;
                }
            }
            catch (Exception ex)
            {
                string msg = MyTag + ": FAILED " + sql + " for " + tableName + " : " + ex.Message;
                Log.Info(msg);
                Log.Debug(msg, ex);
                throw;
            }
        }

        public string GetKafkaSyncTableName(string tableName)
        {
            string prefix = "KFK_";
            return FormatKafkaSyncName(tableName, prefix);
        }

        public string GetKafkaSyncTableIndexName(string tableName)
        {
            string prefix = "KFK_IDX_";
            return FormatKafkaSyncName(tableName, prefix);
        }

        private string FormatKafkaSyncName(string tableName, string prefix)
        {
            string name = prefix + tableName + "_" + ParentThreadId + "_" + Regex.Replace(Processor.SyncName, @"\W+", "_");
            if (name.Length > 128)
                return name.Substring(0, 128);
            return name;
        }

        public void CreateKafkaSyncTable(string tableName)
        {
            using (var cnx = Context.New().Database.GetDbConnection())
            {
                if (cnx.State != ConnectionState.Open) cnx.Open();

                var syncTableName = GetKafkaSyncTableName(tableName);

                var drop = new StringBuilder("DROP TABLE IF EXISTS [" + syncTableName + "]");
                ExecuteNonQuery(drop, tableName, cnx, syncTableName, 900);

                var createAndInsert = new StringBuilder("SELECT " + SnowBase.SYS_ID + " INTO [" + syncTableName + "] FROM [" + tableName + "] " +
                    " WHERE " + SnowBase.KAFKA_SYNCHRONIZED + " IS NULL " +
                    " ORDER BY " + SnowBase.SYS_ID);
                ExecuteNonQuery(createAndInsert, tableName, cnx, syncTableName, 5*900);

                var index = new StringBuilder("CREATE INDEX " + syncTableName + " ON [" + syncTableName + "] (" + SnowBase.SYS_ID + ")");
                ExecuteNonQuery(index, tableName, cnx, syncTableName, 900); 
            }
        }

        public void DropKafkaSyncTable(string tableName)
        {
            using (var cnx = Context.New().Database.GetDbConnection())
            {
                if (cnx.State != ConnectionState.Open) cnx.Open();

                var syncTableName = GetKafkaSyncTableName(tableName);

                var drop = new StringBuilder("DROP TABLE IF EXISTS [" + syncTableName + "]");
                ExecuteNonQuery(drop, tableName, cnx, syncTableName, 900);
            }
        }

        private void ExecuteNonQuery(StringBuilder sql, string tableName, DbConnection cnx, string syncTableName, int timeout)
        {
            try
            {
                var cmd = cnx.CreateCommand();
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = timeout;
                if (Log.IsDebugEnabled)
                {
                    Log.Trace(MyTag + ": sql " + sql);
                }
                var result = cmd.ExecuteNonQuery();
                Log.Info(MyTag + ": " + sql + " for " + tableName + " : " + result);
            }
            catch (Exception ex)
            {
                string msg = MyTag + ": FAILED " + sql + " for " + tableName + " : " + ex.Message;
                Log.Info(msg);
                Log.Debug(msg, ex);
                throw;
            }
        }

        protected List<string> GetIds(string tableName, int offset, int max)
        {
            using (var cnx = Context.New().Database.GetDbConnection())
            {
                if (cnx.State != ConnectionState.Open) cnx.Open();
                
                var sql = new StringBuilder("SELECT " + SnowBase.SYS_ID + " FROM [" + tableName + "] " +
                    " ORDER BY " + SnowBase.SYS_ID +
                    " OFFSET " + offset + " ROWS FETCH NEXT " + max + " ROWS ONLY");

                try
                {
                    var cmd = cnx.CreateCommand();
                    cmd.CommandText = sql.ToString();
                    cmd.CommandTimeout = 800;

                    List<string> ids = new List<string>();
                    using (var read = cmd.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            ids.Add(read.GetString(0));
                        }
                    }

                    if (Log.IsDebugEnabled)
                    {
                        Log.Trace(MyTag + ": sql " + sql);
                        Log.Trace(MyTag + ": ids " + ids.Join());
                        Log.Debug(MyTag + ": found " + ids.Count + " ids in " + tableName + ", offset=" + offset + ", max=" + max);
                    }
                    return ids;
                } catch(Exception ex)
                {
                    string msg = MyTag + ": FAILED " + sql + " for " + tableName + ", offset=" + offset + ", max=" + max + " : " + ex.Message;
                    Log.Info(msg);
                    Log.Debug(msg, ex);
                    throw;
                }
            }
        }

        protected Dictionary<string, object> GetValues(string tableName, string sysId, DbConnection cnx, DbTransaction tx)
        {
            if (cnx.State != ConnectionState.Open) cnx.Open();

            var sql = new StringBuilder("SELECT * FROM [" + tableName + "] " +
                " WHERE " + SnowBase.SYS_ID + " = @" + SnowBase.SYS_ID);

            try
            {
                var cmd = cnx.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = sql.ToString();

                var pk = cmd.CreateParameter();
                pk.ParameterName = "@" + SnowBase.SYS_ID;
                pk.Value = sysId;
                cmd.Parameters.Add(pk);

                Dictionary<string, object> tuples = new Dictionary<string, object>();
                using (var read = cmd.ExecuteReader())
                {
                    read.Read();
                    int fields = read.FieldCount;
                    for (int i = 0; i < fields; i++)
                    {
                        if (!read.IsDBNull(i))
                        {
                            tuples.Add(read.GetName(i), read.GetValue(i));
                        }
                    }
                }

                if (Log.IsDebugEnabled)
                {
                    Log.Debug(MyTag + ": found " + tuples.Count + " values in " + tableName + "[" + sysId + "]");
                }
                return tuples;
            }
            catch (Exception ex)
            {
                string msg = MyTag + ": FAILED " + sql + " for " + tableName + " : " + ex.Message;
                Log.Info(msg);
                Log.Debug(msg, ex);
                throw;
            }
        }

        protected int SetSynchronized(string tableName, string sysId, DbConnection cnx, DbTransaction tx)
        {
            var sql = new StringBuilder("UPDATE [" + tableName + "] " +
                " SET " + SnowBase.KAFKA_SYNCHRONIZED + " =  @" + SnowBase.KAFKA_SYNCHRONIZED + " " +
                " WHERE " + SnowBase.SYS_ID + " = @" + SnowBase.SYS_ID);

            var cmd = cnx.CreateCommand();
            cmd.CommandText = sql.ToString();

            var pk = cmd.CreateParameter();
            pk.ParameterName = "@" + SnowBase.SYS_ID;
            pk.Value = sysId;
            cmd.Parameters.Add(pk);
            var param = cmd.CreateParameter();
            param.ParameterName = "@" + SnowBase.KAFKA_SYNCHRONIZED;
            param.Value = DateTime.Now;
            cmd.Parameters.Add(param);

            cmd.Transaction = tx;

            var res = cmd.ExecuteNonQuery();
            if (res == 1)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(MyTag + ": set synchronized: " + tableName + "[" + sysId + "], res=" + res);
                }
            } else
            {
                Log.Warn(MyTag + ": FAILED synchronized: " + tableName + "[" + sysId + "] : result=" + res + ", sql=" + cmd.CommandText);
            }
            return res;
        }

        protected override void InitTableHierarchy()
        {
            // nothing to do..
        }

        protected override void ProcessSyncConsistency()
        {
            // nothing to do..
        }

        protected override void ProcessSyncFull(KeyValuePair<SnowDictEntry, List<SnowDictEntry>> snowSyncEntry)
        {
            // nothing to do..
        }

        protected override bool TryMigration(bool shallExecute)
        {
            return shallExecute; // noting to do..
        }

        protected override void EndExecute(MonitoringSettings monitoringSettings, InterfaceMonitoringResponse monitoringResponse)
        {
            DropKafkaSyncTable(Table.Name);
            base.EndExecute(monitoringSettings, monitoringResponse);
        }
    }
}
