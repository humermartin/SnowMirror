using log4net;
using Microsoft.EntityFrameworkCore.Migrations;
using MimeKit;
using MirrorRepository.Constants;
using MirrorRepository.Model.InterfaceMonitoring;
using MirrorRepository.NotificationHelper;
using MirrorRepository.Processor;
using MirrorRepository.Base;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Enums;
using MirrorRepository.Model;
using MirrorRepository.Model.SnowDbSyncMgnt;
using MirrorRepository.Synchronization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MirrorRepository
{
    public class SnowProcessorRunner
    {
        public const string ENDED_PROCESSING = "Ended Processing";
        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public static ICollection<SnowProcessorRunner> ProcessorRunners { get; private set; } = new LinkedList<SnowProcessorRunner>();

        public string SnowAccessSettingName { get; set; } = "SnowAccess";
        public string SyncDatabaseSettingName { get; set; } = "ServiceNowDbSync";
        protected InstanzSettings SnowAccessSettings { get; set; }
        protected DatabaseSettings SyncDatabaseSettings { get; set; }
        protected SyncTarget KafkaTargetSettings { get; set; }
        protected SnowDbContext DbContext { get; set; }

        public Guid SynchronizationId { get; set; }
        public string SynchronizationName { get; set; }
        public SnowProcessor Processor { get; protected set; }
        public Task ProcessorTask { get; protected set; }
        public SnowProcessorRunner ServiceSnowProcessorRunner { get; protected set; }
        public string FinalMessage { get; set; }
        public string FinalErrorMessage { get; set; }
        public EnumInvocation Invocation { get; set; }

        public void RunAsync(List<SnowTables> retrySelection = null)
        {
            ProcessorTask = Task.Factory.StartNew(() => Run(retrySelection));
        }

        /// <summary>
        /// run process
        /// </summary>
        /// <param name="retrySelection"></param>
        public void Run(List<SnowTables> retrySelection = null)
        {
            Write("Run: " + SynchronizationId);
            
            ProcessorRunners.Add(this);
            Processor = new SnowProcessor(); // initialize default
            StartSync(SynchronizationId);

            SyncSchedulerModel syncScheduler = new SyncScheduler().GetActiveSynchronization(SynchronizationId);

            if (retrySelection != null && retrySelection.Any())
            {
                syncScheduler.SnowTables = retrySelection;
            }

            if (syncScheduler.SnowTables.Count == 0)
            {
                FinalMessage = "Empty Scheduler.Tables.";
                Log.Info(FinalMessage);
            }
            else
            {
                try
                {
                    var dbsm = new DatabaseSettingsModel();

                    var synchronization = dbsm.FindInternal<Data.SnowDbSyncMgnt.Synchronization>(SynchronizationId);
                    
                    if (synchronization.InstanzSettingsId.HasValue)
                    {
                        synchronization.InstanzSettings = dbsm.FindInstanceSetting<InstanzSettings>(synchronization.InstanzSettingsId.Value);
                    }

                    if (synchronization.DatabaseSettingsId.HasValue)
                    {
                        synchronization.DatabaseSettings = dbsm.FindDatabaseSetting<DatabaseSettings>(synchronization.DatabaseSettingsId.Value);
                    }
                    if (synchronization == null) throw new NullReferenceException("no Synchronization found for: " + SynchronizationId);
                    if (synchronization.InstanzSettings == null) throw new NullReferenceException("Synchronization has no SnowAccess settings: " + synchronization);
                    if (synchronization.DatabaseSettings == null) throw new NullReferenceException("Synchronization has no DbAccess settings: " + synchronization);

                    using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
                    {
                        //load syncType
                        var syncTypeObject = snowEntities.SyncType.FirstOrDefault(s => s.Id == synchronization.SyncTypeId);
                        synchronization.SyncType = syncTypeObject;

                        //load targetType
                        var targetObject = snowEntities.SyncTarget.FirstOrDefault(s => s.Id == synchronization.SyncTargetId);
                        synchronization.SyncTarget = targetObject;
                    }
                    
                    SnowAccessSettings = synchronization.InstanzSettings;
                    SyncDatabaseSettings = synchronization.DatabaseSettings;
                    KafkaTargetSettings = synchronization.SyncTarget;
                    
                    syncScheduler.SetTableDefinitions(SnowAccessSettings.Id, synchronization.SyncType.Id);
                    // Server=myServerName\myInstanceName;Database=myDataBase;User Id=myUsername;Password=myPassword;
                    // Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword;
                    DbContext = new SnowDbContext()
                    {
                        DBHOST = SyncDatabaseSettings.Servername + (SyncDatabaseSettings.Port > 0 ? "," + SyncDatabaseSettings.Port : ""),
                        DBNAME = SyncDatabaseSettings.Databasename,
                        DBUSER = SyncDatabaseSettings.Username,
                        DBPWD = SyncDatabaseSettings.Password
                    }.Init();

                    Enum.TryParse(syncScheduler.SyncType.TypeName, out SyncProcessType syncType);

                    Processor = new SnowProcessor()
                    {
                        Context = DbContext.New(),
                        SnowAccessSettings = SnowAccessSettings,
                        EsbAccessSettings = KafkaTargetSettings,
                        SyncScheduler = syncScheduler,
                        SyncType = syncType,
                        SyncName = syncScheduler.SynchronizationName,
                        Invocation = Invocation
                    };

                    //monitor to SNOW
                    InterfaceMonitoringResponse monitoringResponse = null;
                    MonitoringSettings monitoringSettings = new SyncScheduler().InterfaceMonitoringEnabled();
                    if (monitoringSettings != null && 
                        monitoringSettings.InterfaceMonitoring &&
                        monitoringSettings.MonitoringLevel == SnowDbSyncConstants.LevelPackage)
                    {
                        MonitoringHandler monitoringHandler = new MonitoringHandler();
                        monitoringResponse = monitoringHandler.CreateInterfaceMonitoringRecord(syncScheduler.SnowTableNames, null, monitoringSettings.MonitoringLevel, syncScheduler.SynchronizationName);
                    }

                    // Execute Processing:
                    Write("Process: " + syncScheduler.SynchronizationName + ", proc=" + Processor);
                    Processor.LogMessages.Add(new LogMessage { Key = "", Table = "", Message = "Process: " + syncScheduler.SynchronizationName });

                    //Only generate commands on non-kafka syncs
                    if (KafkaTargetSettings != null && KafkaTargetSettings.TargetType != EnumTargetType.Kafka.ToString())
                    {
                        //generate commands - if commands not empty send schema change mail
                        IReadOnlyList<MigrationCommand> cmds = Processor.GenerateCommands();
                        if (cmds.Any())
                        {
                            SendSchemaChangeNotification(cmds, synchronization);
                        }
                    }
                    
                    Processor.Process();
                    FinalMessage = ENDED_PROCESSING + ": " + syncScheduler.SynchronizationName;
                    Processor.LogMessages.Add(new LogMessage { Key = "", Table = "", Message = FinalMessage });

                    //update record with finished datetime if monitoring is enabled and monitoring level is set to Package
                    if (monitoringSettings != null && 
                        monitoringSettings.InterfaceMonitoring && 
                        monitoringSettings.MonitoringLevel == SnowDbSyncConstants.LevelPackage && 
                        monitoringResponse != null)
                    {
                        MonitoringHandler monitoringHandler = new MonitoringHandler();
                        monitoringHandler.UpdateInterfaceMonitoringRecord(monitoringResponse);
                    }


                    Write(FinalMessage);
                }
                catch (Exception e)
                {
                    var myExc = e;
                    if (e is AggregateException) myExc = ((AggregateException)e).GetBaseException();

                    FinalErrorMessage = "Processing failed: " + syncScheduler.SynchronizationName + ", msg=" + myExc.Message;
                    Processor.LogMessages.Add(new LogMessage { Key = "", Table = "", Message = FinalErrorMessage });
                    Log.Info(FinalErrorMessage, myExc);
                    Write(FinalErrorMessage, myExc);
                }
            }

            StopSync(SynchronizationId);
            ProcessorRunners.Remove(this);

            //collect and retry failed synchronizations
            RetryFailedSynchronizations(SynchronizationId);
            
        }

        void StartSync(Guid syncId)
        {
            Data.SnowDbSyncMgnt.Synchronization mySync = null;
            try
            {
                var dbsm = new DatabaseSettingsModel();
                mySync = dbsm.Find<Data.SnowDbSyncMgnt.Synchronization>(SynchronizationId);
                SynchronizationName = mySync.Name;
                mySync.SyncActiveSinceDate = DateTime.Now.ToString(SnowBase.SNOWDBSYNC_DATEFORMAT);
                mySync.StartDate = DateTime.Now;
                mySync.EndDate = null;
                mySync.LogMessages = Processor.LogMessages;
                mySync.FinalMessage = null;
                mySync.FinalErrorMessage = null;
                if (Invocation == EnumInvocation.Service)
                {
                    mySync.ServiceStartDate = DateTime.Now;
                }
                dbsm.Update(mySync);
            } 
            catch (Exception e)
            {
                Write("cannot Start: " + syncId + " : " + this, e);
                SnowBase.LogEntityException(Log, "cannot Start: " + syncId + " : " + e, e, mySync);
                throw;
            }
        }

        void StopSync(Guid syncId)
        {
            Data.SnowDbSyncMgnt.Synchronization mySync = null;
            try
            {
                var dbsm = new DatabaseSettingsModel();
                mySync = dbsm.Find<Data.SnowDbSyncMgnt.Synchronization>(SynchronizationId);
                if (mySync == null)
                {
                    Log.Warn("no SnowSync entry found for: " + SynchronizationId); 
                    return;
                }
                if (mySync.StartDate == null) mySync.StartDate = DateTime.Now;
                mySync.EndDate = DateTime.Now;
                mySync.SyncActiveSinceDate = null;
                mySync.LogMessages = Processor.LogMessages;
                mySync.FinalMessage = FinalMessage;
                mySync.FinalErrorMessage = FinalErrorMessage;
                dbsm.Update(mySync);
            } 
            catch (Exception e)
            {
                Write("cannot Stop: " + syncId, e);
                SnowBase.LogEntityException(Log, "cannot stop: " + syncId + " : " + e, e, mySync);
                Log.Warn("cannot stop: " + syncId + " : " + e);
            }
        }


        public void RunAsService(string serviceName)
        {
            Log.Info("RunAsService - Service: " + serviceName);
            Write("RunAsService - Service: " + serviceName);
            try
            {
                DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
                Calendar cal = dfi.Calendar;

                var syncScheduler = new SyncScheduler();
                SyncSchedulerModel model = new SyncSchedulerModel();
                List<Data.SnowDbSyncMgnt.Synchronization> syncs = new List<Data.SnowDbSyncMgnt.Synchronization>();

                var svcModel = new ServiceSettingsModel().GetServiceByName(serviceName);
                if (svcModel != null)
                {
                    if (!string.IsNullOrWhiteSpace(svcModel.ServiceName))
                    {
                        syncs = svcModel.ServiceSpecificSynchronizations;
                    }
                    else
                    {
                        Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: ServiceName='{serviceName}' not found in database. Service cannot process synchronizations.");
                        return;
                    }
                }
                else
                {
                    syncs = model.AllCopy<Data.SnowDbSyncMgnt.Synchronization>();
                }
                
                foreach (var sync in syncs)
                {
                    Write("sync: " + sync);
                    if (sync.Enabled)
                    {
                        // do not execute if active SyncProcesses found:
                        var activeSyncProcesses = syncScheduler.FindActive(sync.Id);
                        if (activeSyncProcesses.Count > 0)
                        {
                            Write("skipping while active: " + string.Join(",", activeSyncProcesses.Select(a => a.Key)));
                            continue;
                        }

                        // evaluate interval settings:
                        EnumInterval serviceInterval = EnumInterval.Manual;
                        Enum.TryParse(sync.SyncInterval, out serviceInterval);
                        var activeDays = sync.GetActiveDays();
                        if (serviceInterval != EnumInterval.Daily && activeDays.Count > 0 && !activeDays.Contains(cal.GetDayOfWeek(DateTime.Now)))
                        {
                            Log.Info("not scheduled for today: " + sync);
                        }
                        else
                        {
                            var timeToStartToday = SnowBase.ParseTime(sync.SyncStartTime);

                            switch (serviceInterval)
                            {
                                case EnumInterval.Manual:
                                    continue; // nothing to do

                                case EnumInterval.Periodically:
                                    if (sync.PeriodInterval.HasValue 
                                        && (sync.ServiceStartDate == null
                                            || (sync.EndDate.HasValue && sync.ServiceStartDate < DateTime.Now.AddMinutes(-sync.PeriodInterval.Value))
                                            )
                                        )
                                    {
                                        ExecuteAsService(sync);
                                    }
                                    break;
                                case EnumInterval.Daily:
                                    if (DateTime.Now > timeToStartToday
                                        && (sync.ServiceStartDate == null || (sync.EndDate.HasValue && sync.ServiceStartDate < timeToStartToday)))
                                    {
                                        ExecuteAsService(sync);
                                    }
                                    break;
                                case EnumInterval.Weekly:
                                    {
                                        if (DateTime.Now > timeToStartToday
                                            && (sync.ServiceStartDate == null || (sync.EndDate.HasValue && sync.ServiceStartDate < timeToStartToday.AddDays(-7))))
                                        {
                                            ExecuteAsService(sync);
                                        }
                                    }
                                    break;
                                case EnumInterval.TwoWeeks:
                                    {
                                        if (DateTime.Now > timeToStartToday
                                            && (sync.ServiceStartDate == null || (sync.EndDate.HasValue && sync.ServiceStartDate < timeToStartToday.AddDays(-14))))
                                        {
                                            ExecuteAsService(sync);
                                        }
                                    }
                                    break;
                                case EnumInterval.ThreeWeeks:
                                    {
                                        if (DateTime.Now > timeToStartToday
                                            && (sync.ServiceStartDate == null || (sync.EndDate.HasValue && sync.ServiceStartDate < timeToStartToday.AddDays(-21))))
                                        {
                                            ExecuteAsService(sync);
                                        }
                                    }
                                    break;
                                case EnumInterval.FourWeeks:
                                    {
                                        if (DateTime.Now > timeToStartToday
                                            && (sync.ServiceStartDate == null || (sync.EndDate.HasValue && sync.ServiceStartDate < timeToStartToday.AddDays(-28))))
                                        {
                                            ExecuteAsService(sync);
                                        }
                                    }
                                    break;
                                case EnumInterval.FiveWeeks:
                                    {
                                        if (DateTime.Now > timeToStartToday
                                            && (sync.ServiceStartDate == null || (sync.EndDate.HasValue && sync.ServiceStartDate < timeToStartToday.AddDays(-35))))
                                        {
                                            ExecuteAsService(sync);
                                        }
                                    }
                                    break;
                                case EnumInterval.FirstOfMonth:
                                    {
                                        DateTime now = DateTime.Now;
                                        DateTime firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                                        TimeSpan timeOfDay = timeToStartToday.TimeOfDay;
                                        firstDayOfMonth = firstDayOfMonth.Date + timeOfDay;
                                        if (sync.ServiceStartDate.HasValue)
                                        {
                                            int startDateMonth = sync.ServiceStartDate.Value.Month;
                                            if (DateTime.Now > firstDayOfMonth && now.Month != startDateMonth)
                                            {
                                                ExecuteAsService(sync);
                                            }
                                        }
                                    }
                                  break;
                                default:
                                    Log.Info("unknown serviceInterval: " + sync);
                                    continue;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Write("cannot run: " + e.ToString());
                Log.Info("cannot run: " + e, e);
            }
        }

        public void ExecuteAsService(Data.SnowDbSyncMgnt.Synchronization sync)
        {
            Write("starting: " + sync);
            Log.Info("starting: " + sync);
            SyncSchedulerModel model = new SyncSchedulerModel();
            var synchronization = model.Find<Data.SnowDbSyncMgnt.Synchronization>(sync.Id);
            model.Init(synchronization);
            ExecuteAsService(model);
        }
        public void ExecuteAsService(SyncSchedulerModel sync)
        {
            ServiceSnowProcessorRunner = new SnowProcessorRunner()
            {
                SynchronizationId = sync.SynchronizationId.Value,
                Invocation = EnumInvocation.Service,
            };
            ServiceSnowProcessorRunner.RunAsync();
        }

        public static void Write(String msg, Exception e = null)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains("LogNative"))
            {
                try
                {
                    using (var file = new System.IO.StreamWriter(@"c:\Temp\MirrorService.trace", true))
                    {
                        file.Write(DateTime.Now.ToString() + ": " + msg + (e != null ? " : " + e.StackTrace : "") + "\n");
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// send service now table schema commands to mail recipients
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="synchronization"></param>
        public void SendSchemaChangeNotification(IReadOnlyList<MigrationCommand> cmds, Data.SnowDbSyncMgnt.Synchronization synchronization)
        {
            try
            {
                IReadOnlyList<MigrationCommand> schemaChanges = cmds.Where(x => !string.IsNullOrWhiteSpace(x.CommandText)).ToList();
                if (schemaChanges.Any())
                {
                    AppSettingsModel appSettingsModel = new AppSettingsModel().GetAppSettingsModel();
                    StringBuilder sb = new StringBuilder();
                    NotificationHandler notificationHandler = new NotificationHandler();
                    foreach (var command in schemaChanges)
                    {
                        sb.AppendLine($"<p style='color: #80B6A1; float:left; font-family: Helvetica Neue, Helvetica, Arial, sans-serif;'>{command.CommandText}</p>");
                    }

                    string subject = $"Service-Now DbSync Instanz-{synchronization.InstanzSettings.InstanzName} schema-changes";
                    string body = $"<div><h1 style='color: #80B6A1; float:left; font-family: Helvetica Neue, Helvetica, Arial, sans-serif;'>{subject}</h1><br /><div><span style='color: #293E40;font-family: Helvetica Neue, Helvetica, Arial, sans-serif;'>Hello, this is an automatic notification from ServiceNow DB Sync</span><br /><span style='color: #293E40;font-family: Helvetica Neue, Helvetica, Arial, sans-serif;'>Following commands describe the ServiceNow table schema changes. You can use those statements in your database to update schema.</span></div></div><br /><div><p style='color: #293E40;font-family: Helvetica Neue, Helvetica, Arial, sans-serif;'>{sb.ToString()}</p></div><br /><div><span style='color: #293E40; font-size: 12px; font-family: Helvetica Neue, Helvetica, Arial, sans-serif;'>email was triggered from SNOW DbSync</span></div></body>";

                    //get email recipients
                    SchemaChangeNotifySettings schemaChangeNotifySettings = appSettingsModel.SchemaChangeNotifySettings;
                    List<MailboxAddress> mailBoxAdresses = new List<MailboxAddress>();

                    if (schemaChangeNotifySettings.EmailRecipients.Any())
                    {
                        
                        foreach (var recipient in schemaChangeNotifySettings.EmailRecipients)
                        {
                            MailboxAddress mailboxAddress = new MailboxAddress(recipient.Name, recipient.EmailAddress);
                            mailBoxAdresses.Add(mailboxAddress);
                        }
                    }
                    else
                    {
                        MailboxAddress defaultRecipient = new MailboxAddress("Martin", "martin.humer@a1.at");
                        mailBoxAdresses.Add(defaultRecipient);
                    }
                    
                    notificationHandler.SendNotification(subject, body, mailBoxAdresses);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}, {ex.InnerException}");
            }
        }

        //Retry process for failed synchronization tables
        private void RetryFailedSynchronizations(Guid syncId)
        {
            AppSettingsModel appSettingsModel = new AppSettingsModel().GetAppSettingsModel();
            using (ServiceNowDbSyncMgntEntities snowEntities = new ServiceNowDbSyncMgntEntities())
            {
                var synchronization = snowEntities.Synchronization.FirstOrDefault(s => s.Id == syncId);
                
                if (synchronization == null) return;

                //load syncType
                var syncTypeObject = snowEntities.SyncType.FirstOrDefault(s => s.Id == synchronization.SyncTypeId);
                synchronization.SyncType = syncTypeObject;

                //load targetType
                var targetObject = snowEntities.SyncTarget.FirstOrDefault(s => s.Id == synchronization.SyncTargetId);
                synchronization.SyncTarget = targetObject;

                var syncType = snowEntities.SyncType.FirstOrDefault(t => t.Id == synchronization.SyncTypeId);

                if (syncType.TypeName.Equals("Delta") && !appSettingsModel.ProcessSettings.AutomaticRetryProcessDeltaSync) return;
                if (syncType.TypeName.Equals("Full") && !appSettingsModel.ProcessSettings.AutomaticRetryProcessFullSync) return;

                if (!string.IsNullOrWhiteSpace(synchronization.FinalMessage) && synchronization.FinalMessage.StartsWith(ENDED_PROCESSING))
                {
                    List<SyncProcess> failedTables = snowEntities.SyncProcess.Where(s => s.SynchronizationId == syncId && s.FinalErrorMessage != null).ToList();

                    string failedTablesJoined = string.Join(",", failedTables.Select(x => x.TableName));
                    
                    if (failedTables.Any())
                    {
                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name}: Retry failed processes is enabled. ");
                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name}: Start to retry process for sync: {synchronization.Name} and tables: {failedTablesJoined}");

                        List<SnowTables> retryTables = new List<SnowTables>();

                        foreach (var failedTable in failedTables)
                        {
                            SnowTables snowTable = new SnowTables
                            {
                                Name = failedTable.TableName
                            };

                            retryTables.Add(snowTable);
                        }

                        SnowProcessor processor = new SnowProcessor();
                        processor.RetrySyncProcess(syncId, retryTables, null);
                    }
                }
            }
        }
        
        public override string ToString()
        {
            return GetType().Name+"["+SynchronizationName+ "]: acc=" + SnowAccessSettings + ", dbSettings=" + SyncDatabaseSettings + ", dbCtx=" + DbContext;
        }
    }
}
