using Microsoft.EntityFrameworkCore;
using MirrorRepository.Base;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Helpers;
using MirrorRepository.Model;
using MirrorRepository.REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Ajax.Utilities;
using System.Data.Common;

namespace MirrorRepository.Processor
{
    /// <summary>
    /// Synchronization process for _one_ table.
    /// Used and invoked by SnowProcessor.
    /// </summary>
    public class SnowTask: SyncTaskBase<RestClient>, SyncTask
    {
        public TableHierarchy TableHierarchy { get; internal set; }
        
        public SnowTask()
        {
            MyTag = "SyncTask";
        }

        protected override void ProcessSyncFull(KeyValuePair<SnowDictEntry, List<SnowDictEntry>> snowDictEntry)
        {
            // Rebuild Indexes:
            AppSettingsModel appSettingsModel = new AppSettingsModel().GetAppSettingsModel();
            ScriptSettings scriptSettings = appSettingsModel.ScriptSettings;
            SqlSessionSettings sqlSessionSettings = appSettingsModel.SqlSessionSettings;

            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Start IndexRebuild for table {Table.Name}. CommandTimeout: {scriptSettings.ScriptTimeout} seconds.");
            Context.New().IndexRebuild(Processor.Migration.GetTableName(Table.Name), scriptSettings.ScriptTimeout == 0 ? 1000 : scriptSettings.ScriptTimeout);
            LogMessages.Add(new LogMessage { Key = GetKey(), Table = Table.Name, Message = "IndexRebuild: " + Processor.Migration.GetTableName(Table.Name) });
            const int MaxIter = 10000;
            var rand = new Random(37);
            // Move to Real Table:
            Exception commandError = null;
            for (int i = 0; i < MaxIter; i++)
            {
                if (i >= MaxIter - 1)
                {
                    throw new Exception("max retries for action: DropTable:" + Table.Name, commandError);
                }
                try
                {
                    //stop blocking db user - move stored procedure parameters to appsetting
                    if (sqlSessionSettings != null && sqlSessionSettings.EnableKilleSession)
                    {
                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. start executing stored procedure SQL: {sqlSessionSettings.StoredProcedure} to kill sessions from user: {sqlSessionSettings.SqlUserName}");
                        Context.New().ExecuteStoredProc(sqlSessionSettings.StoredProcedure, sqlSessionSettings.SqlUserName, Context.DBNAME, 1000);
                    }
                    else
                    {
                        if (sqlSessionSettings != null)
                        {
                            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. executing stored procedure SQL: {sqlSessionSettings.StoredProcedure} is disabled");
                        }
                    }

                    //drop table
                    Context.New().DropTable(Table.Name, 1);
                    LogMessages.Add(new LogMessage { Key = GetKey(), Table = Table.Name, Message = "dropped: " + Table.Name });
                    break;
                }
                catch (Exception e)
                {
                    commandError = e;
                    int millis = rand.Next(1, 50);
                    Log.Info("cannot execute DropTable, retrying in " + millis + "ms... : " + e.ToString());
                    try
                    {
                        Thread.Sleep(millis);
                    }
                    catch (Exception et)
                    {
                        Log.Info("cannot sleep.. : " + et.ToString());
                    }
                }
            }
            String newTableName = null;
            for (int i = 0; i < MaxIter; i++)
            {
                if (i >= MaxIter - 1)
                {
                    throw new Exception("max retries for action: RenameTable:" + Table.Name, commandError);
                }
                try
                {
                    newTableName = Context.New().Rename(
                        Processor.Migration.GetTableName(Table.Name), Processor.Migration.SnowTablePrefix);
                    break;
                }
                catch (Exception e)
                {
                    commandError = e;
                    int millis = rand.Next(1, 50);
                    Log.Info("cannot execute rename, retrying in " + millis + "ms... : " + e.ToString());
                    try
                    {
                        Thread.Sleep(millis);
                    }
                    catch (Exception et)
                    {
                        Log.Info("cannot sleep.. : " + et.ToString());
                    }
                }
            }
            var newIndexName = Context.New().Rename(Table.Name + "." +
                Processor.Migration.GetTablePKName(Table.Name), Processor.Migration.SnowTablePrefix);
            foreach (var index in Processor.Migration.GetTableIndexNames(snowDictEntry.Key, snowDictEntry.Value))
            {
                try
                {
                    Context.New().Rename(Table.Name + "." + index.Key, Processor.Migration.SnowTablePrefix);
                }
                catch (Exception e)
                {
                    Log.Info("cannot rename index: " + index.Key, e);
                }
            }
            FinalMessage = "Successfully renamed: " + Processor.Migration.GetTableName(Table.Name) + " to " + newTableName;
            LogMessages.Add(new LogMessage { Key = GetKey(), Table = Table.Name, Message = FinalMessage });

            // Execute SQL-Commands: may only be executed on sync type FULL
            if (Table.SciptCommands != null && Table.SciptCommands.Any())
            {
                Table.SciptCommands.ForEach(c =>
                {
                    try
                    {
                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Start executing command='{c.Command}'. CommandTimeout: {scriptSettings.ScriptTimeout} seconds.");
                        Context.New().ExecuteSqlCommand(c.Command, scriptSettings.ScriptTimeout == 0 ? 1000 : scriptSettings.ScriptTimeout);
                        var message = $"Executed: {c.Command}";
                        Log.Info(message);
                        LogMessages.Add(new LogMessage { Key = GetKey(), Table = Table.Name, Message = message });
                    }
                    catch (Exception e)
                    {
                        var message = $"Failed to executed: {c.Command} : exc= {e.Message}";
                        Log.Info(message, e);
                        LogMessages.Add(new LogMessage { Key = GetKey(), Table = Table.Name, Message = message });

                    }
                });
            }
        }

