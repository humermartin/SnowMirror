using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MirrorRepository.Data.SnowDbSyncMgnt
{
    [Table("ServiceSettings")]
    public partial class ServiceSettings
    {
        [Key]
        public Guid Id { get; set; }

        public string ServiceName { get; set; }

        public string SyncMode { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastChanged { get; set; }
    }
}
