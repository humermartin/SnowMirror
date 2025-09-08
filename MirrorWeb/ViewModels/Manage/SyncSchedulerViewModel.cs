using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MirrorRepository;
using MirrorRepository.Data.SnowDbSyncMgnt;
using MirrorRepository.Enums;
using MirrorRepository.Model;

namespace MirrorWeb.ViewModels.Manage
{
    /// <summary>
    /// Scheduler model
    /// </summary>
    public class SyncSchedulerViewModel
    {
        /// <summary>
        /// Member which holds the targetTypeItem
        /// </summary>
        private List<SelectListItem> _targetTypeItem;

        /// <summary>
        /// Member which holds the kafkaModeItem
        /// </summary>
        private List<SelectListItem> _kafkaModeItem;

        /// <summary>
        /// Member which holds the Interval values
        /// </summary>
        private List<SelectListItem> _intervalListItem;
        
        /// <summary>
        /// DatabaseSetting Selections
        /// </summary>
        private List<SelectListItem> _databaseSettingsListItem;
        
        /// <summary>
        /// InstanzSettings Selections
        /// </summary>
        private List<SelectListItem> _instanzSettingsListItem;
        
        /// <summary>
        /// Member which holds the DaysOfWeek values
        /// </summary>
        private List<SelectListItem> _daysOfWeekListItem;

        public List<SelectListItem> SyncTypes
        {
            get => _intervalListItem = GetSyncTypes();
            set => _intervalListItem = value;
        }

        public List<SelectListItem> DatabaseSettings
        {
            get => _databaseSettingsListItem = GetDatabaseSettings();
            set => _databaseSettingsListItem = value;
        }

        public List<SelectListItem> InstanzSettings
        {
            get => _instanzSettingsListItem = GetInstanzSettings();
            set => _instanzSettingsListItem = value;
        }

        public string SelectedSyncTargetType { get; set; }

        public List<SelectListItem> SyncTargetTypes
        {
            get => _targetTypeItem = GetTargetTypeFromEnum();
            set => _targetTypeItem = value;
        }

        public string SelectedKafkaMode { get; set; }

        public List<SelectListItem> KafkaModes
        {
            get => _kafkaModeItem = GetKafkaModeFromEnum();
            set => _kafkaModeItem = value;
        }

        public List<SelectListItem> SyncTargets { get; set; }
        
        /// <summary>
        /// The current SynchronizationId - or null if new
        /// </summary>
        public Guid? SynchronizationId { get; set; }

        /// <summary>
        /// Gets or set the selected sync type
        /// </summary>
        public Guid? SelectedSyncType { get; set; }
        
        /// <summary>
        /// The selected DatabaseSettings
        /// </summary>
        public Guid? SelectedDatabaseSettings { get; set; }

        /// <summary>
        /// The selected InstanzSettings
        /// </summary>
        public Guid? SelectedInstanzSettings { get; set; }

        
        /// <summary>
        /// Gets or set the selected target
        /// </summary>
        public Guid? SelectedSyncTarget { get; set; }

        /// <summary>
        /// Gets or sets the SyncInterval value
        /// </summary>
        public List<SelectListItem> SyncInverval
        {
            get => _intervalListItem = GetIntervalFromEnum();
            set => _intervalListItem = value;
        }

        /// <summary>
        /// Gets or set the selected interval
        /// </summary>
        public EnumInterval SelectedInterval { get; set; }

        /// <summary>
        /// Gets or sets the Threads per Table
        /// </summary>
        public int ThreadsPerTable { get; set; }

        /// <summary>
        /// Gets or sets the Thread Sleep Time
        /// </summary>
        public int ThreadSleepTime { get; set; }

        /// <summary>
        /// Gets or sets the PageSize value
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the KafkaBlockSize value
        /// </summary>
        public int? KafkaBlockSize { get; set; }

        /// <summary>
        /// Gets or sets the KafkaMode
        /// </summary>
        public string KafkaMode { get; set; }
        /// <summary>
        /// Gets or sets the RunImmediately value
        /// </summary>
        public bool RunImmediately { get; set; }

        /// <summary>
        /// Gets or sets the ActiveSince value
        /// </summary>
        public string ActiveSince { get; set; }

        /// <summary>
        /// Gets or sets the CustomDeltaStart
        /// </summary>
        public string CustomDeltaStart { get; set; }

