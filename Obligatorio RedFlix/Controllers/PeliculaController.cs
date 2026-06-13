using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Obligatorio_RedFlix.Models;

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
        public ActionResult Index()
        {
            RestResponse response = HacerRequest("/3/movie/popular?language=es-ES&page=1");

            ListaPopulares populares = JsonConvert.DeserializeObject<ListaPopulares>(response.Content);

            List<Populares> peliculas = populares.Results
                .Where(pelicula => !string.IsNullOrEmpty(pelicula.Title))
                .ToList();

            return View(peliculas);
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
    }
}