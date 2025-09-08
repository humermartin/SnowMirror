using MirrorRepository.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MirrorRepository.Data.SnowDbSyncMgnt
{

    [Table("ManagementRole")]
    public partial class ManagementRole : ICloneable, ICopyable<ManagementRole>
    {
        [Key]
        public Guid Id { get; set; }

        public string RoleName { get; set; }

        public DateTime? CreateTime { get; set; }
        
        public ManagementRole Copy()
        {
            return (ManagementRole)Clone();
        }

        public override string ToString()
        {
            return GetType().Name+"["+Id+"]: rolename="+RoleName;
        }

        public object Clone()
        {
            var clone = ReflectionHelper.CopyProps(this);
            return clone;
        }
    }

}
