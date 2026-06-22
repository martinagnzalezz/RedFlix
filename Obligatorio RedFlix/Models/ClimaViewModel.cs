using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obligatorio_RedFlix.Models
{
    public class ClimaViewModel
    {
        public string Ciudad { get; set; }
        public double Temperatura { get; set; }
        public int Humedad { get; set; }
        public string Descripcion { get; set; }
        public string Icono { get; set; }
        public string Recomendacion { get; set; }
        public string CategoriaClima { get; set; }
    }
}