using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Newtonsoft.Json.Linq;
using MirrorRepository.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MirrorRepository.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.Collections;

namespace MirrorRepository
{

    public class TupleContext
    {
        public int Found { get; set; }
        public int Page { get; set; }
        public int Tuples { get; set; }
        public int Tuple { get; set; }
        public override string ToString()
        {
            return string.Format("{0}:[{1}/{2}/{3}]", Page, Tuple, Tuples, Found);
        }
    }
    public class WriteReport
    {
        public string LogTag { get; set; } = "";
        public int Pages { get; set; }
        public int Page { get; set; }
        public int Written { get; set; }
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Tuples { get; set; }
        public int Found { get; set; }

        public override string ToString()
        {
            return string.Format(LogTag+": page:{0},written:{1},insert:{2},update:{3},pages:{4}", Page, Written, Inserted, Updated, Pages);
        }
    }

    public class TupleWriter
    {
        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        public TupleContext tCtx { get; private set; } = new TupleContext();
        public SnowMigration Migration { get; set; }
        public string LogTag { get; set; } = "";

        /// <summary>
        /// Insert or Update table with tuple
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tableName"></param>
        /// <param name="tuples"></param>
        /// <returns></returns>
        public WriteReport WriteTuples(SnowDbContext ctx, string tableName, List<JObject> tuples)
        {
            var report = new WriteReport() { LogTag = LogTag, Page = tCtx.Page };
            try {
                if (tuples == null || tuples.Count == 0)
                    return report;

                using (var cnx = ctx.Database.GetDbConnection())
                {
                    return WriteTuples(cnx, tableName, tuples);
                }
            }
            catch (Exception e)
            {
                Log.Info(LogTag+": cannot write ctx=" + ctx + ", table=" + tableName, e);
                throw;
            }
        }

        public WriteReport WriteTuples(DbConnection cnx, string tableName, List<JObject> tuples)
        {
            var report = new WriteReport() { Found = tCtx.Found };
            try
            {
                if (tuples == null || tuples.Count == 0)
                    return report;

                int inserted, updated;
                int res = WriteTuples(cnx, tableName, tuples, out inserted, out updated);

                report.Tuples = tuples.Count;
                report.Written = res;
                report.Inserted = inserted;
                report.Updated = updated;
                return report;
            }
            catch (Exception e)
            {
                Log.Info(LogTag+": cannot write cnx=" + cnx.State + ", table=" + tableName, e);
                throw;
            }
        }

        /// <summary>
        /// Insert or Update table with tuples
        /// </summary>
        /// <param name="cnx"></param>
        /// <param name="tableName"></param>
        /// <param name="tuples"></param>
        /// <param name="inserted"></param>
        /// <param name="updated"></param>
        /// <returns></returns>
        public int WriteTuples(DbConnection cnx, string tableName, List<JObject> tuples, out int inserted, out int updated)
        {
            inserted = 0;
            updated = 0;
            if (Migration?.DbModel?.Tables == null)
            {
                if (Migration == null)
                {
                    Log.Warn(LogTag + ": Migration is null. Model not initialized for name: " + tableName + ", Migration=" + Migration);

                } else if(Migration?.DbModel == null) {
                    
                    Log.Warn(LogTag + ": DbModel is null. Model not initialized for name: " + tableName + ", Migration=" + Migration);
                } 
                else
                {
                    Log.Warn(LogTag + ": Model not initialized for name: " + tableName + ", Migration=" + Migration);
                }
                return 0;
            }
            List<DatabaseTable> tables = Migration.DbModel.Tables.Where(t => t.Name == Migration.SnowTablePrefix + tableName).ToList();
            if (!tables.Any())
            {
                Log.Warn(LogTag + ": no table found in model for name: " + tableName + " : model:" + Migration.DbModel.Tables.Select(t=>t.Name).Join());
                return 0;
            } else
            {
                var table = tables.First();
                return WriteTuples(cnx, table, tuples, out inserted, out updated);
            }
        }

