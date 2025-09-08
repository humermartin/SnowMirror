using Newtonsoft.Json;
using ServiceNowRecordNet.SnowConnector;
using System.ComponentModel;

namespace MirrorRepository.Model.InterfaceMonitoring
{
    [DisplayName("u_interface_monitoring")]
    public class InterfaceMonitoringResponse
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
        public SnowReference SourceRecord { get; set; }

        [JsonProperty("u_target_table")]
        public string TargetTable { get; set; }

        [JsonProperty("u_target_record")]
        public SnowReference TargetRecord { get; set; }

        [JsonProperty("u_comment")]
        public string Comment { get; set; }

        [JsonProperty("sys_id")]
        public string SysId { get; set; }

        [JsonProperty("sys_updated_on")]
        public string SysUpdatedOn { get; set; }

        [JsonProperty("sys_updated_by")]
        public string SysUpdatedBy { get; set; }

        [JsonProperty("sys_created_on")]
        public string SysCreatedOn { get; set; }

        [JsonProperty("sys_created_by")]
        public string SysCreatedBy { get; set; }
    }
}
