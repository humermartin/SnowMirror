using MirrorRepository.Enums;
using System;
using System.Collections.Generic;

namespace MirrorRepository.Model
{
    /// <summary>
    /// class holds the snow tables
    /// </summary>
    [Serializable]
    public class SnowTables
    {
        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the tyble sysid
        /// </summary>
        public Guid? SysId { get; set; }

        /// <summary>
        /// Gets or sets the RowCount value
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// Gets or sets the RowCount value
        /// </summary>
        public int SnowCount { get; set; }

        /// <summary>
        /// Gets or sets the sql row count
        /// </summary>
        public int SqlCount { get; set; }

        /// <summary>
        /// Gets or sets the progress value
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Gets or sets the SyncState
        /// </summary>
        public EnumSyncProcessState SyncState { get; set; }

        /// <summary>
        /// Gets or sets the failed value
        /// </summary>
        public int Failures { get; set; }

        /// <summary>
        /// Gets or sets the inserted value
        /// </summary>
        public int Inserted { get; set; }

        /// <summary>
        /// Gets or sets the updated value
        /// </summary>
        public int Updated { get; set; }

        /// <summary>
        /// Gets or sets the deleted value
        /// </summary>
        public int Deleted { get; set; }

        /// <summary>
        /// Gets or sets the process Messages
        /// </summary>
        public string ProcessMessage { get; set; }

        /// <summary>
        /// Gets or sets the process EndTime
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// Gets or sets the process EndTime
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the calculated duration time value
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// Gets or sets the UsedInOtherSync value
        /// </summary>
        public bool UsedInOtherSync { get; set; }
        
        /// <summary>
        /// Gets or sets the table columns
        /// </summary>
        public List<string> Columns { get; set; }

        /// <summary>
        /// Gets or sets the column restriction value
        /// </summary>
        public bool HasColumnRestriction { get; set; }

        /// <summary>
        /// Gets or sets the table script command values
        /// </summary>
        public List<ScriptCommand> SciptCommands { get; set; }

        /// <summary>
        /// Gets or sets the Enabled value
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the sync target type
        /// </summary>
        public string TargetType { get; set; }

        public override string ToString()
        {
            return "SnowTable["+Name+":"+SysId+"] sync="+SyncState.ToString()+", rows="+RowCount+", progress="+Progress;
        }
    }
}
