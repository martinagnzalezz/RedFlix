using System.Collections.Generic;

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

        public string Emoji { get; set; }
        public string Promocion { get; set; }
        public bool TienePromocionActiva { get; set; }
        public string NombrePromocionActiva { get; set; }
        public string DescripcionPromocionActiva { get; set; }
        public string CondicionPromocionActiva { get; set; }
        public double PorcentajePromocionActiva { get; set; }

        public List<Populares> PeliculasRecomendadas { get; set; }
        public List<Populares> SeriesRecomendadas { get; set; }
        public List<PronosticoDiaViewModel> PronosticoDias { get; set; }
    }
}