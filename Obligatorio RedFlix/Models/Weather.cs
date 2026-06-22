using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obligatorio_RedFlix.Models
{
    public class Weather
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("main")]
        public string Main { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }
    }
}