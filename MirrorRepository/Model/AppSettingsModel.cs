using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using MirrorRepository.Constants;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Model.SnowDbSyncMgnt;
using MirrorRepository.NotificationHelper;

namespace MirrorRepository.Model
{
    public class AppSettingsModel
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        /// <summary>
        /// Gets or sets the notification settings
        /// </summary>
        public NotificationSettings NotificationSettings { get; set; } = new NotificationSettings();

        /// <summary>
        /// Gets or sets the interface monitoring settings
        /// </summary>
        public MonitoringSettings MonitoringSettings { get; set; } = new MonitoringSettings();

        /// <summary>
        /// Gets or sets the sql session settings
        /// </summary>
        public SqlSessionSettings SqlSessionSettings { get; set; } = new SqlSessionSettings();

        /// <summary>
        /// Gets or sets the script settings
        /// </summary>
        public ScriptSettings ScriptSettings { get; set; } = new ScriptSettings();

        /// <summary>
        /// Gets or sets the inheritance settings
        /// </summary>
        public List<SnowTableParent> InheritanceSettings { get; set; } = new List<SnowTableParent> { };

        /// <summary>
        /// Gets or sets the notify recipients settings
        /// </summary>
        public AlertNotifySettings AlertNotifySettings { get; set; } = new AlertNotifySettings();

        /// <summary>
        /// Gets or sets the schema change notify recipients settings
        /// </summary>
        public SchemaChangeNotifySettings SchemaChangeNotifySettings { get; set; } = new SchemaChangeNotifySettings();

        /// <summary>
        /// Gets or sets the process settings
        /// </summary>
        public ProcessSettings ProcessSettings { get; set; } = new ProcessSettings();

        /// <summary>
        /// Gets or sets the grid settings
        /// </summary>
        public GridSettings GridSettings { get; set; } = new GridSettings();

        /// <summary>
        /// Get appsettings
        /// </summary>
        /// <returns></returns>
        public AppSettingsModel GetAppSettingsModel()
        {
            using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
            {
                var appSettings = ctx.AppSettings.ToList();
                if (appSettings.Any())
                {
                    //Notification
                    var notificationSettings = appSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.Notification);
                    if (notificationSettings != null)
                    {
                        NotificationSettings = JsonConvert.DeserializeObject<NotificationSettings>(notificationSettings.Value);
                        NotificationSettings.SmtpPassword = BaseModel.Decryptdata(NotificationSettings.SmtpPassword);
                    }

                    //Interface Monitoring
                    var interfaceMonitoring = appSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.Monitoring);
                    if (interfaceMonitoring?.Value != null)
                    {
                        MonitoringSettings = JsonConvert.DeserializeObject<MonitoringSettings>(interfaceMonitoring.Value);
                    }

                    //Sql Session settings
                    var sqlSessionSettings = appSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.SqlSessionSettings);
                    if (sqlSessionSettings?.Value != null)
                    {
                        SqlSessionSettings = JsonConvert.DeserializeObject<SqlSessionSettings>(sqlSessionSettings.Value);
                    }

                    //ScriptSettings
                    var scriptSettings = appSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.ScriptSettings);
                    if (scriptSettings?.Value != null)
                    {
                        ScriptSettings = JsonConvert.DeserializeObject<ScriptSettings>(scriptSettings.Value);
                    }

                    //InheritanceSettings
                    var inheritanceSettings = appSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.InheritanceSettings);
                    if (inheritanceSettings?.Value != null)
                    {
                        InheritanceSettings = JsonConvert.DeserializeObject<List<SnowTableParent>>(inheritanceSettings.Value);
                    }

                    //NotifyRecipients
                    var notifyRecipients = appSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.AlertNotifySettings);
                    if (notifyRecipients?.Value != null)
                    {
                        AlertNotifySettings = JsonConvert.DeserializeObject<AlertNotifySettings>(notifyRecipients.Value);
                    }

                    //SchemaChangeNotifyRecipients
                    var schemaChangeNotify = appSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.TableSchemaChangeNotify);
                    if (schemaChangeNotify?.Value != null)
                    {
                        SchemaChangeNotifySettings = JsonConvert.DeserializeObject<SchemaChangeNotifySettings>(schemaChangeNotify.Value);
                    }

                    //ProcessSettings
                    var processSettings = appSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.ProcessSettings);
                    if (processSettings?.Value != null)
                    {
                        ProcessSettings = JsonConvert.DeserializeObject<ProcessSettings>(processSettings.Value);
                    }

                    //GridSettings
                    var gridSettings = appSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.GridSettings);
                    if (gridSettings?.Value != null)
                    {
                        GridSettings = JsonConvert.DeserializeObject<GridSettings>(gridSettings.Value);
                    }
                }
            }

            return this;
        }
    }
}
