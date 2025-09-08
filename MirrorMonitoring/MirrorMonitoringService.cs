using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorMonitoring
{
    public partial class MirrorMonitoringService : ServiceBase
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public MirrorMonitoringService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log.Info("Service started.");
            QuartzScheduler qs = new QuartzScheduler();
            qs.Start();
        }

        protected override void OnStop()
        {
            Log.Info("Stopping... ");
        }
    }
}
