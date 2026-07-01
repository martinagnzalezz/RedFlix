using Newtonsoft.Json;
using Obligatorio_RedFlix.Models;
using RestSharp;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    public class CotizacionController : Controller
    {
        private RedFlixDBEntities db = new RedFlixDBEntities();
        private RestResponse HacerRequestCotizacion(string endpoint)
        {
            var options = new RestClientOptions("https://v6.exchangerate-api.com/v6");
            var client = new RestClient(options);

            var request = new RestRequest(endpoint, Method.Get);

            return client.Execute(request);
        }

        public ActionResult Widget()
        {
            CotizacionViewModel vm = ObtenerCotizacion();

            return PartialView("~/Views/Cotizacion/_CotizacionWidget.cshtml", vm);
        }

        public ActionResult PrecioContenido(string tipoContenido, int idTmdb, string titulo)
        {
            PrecioContenidoViewModel vm = ObtenerPreciosContenido(tipoContenido, idTmdb, titulo);

            return PartialView("~/Views/Cotizacion/_PrecioContenido.cshtml", vm);
        }

        private CotizacionViewModel ObtenerCotizacion()
        {
            string apiKey = ConfigurationManager.AppSettings["ExchangeRateApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                return new CotizacionViewModel
                {
                    Correcto = false,
                    MensajeError = "No se encontró la API Key de ExchangeRate en Web.config."
                };
            }

            string endpoint = "/" + apiKey + "/latest/USD";

            RestResponse response = HacerRequestCotizacion(endpoint);

            if (response == null || !response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            {
                return new CotizacionViewModel
                {
                    Correcto = false,
                    MensajeError = "No se pudo obtener la cotización."
                };
            }

            ExchangeRateResponse resultado = JsonConvert.DeserializeObject<ExchangeRateResponse>(response.Content);

            if (resultado == null ||
                resultado.Result != "success" ||
                resultado.ConversionRates == null ||
                !resultado.ConversionRates.ContainsKey("UYU") ||
                !resultado.ConversionRates.ContainsKey("EUR"))
            {
                return new CotizacionViewModel
                {
                    Correcto = false,
                    MensajeError = "La respuesta de la API no contiene UYU o EUR."
                };
            }

            CotizacionViewModel vm = new CotizacionViewModel
            {
                Usd = 1,
                Uyu = resultado.ConversionRates["UYU"],
                Eur = resultado.ConversionRates["EUR"],
                FechaActualizacion = resultado.TimeLastUpdateUtc,
                Correcto = true
            };

            return vm;
        }

        private PrecioContenidoViewModel ObtenerPreciosContenido(string tipoContenido, int idTmdb, string titulo)
        {
            CotizacionViewModel cotizacion = ObtenerCotizacion();

            if (cotizacion == null || !cotizacion.Correcto)
            {
                return new PrecioContenidoViewModel
                {
                    Correcto = false,
                    MensajeError = "No se pudieron calcular los precios porque falló la cotización."
                };
            }

            string tipoBD = tipoContenido == "Serie" ? "serie" : "pelicula";

            var precio = db.PrecioContenidoes.FirstOrDefault(p =>
                p.TmdbId == idTmdb && p.TipoContenido == tipoBD && p.Activo);

            double precioCompraUsd;
            double precioAlquilerUsd;
            int? idPrecio = null;

            if (precio != null)
            {
                precioCompraUsd = (double)precio.PrecioCompra;
                precioAlquilerUsd = (double)precio.PrecioAlquier;
                idPrecio = precio.IdPrecio;
            }
            else
            {
                precioCompraUsd = tipoContenido == "Serie" ? 9.99 : 5.99;
                precioAlquilerUsd = tipoContenido == "Serie" ? 4.99 : 2.99;
            }

            string tipoDisplay = tipoContenido == "Serie" ? "Serie" : "Película";

            return new PrecioContenidoViewModel
            {
                IdTmdb = idTmdb,
                Titulo = titulo,
                TipoContenido = tipoDisplay,

                PrecioCompraUsd = precioCompraUsd,
                PrecioAlquilerUsd = precioAlquilerUsd,

                PrecioCompraUyu = precioCompraUsd * cotizacion.Uyu,
                PrecioAlquilerUyu = precioAlquilerUsd * cotizacion.Uyu,

                PrecioCompraEur = precioCompraUsd * cotizacion.Eur,
                PrecioAlquilerEur = precioAlquilerUsd * cotizacion.Eur,

                CotizacionUyu = cotizacion.Uyu,
                CotizacionEur = cotizacion.Eur,

                IdPrecio = idPrecio,

                Correcto = true
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

    }
}
