
namespace MirrorRepository.Data.SnowDbSyncMgnt
{
    using MirrorRepository.Helpers;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SyncTarget")]
    public partial class SyncTarget : ICloneable, ICopyable<SyncTarget>
    {


        [Key]
        public System.Guid Id { get; set; }

        public string TargetType { get; set; }

        public string Targetname { get; set; }

        public string Endpoint { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public Nullable<System.DateTime> Created { get; set; }

        public Nullable<System.DateTime> LastChanged { get; set; }

        public override string ToString()
        {
            return GetType().Name + "[" + Id + "]: name=" + Targetname;
        }

        public SyncTarget Copy()
        {
            return (SyncTarget)Clone();
        }
        public object Clone()
        {
            var clone = ReflectionHelper.CopyProps(this);
            return clone;
        }
    }

}
