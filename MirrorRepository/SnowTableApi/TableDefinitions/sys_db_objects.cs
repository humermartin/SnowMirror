using System;
using Newtonsoft.Json;

namespace MirrorRepository.SnowTableApi.TableDefinitions
{
    public class SnowObject: Record
    {
        [JsonProperty("name")]
        public string TableName { get; set; }

        [JsonProperty("Selected")]
        public bool Selected { get; set; }

        [JsonProperty("UsedInOtherSync")]
        public bool UsedInOtherSync { get; set; }

        [JsonProperty("UsedInOtherSyncList")]
        public string UsedInOtherSyncList { get; set; }
    }
}
