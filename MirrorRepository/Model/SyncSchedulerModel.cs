using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Enums;
using MirrorRepository.Model.SnowDbSyncMgnt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using MirrorRepository.Base;
using MirrorRepository.Model.SyncParams;
using WebGrease.Css.Extensions;

namespace MirrorRepository.Model
{
    /// <summary>
    /// class SyncScheduler
    /// </summary>
    public class SyncSchedulerModel : BaseModel
    {
        /// <summary>
        /// Member which holds the snowtablenames
        /// </summary>
        protected string _snowTableNames;

        /// <summary>
        /// the current SynchronizationId or null if new
        /// </summary>
        public Guid? SynchronizationId { get; set; }

        /// <summary>
        /// Gets or sets the selected DatabaseId
        /// </summary>
        public Guid? SelectedDatabaseId { get; set; }

        /// <summary>
        /// Gets or sets the selected InstanceId
        /// </summary>
        public Guid? SelectedInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the RunImmediately value
        /// </summary>
        public bool RunImmediately { get; set; }

        /// <summary>
        /// Gets or sets the selected sync type
        /// </summary>
        public SyncType SyncType { get; set; }

        /// <summary>
        /// Gets or sets the selected target
        /// </summary>
        public SyncTarget SyncTarget{ get; set; }

        /// <summary>
        /// DatabaseSettings for this Synchronization
        /// </summary>
        public DatabaseSettings SelectedDatabaseSettings { get; set; }

        /// <summary>
        /// InstanzSettings for this Synchronization
        /// </summary>
        public InstanzSettings SelectedInstanzSettings { get; set; }

        /// <summary>
        /// Gets or sets the SelectedInterval value
        /// </summary>
        public EnumInterval SelectedInterval { get; set; }

        /// <summary>
        /// Gets or sets the selected interval name
        /// </summary>
        public string SelectedIntervalName { get; set; }

        /// <summary>
        /// Gets or sets the ActiveSince value
        /// </summary>
        public string ActiveSince { get; set; }

        /// <summary>
        /// Gets or sets the SyncTime value
        /// </summary>
        public string SyncTime { get; set; }

        /// <summary>
        /// Gets or sets the SelectedDaysOfWeek values
        /// </summary>
        public List<SnowDayOfWeek> SelectedDaysOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the IntervalInMinutes values
        /// </summary>
        public int? IntervalInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the SnowTable values
        /// </summary>
        public List<SnowTables> SnowTables { get; set; } = new List<SnowTables>();

        /// <summary>
        /// Gets or sets the SnowTableNames
        /// </summary>
        public string SnowTableNames { 
            get => _snowTableNames;
            set {
                _snowTableNames = value;
                if (!string.IsNullOrEmpty(_snowTableNames))
                {
                    SnowTables = _snowTableNames.Split(';').ToList().Select(t => new Model.SnowTables() { Name = t }).ToList();
                }
            }
        }
        
        public List<SyncProcess> SyncProcesses { get; set; }

        /// <summary>
        /// Gets or sets the bulk sync value
        /// </summary>
        public bool BulkSync { get; set; }

        /// <summary>
        /// Gets or sets the AutoSchemaUpdate value
        /// </summary>
        public bool AutoSchemaUpdate { get; set; }

        /// <summary>
        /// Gets or sets the synchronization name value
        /// </summary>
        public string SynchronizationName { get; set; }

        /// <summary>
        /// Gets or sets the CustomDeltaStart
        /// </summary>
        public DateTime? CustomDeltaStart { get; set; }

        /// <summary>
        /// Gets or sets the calculated next planned synctime
        /// </summary>
        public string NextPlannedSync { get; set; }

        /// <summary>
        /// Gets or sets the created syncronization value
        /// </summary>
        public DateTime? Created { get; set; }

        public int SnowTablesCount { get; set; }
        
        /// <summary>
        /// Gets or sets the process running value
        /// </summary>
        public bool ProcessRunning { get; set; }

        /// <summary>
        /// Maximum Threads for this Synchronization task
        /// </summary>
        public int MaxThreads { get; set; } = 20;

