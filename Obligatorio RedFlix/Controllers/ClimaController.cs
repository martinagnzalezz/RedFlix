using Newtonsoft.Json;
using Obligatorio_RedFlix.Models;
using RestSharp;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System;

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

        private RestResponse HacerRequestTmdb(string endpoint)
        {
            string token = ConfigurationManager.AppSettings["TmdbToken"];

            var options = new RestClientOptions("https://api.themoviedb.org/3");
            var client = new RestClient(options);

            var request = new RestRequest(endpoint, Method.Get);

            request.AddHeader("Authorization", "Bearer " + token);

            return client.Execute(request);
        }

        public ActionResult Widget()
        {
            ClimaViewModel vm = ObtenerClima();

            return PartialView("~/Views/Clima/_ClimaWidget.cshtml", vm);
        }

        public ActionResult Index()
        {
            ClimaViewModel vm = ObtenerClima();

            if (vm != null)
            {
                vm.PeliculasRecomendadas = ObtenerContenidoPorClima(vm.CategoriaClima);
                vm.PronosticoDias = ObtenerPronosticoDias();
            }

            return View(vm);
        }

        private ClimaViewModel ObtenerClima()
        {
            string apiKey = ConfigurationManager.AppSettings["OpenWeatherApiKey"];

            string endpoint = $"/weather?lat=-34.9&lon=-54.95&appid={apiKey}&units=metric&lang=es";

            RestResponse response = HacerRequestClima(endpoint);

            if (response == null || !response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            {
                return null;
            }

            ClimaWeather clima = JsonConvert.DeserializeObject<ClimaWeather>(response.Content);

            if (clima == null || clima.Main == null || clima.Weather == null || clima.Weather.Count == 0)
            {
                return null;
            }

            string estado = clima.Weather[0].Main.ToLower();
            string descripcion = clima.Weather[0].Description;

            string recomendacion;
            string categoria;
            string emoji;
            string promocion;

            if (estado.Contains("rain") || estado.Contains("drizzle") || estado.Contains("thunderstorm"))
            {
                categoria = "lluvia";
                emoji = "🌧️";
                recomendacion = "Día de lluvia: ideal para ver misterio, suspenso o drama.";
                promocion = "Promo lluvia: 20% OFF en alquileres de suspenso y drama.";
            }
            else if (estado.Contains("cloud"))
            {
                categoria = "nublado";
                emoji = "☁️";
                recomendacion = "Día nublado: ideal para misterio, ciencia ficción o thriller.";
                promocion = "Tarde gris: 15% OFF en películas de misterio.";
            }
            else if (clima.Main.Temp <= 12)
            {
                categoria = "frio";
                emoji = "❄️";
                recomendacion = "Hace frío: recomendadas películas largas, comedias o romance.";
                promocion = "Noche de manta: alquilá 2 películas y pagá 1.";
            }
            else if (clima.Main.Temp >= 25)
            {
                categoria = "calor";
                emoji = "☀️";
                recomendacion = "Hace calor: mejor algo liviano, aventura o acción.";
                promocion = "Promo calor: 10% OFF en acción y aventura.";
            }
            else
            {
                categoria = "templado";
                emoji = "🌤️";
                recomendacion = "Clima tranquilo: buen momento para descubrir algo nuevo.";
                promocion = "Promo recomendada: 10% OFF en películas populares.";
            }

            ClimaViewModel vm = new ClimaViewModel
            {
                Ciudad = clima.Name,
                Temperatura = clima.Main.Temp,
                Humedad = clima.Main.Humidity,
                Descripcion = descripcion,
                Icono = clima.Weather[0].Icon,
                Recomendacion = recomendacion,
                CategoriaClima = categoria,
                Emoji = emoji,
                Promocion = promocion,
                PeliculasRecomendadas = new List<Populares>()
            };

            return vm;
        }

        private List<Populares> ObtenerContenidoPorClima(string categoriaClima)
        {
            string generosPeliculas;
            string generosSeries;

            switch (categoriaClima)
            {
                case "lluvia":
                    generosPeliculas = "18,53,9648"; // Drama, Thriller, Misterio
                    generosSeries = "18,9648,80";    // Drama, Misterio, Crimen
                    break;

                case "frio":
                    generosPeliculas = "10749,35,18"; // Romance, Comedia, Drama
                    generosSeries = "18,35,10751";    // Drama, Comedia, Familia
                    break;

                case "calor":
                    generosPeliculas = "28,12,35";    // Acción, Aventura, Comedia
                    generosSeries = "10759,35,10751"; // Acción/Aventura, Comedia, Familia
                    break;

                case "nublado":
                    generosPeliculas = "9648,878,53"; // Misterio, Ciencia ficción, Thriller
                    generosSeries = "9648,10765,80";  // Misterio, Sci-Fi/Fantasía, Crimen
                    break;

                default:
                    generosPeliculas = "28,35,18";    // Acción, Comedia, Drama
                    generosSeries = "18,35,10759";    // Drama, Comedia, Acción/Aventura
                    break;
            }

            List<Populares> contenidos = new List<Populares>();

            string endpointPeliculas = "/discover/movie?language=es-ES&page=1&sort_by=popularity.desc&with_genres=" + generosPeliculas;

            RestResponse responsePeliculas = HacerRequestTmdb(endpointPeliculas);

            if (responsePeliculas != null && responsePeliculas.IsSuccessful && !string.IsNullOrEmpty(responsePeliculas.Content))
            {
                ListaPopulares resultadoPeliculas = JsonConvert.DeserializeObject<ListaPopulares>(responsePeliculas.Content);

                if (resultadoPeliculas != null && resultadoPeliculas.Results != null)
                {
                    foreach (var pelicula in resultadoPeliculas.Results.Take(4))
                    {
                        pelicula.TipoContenido = "Película";
                        contenidos.Add(pelicula);
                    }
                }
            }

            string endpointSeries = "/discover/tv?language=es-ES&page=1&sort_by=popularity.desc&with_genres=" + generosSeries;

            RestResponse responseSeries = HacerRequestTmdb(endpointSeries);

            if (responseSeries != null && responseSeries.IsSuccessful && !string.IsNullOrEmpty(responseSeries.Content))
            {
                ListaPopulares resultadoSeries = JsonConvert.DeserializeObject<ListaPopulares>(responseSeries.Content);

                if (resultadoSeries != null && resultadoSeries.Results != null)
                {
                    foreach (var serie in resultadoSeries.Results.Take(4))
                    {
                        serie.TipoContenido = "Serie";
                        contenidos.Add(serie);
                    }
                }
            }

            return contenidos
                .OrderByDescending(c => c.Popularity ?? 0)
                .Take(8)
                .ToList();
        }
        private List<PronosticoDiaViewModel> ObtenerPronosticoDias()
        {
            string apiKey = ConfigurationManager.AppSettings["OpenWeatherApiKey"];

            string endpoint = $"/forecast?lat=-34.9&lon=-54.95&appid={apiKey}&units=metric&lang=es";

            RestResponse response = HacerRequestClima(endpoint);

            if (response == null || !response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            {
                return new List<PronosticoDiaViewModel>();
            }

            ClimaForecast forecast = JsonConvert.DeserializeObject<ClimaForecast>(response.Content);

            if (forecast == null || forecast.List == null)
            {
                return new List<PronosticoDiaViewModel>();
            }

            var dias = forecast.List
                .GroupBy(x => DateTime.Parse(x.FechaTexto).Date)
                .Take(5)
                .Select(grupo =>
                {
                    var elegido = grupo.FirstOrDefault(x => x.FechaTexto.Contains("12:00:00")) ?? grupo.First();

                    string estado = elegido.Weather[0].Main.ToLower();

                    string categoria;
                    string emoji;

                    if (estado.Contains("rain") || estado.Contains("drizzle") || estado.Contains("thunderstorm"))
                    {
                        categoria = "lluvia";
                        emoji = "🌧️";
                    }
                    else if (estado.Contains("cloud"))
                    {
                        categoria = "nublado";
                        emoji = "☁️";
                    }
                    else if (elegido.Main.Temp <= 12)
                    {
                        categoria = "frio";
                        emoji = "❄️";
                    }
                    else if (elegido.Main.Temp >= 25)
                    {
                        categoria = "calor";
                        emoji = "☀️";
                    }
                    else
                    {
                        categoria = "templado";
                        emoji = "🌤️";
                    }

                    DateTime fecha = DateTime.Parse(elegido.FechaTexto);

                    return new PronosticoDiaViewModel
                    {
                        Dia = fecha.ToString("dddd dd/MM", new System.Globalization.CultureInfo("es-UY")),
                        Temperatura = elegido.Main.Temp,
                        Descripcion = elegido.Weather[0].Description,
                        Icono = elegido.Weather[0].Icon,
                        CategoriaClima = categoria,
                        Emoji = emoji
                    };
                })
                .ToList();

            return dias;
        }
    }
}