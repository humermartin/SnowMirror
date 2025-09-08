using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorRepository.Model
{
    public class ServiceSettingsModel
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public Guid Id { get; set; }

        public string ServiceName { get; set; }
        
        public List<Data.SnowDbSyncMgnt.Synchronization> ServiceSpecificSynchronizations { get; set; }

        /// <summary>
        /// Get ServiceSettings
        /// </summary>
        /// <returns></returns>
        public ServiceSettingsModel GetServiceByName(string serviceName)
        {
            using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
            {

                ServiceSettingsModel model = new ServiceSettingsModel();

                var svcSetting = ctx.ServiceSettings.FirstOrDefault(s => s.ServiceName == serviceName);

                if (svcSetting != null)
                {
                    model.ServiceName = svcSetting.ServiceName;
                    model.Id = svcSetting.Id;

                    switch (svcSetting.SyncMode)
                    {
                        case "Sql":
                            var sqlTarget = ctx.SyncTarget.Where(c => c.TargetType.Equals("Sql")).ToList();
                            model.ServiceSpecificSynchronizations = ctx.Synchronization.Where(s => sqlTarget.Contains(s.SyncTarget)).ToList();
                            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Service SyncMode is SQL: Scheduled Synchronizations: {String.Join(",", model.ServiceSpecificSynchronizations.Select(s => s.Name))}");
                            break;

                        case "Kafka":
                            var kafkaTarget = ctx.SyncTarget.Where(c => c.TargetType.Equals("Kafka")).ToList();
                            model.ServiceSpecificSynchronizations = ctx.Synchronization.Where(s => kafkaTarget.Contains(s.SyncTarget)).ToList();
                            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Service SyncMode is Kafka: Scheduled Synchronizations: {String.Join(",", model.ServiceSpecificSynchronizations.Select(s => s.Name))}");
                            break;
                    }
                }

                return model;
            }
        }

    }
}
