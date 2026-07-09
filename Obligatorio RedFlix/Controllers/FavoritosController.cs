using Newtonsoft.Json;
using Obligatorio_RedFlix.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace Obligatorio_RedFlix.Controllers
{
    public class FavoritosController : Controller
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

        public ActionResult Index()
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            List<Favorito> favoritos = db.Favoritos
                .Include("Pelicula")
                .Include("Series")
                .Where(f => f.IdUsuario == idUsuario)
                .OrderByDescending(f => f.FechaAgregado)
                .ToList();

            List<FavoritoVistaViewModel> favoritosVista = new List<FavoritoVistaViewModel>();

            foreach (Favorito favorito in favoritos)
            {
                if (favorito.Pelicula != null)
                {
                    RestResponse response = HacerRequest("/3/movie/" + favorito.Pelicula.IdTmdb + "?language=es-ES");

                    Populares detalle = null;

                    if (response != null && !string.IsNullOrEmpty(response.Content))
                    {
                        detalle = JsonConvert.DeserializeObject<Populares>(response.Content);
                    }

                    FavoritoVistaViewModel item = new FavoritoVistaViewModel
                    {
                        IdFavorito = favorito.IdFavorito,
                        IdTmdb = favorito.Pelicula.IdTmdb,
                        Titulo = favorito.Pelicula.Titulo,
                        Tipo = "Película",
                        ImagenUrl = detalle != null ? detalle.ImagenUrl : "/Content/img/no-image.png",
                        Overview = detalle != null ? detalle.Overview : "",
                        VoteAverage = detalle != null ? detalle.VoteAverage : null,
                        FechaAgregado = favorito.FechaAgregado
                    };

                    favoritosVista.Add(item);
                }
                else if (favorito.Series != null)
                {
                    RestResponse response = HacerRequest("/3/tv/" + favorito.Series.IdTmdb + "?language=es-ES");

                    Populares detalle = null;

                    if (response != null && !string.IsNullOrEmpty(response.Content))
                    {
                        detalle = JsonConvert.DeserializeObject<Populares>(response.Content);
                    }

                    FavoritoVistaViewModel item = new FavoritoVistaViewModel
                    {
                        IdFavorito = favorito.IdFavorito,
                        IdTmdb = favorito.Series.IdTmdb,
                        Titulo = favorito.Series.Titulo,
                        Tipo = "Serie",
                        ImagenUrl = detalle != null ? detalle.ImagenUrl : "/Content/img/no-image.png",
                        Overview = detalle != null ? detalle.Overview : "",
                        VoteAverage = detalle != null ? detalle.VoteAverage : null,
                        FechaAgregado = favorito.FechaAgregado
                    };

                    favoritosVista.Add(item);
                }
            }

            return View(favoritosVista);
        }

        public ActionResult AgregarPelicula(int idTmdb, string titulo)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            Pelicula pelicula = db.Set<Pelicula>()
                .FirstOrDefault(p => p.IdTmdb == idTmdb);

            if (pelicula == null)
            {
                pelicula = new Pelicula
                {
                    IdTmdb = idTmdb,
                    Titulo = titulo,
                    Precio = 0
                };

                db.Set<Pelicula>().Add(pelicula);
                db.SaveChanges();
            }

            Favorito favoritoExistente = db.Set<Favorito>()
                .FirstOrDefault(f =>
                    f.IdUsuario == idUsuario &&
                    f.IdPelicula == pelicula.IdPelicula);

            if (favoritoExistente != null)
            {
                db.Set<Favorito>().Remove(favoritoExistente);
                db.SaveChanges();

                TempData["MensajeFavorito"] = "La película fue quitada de favoritos.";
            }
            else
            {
                Favorito favorito = new Favorito
                {
                    IdUsuario = idUsuario,
                    IdPelicula = pelicula.IdPelicula,
                    IdSerie = null,
                    FechaAgregado = DateTime.Now
                };

                db.Set<Favorito>().Add(favorito);
                db.SaveChanges();

                string genero = ObtenerGeneroPrincipalPelicula(idTmdb);
                AgregarPeliculaABibliotecaAutomatica(idUsuario, pelicula, genero);

                TempData["MensajeFavorito"] = "La película fue agregada a favoritos y también a tu biblioteca.";
            }

            return VolverAtras();
        }

        public ActionResult AgregarSerie(int idTmdb, string titulo)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            Series serie = db.Set<Series>()
                .FirstOrDefault(s => s.IdTmdb == idTmdb);

            if (serie == null)
            {
                serie = new Series
                {
                    IdTmdb = idTmdb,
                    Titulo = titulo,
                    Precio = 0
                };

                db.Set<Series>().Add(serie);
                db.SaveChanges();
            }

            Favorito favoritoExistente = db.Set<Favorito>()
                .FirstOrDefault(f =>
                    f.IdUsuario == idUsuario &&
                    f.IdSerie == serie.IdSerie);

            if (favoritoExistente != null)
            {
                db.Set<Favorito>().Remove(favoritoExistente);
                db.SaveChanges();

                TempData["MensajeFavorito"] = "La serie fue quitada de favoritos.";
            }
            else
            {
                Favorito favorito = new Favorito
                {
                    IdUsuario = idUsuario,
                    IdPelicula = null,
                    IdSerie = serie.IdSerie,
                    FechaAgregado = DateTime.Now
                };

                db.Set<Favorito>().Add(favorito);
                db.SaveChanges();

                string genero = ObtenerGeneroPrincipalSerie(idTmdb);
                AgregarSerieABibliotecaAutomatica(idUsuario, serie, genero);

                TempData["MensajeFavorito"] = "La serie fue agregada a favoritos y también a tu biblioteca.";
            }

            return VolverAtras();
        }

        public ActionResult Eliminar(int id)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            Favorito favorito = db.Favoritos
                .FirstOrDefault(f => f.IdFavorito == id && f.IdUsuario == idUsuario);

            if (favorito != null)
            {
                db.Favoritos.Remove(favorito);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        private string ObtenerGeneroPrincipalPelicula(int idTmdb)
        {
            RestResponse response = HacerRequest("/3/movie/" + idTmdb + "?language=es-ES");

            if (response == null || string.IsNullOrEmpty(response.Content))
            {
                return "Sin categoría";
            }

            JObject datos = JObject.Parse(response.Content);
            JArray generos = datos["genres"] as JArray;

            if (generos != null && generos.Count > 0)
            {
                return generos[0]["name"].ToString();
            }

            return "Sin categoría";
        }

        private string ObtenerGeneroPrincipalSerie(int idTmdb)
        {
            RestResponse response = HacerRequest("/3/tv/" + idTmdb + "?language=es-ES");

            if (response == null || string.IsNullOrEmpty(response.Content))
            {
                return "Sin categoría";
            }

            JObject datos = JObject.Parse(response.Content);
            JArray generos = datos["genres"] as JArray;

            if (generos != null && generos.Count > 0)
            {
                return generos[0]["name"].ToString();
            }

            return "Sin categoría";
        }

        private void AgregarPeliculaABibliotecaAutomatica(int idUsuario, Pelicula pelicula, string genero)
        {
            string nombreLista = "Favoritos - " + genero;

            ListasPersonalizada lista = db.ListasPersonalizadas
                .FirstOrDefault(l => l.IdUsuario == idUsuario && l.Nombre == nombreLista);

            if (lista == null)
            {
                lista = new ListasPersonalizada
                {
                    IdUsuario = idUsuario,
                    Nombre = nombreLista,
                    Descripcion = "Lista creada automáticamente según el género de tus favoritos.",
                    FechaCreacion = DateTime.Now
                };

                db.ListasPersonalizadas.Add(lista);
                db.SaveChanges();
            }

            bool yaExiste = db.ListaContenidoes.Any(lc =>
                lc.IdLista == lista.IdLista &&
                lc.IdPelicula == pelicula.IdPelicula);

            if (!yaExiste)
            {
                ListaContenido contenido = new ListaContenido
                {
                    IdLista = lista.IdLista,
                    IdPelicula = pelicula.IdPelicula,
                    IdSerie = null,
                    FechaAgregado = DateTime.Now
                };

                db.ListaContenidoes.Add(contenido);
                db.SaveChanges();
            }
        }

        private void AgregarSerieABibliotecaAutomatica(int idUsuario, Series serie, string genero)
        {
            string nombreLista = "Favoritos - " + genero;

            ListasPersonalizada lista = db.ListasPersonalizadas
                .FirstOrDefault(l => l.IdUsuario == idUsuario && l.Nombre == nombreLista);

            if (lista == null)
            {
                lista = new ListasPersonalizada
                {
                    IdUsuario = idUsuario,
                    Nombre = nombreLista,
                    Descripcion = "Lista creada automáticamente según el género de tus favoritos.",
                    FechaCreacion = DateTime.Now
                };

                db.ListasPersonalizadas.Add(lista);
                db.SaveChanges();
            }

            bool yaExiste = db.ListaContenidoes.Any(lc =>
                lc.IdLista == lista.IdLista &&
                lc.IdSerie == serie.IdSerie);

            if (!yaExiste)
            {
                ListaContenido contenido = new ListaContenido
                {
                    IdLista = lista.IdLista,
                    IdPelicula = null,
                    IdSerie = serie.IdSerie,
                    FechaAgregado = DateTime.Now
                };

                db.ListaContenidoes.Add(contenido);
                db.SaveChanges();
            }
        }
        private ActionResult VolverAtras()
        {
            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }

            return RedirectToAction("Index", "Home");
        }
    }
}