        /// <summary>
        /// Insert or Update table with tuple
        /// </summary>
        /// <param name="cnx"></param>
        /// <param name="table"></param>
        /// <param name="tuples"></param>
        /// <param name="inserted"></param>
        /// <param name="updated"></param>
        /// <returns></returns>
        public int WriteTuples(DbConnection cnx, DatabaseTable table, List<JObject> tuples, out int inserted, out int updated)
        {
            tCtx.Tuples = tuples.Count();
            var tupleCount = 0;
            inserted = 0;
            updated = 0;

            if (cnx.State != ConnectionState.Open) cnx.Open();
            var tx = cnx.BeginTransaction();
            try
            {
                foreach (var tuple in tuples)
                {
                    var pk = GetPk(tuple);
                    if (Exists(cnx, table, pk, tx))
                    {
                        var res = UpdateTuple(cnx, table, pk, tuple, tx);
                        tupleCount += res;
                        updated += res;
                    }
                    else
                    {
                        var res = InsertTuple(cnx, table, tuple, tx);
                        tupleCount += res;
                        inserted += res;
                    }
                }
                tx.Commit();
            }
            catch (Exception e)
            {
                Log.Info(LogTag+": cannot write tuple: " + tupleCount + "/" + tuples.Count + ", cnx=" + cnx.State + ", table=" + table?.Name, e);
                tx.Rollback();
                throw;
            }
            return tupleCount;
        }

        /// <summary>
        /// Insert tuples into table
        /// </summary>
        /// <param name="cnx"></param>
        /// <param name="tableName"></param>
        /// <param name="tuples"></param>
        /// <returns></returns>
        public int InsertTuples(SnowDbContext ctx, string tableName, List<JObject> tuples)
        {
            var table = Migration.DbModel.Tables.Where(t => t.Name == Migration.SnowTablePrefix + tableName).First();
            return InsertTuples(ctx, table, tuples);
        }

        public int InsertTuples(SnowDbContext ctx, DatabaseTable table, List<JObject> tuples)
        {
            using (var cnx = ctx.Database.GetDbConnection())
            {
                return InsertTuples(cnx, table, tuples);
            }
        }

        /// <summary>
        /// Insert tuples into table
        /// </summary>
        /// <param name="cmx"></param>
        /// <param name="tableName"></param>
        /// <param name="tuples"></param>
        /// <returns></returns>
        public int InsertTuples(DbConnection cnx, string tableName, List<JObject> tuples)
        {
            var table = Migration.DbModel.Tables.Where(t => t.Name == Migration.SnowTablePrefix + tableName).First();
            return InsertTuples(cnx, table, tuples);
        }

        /// <summary>
        /// Insert Tuple into table - no existence check!
        /// </summary>
        /// <param name="cnx"></param>
        /// <param name="table"></param>
        /// <param name="tuples"></param>
        /// <returns></returns>
        public int InsertTuples(DbConnection cnx, DatabaseTable table, List<JObject> tuples)
        {
            tCtx.Tuples = tuples.Count();
            var tupleCount = 0;

            if (cnx.State != ConnectionState.Open) cnx.Open();
            var tx = cnx.BeginTransaction();
            try
            {
                foreach (var tuple in tuples)
                {
                    var res = InsertTuple(cnx, table, tuple, tx);
                    tupleCount += res;
                }
                tx.Commit();
            }
            catch (Exception e)
            {
                Log.Info(LogTag+": cannot insert tuple: "+tCtx+", cnx=" + cnx.State + ", table=" + table?.Name, e);
                tx.Rollback();
                throw;
            }
            return tupleCount;
        }

