using System;
using System.Reflection;
using System.ServiceProcess;
using log4net;

namespace MirrorRepository.WindowsServiceController
{
    public class SnowDbSyncServiceController
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private string serviceName;

        public SnowDbSyncServiceController(string serviceName)
        {
            this.serviceName = serviceName;
        }

        // this method will throw an exception if the service is NOT in Running status.
        public void RestartService()
        {
            using (ServiceController service = new ServiceController(serviceName))
            {
                try
                {
                    Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Try to stop Windows Service [{serviceName}].");
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                    Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Windows Service [{serviceName}] stopped.");

                    Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Try to start Windows Service [{serviceName}].");
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                    Log.Info($"{MethodBase.GetCurrentMethod()?.Name}. Windows Service [{serviceName}] started.");
                }
                catch (Exception ex)
                {
                    Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: Can not restart Windows Service [{serviceName}]. {ex.Message}, {ex.InnerException}");
                    throw new Exception($"Can not restart the Windows Service {serviceName}", ex);
                }
            }
        }

        // this method will throw an exception if the service is NOT in Running status.
        public void StopService()
        {
            using (ServiceController service = new ServiceController(serviceName))
            {
                try
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                catch (Exception ex)
                {
                    Log.Error($"{MethodBase.GetCurrentMethod()?.Name}. Error: Can not stop Windows Service [{serviceName}]. {ex.Message}, {ex.InnerException}");
                    throw new Exception($"Can not Stop the Windows Service [{serviceName}]", ex);
                }
            }
        }

        // this method will throw an exception if the service is NOT in Stopped status.
        public void StartService()
        {
            using (ServiceController service = new ServiceController(serviceName))
            {
                try
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Can not Start the Windows Service [{serviceName}]", ex);
                }
            }
        }

        // if service running then restart the service if the service is stopped then start it.
        // this method will not throw an exception.
        public void StartOrRestart()
        {
            if (IsRunningStatus)
                RestartService();
            else if (IsStoppedStatus)
                StartService();
        }

        // stop the service if it is running. if it is already stopped then do nothing.
        // this method will not throw an exception if the service is in Stopped status.
        public void StopServiceIfRunning()
        {
            using (ServiceController service = new ServiceController(serviceName))
            {
                try
                {
                    if (!IsRunningStatus)
                        return;

                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Can not Stop the Windows Service [{serviceName}]", ex);
                }
            }
        }

        public bool IsRunningStatus => Status == ServiceControllerStatus.Running;

        public bool IsStoppedStatus => Status == ServiceControllerStatus.Stopped;

        public ServiceControllerStatus Status
        {
            get
            {
                using (ServiceController service = new ServiceController(serviceName))
                {
                    return service.Status;
                }
            }
        }
    }
}
