using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MirrorRepository.Data.SnowDbSyncMgnt
{

    [Table("TableMonitoring")]
    public partial class TableMonitoring
    {
        [Key]
        public Guid Id { get; set; }

        public string TableName { get; set; }

        public DateTime? GetDeltaRecordsFrom { get; set; }

        public Guid? SyncId { get; set; }

        public Guid? SyncTypeId { get; set; }

        public Guid? InstanzSettingsId { get; set; }

        public Guid? DatabaseSettingsId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string Duration { get; set; }
        
        public int? ThreadsPerTable { get; set; }

        public int? ThreadSleepTime { get; set; }

        public int? PageSize { get; set; }
        
        public int? RequestTimeout { get; set; }

        public string Messages { get; set; }
        
        public string FinalMessage { get; set; }

        public string FinalErrorMessage { get; set; }
        
        public DateTime? Created { get; set; }

        [ForeignKey("SyncTypeId")]
        public virtual SyncType SyncType { get; set; }

        [ForeignKey("DatabaseSettingsId")]
        public virtual DatabaseSettings DatabaseSettings { get; set; }

        [ForeignKey("InstanzSettingsId")]
        public virtual InstanzSettings InstanzSettings { get; set; }
    }

}
