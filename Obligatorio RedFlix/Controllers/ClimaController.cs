using Newtonsoft.Json;
using Obligatorio_RedFlix.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    public class ClimaController : Controller
    {
        private RestResponse HacerRequestClima(string endpoint)
        {
            var options = new RestClientOptions("https://api.openweathermap.org/data/2.5");
            var client = new RestClient(options);

            var request = new RestRequest(endpoint, Method.Get);

            return client.Execute(request);
        }

        public ActionResult Widget()
        {
            string apiKey = ConfigurationManager.AppSettings["OpenWeatherApiKey"];

            string endpoint = $"/weather?q=Maldonado,UY&appid={apiKey}&units=metric&lang=es";

            RestResponse response = HacerRequestClima(endpoint);

            if (response == null || !response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            {
                return PartialView("_ClimaWidget", null);
            }

            ClimaWeather clima = JsonConvert.DeserializeObject<ClimaWeather>(response.Content);

            if (clima == null || clima.Main == null || clima.Weather == null || clima.Weather.Count == 0)
            {
                return PartialView("_ClimaWidget", null);
            }

            string estado = clima.Weather[0].Main.ToLower();
            string descripcion = clima.Weather[0].Description;

            string recomendacion;
            string categoria;

            if (estado.Contains("rain") || estado.Contains("drizzle") || estado.Contains("thunderstorm"))
            {
                recomendacion = "Día de lluvia: ideal para ver misterio, suspenso o drama.";
                categoria = "lluvia";
            }
            else if (clima.Main.Temp <= 12)
            {
                recomendacion = "Hace frío: recomendadas películas largas, comedias o romance.";
                categoria = "frio";
            }
            else if (clima.Main.Temp >= 25)
            {
                recomendacion = "Hace calor: mejor algo liviano, aventura o acción.";
                categoria = "calor";
            }
            else
            {
                recomendacion = "Clima tranquilo: buen momento para descubrir algo nuevo.";
                categoria = "templado";
            }

            ClimaViewModel vm = new ClimaViewModel
            {
                Ciudad = clima.Name,
                Temperatura = clima.Main.Temp,
                Humedad = clima.Main.Humidity,
                Descripcion = descripcion,
                Icono = clima.Weather[0].Icon,
                Recomendacion = recomendacion,
                CategoriaClima = categoria
            };

            
            return PartialView("~/Views/Clima/_ClimaWidget.cshtml", vm);
        }
    }
}
