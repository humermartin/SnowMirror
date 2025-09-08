using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using MirrorRepository.Constants;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorRepository.Model
{
    public class AlertNotifySettings
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        /// <summary>
        /// Gets or sets the synchronization alert notify flag
        /// </summary>
        [JsonProperty("SynchronizationAlertNotify")]
        public bool SynchronizationAlertNotify { get; set; }

        /// <summary>
        /// Gets or sets the synchronization delta interval
        /// </summary>
        [JsonProperty("DeltaSyncIntervalInMinutes")]
        public int DeltaSyncIntervalInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the synchronization failed interval
        /// </summary>
        [JsonProperty("FailedSyncIntervalInMinutes")]
        public int FailedSyncIntervalInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the list of alert notify recipients
        /// </summary>
        [JsonProperty("EmailRecipients")]
        public List<EmailRecipient> EmailRecipients { get; set; }

        /// <summary>
        /// Gets or sets the EmailRecipientsTotalCount
        /// </summary>
        [JsonProperty("EmailRecipientsTotalCount")]
        public int EmailRecipientsTotalCount { get; set; }

        /// <summary>
        /// Gets or sets the NotifyOnFailedSync
        /// </summary>
        [JsonProperty("NotifyOnFailedSync")]
        public bool NotifyOnFailedSync{ get; set; }

        /// <summary>
        /// Gets or sets the NotifyOnNotStartedSync
        /// </summary>
        [JsonProperty("NotifyOnNotStartedSync")]
        public bool NotifyOnNotStartedSync { get; set; }
        

        /// <summary>
        /// update alert notification settings values
        /// </summary>
        /// <param name="alertNotifySettingsModel"></param>
        public void AddOrUpdateAlertNotifyChanges(AlertNotifySettings alertNotifySettingsModel)
        {
            try
            {
                if (alertNotifySettingsModel != null)
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        var alertNotify = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.AlertNotifySettings);

                        if (alertNotify != null)
                        {
                            AlertNotifySettings alertNotifySettings = JsonConvert.DeserializeObject<AlertNotifySettings>(alertNotify?.Value);

                            if (alertNotifySettings != null)
                            {
                                alertNotifySettings.SynchronizationAlertNotify = alertNotifySettingsModel.SynchronizationAlertNotify;
                                alertNotifySettings.NotifyOnFailedSync = alertNotifySettingsModel.NotifyOnFailedSync;
                                alertNotifySettings.NotifyOnNotStartedSync = alertNotifySettingsModel.NotifyOnNotStartedSync;
                                alertNotifySettings.DeltaSyncIntervalInMinutes = alertNotifySettingsModel.DeltaSyncIntervalInMinutes;
                                alertNotifySettings.FailedSyncIntervalInMinutes = alertNotifySettingsModel.FailedSyncIntervalInMinutes;

                                var serializedAlertNotify = JsonConvert.SerializeObject(alertNotifySettings);

                                alertNotify.Value = serializedAlertNotify;

                                ctx.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}: {ex.Message}{ex.InnerException}");
            }
        }
    }
}