        protected int InsertTuple(DbConnection cnx, DatabaseTable table, JObject tuple, DbTransaction tx = null)
        {
            var task = new Dictionary<string, List<object>>();
            object sysId = Guid.Empty;
            var sqlCommand = "undef";

            try
            {
                sysId = GetPk(tuple); //Guid.Parse(tuple.Value<string>(SnowBase.SYS_ID));

                var sql = BuildInsert(table);
                task.Add("sqlCmd", new List<object>(new string[] { sql.ToString() }));

                var cmd = cnx.CreateCommand();
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 600;
                sqlCommand = cmd.CommandText;

                var parms = new List<DbParameter>();

                var crt = cmd.CreateParameter();
                crt.ParameterName = "@"+SnowBase.SNOWDBSYNC_CREATED;
                crt.Value = DateTime.Now;
                parms.Add(crt);


                foreach (var col in table.Columns.Where(c => !SnowSyncColumns.Contains(c.Name)))
                {
                    var tVals = new List<object>();
                    task.Add(col.Name, tVals);

                    object tpVal = null;
                    try
                    {
                        var cp = cmd.CreateParameter();
                        cp.ParameterName = "@"+col.Name;
                        var token = tuple.SelectToken(col.Name);
                        tVals.Add(token);

                        if (token != null && token.HasValues && token.SelectToken("value") != null)
                        {
                            tpVal = token.Value<string>("value");
                        }
                        else
                        {
                            tpVal = tuple[col.Name];
                        }
                        tVals.Add(tpVal);

                        cp.Value = SnowMigration.ToSqlType(col.StoreType, tpVal);
                        tVals.Add(cp.Value);
                        
                        cp.DbType = SnowMigration.GetDbType(col.StoreType); // (DbType)Enum.Parse(typeof(DbType), c.StoreType);
                        tVals.Add(col.StoreType);
                        tVals.Add(cp.DbType);

                        parms.Add(cp);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(LogTag+": cannot map: " + col.Name + " = " + tpVal + ", StoreType=" + col.StoreType + " tuple=" + tuple, e);
                    }
                }

                cmd.Parameters.AddRange(parms.ToArray());
                if (tx != null) cmd.Transaction = tx;
                var res = cmd.ExecuteNonQuery();
                
                tCtx.Tuple++;
                Log.Trace(LogTag + ": inserted[" + tCtx + "]: " + table.Name + "[" + sysId + "]" + ": tuple=" + tuple + ", values=" + task.Select(t => t.Key + ":" + t.Value).Join(","));
                Log.Debug(LogTag + ": inserted[" + tCtx + "]: " + table.Name + "[" + sysId + "]");

                return res;
            }
            catch (Exception e)
            {
                var msg = string.Format(LogTag+ ": cannot insert[" + tCtx + "]: {0} : tuple={1}, task={2} \n {3}", table.Name, tuple, 
                    task.Select(t => t.Key + ":{" + t.Value.Join(",")+"}").Join(",\n"), sqlCommand);
                Log.Info(msg, e);
                throw new Exception(msg, e);
            }
        }

        /// <summary>
        /// Update existing table entries with tuples
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tableName"></param>
        /// <param name="tuples"></param>
        /// <returns></returns>
        public int UpdateTuples(SnowDbContext ctx, string tableName, List<JObject> tuples)
        {
            var table = Migration.DbModel.Tables.Where(t => t.Name == Migration.SnowTablePrefix + tableName).First();
            return UpdateTuples(ctx, table, tuples);
        }

        /// <summary>
        /// Update existing table entries with tuples
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="table"></param>
        /// <param name="tuples"></param>
        /// <returns></returns>
        public int UpdateTuples(SnowDbContext ctx, DatabaseTable table, List<JObject> tuples)
        {
            if (tuples == null || tuples.Count == 0)
                return 0;

            using (var cnx = ctx.Database.GetDbConnection())
            {
                return UpdateTuples(cnx, table, tuples);
            }
        }

        /// <summary>
        /// Update existing table entries with tuples
        /// </summary>
        /// <param name="cnx"></param>
        /// <param name="tableName"></param>
        /// <param name="tuples"></param>
        /// <returns></returns>
        public int UpdateTuples(DbConnection cnx, string tableName, List<JObject> tuples)
        {
            var table = Migration.DbModel.Tables.Where(t => t.Name == Migration.SnowTablePrefix + tableName).First();
            return UpdateTuples(cnx, table, tuples);
        }

