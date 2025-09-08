using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace MirrorWeb.Models
{
    public class SnowParams
    {
        [JsonProperty("BaseUrl")]
        public string BaseUrl { get; set; }

        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }

        [JsonProperty("SysDbObjectUrl")]
        public string SysDbObjectUrl { get; set; }
    }
}