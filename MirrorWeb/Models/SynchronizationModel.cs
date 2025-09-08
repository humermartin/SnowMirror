using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorWeb.Models
{
    public class SynchronizationModel
    {
        /// <summary>
        /// Gets or sets the syncId value
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the sync name value
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the enabled value
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the running value
        /// </summary>
        public bool Running { get; set; }

        /// <summary>
        /// Gets or sets the start time value
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time value
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// Gets or sets the sync url value
        /// </summary>
        public string SyncUrl { get; set; }
    }
}