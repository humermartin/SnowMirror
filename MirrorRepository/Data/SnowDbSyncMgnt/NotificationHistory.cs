using MirrorRepository.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MirrorRepository.Data.SnowDbSyncMgnt
{
    [Table("NotificationHistory")]
    public partial class NotificationHistory : ICloneable, ICopyable<NotificationHistory>
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? InstanceId { get; set; }

        public Guid? SynchronizationId { get; set; }

        public string MessageId { get; set; }

        public string Message { get; set; }

        public DateTime? CreateTime { get; set; }

        public NotificationHistory Copy()
        {
            return (NotificationHistory)Clone();
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