        /// <summary>
        /// Gets or sets the Thread per Table value
        /// </summary>
        public int ThreadsPerTable { get; set; } = 10;

        /// <summary>
        /// Gets or sets the Thread Sleep Time
        /// </summary>
        public int ThreadSleepTime { get; set; } = 1;

        /// <summary>
        /// Maximum Errors per Page per Table
        /// </summary>
        public int MaxErrorsPerPage { get; set; } = 50;
        
        /// <summary>
        /// Size of a page retrieved from Snow
        /// </summary>
        public int PageSize { get; set; } = 1000;

        /// <summary>
        /// Size of Kafka page
        /// </summary>
        public int? KafkaBlockSize { get; set; } = 50;

        /// <summary>
        /// Gets or sets the KafkaMode
        /// </summary>
        public string KafkaMode { get; set; }

        /// <summary>
        /// Timeout for Snow RestClient Requests in seconds
        /// </summary>
        public int RequestTimeout { get; set; } = 30;

        public bool ExecuteCleanup { get; set; }

        /// <summary>
        /// Gets or sets the SubtractMinutesFromDelta value
        /// </summary>
        public int SubtractMinutesFromDelta { get; set; } = 0;

        /// <summary>
        /// Initialize the model
        /// </summary>
        /// <param name="sync"></param>
        /// <returns></returns>
        public SyncSchedulerModel Init(Data.SnowDbSyncMgnt.Synchronization sync)
        {
            using (var entities = new ServiceNowDbSyncMgntEntities())
            {
                //load syncType
                var syncTypeObject = entities.SyncType.FirstOrDefault(s => s.Id == sync.SyncTypeId);
                SyncType = syncTypeObject;

                //load targetType
                var targetObject = entities.SyncTarget.FirstOrDefault(s => s.Id == sync.SyncTargetId);
                SyncTarget = targetObject;

                //load selected DatabaseSettings
                var dbSettingsObject = entities.DatabaseSettings.FirstOrDefault(s => s.Id == sync.DatabaseSettingsId);
                SelectedDatabaseSettings = dbSettingsObject;

                //load selected InstanceSettings
                var dbInstanceObject = entities.InstanzSettings.FirstOrDefault(s => s.Id == sync.InstanzSettingsId);
                SelectedInstanzSettings = dbInstanceObject;
            }

            SynchronizationId = sync.Id;
            SynchronizationName = sync.Name;
            SnowTableNames = sync.SnowTables;
            RunImmediately = sync.RunImmediately;
            SelectedDatabaseId = sync.DatabaseSettingsId;
            SelectedInstanceId = sync.InstanzSettingsId;
            MaxThreads = sync.MaxThreads;
            ThreadsPerTable = sync.ThreadsPerTable;
            ThreadSleepTime = sync.ThreadSleepTime;
            PageSize = sync.PageSize;
            KafkaBlockSize = sync.KafkaBlockSize;
            KafkaMode = sync.KafkaMode;
            MaxErrorsPerPage = sync.MaxErrorsPerPage;
            RequestTimeout = sync.RequestTimeout;
            StartTime = sync.StartDate;
            EndTime = sync.EndDate;
            FinalMessage = sync.FinalMessage;
            FinalErrorMessage = sync.FinalErrorMessage;

            ActiveSince = !string.IsNullOrWhiteSpace(sync.SyncActiveSinceDate) ? sync.SyncActiveSinceDate : "";
                
            SyncTime = sync.SyncStartTime;
            CustomDeltaStart = sync.CustomDeltaStart;
            SubtractMinutesFromDelta = sync.SubtractMinutesFromDelta;

            if (!string.IsNullOrWhiteSpace(sync.SyncInterval))
            {
                Enum.TryParse(sync.SyncInterval, out EnumInterval selEnumInterval);
                SelectedInterval = selEnumInterval;
                SelectedIntervalName = sync.SyncInterval;
            }
            
            SyncTime = sync.SyncStartTime;

            if (sync.GetActiveDays().Any())
            {


                List<SnowDayOfWeek> dayOfWeekSelection = sync.GetActiveDayNames()
                    .Select(d => new SnowDayOfWeek() { Day = d }).ToList();

                SelectedDaysOfWeek = dayOfWeekSelection;
            }

            IntervalInMinutes = sync.PeriodInterval;

           
            BulkSync = true;
            

            AutoSchemaUpdate = sync.AutoSchemaUpdate;
            Created = sync.Created;
            ExecuteCleanup = sync.ExecuteCleanup;

            if (SelectedInstanzSettings != null && SnowTables != null && SnowTables.Any() && SyncType != null)
            {
                SetTableDefinitions(SelectedInstanzSettings.Id, SyncType.Id);
            }

            //calc next planned synchronization
            NextPlannedSync = GetNextPlannedSynchronization(sync);

            return this;
        }

        
        /// <summary>
        /// Gets or sets the FinalErrorMessage value
        /// </summary>
        public string FinalErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the FinalMessage value
        /// </summary>
        public string FinalMessage { get; set; }

