using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MirrorRepository.Model.ESB
{
    public class IncidentUpdatedResponse
    {
        public int status { get; set; }

        [JsonProperty("status-200")]
        public Status status_200 { get; set; }

        [JsonProperty("status-401")]
        public Status status_401 { get; set; }

        [JsonProperty("status-403")]
        public Status status_403 { get; set; }

        [JsonProperty("status-500")]
        public Status status_500 { get; set; }

        public override string ToString()
        {
            return "Response: " + status + " - " 
                + (status_200 != null ? status_200.ToString() : "")
                + (status_401 != null ? status_401.ToString() : "")
                + (status_403 != null ? status_403.ToString() : "")
                + (status_500 != null ? status_500.ToString() : "");
        }

    }

    public class Status
    {
        public string content { get; set; }

        public override string ToString()
        {
            return "content:" + content;
        }
    }
}
