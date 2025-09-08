using System;
using System.Linq;
using System.Reflection;
using log4net;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorRepository.Model.SnowDbSyncMgnt
{
    public class TableMonitoringModel
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        /// <summary>
        /// creates a table monitoring entry identified by invocation flag => service or manual
        /// </summary>
        /// <param name="syncProcess"></param>
        public void AddTableMonitoringRecord(Data.SnowDbSyncMgnt.SyncProcess syncProcess)
        {
            try
            {
                using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                {
                    var synchronization =  ctx.Synchronization.FirstOrDefault(s => s.Id == syncProcess.SynchronizationId);

                    TableMonitoring tableMonitoring = new TableMonitoring();
                    tableMonitoring.Id = Guid.NewGuid();
                    tableMonitoring.TableName = syncProcess.TableName;
                    tableMonitoring.GetDeltaRecordsFrom = syncProcess.GetDeltaRecordsFrom;
                    tableMonitoring.SyncId = syncProcess.SynchronizationId;
                    tableMonitoring.SyncTypeId = synchronization?.SyncTypeId;
                    tableMonitoring.InstanzSettingsId = synchronization?.InstanzSettingsId;
                    tableMonitoring.DatabaseSettingsId = synchronization?.DatabaseSettingsId;
                    tableMonitoring.StartTime = syncProcess.StartTime;
                    tableMonitoring.EndTime = syncProcess.EndTime;
                    if (syncProcess.StartTime != null && syncProcess.EndTime != null)
                    {
                        TimeSpan diff = syncProcess.EndTime.Value - syncProcess.StartTime.Value;
                        tableMonitoring.Duration = $"{diff.Hours:00}:{diff.Minutes:00}:{diff.Seconds:00}";
                    }

                    tableMonitoring.ThreadsPerTable = syncProcess.ThreadsPerTable ?? synchronization?.ThreadsPerTable;
                    tableMonitoring.ThreadSleepTime = syncProcess.ThreadSleepTime ?? synchronization?.ThreadSleepTime;
                    tableMonitoring.PageSize = syncProcess.PageSize ?? synchronization?.PageSize;
                    tableMonitoring.RequestTimeout = syncProcess.RequestTimeout ?? synchronization?.RequestTimeout;
                    tableMonitoring.Messages = syncProcess.Messages;
                    tableMonitoring.FinalMessage = syncProcess.FinalMessage;
                    tableMonitoring.FinalErrorMessage = syncProcess.FinalErrorMessage;
                    tableMonitoring.Created = DateTime.Now;
                    
                    ctx.TableMonitoring.Add(tableMonitoring);
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {e.Message}");
            }
            
        }
    }
}
