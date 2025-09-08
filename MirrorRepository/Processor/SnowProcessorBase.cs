using log4net;
using MirrorRepository.Base;
using MirrorRepository.Enums;
using MirrorRepository.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MirrorRepository.Data.SnowDbSyncMgnt;
using Azure;
using System.IdentityModel.Metadata;
using System.Web.UI.WebControls;

namespace MirrorRepository.Processor
{
    public enum SyncProcessType { Full, Delta, Consistency }
    public abstract class SnowProcessorBase
    {

        private ILog _log = null; // log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        protected ILog Log { get { 
                if (_log == null)
                {
                   _log = LogManager.GetLogger(this.GetType());
                }
                return _log; 
        } }

        public TaskFactory SyncTaskFactory { get; protected set; } = new TaskFactory(TaskScheduler.Default);
        public SyncProcessType SyncType { get; set; } = SyncProcessType.Consistency;
        public string SyncName { get; set; } = "SnowSync";
        public InstanzSettings SnowAccessSettings { get; set; }
        public SyncTarget EsbAccessSettings { get; set; }
        public SyncSchedulerModel SyncScheduler { get; set; }
        public SnowDbContext Context { get; set; }
        public string TableNamePrefix { get; protected set; } = "";
        public SnowMigration Migration { get; protected set; } = new SnowMigration();
        public Dictionary<SnowDictEntry, List<SnowDictEntry>> SnowDictionary { get; protected set; }
        public Dictionary<string, TaskEntry> SyncTasks { get; private set; } = new Dictionary<string, TaskEntry>();
        public DateTime Start { get; private set; } = DateTime.Now;
        public List<LogMessage> LogMessages { get; private set; } = new List<LogMessage>();
        public EnumInvocation Invocation { get; set; }
        public bool ForceSync { get; set; }

        public abstract void Process();
        public abstract void Migrate(SnowTables table, string key);

        protected void LogError(string message, Exception e, string key, SnowTables table)
        {
            LogMessages.Add(new LogMessage { Key = key, Table = table.Name, Message = message });
            Log.Info(key + ": " + message + " : " + table + " : " + e, e);
            Log.Warn(key + ": " + message + " : " + table + " : " + e);
        }

    }
}
