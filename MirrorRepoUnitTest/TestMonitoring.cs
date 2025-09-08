using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using MirrorMonitoring;
using MirrorRepository.Model;
using MirrorRepository.Model.SyncParams;
using MirrorRepository.NotificationHelper;

namespace MirrorRepoUnitTest
{
    [TestClass]
    public class TestMonitoring
    {
        [TestMethod]
        public void TestMonitoringEmail()
        {
            MonitorDeltaSynchronization jobSync = new MonitorDeltaSynchronization();
            var appSettings = new AppSettingsModel().GetAppSettingsModel();
            var result = jobSync.ProcessDeltaSynchronization(appSettings);
        }

        [TestMethod]
        public void EmailrecipientTest()
        {
            List<EmailRecipient> recipients = new List<EmailRecipient>();

            EmailRecipient recipient1 = new EmailRecipient();
            recipient1.Name = "Martin Humer";
            recipient1.EmailAddress = "martin.humer@a1.at";

            recipients.Add(recipient1);

            EmailRecipient recipient2 = new EmailRecipient();
            recipient2.Name = "Martin Privat";
            recipient2.EmailAddress = "humermartin@gmail.com";

            recipients.Add(recipient2);

            AlertNotifySettings alertNotifySettings = new AlertNotifySettings();
            alertNotifySettings.EmailRecipients = new List<EmailRecipient>();
            alertNotifySettings.EmailRecipients= recipients;

            alertNotifySettings.DeltaSyncIntervalInMinutes = 30;
            alertNotifySettings.FailedSyncIntervalInMinutes = 5;
            alertNotifySettings.SynchronizationAlertNotify = true;

            var recipientsToJson = JsonConvert.SerializeObject(alertNotifySettings);

            var result = recipientsToJson;

        }

        [TestMethod]
        public void TestTableParamJson()
        {
            TableParam tblParam = new TableParam();
            var result = tblParam.Init();
            var end = result;
        }
    }
}
