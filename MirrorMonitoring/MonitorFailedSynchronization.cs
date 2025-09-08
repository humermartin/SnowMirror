using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
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
    public class MonitorFailedSynchronization: IJob
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
            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. monitoring for failed synchronization triggered");
            var appSettings = new AppSettingsModel().GetAppSettingsModel();
            if (appSettings.AlertNotifySettings.NotifyOnFailedSync)
            {
                await ProcessFailedSynchronization(appSettings);
            }
            
        }

        /// <summary>
        /// check failed synchronizations
        /// </summary>
        /// <returns></returns>
        public Task<string> ProcessFailedSynchronization(AppSettingsModel appSettings)
        {
            try
            {
                using (ServiceNowDbSyncMgntEntities entities = new ServiceNowDbSyncMgntEntities())
                {
                    NotificationHandler notificationHandler = new NotificationHandler();
                    
                    var failedSynchronizations = entities.SyncProcess.Where(s => s.FinalErrorMessage != null).ToList();

                    foreach (var failedSync in failedSynchronizations)
                    {
                        var aSync = entities.Synchronization.FirstOrDefault(s => s.Id == failedSync.SynchronizationId);

                        Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. {failedSync.TableName} failed. FinalErrorMessage: {failedSync.FinalErrorMessage}. Synchronization: {aSync?.Name}");
                           
                        string subject = "ServiceNOW Database-Sync Monitoring Alert";
                        string htmlBody = MonitorResource.FailedTableAlert;
                        htmlBody = htmlBody.Replace("{alertHeader}", "https://vmsnwwp110.isp.local/MirrorWeb/content/images/SnowDbSyncAlert.jpg");
                        htmlBody = htmlBody.Replace("{aSync.TableName}", failedSync.TableName);
                        htmlBody = htmlBody.Replace("{aSync.Name}", aSync?.Name);
                        htmlBody = htmlBody.Replace("{aSync.SyncType.TypeName}", aSync?.SyncType.TypeName);
                        htmlBody = htmlBody.Replace("{aSync.InstanzSettings.InstanzName}", aSync?.InstanzSettings.InstanzName);
                        htmlBody = htmlBody.Replace("{aSync.StartTime}", $"{failedSync.StartTime:dddd, dd MMMM yyyy HH:mm:ss}");
                        
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
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. {ex.Message}");
            }
            
            Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. executed.");
            return Task.FromResult<string>($"{MethodBase.GetCurrentMethod()?.Name}");
        }
    }
}
