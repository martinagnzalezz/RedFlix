using Newtonsoft.Json;
using Obligatorio_RedFlix.Models;
using RestSharp;
using System;
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

            RestResponse response = HacerRequestCotizacion("/" + apiKey + "/latest/USD");

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

            return new CotizacionViewModel
            {
                Usd = 1,
                Uyu = resultado.ConversionRates["UYU"],
                Eur = resultado.ConversionRates["EUR"],
                FechaActualizacion = resultado.TimeLastUpdateUtc,
                Correcto = true
            };
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
            PrecioContenido precio = db.PrecioContenidoes.FirstOrDefault(p =>
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

            PromocionesClima promocion = ObtenerPromocionClimaAplicable();
            double porcentajeDescuento = promocion != null ? Convert.ToDouble(promocion.PorcentajeDesc) : 0;
            double factorDescuento = 1 - (porcentajeDescuento / 100);
            double precioCompraUsdFinal = promocion != null ? precioCompraUsd * factorDescuento : precioCompraUsd;
            double precioAlquilerUsdFinal = promocion != null ? precioAlquilerUsd * factorDescuento : precioAlquilerUsd;

            return new PrecioContenidoViewModel
            {
                IdTmdb = idTmdb,
                Titulo = titulo,
                TipoContenido = tipoContenido == "Serie" ? "Serie" : "Película",

                PrecioCompraUsd = precioCompraUsd,
                PrecioAlquilerUsd = precioAlquilerUsd,

                PrecioCompraUyu = precioCompraUsd * cotizacion.Uyu,
                PrecioAlquilerUyu = precioAlquilerUsd * cotizacion.Uyu,
                PrecioCompraEur = precioCompraUsd * cotizacion.Eur,
                PrecioAlquilerEur = precioAlquilerUsd * cotizacion.Eur,

                PrecioCompraUsdFinal = precioCompraUsdFinal,
                PrecioAlquilerUsdFinal = precioAlquilerUsdFinal,
                PrecioCompraUyuFinal = precioCompraUsdFinal * cotizacion.Uyu,
                PrecioAlquilerUyuFinal = precioAlquilerUsdFinal * cotizacion.Uyu,
                PrecioCompraEurFinal = precioCompraUsdFinal * cotizacion.Eur,
                PrecioAlquilerEurFinal = precioAlquilerUsdFinal * cotizacion.Eur,

                CotizacionUyu = cotizacion.Uyu,
                CotizacionEur = cotizacion.Eur,
                IdPrecio = idPrecio,

                TienePromocion = promocion != null,
                NombrePromocion = promocion != null ? promocion.Nombre : "",
                CondicionPromocion = promocion != null ? promocion.CondicionClima : "",
                PorcentajeDescuento = porcentajeDescuento,

                Correcto = true
            };
        }

        private PromocionesClima ObtenerPromocionClimaAplicable()
        {
            ClimaActualSimple clima = ObtenerClimaActual();

            if (clima == null)
            {
                return null;
            }

            return db.PromocionesClimas
                .Where(p => p.Activa)
                .Where(p => p.TemperaturaMax == null || clima.Temperatura <= p.TemperaturaMax.Value)
                .ToList()
                .Where(p => string.IsNullOrWhiteSpace(p.CondicionClima) ||
                    NormalizarTexto(p.CondicionClima) == clima.Categoria ||
                    clima.Descripcion.Contains(NormalizarTexto(p.CondicionClima)))
                .OrderByDescending(p => p.PorcentajeDesc)
                .FirstOrDefault();
        }

        private ClimaActualSimple ObtenerClimaActual()
        {
            string apiKey = ConfigurationManager.AppSettings["OpenWeatherApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                return null;
            }

            var options = new RestClientOptions("https://api.openweathermap.org/data/2.5");
            var client = new RestClient(options);
            var request = new RestRequest("/weather?lat=-34.9&lon=-54.95&appid=" + apiKey + "&units=metric&lang=es", Method.Get);
            RestResponse response = client.Execute(request);

            if (response == null || !response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            {
                return null;
            }

            ClimaWeather clima = JsonConvert.DeserializeObject<ClimaWeather>(response.Content);

            if (clima == null || clima.Main == null || clima.Weather == null || clima.Weather.Count == 0)
            {
                return null;
            }

            string estado = NormalizarTexto(clima.Weather[0].Main);
            string descripcion = NormalizarTexto(clima.Weather[0].Description);

            return new ClimaActualSimple
            {
                Temperatura = Convert.ToDecimal(clima.Main.Temp),
                Descripcion = descripcion,
                Categoria = ObtenerCategoriaClima(estado, Convert.ToDecimal(clima.Main.Temp))
            };
        }

        private string ObtenerCategoriaClima(string estado, decimal temperatura)
        {
            if (estado.Contains("rain") || estado.Contains("drizzle") || estado.Contains("thunderstorm") || estado.Contains("lluvia"))
            {
                return "lluvia";
            }

            if (estado.Contains("cloud") || estado.Contains("nube"))
            {
                return "nublado";
            }

            if (temperatura <= 12)
            {
                return "frio";
            }

            if (temperatura >= 25)
            {
                return "calor";
            }

            return "templado";
        }

        private string NormalizarTexto(string texto)
        {
            return string.IsNullOrWhiteSpace(texto)
                ? ""
                : texto.Trim().ToLower()
                    .Replace("í", "i")
                    .Replace("á", "a")
                    .Replace("é", "e")
                    .Replace("ó", "o")
                    .Replace("ú", "u");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}