using Newtonsoft.Json;
using System.Collections.Generic;

namespace Obligatorio_RedFlix.Models
{
    public class ClimaWeather
    {
        [JsonProperty("weather")]
        public List<ClimaWeatherInfo> Weather { get; set; }

        [JsonProperty("main")]
        public ClimaMain Main { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class ClimaWeatherInfo
    {
        [JsonProperty("main")]
        public string Main { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }
    }
}