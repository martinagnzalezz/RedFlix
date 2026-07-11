using System;

namespace Obligatorio_RedFlix.Models
{
    public class ReporteUsuarioViewModel
    {
        public int IdReporte { get; set; }
        public int IdUsuario { get; set; }

        public string NombreUsuario { get; set; }
        public string EmailUsuario { get; set; }

        public string TipoReporte { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }

        public string Estado { get; set; }

        public DateTime FechaReporte { get; set; }
        public DateTime? FechaResolucion { get; set; }
    }
}