using Newtonsoft.Json;
using System.Collections.Generic;

namespace Obligatorio_RedFlix.Models
{
    public class ExchangeRateResponse
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("base_code")]
        public string BaseCode { get; set; }

        [JsonProperty("time_last_update_utc")]
        public string TimeLastUpdateUtc { get; set; }

        [JsonProperty("conversion_rates")]
        public Dictionary<string, double> ConversionRates { get; set; }
    }
}