using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MirrorRepository.SnowTableApi.TableDefinitions
{
    public class SysClusterState: Record
    {
        [JsonProperty("allow_inbound")]
        public string AllowInbound { get; set; }

        [JsonProperty("build_name")]
        public string BuildName { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("system_id")]
        public string SystemId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("schedulers")]
        public string Schedulers { get; set; }

        [JsonProperty("node_type ")]
        public string NodeType { get; set; }

        [JsonProperty("instance_name ")]
        public string instance_name { get; set; }

    }
}
