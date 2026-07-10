using Newtonsoft.Json;
using Obligatorio_RedFlix.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    public class PeliculaController : Controller
    {
        private RedFlixDBEntities db = new RedFlixDBEntities();

        static string token = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJkMTFhMDAwMjVlNjZkYmIxZjQ0ZmZjYzVhZWY5Nzk0OCIsIm5iZiI6MTc3OTQ5OTI4NS40MTY5OTk4LCJzdWIiOiI2YTExMDExNTE3YzM2ZjNjMTBhZWI5NjUiLCJzY29wZXMiOlsiYXBpX3JlYWQiXSwidmVyc2lvbiI6MX0.zTH2CzrPgTnbjCUv2cgxEcrTKySFdp9-ts0EWwX_ICc";

        static RestResponse HacerRequest(string endpoint)
        {
            var options = new RestClientOptions("https://api.themoviedb.org");
            var client = new RestClient(options);

            var request = new RestRequest(endpoint, Method.Get);
            request.AddHeader("Authorization", "Bearer " + token);

            return client.Execute(request);
        }

        // GET: Pelicula
        public ActionResult Index(int pagina = 1)
        {
            int limitePaginasTmdb = 500;

            if (pagina < 1)
            {
                pagina = 1;
            }

            if (pagina > limitePaginasTmdb)
            {
                pagina = limitePaginasTmdb;
            }

            RestResponse response = HacerRequest("/3/discover/movie?language=es-ES&page=" + pagina + "&sort_by=popularity.desc");

            ListaPopulares resultado = JsonConvert.DeserializeObject<ListaPopulares>(response.Content);

            List<Populares> peliculas = new List<Populares>();

            if (resultado != null && resultado.Results != null)
            {
                peliculas = resultado.Results
                    .Where(pelicula => pelicula.Id.HasValue && !string.IsNullOrEmpty(pelicula.Title))
                    .ToList();
            }

            ViewBag.PaginaActual = pagina;

            ViewBag.TotalPaginas = resultado != null && resultado.TotalPages.HasValue
                ? Math.Min((int)resultado.TotalPages.Value, limitePaginasTmdb)
                : 1;

            return View(peliculas);
        }
        public ActionResult Series(int pagina = 1)
        {
            int limitePaginasTmdb = 500;

            if (pagina < 1)
            {
                pagina = 1;
            }

            if (pagina > limitePaginasTmdb)
            {
                pagina = limitePaginasTmdb;
            }

            RestResponse response = HacerRequest("/3/discover/tv?language=es-ES&page=" + pagina + "&sort_by=popularity.desc");

            ListaPopulares resultado = JsonConvert.DeserializeObject<ListaPopulares>(response.Content);

            List<Populares> series = new List<Populares>();

            if (resultado != null && resultado.Results != null)
            {
                series = resultado.Results
                    .Where(serie => serie.Id.HasValue && !string.IsNullOrEmpty(serie.Name))
                    .ToList();
            }

            ViewBag.PaginaActual = pagina;

            ViewBag.TotalPaginas = resultado != null && resultado.TotalPages.HasValue
                ? Math.Min((int)resultado.TotalPages.Value, limitePaginasTmdb)
                : 1;

            return View(series);
        }

        public ActionResult Buscar(string q)
        {
            BusquedaViewModel modelo = new BusquedaViewModel
            {
                Query = q
            };

            if (string.IsNullOrWhiteSpace(q))
            {
                return View(modelo);
            }

            string query = Uri.EscapeDataString(q.Trim());

            RestResponse responsePeliculas = HacerRequest("/3/search/movie?language=es-ES&page=1&query=" + query);
            RestResponse responseSeries = HacerRequest("/3/search/tv?language=es-ES&page=1&query=" + query);
            RestResponse responseActores = HacerRequest("/3/search/person?language=es-ES&page=1&query=" + query);

            if (responsePeliculas != null && responsePeliculas.IsSuccessful && !string.IsNullOrEmpty(responsePeliculas.Content))
            {
                ListaPopulares resultadoPeliculas = JsonConvert.DeserializeObject<ListaPopulares>(responsePeliculas.Content);

                if (resultadoPeliculas != null && resultadoPeliculas.Results != null)
                {
                    modelo.Peliculas = resultadoPeliculas.Results
                        .Where(p => p.Id.HasValue && !string.IsNullOrEmpty(p.Title))
                        .Take(18)
                        .ToList();
                }
            }

            if (responseSeries != null && responseSeries.IsSuccessful && !string.IsNullOrEmpty(responseSeries.Content))
            {
                ListaPopulares resultadoSeries = JsonConvert.DeserializeObject<ListaPopulares>(responseSeries.Content);

                if (resultadoSeries != null && resultadoSeries.Results != null)
                {
                    modelo.Series = resultadoSeries.Results
                        .Where(s => s.Id.HasValue && !string.IsNullOrEmpty(s.Name))
                        .Take(18)
                        .ToList();
                }
            }

            if (responseActores != null && responseActores.IsSuccessful && !string.IsNullOrEmpty(responseActores.Content))
            {
                ListaPersonas resultadoActores = JsonConvert.DeserializeObject<ListaPersonas>(responseActores.Content);

                if (resultadoActores != null && resultadoActores.Results != null)
                {
                    modelo.Actores = resultadoActores.Results
                    .Where(a => a.Id > 0 &&
                        !string.IsNullOrEmpty(a.Nombre) &&
                        !string.IsNullOrEmpty(a.FotoPath))
                    .GroupBy(a => a.Id)
                    .Select(g => g.First())
                    .Take(12)
                    .ToList();
                }
            }

            return View(modelo);
        }

        public ActionResult Actor(long? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Buscar");
            }

            RestResponse responseActor = HacerRequest("/3/person/" + id.Value + "?language=es-ES");
            RestResponse responseCreditos = HacerRequest("/3/person/" + id.Value + "/combined_credits?language=es-ES");

            ActorTmdb actor = null;

            if (responseActor != null && responseActor.IsSuccessful && !string.IsNullOrEmpty(responseActor.Content))
            {
                actor = JsonConvert.DeserializeObject<ActorTmdb>(responseActor.Content);
            }

            ListaCreditosActor creditos = null;

            if (responseCreditos != null && responseCreditos.IsSuccessful && !string.IsNullOrEmpty(responseCreditos.Content))
            {
                creditos = JsonConvert.DeserializeObject<ListaCreditosActor>(responseCreditos.Content);
            }

            ActorDetalleViewModel modelo = new ActorDetalleViewModel
            {
                Actor = actor
            };

            if (creditos != null && creditos.Cast != null)
            {
                modelo.Peliculas = creditos.Cast
                    .Where(c => c.Id.HasValue && c.MediaType == "movie" && !string.IsNullOrEmpty(c.Title))
                    .OrderByDescending(c => c.Popularity ?? 0)
                    .Take(18)
                    .ToList();

                modelo.Series = creditos.Cast
                .Where(c => c.Id.HasValue && c.MediaType == "tv" && !string.IsNullOrEmpty(c.Name))
                .GroupBy(c => c.Id.Value)
                .Select(g => g.First())
                .OrderByDescending(c => c.Popularity ?? 0)
                .Take(18)
                .ToList();
            }

            return View(modelo);
        }

        // GET: Pelicula/Detalle/ID
        public ActionResult Detalle(long? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            RestResponse responsePelicula = HacerRequest("/3/movie/" + id.Value + "?language=es-ES");

            Populares pelicula = JsonConvert.DeserializeObject<Populares>(responsePelicula.Content);

            RestResponse responseVideos = HacerRequest("/3/movie/" + id.Value + "/videos?language=es-ES");
            RestResponse responseActores = HacerRequest("/3/movie/" + id.Value + "/credits?language=es-ES");

            ListaVideos videos = JsonConvert.DeserializeObject<ListaVideos>(responseVideos.Content);
            ListaActores actores = null;

            if (responseActores != null && !string.IsNullOrEmpty(responseActores.Content))
            {
                actores = JsonConvert.DeserializeObject<ListaActores>(responseActores.Content);
            }

            VideoTmdb trailer = null;

            if (videos != null && videos.Results != null)
            {
                trailer = videos.Results
                    .FirstOrDefault(v => v.Site == "YouTube" && v.Tipo == "Trailer");
            }

            bool estaEnFavoritos = false;

            if (Session["UsuarioId"] != null)
            {
                int idUsuario = Convert.ToInt32(Session["UsuarioId"]);
                int idTmdb = Convert.ToInt32(id.Value);

                estaEnFavoritos = db.Set<Favorito>().Any(f =>
                    f.IdUsuario == idUsuario &&
                    f.Pelicula != null &&
                    f.Pelicula.IdTmdb == idTmdb
                );
            }

            ViewBag.EstaEnFavoritos = estaEnFavoritos;

            PeliculaDetalleViewModel modelo = new PeliculaDetalleViewModel
            {
                Pelicula = pelicula,
                Trailer = trailer,
                PrecioContenido = ArmarPrecioContenido(
                    Convert.ToInt32(id.Value),
                    pelicula.Title,
                    "pelicula"),
                Actores = actores != null && actores.Cast != null
                    ? actores.Cast.Take(8).ToList()
                    : new List<ActorTmdb>()
            };

            if (Session["UsuarioId"] != null)
            {
                int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

                ViewBag.ListasUsuario = db.ListasPersonalizadas
                    .Where(l => l.IdUsuario == idUsuario)
                    .OrderBy(l => l.Nombre)
                    .ToList();
            }

            ViewBag.GuardadoEnLista = false;

            return View(modelo);
        }
        public ActionResult DetalleSerie(long? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Series");
            }

            RestResponse responseSerie = HacerRequest("/3/tv/" + id.Value + "?language=es-ES");

            Populares serie = null;

            if (responseSerie != null && !string.IsNullOrEmpty(responseSerie.Content))
            {
                serie = JsonConvert.DeserializeObject<Populares>(responseSerie.Content);
            }

            RestResponse responseVideos = HacerRequest("/3/tv/" + id.Value + "/videos?language=es-ES");
            RestResponse responseActores = HacerRequest("/3/tv/" + id.Value + "/credits?language=es-ES");

            ListaVideos videos = null;
            ListaActores actores = null;

            if (responseVideos != null && !string.IsNullOrEmpty(responseVideos.Content))
            {
                videos = JsonConvert.DeserializeObject<ListaVideos>(responseVideos.Content);
            }

            if (responseActores != null && !string.IsNullOrEmpty(responseActores.Content))
            {
                actores = JsonConvert.DeserializeObject<ListaActores>(responseActores.Content);
            }

            VideoTmdb trailer = null;

            if (videos != null && videos.Results != null)
            {
                trailer = videos.Results
                    .FirstOrDefault(v => v.Site == "YouTube" && v.Tipo == "Trailer");
            }

            bool estaSerieEnFavoritos = false;

            if (Session["UsuarioId"] != null)
            {
                int idUsuario = Convert.ToInt32(Session["UsuarioId"]);
                int idTmdb = Convert.ToInt32(id.Value);

                estaSerieEnFavoritos = db.Set<Favorito>().Any(f =>
                    f.IdUsuario == idUsuario &&
                    f.Series != null &&
                    f.Series.IdTmdb == idTmdb
                );
            }

            ViewBag.EstaEnFavoritos = estaSerieEnFavoritos;

            PeliculaDetalleViewModel modelo = new PeliculaDetalleViewModel
            {
                Pelicula = serie,
                Trailer = trailer,
                PrecioContenido = ArmarPrecioContenido(
                    Convert.ToInt32(id.Value),
                    serie.Name,
                    "serie"),
                Actores = actores != null && actores.Cast != null
                    ? actores.Cast.Take(8).ToList()
                    : new List<ActorTmdb>()
            };

            if (Session["UsuarioId"] != null)
            {
                int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

                ViewBag.ListasUsuario = db.ListasPersonalizadas
                    .Where(l => l.IdUsuario == idUsuario)
                    .OrderBy(l => l.Nombre)
                    .ToList();
            }

            ViewBag.GuardadoEnLista = false;

            return View(modelo);
        }

        private PrecioContenidoViewModel ArmarPrecioContenido(int idTmdb, string titulo, string tipoContenido)
        {
            double precioCompraUsd = 5.99;
            double precioAlquilerUsd = 2.99;

            int? idPrecio = null;

            var precioBD = db.PrecioContenidoes.FirstOrDefault(p =>
                p.TmdbId == idTmdb &&
                p.TipoContenido.ToLower() == tipoContenido.ToLower() &&
                p.Activo == true);

            if (precioBD != null)
            {
                idPrecio = precioBD.IdPrecio;
                precioCompraUsd = Convert.ToDouble(precioBD.PrecioCompra);
                precioAlquilerUsd = Convert.ToDouble(precioBD.PrecioAlquier);
            }

            double cotizacionUyu = 40.25;
            double cotizacionEur = 0.88;

            
            decimal temperaturaActual = 8;
            string condicionClimaActual = "Frio";

            var promocion = db.PromocionesClimas
                .Where(p => p.Activa == true)
                .Where(p => p.TemperaturaMax >= temperaturaActual)
                .OrderByDescending(p => p.PorcentajeDesc)
                .FirstOrDefault();

            bool tienePromocion = false;
            string nombrePromocion = "";
            string condicionPromocion = "";
            double porcentajeDescuento = 0;

            if (promocion != null)
            {
                tienePromocion = true;
                nombrePromocion = promocion.Nombre;
                condicionPromocion = promocion.CondicionClima;
                porcentajeDescuento = Convert.ToDouble(promocion.PorcentajeDesc);
            }

            double factorDescuento = 1 - (porcentajeDescuento / 100);

            double precioCompraUsdFinal = precioCompraUsd;
            double precioAlquilerUsdFinal = precioAlquilerUsd;

            if (tienePromocion)
            {
                precioCompraUsdFinal = precioCompraUsd * factorDescuento;
                precioAlquilerUsdFinal = precioAlquilerUsd * factorDescuento;
            }

            return new PrecioContenidoViewModel
            {
                IdTmdb = idTmdb,
                Titulo = titulo,
                TipoContenido = tipoContenido,

                PrecioCompraUsd = precioCompraUsd,
                PrecioAlquilerUsd = precioAlquilerUsd,

                PrecioCompraUyu = precioCompraUsd * cotizacionUyu,
                PrecioAlquilerUyu = precioAlquilerUsd * cotizacionUyu,

                PrecioCompraEur = precioCompraUsd * cotizacionEur,
                PrecioAlquilerEur = precioAlquilerUsd * cotizacionEur,

                PrecioCompraUsdFinal = precioCompraUsdFinal,
                PrecioAlquilerUsdFinal = precioAlquilerUsdFinal,

                PrecioCompraUyuFinal = precioCompraUsdFinal * cotizacionUyu,
                PrecioAlquilerUyuFinal = precioAlquilerUsdFinal * cotizacionUyu,

                PrecioCompraEurFinal = precioCompraUsdFinal * cotizacionEur,
                PrecioAlquilerEurFinal = precioAlquilerUsdFinal * cotizacionEur,

                CotizacionUyu = cotizacionUyu,
                CotizacionEur = cotizacionEur,

                IdPrecio = idPrecio,

                TienePromocion = tienePromocion,
                NombrePromocion = nombrePromocion,
                CondicionPromocion = condicionPromocion,
                PorcentajeDescuento = porcentajeDescuento,

                Correcto = true
            };
        }
    }
}
