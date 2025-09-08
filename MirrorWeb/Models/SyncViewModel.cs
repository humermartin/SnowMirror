using System;

namespace MirrorWeb.Models
{
    public class SyncViewModel
    {
        public Guid Id { get; set; }

        public bool Enabled { get; set; }

        public string SyncName { get; set; }

        public string Instance { get; set; }

        public string PlannedStart { get; set; }

        public string NextStart { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public string Duration { get; set; }

        public int Period { get; set; }

        public string PlannedWeekDay { get; set; }

        public string TableName { get; set; }

        public int RecordsFound { get; set; }

        public int RecordsSynchronized { get; set; }

        public int RecordsUpdated { get; set; }
        
        public int RecordsInserted { get; set; }

        public int RecordsPosted { get; set; }
    }
}