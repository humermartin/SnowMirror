using log4net;
using MirrorRepository;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorService
{
    public partial class MirrorService : ServiceBase
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        Task ProcessorTask;
        bool Stopped = true;
        public MirrorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Write("OnStart:");
            try
            {
                Log.Info("Starting... ");
                Stopped = false;
                
                ProcessorTask = Task.Factory.StartNew(RunPeriodically);
            } catch (Exception e)
            {
                Write(e.ToString());
            }
        }

        protected override void OnStop()
        {
            Stopped = true;
            Log.Info("Stopping... ");
        }

        protected void RunPeriodically()
        {
            Write("RunPeriodically:");
            var Runner = new SnowProcessorRunner();
            while (!Stopped)
            {
                try
                {
                    Runner.RunAsService(this.ServiceName);
                }
                catch (Exception e)
                {
                    Write("cannot run: " + e.ToString());
                    Log.Info("cannot run: " + e, e);
                }

                if (!Stopped)
                {
                    Thread.Sleep(15 * 1000);
                }
            }
            Write("RunPeriodically: END");
        }

        /// <summary>
        /// silly copy of SnowProcessorRunner.Write :-/ (TFR)
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        public void Write(String msg, Exception e = null)
        {
            Log.Info(msg, e);
            if (ConfigurationManager.AppSettings.AllKeys.Contains("LogNative"))
            {
                try
                {
                    using (var file = new System.IO.StreamWriter(@"c:\Temp\MirrorService.trace", true))
                    {
                        file.Write(DateTime.Now.ToString() + ": " + msg + (e != null ? " : " + e.StackTrace : "") + "\n");
                    }
                }
                catch { }
            }
        }
    }
}
