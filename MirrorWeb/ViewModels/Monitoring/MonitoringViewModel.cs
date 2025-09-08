using System.Collections.Generic;
using MirrorWeb.Models;

namespace MirrorWeb.ViewModels.Monitoring
{
    public class MonitoringViewModel
    {
        public int TableRecordsTotalCount { get; set; }

        public List<TableRecordMonitoringModel> TableRecords { get; set; }

        public string LoadedInSeconds { get; set; }
    }
}