namespace Obligatorio_RedFlix.Models
{
    public class CotizacionViewModel
    {
        public double Usd { get; set; }
        public double Uyu { get; set; }
        public double Eur { get; set; }

        public string FechaActualizacion { get; set; }

        public bool Correcto { get; set; }
        public string MensajeError { get; set; }
    }
}