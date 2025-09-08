using System.Collections.Generic;

namespace MirrorWeb.Models
{
    public class DashboardModel
    {
        /// <summary>
        /// Member which holds the RunningSync List
        /// </summary>
        public List<SyncViewModel> RunningSyncViewModel { get; set; }

        /// <summary>
        /// Member which holds the RunningSync TotalCount
        /// </summary>
        public int RunningSyncViewModelTotalCount { get; set; }

        /// <summary>
        /// Member which holds the FullSync List
        /// </summary>
        public List<SyncViewModel> SyncFullViewModels { get; set; }

        /// <summary>
        /// Member which holds the FullSync TotalCount
        /// </summary>
        public int SyncFullViewModelsTotalCount { get; set; }

        /// <summary>
        /// Member which holds the DeltaSync List
        /// </summary>
        public List<SyncViewModel> SyncDeltaViewModels { get; set; }

        /// <summary>
        /// Member which holds the DeltaSync TotalCount
        /// </summary>
        public int SyncDeltaViewModelsTotalCount { get; set; }

        /// <summary>
        /// Member which holds the Kafka DeltaSync List
        /// </summary>
        public List<SyncViewModel> SyncKafkaDeltaViewModels { get; set; }

        /// <summary>
        /// Member which holds the Kafka DeltaSync TotalCount
        /// </summary>
        public int SyncKafkaDeltaViewModelsTotalCount { get; set; }
    }
}