        protected override void ProcessSyncConsistency()
        {
            if (ExecuteCleanup)
            {
                using (var cnx = Context.New().Database.GetDbConnection())
                {
                    var tw = new TupleWriter() { Migration = Processor.Migration, LogTag = MyTag };
                    var deleted = tw.CleanupStaleTuples(cnx, Table.Name, Entity.StartTime.Value);
                    Entity = Scheduler.GetOrCreate(Table.Name, SynchronizationId, GetKey());
                    Entity.RecordsDeleted = deleted;
                    Scheduler.Update(Entity);
                }
            }
        }

        protected override bool TryMigration(bool shallExecute)
        {
            try
            {
                Processor.Migrate(Table, GetKey());
            }
            catch (Exception e)
            {
                shallExecute = false;
                LogError(MyTag + ": Schema update failed for table " + Table.Name + " : " + e.Message, e);
            }

            return shallExecute;
        }

        public override WriteReport ExecutePage(RestClient myClient, QueueEntry myPage, SyncProcess syncProcEntry, DbConnection cnx)
        {
            Log.Debug(MyTag + ": execPage: " + myPage + " on " + Table);
            var response = Query(myClient, SyncType, myPage.Page, syncProcEntry);
            if (response.result.Length == 0 && !response.ContinueOnEmptyResponse) // Task finished!
            {
                Log.Info(string.Format(MyTag + ": empty response for page={0}, breaking..", myPage));
                return null;
            }

            var tw = new TupleWriter() { Migration = Processor.Migration, LogTag = MyTag };
            tw.tCtx.Page = myPage.Page;
            tw.tCtx.Found = Found;

            // execute transactional sync to database:
            Log.Info(string.Format(MyTag + ": writing page={0}: ctx={1}", myPage, tw.tCtx));
            return tw.WriteTuples(cnx, Table.Name, response.result.ToList());
        }

        protected override void InitTableHierarchy()
        {
            if (SyncType != SyncProcessType.Delta) //delta sync does not use table inheritance
            {
                TableHierarchy = new TableHierarchy(Table.Name, MyTag, Context, SynchronizationId);
            }
        }

        protected override int CountEntries()
        {
            PageQueue.Table = Table.Name;
            var count = Client.Count(Table.Name);
            return count.result.stats.count;
        }

        public QueryResponse Query(RestClient myClient, SyncProcessType type, int myPage, SyncProcess syncProcessEntity)
        {
            QueryResponse response;
            string content = "";
            bool ContinueOnEmptyResponse = false;

            switch (type)
            {
                case SyncProcessType.Delta:

                    DateTime deltaStart = GetDeltaStartTimeAsDate(syncProcessEntity);

                    content = myClient.Read("/api/now/table/" + Table.Name,
                        SnowParms.New
                            .nocount()
                            .between(PROP.sys_updated_on, $"javascript:gs.dateGenerate('{deltaStart.Date:yyyy-MM-dd}','{deltaStart:HH:mm:ss}')", $"javascript:gs.dateGenerate('{DateTime.Now.Date:yyyy-MM-dd}','{DateTime.Now:HH:mm:ss}')")
                            .orderByDesc(PROP.sys_updated_on)
                            .offset(myPage * PageSize)
                            .limit(PageSize)
                            .columns(Table.Columns));

                    break;
                case SyncProcessType.Consistency:
                default:

                    if (TableHierarchy.DerivedFromParent && TableHierarchy.InheritanceTableSyncEnabled)
                    {
                        int maxTblHierarchyPageSize = 120;
                        List<string> ids = TableHierarchy.SysIDs.Skip(myPage * maxTblHierarchyPageSize).Take(maxTblHierarchyPageSize).ToList();
                        if (ids.Count == 0)
                        {
                            Log.Info(string.Format(MyTag + ": derived : page={0}/{1} : empty: {2}", myPage, maxTblHierarchyPageSize, TableHierarchy));
                        }

                        Log.Info(string.Format(MyTag + ": derived : page={0}/{1} : found: {2} entries for {3}", myPage, maxTblHierarchyPageSize, ids.Count, TableHierarchy));
                        ContinueOnEmptyResponse = ids.Count > 0;

                        if (ids.Count > 0)
                        {
                            content = myClient.Read("/api/now/table/" + Table.Name,
                                SnowParms.New.nocount()
                                    .inList(PROP.sys_id, ids)
                                    .columns(Table.Columns));
                        }
                    }
                    else
                    {
                        content = myClient.Read("/api/now/table/" + Table.Name,
                        SnowParms.New
                            .nocount()
                            .orderBy(PROP.sys_created_on)
                            .offset(myPage * PageSize)
                            .limit(PageSize)
                            .columns(Table.Columns));
                    }
                    break;
            }
            if (content.IsNullOrWhiteSpace())
            {
                response = new QueryResponse() { result = new Newtonsoft.Json.Linq.JObject[] { } };
            }
            else
            {
                response = myClient.Deserialize<QueryResponse>(content);
            }
            response.ContinueOnEmptyResponse = ContinueOnEmptyResponse;
            return response;
        }


    }

}
