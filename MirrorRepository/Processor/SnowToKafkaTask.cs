using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MirrorRepository.Base;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Helpers;
using MirrorRepository.Model;
using MirrorRepository.Model.ESB;
using MirrorRepository.Model.InterfaceMonitoring;
using MirrorRepository.Model.Kafka;
using MirrorRepository.REST;

namespace MirrorRepository.Processor
{
    public class SnowToKafkaTask: SyncTaskBase<RestClient>, SyncTask
    {
        public TableHierarchy TableHierarchy { get; internal set; }

        public int KafkaBlockSize { get; set; } = 50;

        public SnowToKafkaTask()
        {
            MyTag = "SnowToKafkaTask";
        }

        protected override (bool, Data.SnowDbSyncMgnt.Synchronization, KeyValuePair<SnowDictEntry, List<SnowDictEntry>> snowDictEntry,
            MonitoringSettings, InterfaceMonitoringResponse) ValidateAndInit()
        {
            //KafkaSyncTable PoolSize = 1; // is not designed for correct Paging of "where kafka_synchronized is null"!!
            (bool, Data.SnowDbSyncMgnt.Synchronization, KeyValuePair<SnowDictEntry, List<SnowDictEntry>> snowDictEntry, MonitoringSettings, InterfaceMonitoringResponse) value = base.ValidateAndInit();
            return value;
        }

        protected override int CountEntries()
        {
            // get record count from sys_updated to now
            var syncProcEntry = Scheduler.GetOrCreate(Table.Name, SynchronizationId, GetKey());
            DateTime deltaStart = GetDeltaStartTimeAsDate(syncProcEntry);

            PageQueue.Table = Table.Name;
            var count = Client.DeltaCount(deltaStart, Table.Name);
            return count.result.stats.count;
        }

        /// <summary>
        /// execute page
        /// </summary>
        /// <param name="client"></param>
        /// <param name="myPage"></param>
        /// <param name="syncProcEntry"></param>
        /// <param name="cnx"></param>
        /// <returns></returns>
        public override WriteReport ExecutePage(RestClient client, QueueEntry myPage, SyncProcess syncProcEntry, DbConnection cnx)
        {
            Log.Debug(MyTag + ": execPage: " + myPage + " on " + Table);
            var response = Query(client, SyncType, myPage.Page, syncProcEntry);
            if (response.result.Length == 0 && !response.ContinueOnEmptyResponse) // Task finished!
            {
                Log.Info(string.Format(MyTag + ": empty response for page={0}, breaking..", myPage));
                return null;
            }

            //get ESBClient
            return WriteTuples(EsbClient, cnx, Table.Name, response.result.ToList(), myPage);
        }

        public WriteReport WriteTuples(EsbClient client, DbConnection cnx, string tableName, List<JObject> response, QueueEntry myPage)
        {
            var report = new WriteReport() { Found = response.Count };
            try
            {
                if (response.Count == 0)
                    return report;
                
                int tupleCount = response.Count;
                int count = 0;
                while (response.Count > 0)
                {
                    var responseBlock = response.Take(response.Count > KafkaBlockSize ? KafkaBlockSize : response.Count).ToList();
                    count += WriteTupleArray(tableName, responseBlock);
                    response = response.Except(responseBlock).ToList();
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

        public int WriteTupleArray(string tableName, List<JObject> responseBlock)
        {
            KafkaDataEvent kde = new KafkaDataEvent()
            {
                tableName = tableName,
                data = responseBlock.ToArray()
            };
            
            EsbClient.WriteData(EsbClient.Esb_Service_Paths.IncidentUpdated, JsonConvert.SerializeObject(kde), responseBlock.First().ToString(), responseBlock.Count);

            return responseBlock.Count;
        }
        
        protected override bool TryMigration(bool shallExecute)
        {
            // nothing to do..
            return false;
        }

        protected override void ProcessSyncFull(KeyValuePair<SnowDictEntry, List<SnowDictEntry>> snowSyncEntry)
        {
            // nothing to do..
        }

        protected override void ProcessSyncConsistency()
        {
            // nothing to do..
        }

        protected override void InitTableHierarchy()
        {
            // nothing to do..
        }
        
        /// <summary>
        /// get snow data - snowToKafka syncs only in delta mode
        /// </summary>
        /// <param name="myClient"></param>
        /// <param name="type"></param>
        /// <param name="myPage"></param>
        /// <param name="syncProcessEntity"></param>
        /// <returns></returns>
        public QueryResponse Query(RestClient myClient, SyncProcessType type, int myPage, SyncProcess syncProcessEntity)
        {
            QueryResponse response;
            
            bool continueOnEmptyResponse = false;

            DateTime deltaStart = GetDeltaStartTimeAsDate(syncProcessEntity);

            string content = myClient.Read("/api/now/table/" + Table.Name,
            SnowParms.New
                .nocount()
                .between(PROP.sys_updated_on, $"javascript:gs.dateGenerate('{deltaStart.Date:yyyy-MM-dd}','{deltaStart:HH:mm:ss}')", $"javascript:gs.dateGenerate('{DateTime.Now.Date:yyyy-MM-dd}','{DateTime.Now:HH:mm:ss}')")
                .orderByDesc(PROP.sys_updated_on)
                .offset(myPage * PageSize)
                .limit(PageSize)
                .columns(Table.Columns));

            
            if (content.IsNullOrWhiteSpace())
            {
                response = new QueryResponse() { result = new Newtonsoft.Json.Linq.JObject[] { } };
            }
            else
            {
                response = myClient.Deserialize<QueryResponse>(content);
            }

            response.ContinueOnEmptyResponse = continueOnEmptyResponse;
            return response;
        }
    }
}
