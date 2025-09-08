using System;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using MirrorRepository.Constants;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorRepository.Model
{
    public class GridSettings
    {

        /// <summary>
        /// Member which holds the log4net logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        /// <summary>
        /// Member which holds the table row count from sql table
        /// </summary>
        public bool EnableColumnSqlCount { get; set; }

        /// <summary>
        /// Member which holds the table row count from sync process table
        /// </summary>
        public bool EnableColumnRecordCount { get; set; }
        
        /// <summary>
        /// Member which holds the table row count from service-now table
        /// </summary>
        public bool EnableColumnSnowCount { get; set; }

        public void UpdateGridChanges(GridSettings gridModel)
        {
            try
            {
                if (gridModel != null)
                {
                    using (ServiceNowDbSyncMgntEntities ctx = new ServiceNowDbSyncMgntEntities())
                    {
                        var gridSettings = ctx.AppSettings.FirstOrDefault(a => a.Key == SnowDbSyncConstants.GridSettings);

                        if (gridSettings != null)
                        {
                            GridSettings gridSettingsModel = JsonConvert.DeserializeObject<GridSettings>(gridSettings?.Value);

                            if (gridSettingsModel != null)
                            {
                                gridSettingsModel.EnableColumnSqlCount = gridModel.EnableColumnSqlCount;
                                gridSettingsModel.EnableColumnRecordCount= gridModel.EnableColumnRecordCount;
                                gridSettingsModel.EnableColumnSnowCount = gridModel.EnableColumnSnowCount;
                            }
                            
                            var serializedGridSettings = JsonConvert.SerializeObject(gridSettingsModel);

                            gridSettings.Value = serializedGridSettings;

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
