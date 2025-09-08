using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using MirrorRepository.Base;
using MirrorRepository.Constants;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Enums;
using MirrorRepository.Helpers;
using MirrorRepository.Model;
using MirrorRepository.Model.InterfaceMonitoring;
using MirrorRepository.Model.SnowDbSyncMgnt;
using MirrorRepository.Model.SyncParams;
using MirrorRepository.REST;
using MirrorRepository.Synchronization;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorRepository.Processor
{
    public abstract class SyncTaskBase<C> where C:IClient<C>
    {
        protected ILog _log = null; // log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        protected ILog Log { get
            {
                if (_log == null)
                {
                    _log = LogManager.GetLogger(this.GetType());
                }
                return _log;
            }
        }
        private string _myTag;
        public string MyTag { get { return _myTag + "[" + Thread.CurrentThread.ManagedThreadId + "/" + ParentThreadId + "]";  }
            set {
                _myTag = "" + value;
            } 
        }
        public int KafkaBlockSize { get; set; } = 50;
        public SyncProcessType SyncType { get; internal set; } = SyncProcessType.Consistency;
        public SyncScheduler Scheduler { get; set; } = new SyncScheduler();
        public SnowProcessorBase Processor { get; internal set; }
        public List<LogMessage> LogMessages { get; protected set; } = new List<LogMessage>();
        public Guid SynchronizationId { get; set; }
        public SnowDbContext Context { get; set; }
        public SnowTables Table { get; internal set; }
        public int PageSize { get; set; } = 1000;
        public int MaxErrorsPerPage { get; set; } = 50;
        public int PoolSize { get; set; } = 10;
        public int RequestTimeout { get; set; } = 30;
        public int SleepTime { get; set; } = 1;
        public int IntervalInMinutes { get; set; } = 15;
        protected PageQueue PageQueue = new PageQueue();
        public int Pages { get; protected set; }
        public bool Stop { get; set; }
        public WriteReport[] Reports { get; protected set; }
        public SyncProcess Entity { get; protected set; }
        public int Found { get; protected set; }
        public string FinalMessage { get; set; }
        public string FinalErrorMessage { get; set; }
        public EnumInvocation Invocation { get; set; }
        public bool ExecuteCleanup { get; set; }
        public C Client { get; set; }
        public EsbClient EsbClient { get; set; }
        public int ParentThreadId { get; set; } 


        /// <summary>
        /// Execute the complete update process for one table of the selected Synchronization.
        /// This generates and supervises the paged synchronization.
        /// </summary>
        public void Execute()
        {
            (bool shallExecute, Data.SnowDbSyncMgnt.Synchronization mySynchronization, KeyValuePair<SnowDictEntry, List<SnowDictEntry>> snowDictEntry,
                MonitoringSettings monitoringSettings, InterfaceMonitoringResponse monitoringResponse) = ValidateAndInit();

            if (shallExecute)
            {
                LogMessages.Add(new LogMessage
                {
                    Key = GetKey(),
                    Table = Table.Name,
                    Message = "Starting: " + Table.Name + " ThreadPool: " + PoolSize
                });

                //shallExecute = TryMigration(shallExecute); must be done by Processor!!!

                if (shallExecute)
                {
                    // #ICTS-5048: overrule specific syncSettings:
                    OverRuleDefaultPerformance(mySynchronization, Table.Name);

                    // Execute Threads:
                    Reports = new WriteReport[PoolSize];
                    var tasks = new Task[PoolSize];

                    try
                    {
                        for (int i = 0; i < PoolSize; i++)
                        {
                            int threadIndex = i;
                            Reports[threadIndex] = new WriteReport() { LogTag = Table.Name + "[" + threadIndex + "]" };
                            tasks[threadIndex] = Processor.SyncTaskFactory.StartNew(() => ExecutePages(threadIndex));
                        }
                        Task.WaitAll(tasks);

                        // Report Execution results:
                        FinalMessage = "All tasks finished.";
                        if (Stop)
                        {
                            FinalMessage = "Processing interrupted!";
                        }
                        LogMessages.Add(new LogMessage { Key = GetKey(), Table = Table.Name, Message = FinalMessage });

                        if (!Stop || Processor.ForceSync)
                        {
                            try
                            {
                                switch (SyncType)
                                {
                                    case SyncProcessType.Full:
                                        ProcessSyncFull(snowDictEntry);

                                        break;

                                    case SyncProcessType.Consistency:
                                        ProcessSyncConsistency();
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                LogError("Final processing failed for table " + Table.Name + " : " + e.Message, e);
                            }
                        }
                        else
                        {
                            Log.Info("Interrupt: Final processing skipped for table " + Table.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is AggregateException) e = ((AggregateException)e).GetBaseException();
                        FinalErrorMessage = "Processing failed: " + e.Message;
                        List<AggregateException> failures = tasks.ToList().Where(t => t.Exception != null).Select(t => t.Exception).ToList();
                        if (failures.Count > 0)
                        {
                            FinalMessage = "Finished with Errors!" + (Stop ? " " + FinalMessage : "");
                            FinalErrorMessage += "; Errors: " + string.Join(",", failures.Select(ex => ex.Message));
                            LogMessages.Add(new LogMessage { Key = GetKey(), Table = Table.Name, Message = FinalErrorMessage });
                        }
                        LogMessages.Add(new LogMessage { Key = GetKey(), Table = Table.Name, Message = FinalErrorMessage });
                    }
                }
            }

            EndExecute(monitoringSettings, monitoringResponse);
        }

        public string GetKey()
        {
            return Enum.GetName(typeof(SyncProcessType), SyncType) + ":" + Table?.Name + ":" + Processor.SyncName;
        }


        protected void LogError(string message, Exception e)
        {
            FinalErrorMessage = message;
            LogMessages.Add(new LogMessage { Key = GetKey(), Table = Table.Name, Message = FinalErrorMessage });
            Log.Info(MyTag + ": " + FinalErrorMessage + " : " + Entity + " : " + e, e);
            Log.Warn(MyTag + ": " + FinalErrorMessage + " : " + Entity + " : " + e);
        }

        /// <summary>
        /// Execute the transactional paged insert/update to database.
        /// This is the final real workhorse of the synchronization process!
        /// </summary>
        /// <param name="threadIndex"></param>
        protected void ExecutePages(int threadIndex)
        {
            var myClient = Client.New();
            using (var cnx = Context.New().Database.GetDbConnection())
            {
                while (!Stop)
                {
                    var SyncProcEntry = Scheduler.GetOrCreate(Table.Name, SynchronizationId, GetKey());

                    // check for Synchronization Break:
                    if (SyncProcEntry != null && SyncProcEntry.StopProcess)
                    {
                        Stop = true;
                        Log.Info(MyTag + ": process stopped: " + SyncProcEntry);
                        continue;
                    }

                    // check for Synchronization Suspend:
                    if (SyncProcEntry != null && SyncProcEntry.SuspendProcess)
                    {
                        Log.Info(MyTag + ": process suspended: " + SyncProcEntry);
                        Thread.Sleep(SleepTime * 1000);
                        continue;
                    }

                    var myPage = PageQueue.GetPage();
                    Log.Info(string.Format(MyTag + ": running page={0}", myPage));

                    if (myPage.Failures > MaxErrorsPerPage)
                    {
                        PageQueue.FailedPages.Add(myPage);
                        //AppendFinalErrorMessage("MaxErrors exceeded for " + Table.Name);
                        Log.Info(MyTag + ": max retries exceeded: " + myPage.Failures + " for " + myPage);
                        continue;
                    }

                    try
                    {
                        var report = ExecutePage(myClient, myPage, SyncProcEntry, cnx);
                        if (report == null) // Task finished!
                        {
                            Log.Info(string.Format(MyTag + ": empty report for page={0}, breaking..", myPage));
                            break;
                        }
                        // update sync status
                        report.Page = myPage.Page;
                        Pages++;
                        report.Pages = Pages;

                        var syncProc = UpdateEntity(report, myPage);
                        Log.Info(MyTag + ": read report: " + report);
                        Reports[threadIndex] = report;

                        //testing only:
                        if (Processor.ForceSync && Pages > 1)
                        {
                            Stop = true;
                        }

                    }
                    catch (Exception e)
                    {
                        PageQueue.ReturnPage(myPage);
                        //LogMessages.Add(new LogMessage { Key = GetKey(), Table = Table.Name, Message = "failed to read page: " + myPage });
                        Log.Info(string.Format(MyTag + ": failed to read page={0}", myPage), e);
                        Thread.Sleep(SleepTime * 1000);
                    }
                }
            }
        }

        public abstract WriteReport ExecutePage(C client, QueueEntry myPage, SyncProcess syncProcEntry, DbConnection cnx);
        protected abstract bool TryMigration(bool shallExecute);

        protected abstract void ProcessSyncFull(KeyValuePair<SnowDictEntry, List<SnowDictEntry>> snowSyncEntry);
        protected abstract void ProcessSyncConsistency();

        protected virtual void EndExecute(MonitoringSettings monitoringSettings, InterfaceMonitoringResponse monitoringResponse)
        {
            if (PageQueue.FailedPages.Count > 0)
            {
                AppendFinalErrorMessage("MaxErrors exceeded for pages: " + string.Join(",", PageQueue.FailedPages.Select(p => p.Page)));
            }

            try
            {
                Entity = Scheduler.GetOrCreate(Table.Name, SynchronizationId, GetKey());
                Entity.SyncTime = DateTime.Now;
                Entity.EndTime = DateTime.Now;
                if (SyncType == SyncProcessType.Delta)
                {
                    //set delta start
                    Entity.GetDeltaRecordsFrom = GetDeltaStartTimeAsDate(Entity);

                    //reset custom delta start if set on table
                    ResetCustomDeltaStart(SynchronizationId, Table.Name, FinalErrorMessage, Entity.GetDeltaRecordsFrom);
                }
                Entity.PreviousEndTime = Entity.EndTime;
                Entity.PreviousStartTime = Entity.StartTime;
                Entity.Page = PageQueue.FailedPages.OrderByDescending(e => e.Failures).Select(e => e.Page).FirstOrDefault();
                Entity.Failures = PageQueue.FailedPages.Count;
                Entity.FinalMessage = FinalMessage;
                Entity.FinalErrorMessage = FinalErrorMessage;
                Entity.LogMessages = LogMessages;
                Scheduler.Update(Entity);

                //create table monitoring record
                try
                {
                    Task.Factory.StartNew(() => new TableMonitoringModel().AddTableMonitoringRecord(Entity));
                }
                catch (Exception ex)
                {
                    Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error create table monitoring for table:{Entity.TableName}. {ex.Message}");
                }


            }
            catch (Exception e)
            {
                Log.Info(MyTag + ": cannot update: " + Entity, e);
                Log.Warn(MyTag + ": cannot update: " + Entity + " : " + e);
            }

            if (monitoringSettings != null)
            {
                UpdateMonitoring(monitoringResponse, monitoringSettings);
            }
        }

        protected virtual (bool, Data.SnowDbSyncMgnt.Synchronization, KeyValuePair<SnowDictEntry, List<SnowDictEntry>> snowDictEntry, 
            MonitoringSettings, InterfaceMonitoringResponse) ValidateAndInit()
        {
            // validate Input:
            if (Table == null) throw new ArgumentNullException("Execution impossible, Table is null.");
            if (string.IsNullOrEmpty(Table.Name)) throw new ArgumentNullException("Execution impossible, Table.Name is null or empty.");

            MyTag = Table.Name + "[" + GetKey() + "]";

            // check, if same sync is active:
            var activeSyncs = Scheduler.FindActive(Table.Name, SynchronizationId);
            if (activeSyncs.Count > 0)
            {
                Log.Warn("ValidateAndInit["+MyTag+"}: stopping for running sync: " + string.Join(" : ", activeSyncs));
                return (false, null, new KeyValuePair<SnowDictEntry, List<SnowDictEntry>>(), null, null);
            }

            InitEntity(); // create some Entry independently from any concurrently running sync!!
            InitTableHierarchy();

            if (Processor.ForceSync) // for testing!
            {
                PoolSize = 1;
            }

            Client.Timeout = RequestTimeout; // initialize for next New() in ExecutePages

            bool shallExecute = true;
            var snowDictEntry = Processor.SnowDictionary.FirstOrDefault(t => t.Key.name == Table.Name.Trim());
            if (snowDictEntry.Key == null)
            {
                shallExecute = false;
                LogError("No DictionaryEntry found for table " + Table.Name, null);
            }
            else
            {
                Table.SysId = snowDictEntry.Key.sys_id;
            }

            Found = CountEntries();
            InitEntity(); // update Entry independently from any concurrently running sync!!

            activeSyncs.Clear();
            var mySynchronization = new DatabaseSettingsModel().Find<Data.SnowDbSyncMgnt.Synchronization>(SynchronizationId);
            if (mySynchronization != null)
            {
                // prohibit concurrent synchronizations of same table: 
                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    DatabaseSettings dbSettings = ctx.DatabaseSettings.FirstOrDefault(d => d.Id == mySynchronization.DatabaseSettingsId);
                    SyncTarget target = ctx.SyncTarget.FirstOrDefault(t => t.Id == mySynchronization.SyncTargetId);

                    if (target == null || target.TargetType.Equals(EnumTargetType.Sql.ToString()))
                    {
                        activeSyncs = Scheduler.FindActive(Table.Name, syncId: mySynchronization.Id, currentDatabaseSettings: dbSettings)
                            .Where(s => s.SynchronizationId != SynchronizationId).ToList();
                    }
                    
                }
               
            }
            if (activeSyncs.Count > 0)
            {
                shallExecute = false;
                LogError("Execution["+MyTag+"] denied for concurrent sync: " + string.Join(" : ", activeSyncs), null);
            }

            InterfaceMonitoringResponse monitoringResponse = null;
            MonitoringSettings monitoringSettings = new SyncScheduler().InterfaceMonitoringEnabled();

            if (shallExecute)
            {
                if (monitoringSettings != null &&
                    monitoringSettings.InterfaceMonitoring &&
                    monitoringSettings.MonitoringLevel == SnowDbSyncConstants.LevelSingleTable)
                {
                    MonitoringHandler monitoringHandler = new MonitoringHandler();
                    monitoringResponse = monitoringHandler.CreateInterfaceMonitoringRecord(Table.Name, Found, monitoringSettings.MonitoringLevel, mySynchronization?.Name);
                }
            }

            return (shallExecute, mySynchronization, snowDictEntry, monitoringSettings, monitoringResponse);
        }

        protected abstract void InitTableHierarchy();

        protected abstract int CountEntries();

        //calculate the delta synchronization start time
        public static DateTime GetDeltaStartTimeAsDate(SyncProcess syncProcessEntity)
        {
            DateTime deltaStart;

            using (var ctx = new ServiceNowDbSyncMgntEntities())
            {
                //custom delta start is set?
                var synchronization = ctx.Synchronization.FirstOrDefault(s => s.Id == syncProcessEntity.SynchronizationId);

                // 1. use CustomDeltaStart from Synchronization table for all tables in synchronization
                if (synchronization?.CustomDeltaStart != null)
                {
                    return synchronization.CustomDeltaStart.Value;
                }

                // 2. use CustomDeltaStart from table params
                SnowTableDefinition tableDefinitionEntity = ctx.SnowTableDefinition.FirstOrDefault(t => t.Table.Equals(syncProcessEntity.TableName));
                if (synchronization != null && tableDefinitionEntity != null && !string.IsNullOrWhiteSpace(tableDefinitionEntity.TableParams))
                {
                    List<TableParam> tblParams = tableDefinitionEntity.TableParameters;
                    var tableParam = tblParams.FirstOrDefault(t => t.InstanceId == synchronization.InstanzSettingsId);
                    if (tableParam != null)
                    {
                        SyncParameter syncParams = tableParam.SynchronizationTypes.FirstOrDefault(t => t.SyncTypeId == synchronization.SyncTypeId)?.SyncParameter;
                        if (syncParams != null && syncParams.CustomDeltaStart != null)
                        {
                            return syncParams.CustomDeltaStart.Value;
                        }
                    }
                }

                // 3. use PreviousStartTime from SyncProcess table for current table
                if (syncProcessEntity.PreviousStartTime != null)
                {
                    deltaStart = syncProcessEntity.PreviousStartTime.Value;
                }
                else
                {

                    synchronization = ctx.Synchronization.FirstOrDefault(s => s.Id == syncProcessEntity.SynchronizationId);
                    var fullSyncProcess = ctx.SyncProcess.FirstOrDefault(b => b.TableName == syncProcessEntity.TableName && b.Synchronization.SyncType.TypeName.Equals("Full") && b.Synchronization.InstanzSettingsId.Value == synchronization.InstanzSettingsId);

                    if (fullSyncProcess?.EndTime != null)
                    {
                        deltaStart = fullSyncProcess.EndTime.Value;
                    }
                    else
                    {
                        DateTime fallbackDeltaStart = DateTime.Now.AddMinutes(-2 * 1440);
                        deltaStart = fallbackDeltaStart;
                    }

                }

                if (synchronization != null && synchronization.SubtractMinutesFromDelta > 0)
                {
                    deltaStart = deltaStart.AddMinutes(-1 * synchronization.SubtractMinutesFromDelta);
                }
            }


            return deltaStart;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void InitEntity()
        {
            try
            {
                var syncType = Scheduler.GetOrCreateSyncType(Enum.GetName(typeof(SyncProcessType), SyncType));
                Entity = Scheduler.GetOrCreate(Table.Name, SynchronizationId, GetKey());
                Entity.Key = GetKey();
                Entity.StartTime = DateTime.Now;
                Entity.SyncTime = DateTime.Now;
                Entity.EndTime = null;
                Entity.TableName = Table.Name;
                Entity.SysId = Table.SysId;
                Entity.RecordsFound = Found;
                Entity.RecordsSynchronized = 0;
                Entity.RecordsInserted = 0;
                Entity.RecordsUpdated = 0;
                Entity.Pages = 0;
                Entity.Page = 0;
                Entity.Failures = 0;
                Entity.MaxFailures = 0;
                Entity.MaxFailuresPage = 0;
                Entity.FinalMessage = FinalMessage;
                Entity.FinalErrorMessage = FinalErrorMessage;
                Entity.LogMessages = LogMessages;
                if (Invocation == EnumInvocation.Service)
                {
                    Entity.ServiceStartTime = DateTime.Now;
                }
                Scheduler.Update(Entity);
            }
            catch (Exception e)
            {
                Log.Info(MyTag + ": cannot init " + GetKey(), e);
                throw; // must not continue!!
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected SyncProcess UpdateEntity(WriteReport report, QueueEntry queueEntry)
        {
            try
            {
                Entity = Scheduler.GetOrCreate(Table.Name, SynchronizationId, GetKey());
                Entity.SyncTime = DateTime.Now;
                Entity.RecordsSynchronized += report.Tuples; // report.Inserted + report.Updated;
                Entity.RecordsInserted += report.Inserted;
                Entity.RecordsUpdated += report.Updated;
                Entity.Pages = report.Pages;
                Entity.Page = queueEntry.Page;
                Entity.Failures += queueEntry.Failures;
                var maxEntity = PageQueue.GetMaxFails();
                if (maxEntity != null)
                {
                    Entity.MaxFailures = maxEntity.Failures;
                    Entity.MaxFailuresPage = maxEntity.Page;
                }
                Entity.LogMessages = LogMessages;
                Scheduler.Update(Entity);
                Log.Debug("updated[" + GetKey() + "]: " + Entity);
                return Entity;
            }
            catch (Exception e)
            {
                Log.Info("cannot update[" + GetKey() + "]: " + report, e);
                // may continue 
            }
            return null;
        }

        protected static void UpdateMonitoring(InterfaceMonitoringResponse monitoringResponse, MonitoringSettings monitoringSettings)
        {
            //update record with finished datetime if monitoring is enabled and monitoring level is set to SingleTable
            if (monitoringSettings != null && monitoringSettings.InterfaceMonitoring && monitoringSettings.MonitoringLevel == SnowDbSyncConstants.LevelSingleTable && monitoringResponse != null)
            {
                MonitoringHandler monitoringHandler = new MonitoringHandler();
                monitoringHandler.UpdateInterfaceMonitoringRecord(monitoringResponse);
            }
        }

        /// <summary>
        /// reset custom delta start to null
        /// </summary>
        /// <param name="synchGuid"></param>
        /// <param name="tableName"></param>
        /// <param name="finalErrorMsg"></param>
        /// <param name="deltaStart"></param>
        protected void ResetCustomDeltaStart(Guid synchGuid, string tableName, string finalErrorMsg, DateTime? deltaStart)
        {
            try
            {
                if (synchGuid != Guid.Empty && !string.IsNullOrWhiteSpace(tableName))
                {
                    using (var ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        var synchronization = ctx.Synchronization.FirstOrDefault(s => s.Id == synchGuid);
                        var syncTarget = ctx.SyncTarget.FirstOrDefault(s => s.Id == synchronization.SyncTargetId);

                        // Get CustomDeltaStart from table params
                        SnowTableDefinition tableDefinitionEntity = ctx.SnowTableDefinition.FirstOrDefault(t => t.Table.Equals(tableName));
                        if (synchronization != null &&
                            tableDefinitionEntity != null &&
                            !string.IsNullOrWhiteSpace(tableDefinitionEntity.TableParams) &&
                            syncTarget != null)
                        {
                            List<TableParam> tblParams = JsonConvert.DeserializeObject<List<TableParam>>(tableDefinitionEntity.TableParams);
                            var instance = ctx.InstanzSettings.FirstOrDefault(i => i.Id == synchronization.InstanzSettingsId);
                            var tableParam = tblParams?.FirstOrDefault(t => t.InstanceName != null && t.InstanceName == instance?.InstanzName);
                            if (tableParam != null)
                            {
                                SyncParameter syncParams = tableParam.SynchronizationTypes.FirstOrDefault(t => t.SyncTypeId == synchronization.SyncTypeId)?.SyncParameter;
                                if (syncParams != null)
                                {
                                    
                                    if (!string.IsNullOrWhiteSpace(finalErrorMsg))
                                    {
                                        //delta sync failed set custom delta to previous start time
                                        syncParams.CustomDeltaStart = deltaStart;
                                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Sync failed for tableSync failed for table: {tableName} and instance: {synchronization.Name}. Set CustomDeltaStart to: {deltaStart}.");

                                        
                                    }
                                    else if(syncParams.CustomDeltaStart != null)
                                    {
                                        //reset custom delta start to null
                                        string currentDeltaStart = syncParams.CustomDeltaStart.ToString();
                                        syncParams.CustomDeltaStart = null;
                                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Reset CustomDeltaStart for table: {tableName} and instance: {synchronization.Name}. Previous: {currentDeltaStart}.");
                                    }

                                    var serTableParams = JsonConvert.SerializeObject(tblParams);
                                    tableDefinitionEntity.TableParams = serTableParams;

                                    ctx.SaveChanges();

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Failed reset custom delta start. Error: {e.Message} {e.InnerException}");
            }
            
        }

        /// <summary>
        /// overrule default synchronization parameters
        /// </summary>
        /// <param name="synchronization"></param>
        /// <param name="tableName"></param>
        protected void OverRuleDefaultPerformance(Data.SnowDbSyncMgnt.Synchronization synchronization, string tableName)
        {
            try
            {
                if (synchronization != null)
                {
                    using (var ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        var tblDefinition = ctx.SnowTableDefinition.FirstOrDefault(t => t.Table.Equals(tableName));
                        if (tblDefinition != null && !string.IsNullOrWhiteSpace(tblDefinition.TableParams))
                        {
                            List<TableParam> tableParams = tblDefinition.TableParameters;
                            var instanceParam = tableParams.FirstOrDefault(p => p.InstanceId == synchronization.InstanzSettingsId);

                            if (instanceParam != null)
                            {
                                SyncParameter syncParams = instanceParam.SynchronizationTypes.FirstOrDefault(t => t.SyncTypeId == synchronization.SyncTypeId)?.SyncParameter;

                                if (syncParams != null)
                                {
                                    if (syncParams.ThreadsPerTable.HasValue && syncParams.ThreadsPerTable > 0)
                                    {
                                        PoolSize = syncParams.ThreadsPerTable.Value;
                                        Log.Info("PoolSize overruled by SyncProcess: " + PoolSize);
                                    }
                                    if (syncParams.ThreadSleepTime.HasValue && syncParams.ThreadSleepTime > 0)
                                    {
                                        SleepTime = syncParams.ThreadSleepTime.Value;
                                        Log.Info("SleepTime overruled by SyncProcess: " + SleepTime);
                                    }
                                    if (syncParams.PageSize.HasValue && syncParams.PageSize > 0)
                                    {
                                        PageSize = syncParams.PageSize.Value;
                                        Log.Info("PageSize overruled by SyncProcess: " + PageSize);
                                    }
                                    if (syncParams.RequestTimeout.HasValue && syncParams.RequestTimeout > 0)
                                    {
                                        RequestTimeout = syncParams.RequestTimeout.Value;
                                        Log.Info("RequestTimeout overruled by SyncProcess: " + RequestTimeout);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: Error: {ex.Message}");
            }
        }

        protected void AppendFinalErrorMessage(string msg)
        {
            FinalErrorMessage = (string.IsNullOrEmpty(FinalErrorMessage) ? "" : FinalErrorMessage + "; ") + msg;
        }

        public override string ToString()
        {
            return GetType().Name + "[" + GetKey() + "]: table=" + Table?.Name + ", proc=" + Processor + ", dbCtx=" + Context;
        }
    }
}
