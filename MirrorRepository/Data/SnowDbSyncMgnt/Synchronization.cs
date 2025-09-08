using MirrorRepository.Base;
using MirrorRepository.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MirrorRepository.Data.SnowDbSyncMgnt
{

    [Table("Synchronization")]
    public partial class Synchronization : ICloneable, ICopyable<Synchronization>
    {
        [Key]
        public Guid Id { get; set; }
        
        public bool Enabled { get; set; }

        public Guid? SyncTypeId { get; set; }

        public Guid? DatabaseSettingsId { get; set; }

        public Guid? InstanzSettingsId { get; set; }

        public bool RunImmediately { get; set; }

        public string Name { get; set; }

        public Guid? SyncTargetId{ get; set; }

        public string SyncInterval { get; set; }

        public string SyncActiveSinceDate { get; set; }

        public string SyncStartTime { get; set; }

        public string DaysOfWeek { get; set; }

        public int? PeriodInterval { get; set; }

        public int MaxThreads { get; set; } = 20;

        public int ThreadsPerTable { get; set; } = 10;

        public int ThreadSleepTime { get; set; } = 1;

        public bool AutoSchemaUpdate { get; set; }

        public string SnowTables { get; set; }

        public string UsedCoreTables { get; set; }

        public string SnowColumns { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? ServiceStartDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public System.DateTime? CustomDeltaStart { get; set; }

        public int SubtractMinutesFromDelta { get; set; } = 0;

        public int MaxErrorsPerPage { get; set; } = 50;

        public int PageSize { get; set; } = 1000;

        public int? KafkaBlockSize{ get; set; } = 50;

        public string KafkaMode { get; set; }

        public int RequestTimeout { get; set; } = 30;

        public string FinalMessage { get; set; }

        public string FinalErrorMessage { get; set; }

        public bool ExecuteCleanup { get; set; }

        public string UpdatedBy { get; set; }

        public string CreatedBy { get; set; }

        [ForeignKey("SyncTypeId")]
        public virtual SyncType SyncType { get; set; }

        [ForeignKey("SyncTargetId")]
        public virtual SyncTarget SyncTarget { get; set; }

        [ForeignKey("DatabaseSettingsId")]
        public virtual DatabaseSettings DatabaseSettings { get; set; }

        [ForeignKey("InstanzSettingsId")]
        public virtual InstanzSettings InstanzSettings { get; set; }

        public string Messages { get; set; }

        [NotMapped]
        public List<LogMessage> LogMessages
        {
            get { return LogMessage.Deserialize(Messages); }
            set { Messages = LogMessage.Serialize(value); }
        }


        /// <summary>
        /// get the list of System.DayOfWeek.GetName for this sync
        /// </summary>
        // Do NOT make this a Property!! See: ICopyable!!
        public List<string> GetActiveDayNames()
        {
            return GetActiveDays().Select(d => Enum.GetName(typeof(DayOfWeek), d)).ToList();
        }

        /// <summary>
        /// get the list of System.DayOfWeek for this sync
        /// </summary>
        // Do NOT make this a Property!! See: ICopyable!!
        public List<DayOfWeek> GetActiveDays()
        {
            var result = new List<DayOfWeek>();
            if (!string.IsNullOrEmpty(DaysOfWeek))
            {
                DayOfWeek dof;
                foreach (var day in DaysOfWeek.Split(','))
                {
                    if (Enum.TryParse<DayOfWeek>(day, out dof))
                        result.Add(dof);
                }
            }
            return result;
        }

        // Do NOT make this a Property!! See: ICopyable!!
        public void SetActiveDays(IEnumerable<string> days) {
            var dows = new List<int>();
            foreach (string day in days) 
            {
                DayOfWeek dow;
                if (Enum.TryParse<DayOfWeek>(day, out dow))
                    dows.Add((int)dow);
            }
            DaysOfWeek = string.Join(",", dows);
        }

        public Synchronization Copy()
        {
            return (Synchronization)Clone();
        }

        public override string ToString()
        {
            return GetType().Name+"["+Id+"]: name="+Name + ", type=" + SyncType + ", enabled="+Enabled;
        }
        public object Clone()
        {
            //var clone = (Synchronization)MemberwiseClone();
            //var clone = new Synchronization();
            //clone.SyncType = SyncType ?? SyncType.Copy();
            //clone.DatabaseSettings = DatabaseSettings ?? DatabaseSettings.Copy();
            //clone.InstanzSettings = InstanzSettings ?? InstanzSettings.Copy();
            var clone = ReflectionHelper.CopyProps(this);
            return clone;
        }
    }

}