        /// <summary>
        /// Update existing table entries with tuples
        /// </summary>
        /// <param name="cnx"></param>
        /// <param name="table"></param>
        /// <param name="tuples"></param>
        /// <returns></returns>
        public int UpdateTuples(DbConnection cnx, DatabaseTable table, List<JObject> tuples)
        {
            tCtx.Tuples = tuples.Count();
            var tupleCount = 0;

            if (cnx.State != ConnectionState.Open) cnx.Open();
            var tx = cnx.BeginTransaction();

            object sysId = null; //Guid sysId = Guid.Empty;
            try
            {
                foreach (var tuple in tuples)
                {
                    sysId = GetPk(tuple); //Guid.Parse(tuple.Value<string>(SnowBase.SYS_ID));
                    var res = UpdateTuple(cnx, table, sysId, tuple, tx);
                    tupleCount += res;
                }
                tx.Commit();
            }
            catch (Exception e)
            {
                Log.Info(LogTag+ ": cannot update[" + tCtx + "]: tuple: " + tupleCount + "/" + tuples.Count + ", sys_id=" + sysId + ", cnx=" + cnx.State + ", table=" + table?.Name, e);
                tx.Rollback();
                throw;
            }
            return tupleCount;
        }

        /// <summary>
        /// Update existing table entry with tuple
        /// </summary>
        /// <param name="cnx"></param>
        /// <param name="table"></param>
        /// <param name="sysId"></param>
        /// <param name="tuple"></param>
        /// <returns></returns>
        protected int UpdateTuple(DbConnection cnx, DatabaseTable table, object sysId, JObject tuple, DbTransaction tx = null)
        {
            var task = new Dictionary<string, List<object>>();
            var sqlCommand = "undef";

            try
            {
                var sql = BuildUpdate(table);
                task.Add("sqlCmd", new List<object>(new string[] { sql.ToString() }));

                var cmd = cnx.CreateCommand();
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 600;
                sqlCommand = cmd.CommandText;

                var parms = new List<DbParameter>();

                var pk = cmd.CreateParameter();
                pk.ParameterName = "@"+SnowBase.SYS_ID;
                pk.Value = sysId;
                parms.Add(pk);

                var upd = cmd.CreateParameter();
                upd.ParameterName = "@"+SnowBase.SNOWDBSYNC_UPDATED;
                upd.Value = DateTime.Now;
                parms.Add(upd);

                var ksyn = cmd.CreateParameter();
                ksyn.ParameterName = "@" + SnowBase.KAFKA_SYNCHRONIZED;
                ksyn.Value = DBNull.Value;
                parms.Add(ksyn);

                foreach (var col in table.Columns.Where(c => !InternalColumns.Contains(c.Name)))
                {
                    var tVals = new List<object>();
                    task.Add(col.Name, tVals);

                    object tpVal = null;
                    try
                    {
                        var cp = cmd.CreateParameter();
                        cp.ParameterName = "@"+col.Name;
                        var token = tuple.SelectToken(col.Name);
                        tVals.Add(token);
                        if (token != null && token.HasValues && token.SelectToken("value") != null)
                        {
                            tpVal = token.Value<string>("value");
                        }
                        else
                        {
                            tpVal = tuple[col.Name];
                        }
                        tVals.Add(tpVal);

                        cp.Value = SnowMigration.ToSqlType(col.StoreType, tpVal);
                        tVals.Add(cp.Value);
                        
                        cp.DbType = SnowMigration.GetDbType(col.StoreType); // (DbType)Enum.Parse(typeof(DbType), c.StoreType);
                        tVals.Add(col.StoreType);
                        tVals.Add(cp.DbType);

                        parms.Add(cp);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(LogTag+": cannot map: "+SnowBase.SYS_ID + "=" + sysId + ": " + col.Name + " = " + tpVal + " tuple=" + tuple, e);
                    }
                }

                cmd.Parameters.AddRange(parms.ToArray());
                if (tx != null) cmd.Transaction = tx;
                var res = cmd.ExecuteNonQuery();

                tCtx.Tuple++;
                Log.Trace(LogTag+ ": updated[" + tCtx + "]: " + table.Name + "[" + sysId + "]: tuple=" + tuple);
                Log.Debug(LogTag + ": updated[" + tCtx + "]: " + table.Name + "[" + sysId + "]");

                return res;
            }
            catch (Exception e)
            {
                var msg = string.Format(LogTag+ ": cannot update[" + tCtx + "]: {0} : tuple={1}, task={2} \n {3}", table.Name, tuple,
                    task.Select(t => t.Key + ":{" + t.Value.Join(",") + "}").Join(",\n"), sqlCommand);
                Log.Info(msg, e);
                throw new Exception(msg, e);
            }
        }

