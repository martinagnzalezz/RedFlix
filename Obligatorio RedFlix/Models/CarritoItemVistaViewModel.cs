namespace Obligatorio_RedFlix.Models
{
    public class CarritoItemVistaViewModel
    {
        public int IdCarritoItem { get; set; }

        public string Titulo { get; set; }
        public string TipoContenido { get; set; }

        public string TipoOperacion { get; set; }
        public string Moneda { get; set; }

        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Total { get; set; }

        public decimal PrecioOriginal { get; set; }
        public decimal TotalOriginal { get; set; }
        public bool TienePromocion { get; set; }
        public string NombrePromocion { get; set; }
        public decimal PorcentajeDescuento { get; set; }
    }
}
