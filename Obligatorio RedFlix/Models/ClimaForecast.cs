using Newtonsoft.Json;
using System.Collections.Generic;

namespace Obligatorio_RedFlix.Models
{
    public class ClimaForecast
    {
        [JsonProperty("list")]
        public List<ClimaForecastItem> List { get; set; }
    }

    public class ClimaForecastItem
    {
        [JsonProperty("main")]
        public ClimaMain Main { get; set; }

        [JsonProperty("weather")]
        public List<ClimaWeatherInfo> Weather { get; set; }

        [JsonProperty("dt_txt")]
        public string FechaTexto { get; set; }
    }
}