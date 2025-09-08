using System;

namespace MirrorWeb.Models
{
    public class SyncProcessOverrideModel
    {
        /// <summary>
        /// Gets or sets the Enabled value
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the synchronizationId value
        /// </summary>
        public Guid SynchronizationId { get; set; }

        /// <summary>
        /// Gets or sets the TableName value
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the ThreadsPerTable value
        /// </summary>
        public int? ThreadsPerTable { get; set; }

        /// <summary>
        /// Gets or set the ThreadSleepTime list value
        /// </summary>
        public int? ThreadSleepTime { get; set; }

        /// <summary>
        /// Gets or sets the PageSize value
        /// </summary>
        public int? PageSize { get; set; }
        
        /// <summary>
        /// Gets or sets the RequestTimeout value
        /// </summary>
        public int? RequestTimeout { get; set; }

        /// <summary>
        /// Gets or sets the table inheritance enabled flag
        /// </summary>
        public bool? TableInheritanceEnabled { get; set; }

        /// <summary>
        /// Gets or sets the table inheritance flag
        /// </summary>
        public bool? TableInheritance { get; set; }

        /// <summary>
        /// Gets or sets the SyncType
        /// </summary>
        public bool? IsDelta { get; set; }

        /// <summary>
        /// Gets or sets the custom delta start
        /// </summary>
        public string CustomDeltaStart { get; set; }
    }
}