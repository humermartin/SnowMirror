using System;
using System.Collections.Generic;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorWeb.Models
{
    public class TableRecordMonitoringModel
    {
        /// <summary>
        /// Gets or sets the id value
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the TableName
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the Instance
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// Gets or sets the DeltaRecordFrom value
        /// </summary>
        public DateTime? GetDeltaRecordsFrom { get; set; }

        /// <summary>
        /// Gets or sets the SyncId
        /// </summary>
        public string SyncId { get; set; }

        /// <summary>
        /// Gets or sets the SyncTypeId
        /// </summary>
        public string SyncTypeId { get; set; }

        /// <summary>
        /// Gets or sets the StartTime
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the EndTime
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the Duration
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// Gets or sets the Period
        /// </summary>
        public string Period { get; set; }


    }
}