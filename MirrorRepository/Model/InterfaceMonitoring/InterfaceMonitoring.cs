using Newtonsoft.Json;
using System.ComponentModel;

namespace MirrorRepository.Model.InterfaceMonitoring
{
    [DisplayName("u_interface_monitoring")]
    public class InterfaceMonitoring
    {
        [JsonProperty("u_interface")]
        public string Interface { get; set; }

        [JsonProperty("u_direction")]
        public string Direction { get; set; }

        [JsonProperty("u_message_type")]
        public string MessageType { get; set; }

        [JsonProperty("u_inbound_timestamp")]
        public string InboundTimestamp { get; set; }

        [JsonProperty("u_outbound_timestamp")]
        public string OutboundTimestamp { get; set; }

        [JsonProperty("u_source_table")]
        public string SourceTable { get; set; }

        [JsonProperty("u_source_record")]
        public string SourceRecordSysId { get; set; }

        [JsonProperty("u_target_table")]
        public string TargetTable { get; set; }

        [JsonProperty("u_target_record")]
        public string TargetRecordSysId { get; set; }

        [JsonProperty("u_comment")]
        public string Comment { get; set; }

    }
}
