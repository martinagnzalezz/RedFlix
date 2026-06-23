using Newtonsoft.Json;

namespace Obligatorio_RedFlix.Models
{
    public class ClimaMain
    {
        [JsonProperty("temp")]
        public double Temp { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }
    }
}