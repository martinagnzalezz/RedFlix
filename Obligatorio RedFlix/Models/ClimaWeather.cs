using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obligatorio_RedFlix.Models
{
    public class ClimaWeather
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("main")]
        public ClimaMain Main { get; set; }

        [JsonProperty("weather")]
        public List<ClimaWeather> Weather { get; set; }
        public string Icon { get; internal set; }
        public string Description { get; internal set; }
    }
}