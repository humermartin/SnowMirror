using Newtonsoft.Json;
using System.Collections.Generic;

namespace MirrorRepository.SnowTableApi
{
    public class ColumnResponse
    {
        [JsonProperty("result")]
        public List<SnowColumns> SnowColumns { get; set; }
    }

    public class SnowColumns
    {
        [JsonProperty("Selected")]
        public bool Selected { get; set; }

        [JsonProperty("element")]
        public Element Element { get; set; }

        [JsonProperty("max_length")]
        public MaxLength MaxLength { get; set; }

        [JsonProperty("column_label")]
        public ColumnLabel ColumnLabel { get; set; }

        [JsonProperty("internal_type")]
        public InternalType InternalType { get; set; }

        [JsonProperty("sys_id")]
        public SysId SysId { get; set; }

        [JsonProperty("sys_name")]
        public SysName SysName { get; set; }
    }

    public class Element
    {
        [JsonProperty("display_value")]
        public string DisplayValue { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class MaxLength
    {
        [JsonProperty("display_value")]
        public string DisplayValue{ get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class ColumnLabel
    {
        [JsonProperty("display_value")]
        public string DisplayValue { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class InternalType
    {
        [JsonProperty("display_value")]
        public string DisplayValue { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class SysId
    {
        [JsonProperty("display_value")]
        public string DisplayValue { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class SysName
    {
        [JsonProperty("display_value")]
        public string DisplayValue { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
