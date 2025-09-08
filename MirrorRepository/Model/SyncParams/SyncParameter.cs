using System;

namespace MirrorRepository.Model.SyncParams
{
    /// <summary>
    /// Holds 
    /// </summary>
    public class SyncParameter
    {
        public bool? Enabled { get; set; }
        
        public int? ThreadsPerTable { get; set; }

        public int? ThreadSleepTime { get; set; }
        
        public int? PageSize { get; set; }

        public int? RequestTimeout { get; set; }
        
        public bool? TableInheritance { get; set; }

        public DateTime? CustomDeltaStart { get; set; }
    }
}
