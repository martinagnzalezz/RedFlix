using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obligatorio_RedFlix.Models
{
    public class ClimaMain
    {
        [JsonProperty("temp")]
        public double Temp { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        internal string ToLower()
        {
            throw new NotImplementedException();
        }
    }
}