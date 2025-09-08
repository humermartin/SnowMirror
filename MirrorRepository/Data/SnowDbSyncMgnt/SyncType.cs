
namespace MirrorRepository.Data.SnowDbSyncMgnt
{
    using MirrorRepository.Helpers;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SyncType")]
    public partial class SyncType : ICloneable, ICopyable<SyncType>
    {


        [Key]
        public System.Guid Id { get; set; }

        public string TypeName { get; set; }

        public Nullable<System.DateTime> Created { get; set; }

        public override string ToString()
        {
            return GetType().Name + "[" + Id + "]: name=" + TypeName;
        }

        public SyncType Copy()
        {
            return (SyncType)Clone();
        }
        public object Clone()
        {
            var clone = ReflectionHelper.CopyProps(this);
            return clone;
        }
    }

}
