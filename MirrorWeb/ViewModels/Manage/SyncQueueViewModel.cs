using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MirrorWeb.ViewModels.Manage
{
    /// <summary>
    /// Scheduler model
    /// </summary>
    public class SyncQueueViewModel
    {
        /// <summary>
        /// Gets or sets the SyncNameList values for Sql target
        /// </summary>
        public IList<SelectListItem> SyncNameListSqlDb { get; set; }

        /// <summary>
        /// Gets or sets the SyncNameList values for Kafka target
        /// </summary>
        public IList<SelectListItem> SyncNameListKafka { get; set; }

        /// <summary>
        /// Gets or sets the selected synchronzation for target SqlDb
        /// </summary>
        public Guid? SelectedSynchronizationIdSqlDb { get; set; }

        /// <summary>
        /// Gets or sets the selected synchronzation for target Kafka
        /// </summary>
        public Guid? SelectedSynchronizationIdKafka { get; set; }

        /// <summary>
        /// Gets or sets the Instances
        /// </summary>
        public IList<SelectListItem> InstanceList { get; set; }

        /// <summary>
        /// Gets or sets the selected instanceId
        /// </summary>
        public Guid? SelectedInstanceId { get; set; }

    }
}