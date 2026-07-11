using Newtonsoft.Json;
using Obligatorio_RedFlix.Models;
using RestSharp;
using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Web; 
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    public class HomeController : Controller
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
        public ActionResult Index()
        {
            if (Session["RolNombre"] != null && Session["RolNombre"].ToString() == "Promociones")
            {
                return RedirectToAction("Index", "Promociones");
            }

            List<Populares> peliculas = ObtenerPopulares("/3/discover/movie?language=es-ES&page=1&sort_by=popularity.desc", true, 20);
            List<Populares> series = ObtenerPopulares("/3/discover/tv?language=es-ES&page=1&sort_by=popularity.desc", false, 20);

            ViewBag.Series = series;

            return View(peliculas);
        }

        private List<Populares> ObtenerPopulares(string endpoint, bool sonPeliculas, int cantidad)
        {
            RestResponse response = HacerRequest(endpoint);

            if (response == null || !response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            {
                return new List<Populares>();
            }

            ListaPopulares populares = JsonConvert.DeserializeObject<ListaPopulares>(response.Content);

            if (populares == null || populares.Results == null)
            {
                return new List<Populares>();
            }

            return populares.Results
                .Where(p => sonPeliculas
                    ? !string.IsNullOrEmpty(p.Title)
                    : !string.IsNullOrEmpty(p.Name))
                .Take(cantidad)
                .ToList();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page."; return View();
        }
    }
}