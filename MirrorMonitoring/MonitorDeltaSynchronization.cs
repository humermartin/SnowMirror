using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using MimeKit;
using Quartz;
using MirrorMonitoring.Resource;
using MirrorRepository;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Model;
using MirrorRepository.NotificationHelper;

namespace MirrorMonitoring
{
    public class MonitorDeltaSynchronization: IJob
    {
        /// <summary>
        /// Gets or sets the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        /// <summary>
        /// execute task scheduler
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. monitoring for delta synchronization triggered.");
            var appSettings = new AppSettingsModel().GetAppSettingsModel();
            if (appSettings.AlertNotifySettings.NotifyOnNotStartedSync)
            {
                await ProcessDeltaSynchronization(appSettings);
            }
        }

        /// <summary>
        /// check synchronizations running periods
        /// </summary>
        /// <returns></returns>
        public Task<string> ProcessDeltaSynchronization(AppSettingsModel appSettings)
        {
            try
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    NotificationHandler notificationHandler = new NotificationHandler();
                    
                    var activeSynchronizations = entities.Synchronization.Where(s => s.Enabled == true && s.SyncType.TypeName.Equals("Delta")).ToList();

                    foreach (var aSync in activeSynchronizations)
                    {
                        if (aSync.PeriodInterval != null)
                        {
                            var currentTimeMinusDoublePeriod =  DateTime.Now.AddMinutes(-((int) aSync.PeriodInterval * 2));
                            string requestedNextStart = String.Empty;
                            if (aSync.StartDate.HasValue)
                            {
                                requestedNextStart = aSync.StartDate.Value.AddMinutes((int)aSync.PeriodInterval).ToString("dddd, dd MMMM yyyy HH:mm:ss");
                            }
                            
                            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. {aSync.Name} - Last Synch started at: {aSync.StartDate:dddd, dd MMMM yyyy HH:mm:ss}.");
                            if (currentTimeMinusDoublePeriod > aSync.StartDate)
                            {
                                Log.Warn($"{MethodBase.GetCurrentMethod()?.Name}. {aSync.Name} - Last start date is longer than 2 period interval. Please check the sync.");
                                string subject = "ServiceNOW Database-Sync Monitoring Alert";
                                string htmlBody = MonitorResource.DeltaAlert;
                                htmlBody = htmlBody.Replace("{alertHeader}", "https://vmsnwwp110.isp.local/MirrorWeb/content/images/SnowDbSyncAlert.jpg");
                                htmlBody = htmlBody.Replace("{aSync.Name}", aSync.Name);
                                htmlBody = htmlBody.Replace("{aSync.PeriodInterval}", aSync.PeriodInterval.ToString());
                                htmlBody = htmlBody.Replace("{aSync.SyncType.TypeName}", aSync.SyncType.TypeName);
                                htmlBody = htmlBody.Replace("{aSync.InstanzSettings.InstanzName}", aSync.InstanzSettings.InstanzName);
                                htmlBody = htmlBody.Replace("{requestedNextStart}", requestedNextStart);
                                
                                List<MailboxAddress> mailBoxAddresses = new List<MailboxAddress>();
                                if (appSettings.AlertNotifySettings != null && appSettings.AlertNotifySettings.EmailRecipients.Any())
                                {
                                    foreach (var recipient in appSettings.AlertNotifySettings.EmailRecipients)
                                    {
                                        MailboxAddress mailboxAddress = new MailboxAddress(recipient.Name, recipient.EmailAddress);
                                        mailBoxAddresses.Add(mailboxAddress);
                                    }
                                }
                                else
                                {
                                    MailboxAddress defaultRecipient = new MailboxAddress("Martin", "martin.humer@a1.at");
                                    mailBoxAddresses.Add(defaultRecipient);
                                }
                                Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Send mail to {mailBoxAddresses.First()}.");
                                try
                                {
                                    notificationHandler.SendNotification(subject, htmlBody, mailBoxAddresses);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. {ex.Message}");
                                }
                                
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. {ex.Message}");
            }
            
            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. executed.");
            return Task.FromResult<string>($"{MethodBase.GetCurrentMethod()?.Name}");
        }
    }
}
