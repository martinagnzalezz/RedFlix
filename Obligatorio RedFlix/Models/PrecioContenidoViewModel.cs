namespace Obligatorio_RedFlix.Models
{
    public class PrecioContenidoViewModel
    {
        public int IdTmdb { get; set; }
        public string Titulo { get; set; }
        public string TipoContenido { get; set; }

        public double PrecioCompraUsd { get; set; }
        public double PrecioAlquilerUsd { get; set; }

        public double PrecioCompraUyu { get; set; }
        public double PrecioAlquilerUyu { get; set; }

        public double PrecioCompraEur { get; set; }
        public double PrecioAlquilerEur { get; set; }

        public double CotizacionUyu { get; set; }
        public double CotizacionEur { get; set; }

        public bool Correcto { get; set; }
        public string MensajeError { get; set; }
    }
}