        public static readonly List<string> SnowSyncColumns = new List<string>() { SnowBase.SNOWDBSYNC_CREATED, SnowBase.SNOWDBSYNC_UPDATED, SnowBase.KAFKA_SYNCHRONIZED };
        public static readonly List<string> InternalColumns = new List<string>() { SnowBase.SYS_ID, SnowBase.SNOWDBSYNC_CREATED, SnowBase.SNOWDBSYNC_UPDATED, SnowBase.KAFKA_SYNCHRONIZED };

        public StringBuilder BuildInsert(DatabaseTable table)
        {
            var columns = table.Columns.Where(c => !SnowSyncColumns.Contains(c.Name));

            var names = new List<string>() { "[" + SnowBase.SNOWDBSYNC_CREATED + "]" };
            names.AddRange(columns.Select(c => "["+c.Name+"]"));
            
            var values = new List<string>() { "@" + SnowBase.SNOWDBSYNC_CREATED };
            values.AddRange(columns.Select(c => "@" + c.Name));
            
            var sql = new StringBuilder("INSERT INTO [" + table.Name + "] (");
            sql.Append(names.Join(",")).Append(") values (");
            sql.Append(values.Join(",")).Append(")");
            return sql;
        }

        public StringBuilder BuildUpdate(DatabaseTable table)
        {
            var columns = table.Columns.Where(c => !InternalColumns.Contains(c.Name));
            
            var names = new List<string>() { 
                SnowBase.SNOWDBSYNC_UPDATED + "=@" + SnowBase.SNOWDBSYNC_UPDATED, 
                SnowBase.KAFKA_SYNCHRONIZED + "=@" + SnowBase.KAFKA_SYNCHRONIZED 
            };
            names.AddRange(columns.Select(c => "[" + c.Name + "]" + "=@" + c.Name));

            var sql = new StringBuilder("UPDATE [" + table.Name + "] SET ");
            sql.Append(names.Join(",")).Append(" WHERE " + SnowBase.SYS_ID + "=@" + SnowBase.SYS_ID);
            return sql;
        }

        public object GetPk(JObject tuple)
        {
            var sysId = tuple.Value<string>(SnowBase.SYS_ID);
            if (SnowMigration.SysIdType == typeof(Guid))
            {
                return Guid.Parse(sysId);
            }
            return sysId;
        }

        public bool Exists(DbConnection cnx, DatabaseTable table, object sysId, DbTransaction tx = null)
        {
            return Exists(cnx, table.Name, sysId, tx);
        }

        public bool Exists(DbConnection cnx, string tableName, object sysId, DbTransaction tx = null)
        {
            var sql = new StringBuilder("SELECT "+SnowBase.SYS_ID+ " FROM [" + tableName + "] ")
                .Append(" WHERE " + SnowBase.SYS_ID + " = @" + SnowBase.SYS_ID);

            var cmd = cnx.CreateCommand();
            cmd.CommandText = sql.ToString();
            cmd.CommandTimeout = 600;

            var parms = new List<DbParameter>();

            var pk = cmd.CreateParameter();
            pk.ParameterName = "@" + SnowBase.SYS_ID;
            pk.Value = sysId;
            parms.Add(pk);

            cmd.Parameters.AddRange(parms.ToArray());
            if (tx != null) cmd.Transaction = tx;

            var res = false;
            using (var read = cmd.ExecuteReader())
            {
                res = read.HasRows;
            }

            Log.Debug(LogTag+": found: " + tableName + "[" + sysId + "]");
            return res;
        }


