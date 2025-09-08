using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MirrorRepository.Data.SnowDbSyncMgnt;

namespace MirrorWeb.Models
{
    public class SynchronizationListModel
    {
        public List<SynchronizationModel> Synchronizations { get; set; }

        public int SynchronizationCount { get; set; }
    }
}