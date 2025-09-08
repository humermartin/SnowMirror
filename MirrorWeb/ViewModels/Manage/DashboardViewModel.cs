using System;
using System.Collections.Generic;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorWeb.ViewModels.Manage
{
    public class DashboardViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the synchronization list
        /// </summary>
        public List<Synchronization> Synchronizations { get; set; }

        /// <summary>
        /// Gets or sets total number of synchronizations
        /// </summary>
        public int TotalNumberOfSynchronizations { get; set; }

        /// <summary>
        /// Gets or sets the last success sync value
        /// </summary>
        public DateTime? LastSuccessSync { get; set; }

        /// <summary>
        /// Gets or sets the failed synchronizations
        /// </summary>
        public List<SyncProcess> FailedSynchronizations { get; set; }

        /// <summary>
        /// Gets or sets the last success synchronizations 
        /// </summary>
        public List<SyncProcess> LastSuccessSynchronizations { get; set; }

        /// <summary>
        /// Gets or sets the active synchronizations
        /// </summary>
        public List<SyncProcess> ActiveSynchronizations { get; set; }

    }
}