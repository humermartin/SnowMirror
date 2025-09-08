using System;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using MirrorRepository.Constants;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorRepository.Model
{
    public class ProcessSettings
    {

        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        /// <summary>
        /// Member which holds the automatic retry process for full syncs
        /// </summary>
        public bool AutomaticRetryProcessFullSync { get; set; }

        /// <summary>
        /// Member which holds the automatic retry process for delta syncs
        /// </summary>
        public bool AutomaticRetryProcessDeltaSync { get; set; }

        public void UpdateProcessChanges(ProcessSettings processModel)
        {
            try
            {
                if (processModel != null)
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        var processSettings = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.ProcessSettings);

                        if (processSettings != null)
                        {
                            ProcessSettings processSettingsModel = JsonConvert.DeserializeObject<ProcessSettings>(processSettings?.Value);

                            if (processSettingsModel != null)
                            {
                                processSettingsModel.AutomaticRetryProcessFullSync = processModel.AutomaticRetryProcessFullSync;
                                processSettingsModel.AutomaticRetryProcessDeltaSync = processModel.AutomaticRetryProcessDeltaSync;
                            }
                            
                            var serializedProcessSettings = JsonConvert.SerializeObject(processSettingsModel);

                            processSettings.Value = serializedProcessSettings;

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
    }
}