        /// <summary>
        /// Gets or sets the EndTime
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the StartTime value
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the IsAdmin Role value
        /// </summary>
        public bool IsAdmin { get; set; }
        
        /// <summary>
        /// Stop running process from single table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="syncId"></param>
        public void StopRunningProcess(string table, Guid syncId)
        {
            ListDictionary tableDictionary = new ListDictionary {{table, syncId}};
            StopRunningProcess(tableDictionary);
        }

        /// <summary>
        /// Stop running processes from tables restricted to SnowInstance
        /// </summary>
        /// <param name="tableList"></param>
        public void StopRunningProcess(ListDictionary tableList)
        {
            if (tableList != null)
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    foreach (DictionaryEntry tableDict in tableList)
                    {
                        //get all processes from tableSysId where EndTime is null => running table processes
                        var snowSingleSync = entities.Synchronization.FirstOrDefault(i => i.Id == (Guid)tableDict.Value);
                        
                        if (snowSingleSync != null)
                        {
                            List <Guid> syncList = entities.Synchronization.Where(i => i.InstanzSettingsId == snowSingleSync.InstanzSettingsId && i.SyncTypeId == snowSingleSync.SyncTypeId).Select(k => k.Id).ToList();

                            var syncProcess = entities.SyncProcess.Where(s => syncList.Contains(s.SynchronizationId) && s.TableName == (string)tableDict.Key && s.EndTime == null);
                            syncProcess.ForEach(s => s.StopProcess = true);
                            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}: Process stopped for table: {(string)tableDict.Key} and Synchronization: {snowSingleSync.Name}");
                        }
                    }
                    
                    entities.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Suspend running process from single table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="syncId"></param>
        public void SuspendRunningProcess(string table, Guid syncId)
        {
            ListDictionary tableDictionary = new ListDictionary { { table, syncId } };
            SuspendRunningProcess(tableDictionary);
        }

