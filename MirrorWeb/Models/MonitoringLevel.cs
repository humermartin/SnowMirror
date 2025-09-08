using System.Collections.Generic;

namespace MirrorWeb.Models
{
    public class MonitoringLevel
    {
        public string MonitoringLevelKey { get; set; }

        public string MonitoringLevelValue { get; set; }

        public List<MonitoringLevel> Init()
        {
            var snowInstances = new List<MonitoringLevel>
            {
                new MonitoringLevel {MonitoringLevelKey = "FullPackage", MonitoringLevelValue = "FullPackage"},
                new MonitoringLevel {MonitoringLevelKey = "SingleTable", MonitoringLevelValue = "SingleTable"}
            };

            return snowInstances;
        }
    }
}