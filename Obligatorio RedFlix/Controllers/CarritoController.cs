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
                        Total = i.Precio * i.Cantidad
                    })
                    .ToList();
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

            decimal precioFinal = CalcularPrecio(tipoContenido, tipoOperacion, moneda);
            decimal precioBaseUsd = ObtenerPrecioBaseUsd(tipoContenido, tipoOperacion);

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
                    i.IdPelicula == idPelicula &&
                    i.IdSerie == idSerie &&
                    i.TipoOperacion == tipoOperacion &&
                    i.Moneda == moneda);

            if (itemExistente != null)
            {
                itemExistente.Cantidad = itemExistente.Cantidad + 1;
            }
            else
            {
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
            }

            db.SaveChanges();

            TempData["MensajeCarrito"] = "El contenido fue agregado al carrito.";

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
        private decimal ObtenerPrecioBaseUsd(string tipoContenido, string tipoOperacion)
        {
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

        private decimal CalcularPrecio(string tipoContenido, string tipoOperacion, string moneda)
        {
            decimal precioUsd = ObtenerPrecioBaseUsd(tipoContenido, tipoOperacion);

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
                return precioUsd * cotizacion.Uyu;
            }

            if (moneda == "EUR")
            {
                return precioUsd * cotizacion.Eur;
            }

            return precioUsd;
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
}