        /// <summary>
        /// Cleanup of stale tuples - older than last synchronization
        /// </summary>
        /// <param name="cnx"></param>
        /// <param name="tableName"></param>
        /// <param name="lastSync"></param>
        /// <returns></returns>
        public int CleanupStaleTuples(DbConnection cnx, string tableName, DateTime lastSync)
        {
            var table = Migration.DbModel.Tables.Where(t => t.Name == Migration.SnowTablePrefix + tableName).First();
            return CleanupStaleTuples(cnx, table, lastSync);
        }

        /// <summary>
        /// Cleanup of stale tuples - older than last synchronization
        /// </summary>
        /// <param name="cnx"></param>
        /// <param name="table"></param>
        /// <param name="lastSync"></param>
        /// <returns></returns>
        public int CleanupStaleTuples(DbConnection cnx, DatabaseTable table, DateTime lastSync)
        {
            if (lastSync == null)
            {
                Log.Info(LogTag + ": cannot cleanup[" + tCtx + "]: table=" + table?.Name + ", lastSync=" + lastSync);
                return 0;
            }

            var tupleCount = 0;

            if (cnx.State != ConnectionState.Open) cnx.Open();
            var tx = cnx.BeginTransaction();

            try
            {
                var res = CleanupStaleTuples(cnx, table, lastSync, tx);
                tupleCount += res;
                tx.Commit();
            }
            catch (Exception e)
            {
                Log.Info(LogTag + ": cannot cleanup[" + tCtx + "]: table=" + table?.Name + ", lastSync=" + lastSync, e);
                tx.Rollback();
                throw;
            }
            return tupleCount;
        }

        /// <summary>
        /// Cleanup of stale tuples - older than last synchronization
        /// </summary>
        /// <param name="cnx"></param>
        /// <param name="table"></param>
        /// <param name="lastSync"></param>
        /// <param name="tx"></param>
        /// <returns></returns>
        protected int CleanupStaleTuples(DbConnection cnx, DatabaseTable table, DateTime lastSync, DbTransaction tx = null)
        {
            var task = new Dictionary<string, List<object>>();
            const string LASTSYNC = "LASTSYNC";
            var sqlCommand = "undef";

            try
            {
                var sql = "DELETE from " + table.Name + " WHERE " 
                    + " ( " + SnowBase.SNOWDBSYNC_UPDATED + " is not null and " + SnowBase.SNOWDBSYNC_UPDATED + " < @"+LASTSYNC+" ) "
                    + " or "
                    + " ( " + SnowBase.SNOWDBSYNC_UPDATED + " is null and " + SnowBase.SNOWDBSYNC_CREATED + " < @"+LASTSYNC+" )";
                task.Add("sqlCmd", new List<object>(new string[] { sql.ToString() }));

                var cmd = cnx.CreateCommand();
                cmd.CommandText = sql.ToString();
                sqlCommand = cmd.CommandText;

                var parms = new List<DbParameter>();

                var ls = cmd.CreateParameter();
                ls.ParameterName = "@" + LASTSYNC;
                ls.Value = lastSync;
                parms.Add(ls);

                cmd.Parameters.AddRange(parms.ToArray());
                if (tx != null) cmd.Transaction = tx;
                var res = cmd.ExecuteNonQuery();

                Log.Info(LogTag + ": deleted[" + tCtx + "]: " + table.Name + ": lastSync=" + lastSync + ", result=" + res);

                return res;
            }
            catch (Exception e)
            {
                var msg = string.Format(LogTag + ": cannot delete[" + tCtx + "]: {0} : lastSync={1}, \n {2}", table.Name, lastSync, sqlCommand);
                Log.Info(msg, e);
                throw new Exception(msg, e);
            }
        }
    }
}
