using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MirrorWeb.Helpers.Kendo
{
    [Serializable]
    public class KendoFilters
    {

        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("logic")]
        public string Logic { get; set; }

        [JsonProperty("filters")]
        public List<KendoFilterOperator> Filters { get; set; }

    }
}