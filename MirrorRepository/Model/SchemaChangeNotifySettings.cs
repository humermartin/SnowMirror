using log4net;
using Newtonsoft.Json;
using MirrorRepository.Constants;
using MirrorRepository.Data.SnowDbSyncMgnt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MirrorRepository.Model
{
    public class SchemaChangeNotifySettings
    {
        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        
        /// <summary>
        /// Gets or sets the list of alert notify recipients
        /// </summary>
        [JsonProperty("EmailRecipients")]
        public List<EmailRecipient> EmailRecipients { get; set; }

        /// <summary>
        /// Gets or sets the email recipients count
        /// </summary>
        [JsonProperty("EmailRecipientsTotalCount")]
        public int EmailRecipientsTotalCount { get; set; }

        /// <summary>
        /// update table schema change settings values
        /// </summary>
        /// <param name="schemaChangeNotifyModel"></param>
        public void AddOrUpdateSchemaChangeNotifyChanges(SchemaChangeNotifySettings schemaChangeNotifyModel)
        {
            try
            {
                if (schemaChangeNotifyModel != null)
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        var schemaChangeNotify = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.TableSchemaChangeNotify);

                        if (schemaChangeNotify != null)
                        {
                            SchemaChangeNotifySettings schemaChangeNotifySettings = JsonConvert.DeserializeObject<SchemaChangeNotifySettings>(schemaChangeNotify?.Value);

                            if (schemaChangeNotifySettings != null)
                            {
                                schemaChangeNotifySettings.EmailRecipients = schemaChangeNotifyModel.EmailRecipients;
                                
                                var serializedSchemaChangeNotify = JsonConvert.SerializeObject(schemaChangeNotifySettings);

                                schemaChangeNotify.Value = serializedSchemaChangeNotify;

                                ctx.SaveChanges();
                            }
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
