using Newtonsoft.Json;

namespace MirrorRepository.Model.RecordCount

{
    public class CountResult
    {
        [JsonProperty("stats")]
        public Stats Stats { get; set; }
    }
}
