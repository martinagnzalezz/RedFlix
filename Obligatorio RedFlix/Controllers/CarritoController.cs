using Newtonsoft.Json;
using Obligatorio_RedFlix.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    public class CarritoController : Controller
    {
        private RedFlixDBEntities db = new RedFlixDBEntities();

        public ActionResult Index()
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            Carrito carrito = db.Set<Carrito>()
                .FirstOrDefault(c => c.IdUsuario == idUsuario);

            List<CarritoItemVistaViewModel> items = new List<CarritoItemVistaViewModel>();

            if (carrito != null)
            {
                items = db.Set<CarritoItem>()
                    .Where(i => i.IdCarrito == carrito.IdCarrito)
                    .ToList()
                    .Select(i => new CarritoItemVistaViewModel
                    {
                        IdCarritoItem = i.IdCarritoItem,

                        Titulo = i.Pelicula != null
                            ? i.Pelicula.Titulo
                            : i.Series != null
                                ? i.Series.Titulo
                                : "Sin título",

                        TipoContenido = i.IdPelicula != null ? "Película" : "Serie",

                        TipoOperacion = i.TipoOperacion,
                        Moneda = i.Moneda,

                        Cantidad = i.Cantidad,
                        Precio = i.Precio,
                        Total = i.Precio * i.Cantidad,

                        PrecioOriginal = CalcularPrecioSinPromocion(
                            i.IdPelicula != null ? i.Pelicula.IdTmdb : i.Series.IdTmdb,
                            i.IdPelicula != null ? "Película" : "Serie",
                            i.TipoOperacion,
                            i.Moneda),
                            TotalOriginal = CalcularPrecioSinPromocion(
                            i.IdPelicula != null ? i.Pelicula.IdTmdb : i.Series.IdTmdb,
                            i.IdPelicula != null ? "Película" : "Serie",
                            i.TipoOperacion,
                            i.Moneda) * i.Cantidad
                    })
                    .ToList();

                foreach (var item in items)
                {
                    if (item.PrecioOriginal > item.Precio)
                    {
                        item.TienePromocion = true;
                        item.PorcentajeDescuento = Math.Round((1 - (item.Precio / item.PrecioOriginal)) * 100, 2);
                        item.NombrePromocion = "Promoción por clima";
                    }
                }
            }

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Agregar(int idTmdb, string titulo, string tipoContenido, string tipoOperacion, string moneda)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            ResultadoPrecio resultadoPrecio = CalcularPrecio(idTmdb, tipoContenido, tipoOperacion, moneda);
            decimal precioFinal = resultadoPrecio.PrecioFinal;
            decimal precioBaseUsd = ObtenerPrecioBaseUsd(idTmdb, tipoContenido, tipoOperacion);

            Carrito carrito = db.Set<Carrito>()
                .FirstOrDefault(c => c.IdUsuario == idUsuario);

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    IdUsuario = idUsuario,
                    FechaCreacion = DateTime.Now
                };

                db.Set<Carrito>().Add(carrito);
                db.SaveChanges();
            }

            int? idPelicula = null;
            int? idSerie = null;

            if (tipoContenido == "Serie")
            {
                Series serie = db.Set<Series>()
                    .FirstOrDefault(s => s.IdTmdb == idTmdb);

                if (serie == null)
                {
                    serie = new Series
                    {
                        IdTmdb = idTmdb,
                        Titulo = titulo,
                        Precio = precioBaseUsd
                    };

                    db.Set<Series>().Add(serie);
                    db.SaveChanges();
                }

                idSerie = serie.IdSerie;
            }
            else
            {
                Pelicula pelicula = db.Set<Pelicula>()
                    .FirstOrDefault(p => p.IdTmdb == idTmdb);

                if (pelicula == null)
                {
                    pelicula = new Pelicula
                    {
                        IdTmdb = idTmdb,
                        Titulo = titulo,
                        Precio = precioBaseUsd
                    };

                    db.Set<Pelicula>().Add(pelicula);
                    db.SaveChanges();
                }

                idPelicula = pelicula.IdPelicula;
            }

            CarritoItem itemExistente = db.Set<CarritoItem>()
     .FirstOrDefault(i =>
         i.IdCarrito == carrito.IdCarrito &&
         (
             (idPelicula != null && i.IdPelicula == idPelicula) ||
             (idSerie != null && i.IdSerie == idSerie)
         )
     );

            if (itemExistente != null)
            {
                TempData["MensajeCarrito"] = "Este contenido ya está en el carrito. Si querés cambiar la moneda o el tipo de operación, eliminálo primero y agregalo nuevamente.";
                return VolverAtras();
            }

            CarritoItem item = new CarritoItem
            {
                IdCarrito = carrito.IdCarrito,
                IdPelicula = idPelicula,
                IdSerie = idSerie,
                Cantidad = 1,
                Precio = precioFinal,
                TipoOperacion = tipoOperacion,
                Moneda = moneda
            };

            db.Set<CarritoItem>().Add(item);
            db.SaveChanges();

            if (resultadoPrecio.TienePromocion)
            {
                TempData["MensajeCarrito"] = "El contenido fue agregado al carrito con la promoción '" + resultadoPrecio.NombrePromocion + "' aplicada.";
            }
            else
            {
                TempData["MensajeCarrito"] = "El contenido fue agregado al carrito.";
            }

            return VolverAtras();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int idCarritoItem)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            CarritoItem item = db.Set<CarritoItem>()
                .FirstOrDefault(i => i.IdCarritoItem == idCarritoItem && i.Carrito.IdUsuario == idUsuario);

            if (item != null)
            {
                db.Set<CarritoItem>().Remove(item);
                db.SaveChanges();

                TempData["MensajeCarrito"] = "El item fue eliminado del carrito.";
            }

            return RedirectToAction("Index", "Carrito");
        }
        private decimal ObtenerPrecioBaseUsd(int idTmdb, string tipoContenido, string tipoOperacion)
        {
            string tipoBD = tipoContenido == "Serie" ? "serie" : "pelicula";

            PrecioContenido precio = db.PrecioContenidoes.FirstOrDefault(p =>
                p.TmdbId == idTmdb &&
                p.TipoContenido == tipoBD &&
                p.Activo);

            if (precio != null)
            {
                return tipoOperacion == "Compra"
                    ? precio.PrecioCompra
                    : precio.PrecioAlquier;
            }

            if (tipoContenido == "Serie")
            {
                if (tipoOperacion == "Compra")
                {
                    return 9.99m;
                }

                return 4.99m;
            }

            if (tipoOperacion == "Compra")
            {
                return 5.99m;
            }

            return 2.99m;
        }

        private ResultadoPrecio CalcularPrecio(int idTmdb, string tipoContenido, string tipoOperacion, string moneda)
        {
            decimal precioUsd = ObtenerPrecioBaseUsd(idTmdb, tipoContenido, tipoOperacion);
            PromocionesClima promocion = ObtenerPromocionClimaAplicable();

            decimal precioUsdFinal = precioUsd;

            if (promocion != null)
            {
                decimal factorDescuento = 1 - (promocion.PorcentajeDesc / 100);
                precioUsdFinal = Math.Round(precioUsd * factorDescuento, 2);
            }

            decimal precioFinal = ConvertirDesdeUsd(precioUsdFinal, moneda);

            return new ResultadoPrecio
            {
                PrecioOriginal = ConvertirDesdeUsd(precioUsd, moneda),
                PrecioFinal = precioFinal,
                TienePromocion = promocion != null,
                NombrePromocion = promocion != null ? promocion.Nombre : "",
                PorcentajeDescuento = promocion != null ? promocion.PorcentajeDesc : 0
            };
        }

        private decimal CalcularPrecioSinPromocion(int idTmdb, string tipoContenido, string tipoOperacion, string moneda)
        {
            decimal precioUsd = ObtenerPrecioBaseUsd(idTmdb, tipoContenido, tipoOperacion);
            return ConvertirDesdeUsd(precioUsd, moneda);
        }

        private decimal ConvertirDesdeUsd(decimal precioUsd, string moneda)
        {
            if (moneda == "USD")
            {
                return precioUsd;
            }

            CotizacionSimple cotizacion = ObtenerCotizacion();
            if (cotizacion == null)
            {
                return precioUsd;
            }

            if (moneda == "UYU")
            {
                return Math.Round(precioUsd * cotizacion.Uyu, 2);
            }

            if (moneda == "EUR")
            {
                return Math.Round(precioUsd * cotizacion.Eur, 2);
            }

            return precioUsd;
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

        private CotizacionSimple ObtenerCotizacion()
        {
            string apiKey = ConfigurationManager.AppSettings["ExchangeRateApiKey"];

            var options = new RestClientOptions("https://v6.exchangerate-api.com/v6");
            var client = new RestClient(options);

            var request = new RestRequest("/" + apiKey + "/latest/USD", Method.Get);

            RestResponse response = client.Execute(request);

            if (response == null || !response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            {
                return null;
            }

            ExchangeRateResponse resultado = JsonConvert.DeserializeObject<ExchangeRateResponse>(response.Content);

            if (resultado == null ||
                resultado.ConversionRates == null ||
                !resultado.ConversionRates.ContainsKey("UYU") ||
                !resultado.ConversionRates.ContainsKey("EUR"))
            {
                return null;
            }

            return new CotizacionSimple
            {
                Uyu = Convert.ToDecimal(resultado.ConversionRates["UYU"]),
                Eur = Convert.ToDecimal(resultado.ConversionRates["EUR"])
            };
        }

        private ActionResult VolverAtras()
        {
            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }

            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    public class CotizacionSimple
    {
        public decimal Uyu { get; set; }
        public decimal Eur { get; set; }
    }

    public class ResultadoPrecio
    {
        public decimal PrecioOriginal { get; set; }
        public decimal PrecioFinal { get; set; }
        public bool TienePromocion { get; set; }
        public string NombrePromocion { get; set; }
        public decimal PorcentajeDescuento { get; set; }
    }

    public class ClimaActualSimple
    {
        public decimal Temperatura { get; set; }
        public string Categoria { get; set; }
        public string Descripcion { get; set; }
    }
}