using Newtonsoft.Json;

namespace MirrorRepository.Model
{
    public class SnowTableChild
    {
        [JsonProperty("TableName")]
        public string TableName { get; set; }
    }
}
