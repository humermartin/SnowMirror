using Newtonsoft.Json;

namespace MirrorRepository.Model.RecordCount
{
    public class Stats
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