        /// <summary>
        /// Gets or sets the SubtractMinutesFromDelta value
        /// </summary>
        public int SubtractMinutesFromDelta { get; set; }

        /// <summary>
        /// Gets or sets the start time value
        /// </summary>
        public string Time { get; set; }
        
        /// <summary>
        /// Gets or sets the Days of week enum values
        /// </summary>
        public List<SelectListItem> DaysOfWeek
        {
            get => _daysOfWeekListItem = GetDaysOfWeek();
            set => _daysOfWeekListItem = value;
        }

        /// <summary>
        /// Gets or sets the selected days of week
        /// </summary>
        public List<SnowDayOfWeek> SelectedDaysOfWeek { get; set; }

        /// <summary>
        /// Gets or set the minute interval value
        /// </summary>
        public int IntervalInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the synchronization name
        /// </summary>
        public string SynchronizationName { get; set; }

        /// <summary>
        /// Gets or sets the Request Timeout value
        /// </summary>
        public int RequestTimeout { get; set; }

        /// <summary>
        /// Pass Interval enum to SelectListItem
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetIntervalFromEnum()
        {
            List<SelectListItem> listItem = Enum.GetValues(typeof(EnumInterval)).Cast<EnumInterval>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString()
            }).ToList();

            listItem.Insert(0, new SelectListItem{Text = "", Value = "0"});

            return listItem;
        }
        
        /// <summary>
        /// Pass DaysOfWeek enum to SelectListItem
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetDaysOfWeek()
        {
            List<SelectListItem> listItem = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>()
                .Select(v => new SelectListItem
                {
                    Text = Enum.GetName(typeof(DayOfWeek), v),
                    Value = ((int)v).ToString()
                }).ToList();

            return listItem;
        }

        /// <summary>
        /// Gets the enum target types
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetTargetTypeFromEnum()
        {
            List<SelectListItem> targetTypeItem = Enum.GetValues(typeof(EnumTargetType)).Cast<EnumTargetType>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString()
            }).ToList();

            return targetTypeItem;
        }

        /// <summary>
        /// Gets the enum kafka Mode
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetKafkaModeFromEnum()
        {
            List<SelectListItem> kafkaModeItem = Enum.GetValues(typeof(EnumKafkaMode)).Cast<EnumKafkaMode>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString()
            }).ToList();
            kafkaModeItem.Insert(0, new SelectListItem() { Text = "", Value = null });

            return kafkaModeItem;
        }

        /// <summary>
        /// Get synchronization types
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetSyncTypes()
        {
            using (ServiceNowDbSyncMgntEntities snowMgntEntities = new ServiceNowDbSyncMgntEntities())
            {
                var syncTypes = (from s in snowMgntEntities.SyncType
                    select new SelectListItem()
                    {
                        Selected = false,
                        Text = s.TypeName,
                        Value = s.Id.ToString()
                    }).ToList();

                return syncTypes;
            }
        }

        /// <summary>
        /// Get DatabaseSettings
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetDatabaseSettings()
        {
            using (ServiceNowDbSyncMgntEntities snowMgntEntities = new ServiceNowDbSyncMgntEntities())
            {
                var syncTypes = (from s in snowMgntEntities.DatabaseSettings
                                 select new SelectListItem()
                                 {
                                     Selected = false,
                                     Text = s.Instancename,
                                     Value = s.Id.ToString()
                                 }).ToList();

                return syncTypes;
            }
        }

        /// <summary>
        /// Get InstanzSettings
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetInstanzSettings()
        {
            using (ServiceNowDbSyncMgntEntities snowMgntEntities = new ServiceNowDbSyncMgntEntities())
            {
                var instanzSettings = (from s in snowMgntEntities.InstanzSettings
                                 select new SelectListItem()
                                 {
                                     Selected = false,
                                     Text = s.InstanzName,
                                     Value = s.Id.ToString()
                                 }).ToList();

                return instanzSettings;
            }
        }

        /// <summary>
        /// get SyncTargets
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetSyncTargets()
        {
            using (ServiceNowDbSyncMgntEntities snowMgntEntities = new ServiceNowDbSyncMgntEntities())
            {
                var syncTargets = (from s in snowMgntEntities.SyncTarget
                    select new SelectListItem()
                    {
                        Selected = false,
                        Text = s.Targetname,
                        Value = s.Id.ToString()
                    }).ToList();

                return syncTargets;
            }
        }
    }
}