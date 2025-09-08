using log4net;
using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Model.SnowDbSyncMgnt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MailKit.Security;
using MirrorRepository.Constants;

namespace MirrorRepository.NotificationHelper
{
    public class NotificationHandler
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        /// <summary>
        /// Member which holds the notification settings
        /// </summary>
        private NotificationSettings NotificationSettings { get; set; }

        /// <summary>
        /// constructor initialize notification settings
        /// </summary>
        public NotificationHandler()
        {
            using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
            {
                var notification = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.Notification);

                NotificationSettings = JsonConvert.DeserializeObject<NotificationSettings>(notification?.Value);
                NotificationSettings.SmtpPassword = BaseModel.Decryptdata(NotificationSettings.SmtpPassword);
            }
        }

        /// <summary>
        /// send mail from asset-interface
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="mailboxAddresses"></param>
        public void SendNotification(string subject, string body, List<MailboxAddress> mailboxAddresses)
        {
            try
            {
                if (NotificationSettings.MailSendEnabled)
                {
                    MimeMessage message = new MimeMessage();

                    MailboxAddress from = new MailboxAddress(NotificationSettings.MailBoxUserName, NotificationSettings.MailBoxSender);
                    message.From.Add(from);
                    
                    message.To.AddRange(mailboxAddresses);
                    
                    message.Subject = subject;

                    BodyBuilder bodyBuilder = new BodyBuilder
                    {
                        HtmlBody = body

                    };

                    message.Body = bodyBuilder.ToMessageBody();

                    SmtpClient client = new SmtpClient();
                    client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };
                    client.Connect(NotificationSettings.MailServer, NotificationSettings.MailServerPort, SecureSocketOptions.StartTls);
                    client.Authenticate(NotificationSettings.SmtpUserName, NotificationSettings.SmtpPassword);

                    client.Send(message);
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: {ex.Message}, {ex.InnerException}");
                throw;
            }
            
        }

        /// <summary>
        /// Update Notification changes
        /// </summary>
        /// <param name="notificationModel"></param>
        public void UpdateNotificationChanges(NotificationSettings notificationModel)
        {
            try
            {
                if (notificationModel != null)
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        var notification = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.Notification);

                        if (notification != null)
                        {
                            NotificationSettings notificationSettings = JsonConvert.DeserializeObject<NotificationSettings>(notification?.Value);

                            notificationSettings.MailSendEnabled = notificationModel.MailSendEnabled;
                            notificationSettings.MailServer = notificationModel.MailServer;
                            notificationSettings.MailServerPort = notificationModel.MailServerPort;
                            notificationSettings.UseSsl = notificationModel.UseSsl;
                            notificationSettings.MailBoxUserName = notificationModel.MailBoxUserName;
                            notificationSettings.MailBoxSender = notificationModel.MailBoxSender;
                            notificationSettings.SmtpUserName = notificationModel.SmtpUserName;
                            notificationSettings.SmtpPassword = BaseModel.Encryptdata(notificationModel.SmtpPassword);

                            var serializedNotification = JsonConvert.SerializeObject(notificationSettings);

                            notification.Value = serializedNotification;

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
        /// check if there is already a history entry found
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="synchronizationId"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public bool NotificationHistoryExist(Guid instanceId, Guid synchronizationId, string messageId)
        {
            using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
            {
                var notification = ctx.NotificationHistory.FirstOrDefault(n => n.InstanceId == instanceId && n.SynchronizationId == synchronizationId && n.MessageId == messageId);

                if (notification != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// add notification record to history
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="synchronizationId"></param>
        /// <param name="messageId"></param>
        /// <param name="message"></param>
        public void AddToHistory(Guid instanceId, Guid synchronizationId, string messageId, string message)
        {
            using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
            {
                var notification = ctx.NotificationHistory.FirstOrDefault(n => n.InstanceId == instanceId &&  n.SynchronizationId == synchronizationId && n.MessageId == messageId);

                if (notification == null)
                {
                    NotificationHistory notificationHistory = new NotificationHistory();
                    notificationHistory.Id = Guid.NewGuid();
                    notificationHistory.InstanceId = instanceId;
                    notificationHistory.SynchronizationId = synchronizationId;
                    notificationHistory.MessageId = messageId;
                    notificationHistory.Message = message;
                    notificationHistory.CreateTime = DateTime.Now;

                    ctx.NotificationHistory.Add(notificationHistory);
                    ctx.SaveChanges();

                    Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Add new entity to history with instanceId:{instanceId}, messageId:{messageId}.");
                }
                else
                {
                    Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Cannot add entity to history. There is already an entity found with instanceId:{instanceId}, messageId:{messageId}.");
                }
            }
        }
    }
}
