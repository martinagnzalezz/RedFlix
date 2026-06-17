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
            RestResponse response = HacerRequest("/3/discover/movie?language=es-ES&page=1&sort_by=popularity.desc");

            List<Populares> peliculas = new List<Populares>();

            if (response != null && !string.IsNullOrEmpty(response.Content))
            {
                ListaPopulares populares =
                    JsonConvert.DeserializeObject<ListaPopulares>(response.Content);

                if (populares != null && populares.Results != null)
                {
                    peliculas = populares.Results
                        .Where(p => !string.IsNullOrEmpty(p.Title))
                        .Take(8)
                        .ToList();
                }
            }

            return View(peliculas);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page."; return View();
        }
    }
}