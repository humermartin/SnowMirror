using System;
using Newtonsoft.Json;

namespace MirrorWeb.Helpers.Kendo
{
    [Serializable]
    public class KendoFilterOperator
    {

        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

    }
}