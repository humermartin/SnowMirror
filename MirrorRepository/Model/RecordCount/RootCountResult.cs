using Newtonsoft.Json;

namespace MirrorRepository.Model.RecordCount
{
    public class RootCountResult
    {
        [JsonProperty("result")]
        public CountResult Result { get; set; }
    }
}
