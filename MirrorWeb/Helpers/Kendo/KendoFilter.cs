using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MirrorWeb.Helpers.Kendo
{
    [Serializable]
    public class KendoFilter
    {
        [JsonProperty("logic")]
        public string Logic { get; set; }

        [JsonProperty("filters")]
        public List<KendoFilters> Filter { get; set; }
    }
}