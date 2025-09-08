using MirrorRepository.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MirrorRepository.Data.SnowDbSyncMgnt
{

    [Table("Principals")]
    public partial class Principals : ICloneable, ICopyable<Principals>
    {
        [Key]
        public Guid Id { get; set; }

       public string UserName { get; set; }

        public Guid RoleId { get; set; }
        
        public bool Active { get; set; }
        
        public DateTime CreateTime { get; set; }

        [ForeignKey("RoleId")]
        public virtual ManagementRole ManagementRole { get; set; }

        public Principals Copy()
        {
            return (Principals)Clone();
        }

        public override string ToString()
        {
            return GetType().Name+"["+Id+"]: username="+UserName + ", role=" + ManagementRole + ", active="+Active;
        }
        public object Clone()
        {
            var clone = ReflectionHelper.CopyProps(this);
            return clone;
        }
    }

}
