using System.Reflection;
using log4net;
using Quartz;
using Quartz.Impl;
using MirrorRepository.Model;

namespace MirrorMonitoring
{
    public class QuartzScheduler
    {
        /// <summary>
        /// Gets or sets the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public async void Start()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            var appSettings = new AppSettingsModel().GetAppSettingsModel();

            //is synchronization monitoring enabled
            if (appSettings.AlertNotifySettings?.SynchronizationAlertNotify == true)
            {
                Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Scheduler is enabled.");
                
                int intervalDeltaMonitoring = 1;
                int intervalFailedMonitoring = 1;

                if (appSettings.AlertNotifySettings.DeltaSyncIntervalInMinutes > 0)
                {
                    intervalDeltaMonitoring = appSettings.AlertNotifySettings.DeltaSyncIntervalInMinutes;
                    Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. monitoring for delta synchronization is on. interval in minutes: {intervalDeltaMonitoring}.");
                }

                if (appSettings.AlertNotifySettings.FailedSyncIntervalInMinutes > 0)
                {
                    intervalFailedMonitoring = appSettings.AlertNotifySettings.FailedSyncIntervalInMinutes;
                    Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. monitoring for failed synchronization is on. interval in minutes: {intervalFailedMonitoring}.");
                }

                

                //Job - Monitor for Delta Synchronization
                IJobDetail monitorDeltaSynchronization = JobBuilder.Create<MonitorDeltaSynchronization>().Build();

                // Trigger the job to run now, and then repeat every x minutes = default is 1
                ITrigger triggerDeltaSynchronization = TriggerBuilder.Create()
                    .WithIdentity("triggerDeltaSynchronization", "groupLifeCycle")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(intervalDeltaMonitoring)
                        .RepeatForever())
                    .Build();

                //Job - Monitor for Failed Synchronization
                IJobDetail monitorFailedSynchronization = JobBuilder.Create<MonitorFailedSynchronization>().Build();

                // Trigger the job to run now, and then repeat every x minutes = default is 1
                ITrigger triggerFailedSynchronization = TriggerBuilder.Create()
                    .WithIdentity("triggerFailedSynchronization", "groupLifeCycle")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(intervalFailedMonitoring)
                        .RepeatForever())
                    .Build();

                await scheduler.ScheduleJob(monitorDeltaSynchronization, triggerDeltaSynchronization);
                await scheduler.ScheduleJob(monitorFailedSynchronization, triggerFailedSynchronization);
                await scheduler.Start();
            }
            else
            {
                Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Scheduler is disabled. Synchronization monitoring is off.");
            }
        }
    }
}
