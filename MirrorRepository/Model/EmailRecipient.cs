using Newtonsoft.Json;

namespace MirrorRepository.Model
{
    public class EmailRecipient
    {
        [JsonProperty("EmailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }
}
