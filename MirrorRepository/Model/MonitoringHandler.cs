using Newtonsoft.Json;
using MirrorRepository.Data.SnowDbSyncMgnt;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using MimeKit;
using ServiceNowRecordNet;
using ServiceNowRecordNet.Enums;
using MirrorRepository.Constants;
using MirrorRepository.Enums;
using MirrorRepository.Model.InterfaceMonitoring;
using MirrorRepository.NotificationHelper;
using Timer = System.Timers.Timer;

namespace MirrorRepository.Model
{
    public class MonitoringHandler
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public void UpdateMonitoringChanges(MonitoringSettings monitoringModel)
        {
            try
            {
                if (monitoringModel != null)
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        var monitoring = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.Monitoring);

                        if (monitoring != null)
                        {
                            MonitoringSettings monitoringSettings = JsonConvert.DeserializeObject<MonitoringSettings>(monitoring?.Value);

                            monitoringSettings.InterfaceMonitoring = monitoringModel.InterfaceMonitoring;
                            monitoringSettings.MonitoringLevel = monitoringModel.MonitoringLevel;
                            
                            var serializedMonitoring = JsonConvert.SerializeObject(monitoringSettings);

                            monitoring.Value = serializedMonitoring;

                            ctx.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}{ex.InnerException}");
            }
        }

        /// <summary>
        /// create Service-Now interface monitoring record
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="recordsFound"></param>
        /// <param name="monitoringLevel"></param>
        /// <param name="syncName"></param>
        /// <returns></returns>
        public InterfaceMonitoringResponse CreateInterfaceMonitoringRecord(string tableName, int? recordsFound, string monitoringLevel, string syncName)
        {
            InterfaceMonitoring.InterfaceMonitoring interfaceMonitoring = new InterfaceMonitoring.InterfaceMonitoring
            {
                Interface = "SnowDbSync",
                Direction = "Inbound",
                MessageType = "REST",
                InboundTimestamp = DateTime.Now.ToString()
            };

            if (!string.IsNullOrEmpty(monitoringLevel))
            {
                if (monitoringLevel.Equals(SnowDbSyncConstants.LevelPackage))
                {
                    interfaceMonitoring.SourceTable = null;
                    interfaceMonitoring.TargetTable = null;
                    interfaceMonitoring.Comment = $"{syncName} - processing tables: {tableName}.";

                }
                else if (monitoringLevel.Equals(SnowDbSyncConstants.LevelSingleTable))
                {
                    interfaceMonitoring.SourceTable = tableName;
                    interfaceMonitoring.TargetTable = tableName;
                    interfaceMonitoring.Comment = $"{syncName} - processing table: {tableName}. get {recordsFound} records.";
                }
            }

            string monitoringInstance = ConfigurationManager.AppSettings["MonitoringInstance"];
            Enum.TryParse(monitoringInstance, out SnowInstanceEnum enumInstance);

            ServiceNowRecord addNowRecord = new ServiceNowRecord();
            var result = addNowRecord.Add<InterfaceMonitoringResponse, InterfaceMonitoring.InterfaceMonitoring>(interfaceMonitoring, enumInstance, "u_interface_monitoring");
            return result?.SnowTable;
        }

        /// <summary>
        /// update Service-Now interface monitoring record
        /// </summary>
        /// <param name="monitoringResponse"></param>
        /// <returns></returns>
        public void UpdateInterfaceMonitoringRecord(InterfaceMonitoringResponse monitoringResponse)
        {
            if (monitoringResponse != null && !string.IsNullOrWhiteSpace(monitoringResponse.SysId))
            {
                InterfaceMonitoring.InterfaceMonitoring interfaceMonitoring = new InterfaceMonitoring.InterfaceMonitoring
                {
                    InboundTimestamp = monitoringResponse.InboundTimestamp,
                    OutboundTimestamp = DateTime.Now.ToString()
                };

                string monitoringInstance = ConfigurationManager.AppSettings["MonitoringInstance"];
                Enum.TryParse(monitoringInstance, out SnowInstanceEnum enumInstance);

                ServiceNowRecord updateNowRecord = new ServiceNowRecord();
                var result = updateNowRecord.Update<InterfaceMonitoringResponse, InterfaceMonitoring.InterfaceMonitoring>(interfaceMonitoring, enumInstance, "u_interface_monitoring", monitoringResponse.SysId);
                if (result?.SnowTable != null)
                {
                    Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Interface Monitoring record updated. SysId: {monitoringResponse.SysId}");
                }
                else
                {
                    Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Could not update table u_interface_monitoring with SysId: {monitoringResponse.SysId}.");
                }
            }
            else
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Could not update table u_interface_monitoring because of possible not created object.");
            }
        }
    }
}
