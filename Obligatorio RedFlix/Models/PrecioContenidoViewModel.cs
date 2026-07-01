namespace Obligatorio_RedFlix.Models
{
    public class PrecioContenidoViewModel
    {
        public int IdTmdb { get; set; }
        public string Titulo { get; set; }
        public string TipoContenido { get; set; }

        // Precio original en USD
        public double PrecioCompraUsd { get; set; }
        public double PrecioAlquilerUsd { get; set; }

        // Precio original convertido
        public double PrecioCompraUyu { get; set; }
        public double PrecioAlquilerUyu { get; set; }

        public double PrecioCompraEur { get; set; }
        public double PrecioAlquilerEur { get; set; }

        // Precio final con descuento
        public double PrecioCompraUsdFinal { get; set; }
        public double PrecioAlquilerUsdFinal { get; set; }

        public double PrecioCompraUyuFinal { get; set; }
        public double PrecioAlquilerUyuFinal { get; set; }

        public double PrecioCompraEurFinal { get; set; }
        public double PrecioAlquilerEurFinal { get; set; }

        public double CotizacionUyu { get; set; }
        public double CotizacionEur { get; set; }

        public bool Correcto { get; set; }
        public string MensajeError { get; set; }

        public int? IdPrecio { get; set; }
        public bool TienePrecioCargado => IdPrecio.HasValue;

        // Promoción
        public bool TienePromocion { get; set; }
        public string NombrePromocion { get; set; }
        public string CondicionPromocion { get; set; }
        public double DescuentoPorcentaje { get; set; }
        public double PorcentajeDescuento { get; internal set; }
    }
}