        /// <summary>
        /// Suspend running processes from tables restricted to SnowInstance
        /// </summary>
        /// <param name="tableList"></param>
        public void SuspendRunningProcess(ListDictionary tableList)
        {
            try
            {
                if (tableList != null)
                {
                    using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                    {
                        foreach (DictionaryEntry tableDict in tableList)
                        {
                            //get all processes from tableSysId where EndTime is null => running table processes
                            var snowSingleSync = entities.Synchronization.FirstOrDefault(i => i.Id == (Guid)tableDict.Value);
                            if (snowSingleSync != null)
                            {
                                List<Guid> syncList = entities.Synchronization.Where(i => i.InstanzSettingsId == snowSingleSync.InstanzSettingsId && i.SyncTypeId == snowSingleSync.SyncTypeId).Select(k => k.Id).ToList();

                                var syncProcess = entities.SyncProcess.Where(s => syncList.Contains(s.SynchronizationId) && s.TableName == (string)tableDict.Key && s.EndTime == null);
                                syncProcess.ForEach(s => s.SuspendProcess = true);
                                Log.Info($"{MethodBase.GetCurrentMethod()?.Name}: Process suspended for table: {(string)tableDict.Key} and Synchronization: {snowSingleSync.Name}");
                            }
                        }

                        entities.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}, {e.InnerException}");
            }
            
        }

        /// <summary>
        /// Continue previous suspended process from single table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="syncId"></param>
        public void ContinueSuspendedProcess(string table, Guid syncId)
        {
            ListDictionary tableDictionary = new ListDictionary { { table, syncId } };
            ContinueSuspendedProcess(tableDictionary);
        }

        /// <summary>
        /// Continue previous suspended processes from tables restricted to SnowInstance
        /// </summary>
        /// <param name="tableList"></param>
        public void ContinueSuspendedProcess(ListDictionary tableList)
        {
            try
            {
                if (tableList != null)
                {
                    using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                    {
                        foreach (DictionaryEntry tableDict in tableList)
                        {
                            //get all processes from tableSysId where EndTime is null => running table processes
                            var snowSingleSync =
                                entities.Synchronization.FirstOrDefault(i => i.Id == (Guid) tableDict.Value);
                            if (snowSingleSync != null)
                            {
                                List<Guid> syncList = entities.Synchronization.Where(i => i.InstanzSettingsId == snowSingleSync.InstanzSettingsId && i.SyncTypeId == snowSingleSync.SyncTypeId).Select(k => k.Id).ToList();

                                var syncProcess = entities.SyncProcess.Where(s => syncList.Contains(s.SynchronizationId) && s.TableName == (string) tableDict.Key && s.EndTime == null);
                                syncProcess.ForEach(s => s.SuspendProcess = false);
                                Log.Info(
                                    $"{MethodBase.GetCurrentMethod()?.Name}: Continue suspended process for table: {(string) tableDict.Key} and SnowInstance: {snowSingleSync.Name}");
                            }
                        }

                        entities.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}, {e.InnerException}");
            }

        }

        /// <summary>
        /// set restricted table columns
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="syncTypeId"></param>
        /// <returns></returns>
        public List<string> SetTableDefinitions(Guid instanceId, Guid syncTypeId)
        {
            using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
            {
                foreach (var table in SnowTables)
                {
                    SnowTableDefinition snowTableDefinition = ctx.SnowTableDefinition.FirstOrDefault(s => s.Table == table.Name);

                    if (snowTableDefinition != null && !string.IsNullOrWhiteSpace(snowTableDefinition.TableParams))
                    {
                        List<TableParam> tblParams = snowTableDefinition.TableParameters;
                        var instance = ctx.InstanzSettings.FirstOrDefault(i => i.Id == instanceId);
                        var syncType = ctx.SyncType.FirstOrDefault(s => s.Id == syncTypeId);

                        var tableParam = tblParams.FirstOrDefault(t => t.InstanceName == instance?.InstanzName);

                        SyncParameter syncParams = tableParam?.SynchronizationTypes.FirstOrDefault(t => t.SyncTypeName == syncType?.TypeName)?.SyncParameter;

                        if (syncParams != null)
                        {
                            table.Enabled = syncParams.Enabled ?? false;
                        }
                    }

                    if (snowTableDefinition?.TableParameters != null)
                    {
                        List<TableParam> tblParams = snowTableDefinition.TableParameters;
                        var tableParam = tblParams.FirstOrDefault(t => t.InstanceId == instanceId);
                        table.Columns = tableParam?.SnowColummns;
                    }

                    if (snowTableDefinition != null && !string.IsNullOrWhiteSpace(snowTableDefinition.PostScripts))
                    {
                        List<ScriptCommand> scriptCommandList = JsonConvert.DeserializeObject<List<ScriptCommand>>(snowTableDefinition.PostScripts);
                        table.SciptCommands = scriptCommandList;
                    }

                }
            }

            return null;
        }

        /// <summary>
        /// Get next planning synchronization time
        /// </summary>
        /// <param name="synchronization"></param>
        /// <returns></returns>
        private string GetNextPlannedSynchronization(Data.SnowDbSyncMgnt.Synchronization synchronization)
        {
            //Todo
            return null;
        }
        
        public override string ToString()
        {
            return GetType().Name + "[" + SynchronizationId + "]: " + SynchronizationName;
        }

        
    }
}
