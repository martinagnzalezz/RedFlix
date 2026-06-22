using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obligatorio_RedFlix.Models
{
	public partial class Clima
	{
        public int IdClima { get; set; }
        public string Ciudad { get; set; }
        public Nullable<decimal> Temperatura { get; set; }
        public string EstadoClima { get; set; }
        public Nullable<int> Humedad { get; set; }
        public System.DateTime FechaConsulta { get; set; }
    }
}