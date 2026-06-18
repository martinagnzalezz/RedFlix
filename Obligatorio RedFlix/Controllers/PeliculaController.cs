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
                    .Where(pelicula => !string.IsNullOrEmpty(pelicula.Title))
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
                    .Where(serie => !string.IsNullOrEmpty(serie.Name))
                    .ToList();
            }

            ViewBag.PaginaActual = pagina;

            ViewBag.TotalPaginas = resultado != null && resultado.TotalPages.HasValue
                ? Math.Min((int)resultado.TotalPages.Value, limitePaginasTmdb)
                : 1;

            return View(series);
        }
        // GET: Pelicula/Detalle/ID
        public ActionResult Detalle(long id)
        {
            RestResponse responsePelicula = HacerRequest("/3/movie/" + id + "?language=es-ES");

            Populares pelicula = JsonConvert.DeserializeObject<Populares>(responsePelicula.Content);

            RestResponse responseVideos = HacerRequest("/3/movie/" + id + "/videos?language=es-ES");

            ListaVideos videos = JsonConvert.DeserializeObject<ListaVideos>(responseVideos.Content);

            VideoTmdb trailer = null;

            if (videos != null && videos.Results != null)
            {
                trailer = videos.Results
                    .FirstOrDefault(v => v.Site == "YouTube" && v.Tipo == "Trailer");
            }

            PeliculaDetalleViewModel modelo = new PeliculaDetalleViewModel
            {
                Pelicula = pelicula,
                Trailer = trailer
            };

            return View(modelo);
        }
        public ActionResult DetalleSerie(long id)
        {
            RestResponse responseSerie = HacerRequest("/3/tv/" + id + "?language=es-ES");

            Populares serie = null;

            if (responseSerie != null && !string.IsNullOrEmpty(responseSerie.Content))
            {
                serie = JsonConvert.DeserializeObject<Populares>(responseSerie.Content);
            }

            RestResponse responseVideos = HacerRequest("/3/tv/" + id + "/videos?language=es-ES");

            ListaVideos videos = null;

            if (responseVideos != null && !string.IsNullOrEmpty(responseVideos.Content))
            {
                videos = JsonConvert.DeserializeObject<ListaVideos>(responseVideos.Content);
            }

            VideoTmdb trailer = null;

            if (videos != null && videos.Results != null)
            {
                trailer = videos.Results
                    .FirstOrDefault(v => v.Site == "YouTube" && v.Tipo == "Trailer");
            }

            PeliculaDetalleViewModel modelo = new PeliculaDetalleViewModel
            {
                Pelicula = serie,
                Trailer = trailer
            };

            return View(modelo);
        }
    }
}