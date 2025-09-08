using Newtonsoft.Json;
using System.Collections.Generic;

namespace MirrorRepository.Model
{
    public class SnowTableParent
    {
        [JsonProperty("TableName")]
        public string TableName { get; set; }

        [JsonProperty("SnowTableChildren")]
        public List<SnowTableChild> SnowTableChildren { get; set; }
    }
}
