using MirrorRepository.Constants;
using MirrorRepository.Model;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MirrorWeb.ViewModels.Manage
{
    public class GeneralSettingsViewModel
    {
        /// <summary>
        /// Gets or sets the Monitoring Levels
        /// </summary>
        public List<SelectListItem> MonitoringLevels
        {
            get
            {
                List<SelectListItem> listItem = new List<SelectListItem>();
                SelectListItem sPackageItem = new SelectListItem {Text = SnowDbSyncConstants.LevelPackage, Value = SnowDbSyncConstants.LevelPackage };
                SelectListItem sSingleTableItem = new SelectListItem {Text = SnowDbSyncConstants.LevelSingleTable, Value = SnowDbSyncConstants.LevelSingleTable };
                listItem.Add(sPackageItem);
                listItem.Add(sSingleTableItem);

                return listItem;
            }
        }

        /// <summary>
        /// Gets or sets the MonitoringSettings
        /// </summary>
        public MonitoringSettings MonitoringSettings { get; set; } = new MonitoringSettings();

        /// <summary>
        /// Gets or sets the Sql session settings
        /// </summary>
        public SqlSessionSettings SqlSessionSettings { get; set; } = new SqlSessionSettings();

        /// <summary>
        /// Gets or sets the alert notify settings
        /// </summary>
        public AlertNotifySettings AlertNotifySettings { get; set; } = new AlertNotifySettings();

        /// <summary>
        /// Gets or sets the table schema change notify settings
        /// </summary>
        public SchemaChangeNotifySettings SchemaChangeNotifySettings { get; set; } = new SchemaChangeNotifySettings();

        /// <summary>
        /// Gets or sets the process settings
        /// </summary>
        public ProcessSettings ProcessSettings { get; set; } = new ProcessSettings();

        /// <summary>
        /// Gets or sets the process settings
        /// </summary>
        public GridSettings GridSettings { get; set; } = new GridSettings();
    }
}