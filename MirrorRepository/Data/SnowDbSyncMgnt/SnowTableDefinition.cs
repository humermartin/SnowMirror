using MirrorRepository.Helpers;
using MirrorRepository.Model.SyncParams;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace MirrorRepository.Data.SnowDbSyncMgnt
{
    [Table("SnowTableDefinition")]
    public partial class SnowTableDefinition: ICloneable, ICopyable<SnowTableDefinition>
    {
        [Key]
        public Guid Id { get; set; }

        public string Table { get; set; }

        public bool Enabled { get; set; }

        public string Columns { get; set; }

        public string PostScripts { get; set; }

        public Guid InstanceId { get; set; }

        public DateTime? CreateTime { get; set; }

        public int? ThreadsPerTable { get; set; }

        public int? ThreadSleepTime { get; set; }

        public int? PageSize { get; set; }

        public int? RequestTimeout { get; set; }

        public string TableParams { get; set; }

        [NotMapped]
        public List<TableParam> TableParameters {
            get {
                if (String.IsNullOrWhiteSpace(TableParams)) 
                    return new List<TableParam>();
                return JsonConvert.DeserializeObject<List<TableParam>>(TableParams);
            }
            set
            {
                if (value == null)
                    TableParams = null;
                else
                    TableParams = JsonConvert.SerializeObject(value);
            }
        }

        public SnowTableDefinition Copy()
        {
            return (SnowTableDefinition)Clone();
        }
        public override string ToString()
        {
            return GetType().Name + "[" + Id + "]";
        }
        public object Clone()
        {
            var clone = ReflectionHelper.CopyProps(this);
            return clone;
        }
    }
}
