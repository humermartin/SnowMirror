using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MirrorRepository.Data.SnowDbSyncMgnt
{
    [Table("AppSettings")]
    public partial class AppSettings
    {
        [Key]
        public Guid Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public DateTime Created { get; set; }
    }
}
