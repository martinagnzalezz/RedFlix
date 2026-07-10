using Obligatorio_RedFlix.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;

namespace Obligatorio_RedFlix.Controllers
{
    public class BibliotecaController : Controller
    {
        private RedFlixDBEntities db = new RedFlixDBEntities();

        static string token = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJkMTFhMDAwMjVlNjZkYmIxZjQ0ZmZjYzVhZWY5Nzk0OCIsIm5iZiI6MTc3OTQ5OTI4NS40MTY5OTk4LCJzdWIiOiI2YTExMDExNTE3YzM2ZjNjMTBhZWI5NjUiLCJzY29wZXMiOlsiYXBpX3JlYWQiXSwidmVyc2lvbiI6MX0.zTH2CzrPgTnbjCUv2cgxEcrTKySFdp9-ts0EWwX_ICc";

        private RestResponse HacerRequest(string endpoint)
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

            var listas = db.ListasPersonalizadas
                .Include("ListaContenidoes")
                .Where(l => l.IdUsuario == idUsuario)
                .OrderByDescending(l => l.FechaCreacion)
                .ToList();

            return View(listas);
        }

        public ActionResult Crear()
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(string nombre, string descripcion)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["MensajeBiblioteca"] = "El nombre de la lista es obligatorio.";
                return RedirectToAction("Crear");
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            ListasPersonalizada lista = new ListasPersonalizada
            {
                Nombre = nombre,
                Descripcion = descripcion,
                FechaCreacion = DateTime.Now,
                IdUsuario = idUsuario
            };

            db.ListasPersonalizadas.Add(lista);
            db.SaveChanges();

            TempData["MensajeBiblioteca"] = "La lista fue creada correctamente.";

            return RedirectToAction("Index");
        }

        public ActionResult Detalle(int? id)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!id.HasValue)
            {
                return RedirectToAction("Index", "Biblioteca");
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            var lista = db.ListasPersonalizadas
                .Include("ListaContenidoes.Pelicula")
                .Include("ListaContenidoes.Series")
                .FirstOrDefault(l => l.IdLista == id.Value && l.IdUsuario == idUsuario);

            if (lista == null)
            {
                return RedirectToAction("Index", "Biblioteca");
            }

            Dictionary<string, string> imagenesContenido = new Dictionary<string, string>();

            foreach (var item in lista.ListaContenidoes)
            {
                if (item.Pelicula != null)
                {
                    string clave = "Película-" + item.Pelicula.IdTmdb;
                    imagenesContenido[clave] = ObtenerImagenTmdb(item.Pelicula.IdTmdb, "Película");
                }
                else if (item.Series != null)
                {
                    string clave = "Serie-" + item.Series.IdTmdb;
                    imagenesContenido[clave] = ObtenerImagenTmdb(item.Series.IdTmdb, "Serie");
                }
            }

            ViewBag.ImagenesContenido = imagenesContenido;

            return View(lista);
        }

        public ActionResult Eliminar(int id)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            var lista = db.ListasPersonalizadas
                .Include("ListaContenidoes")
                .FirstOrDefault(l => l.IdLista == id && l.IdUsuario == idUsuario);

            if (lista != null)
            {
                db.ListaContenidoes.RemoveRange(lista.ListaContenidoes);
                db.ListasPersonalizadas.Remove(lista);
                db.SaveChanges();

                TempData["MensajeBiblioteca"] = "La lista fue eliminada.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarContenido(int idLista, int idTmdb, string titulo, string tipoContenido)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            ListasPersonalizada lista = db.ListasPersonalizadas
                .FirstOrDefault(l => l.IdLista == idLista && l.IdUsuario == idUsuario);

            if (lista == null)
            {
                TempData["MensajeBiblioteca"] = "No se encontró la lista seleccionada.";
                return VolverAtras();
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
                        Precio = 0
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
                        Precio = 0
                    };

                    db.Set<Pelicula>().Add(pelicula);
                    db.SaveChanges();
                }

                idPelicula = pelicula.IdPelicula;
            }

            bool yaExiste = db.ListaContenidoes.Any(lc =>
                lc.IdLista == idLista &&
                (
                    (idPelicula != null && lc.IdPelicula == idPelicula) ||
                    (idSerie != null && lc.IdSerie == idSerie)
                )
            );

            if (yaExiste)
            {
                TempData["MensajeBiblioteca"] = "Este contenido ya está en la lista seleccionada.";
                return VolverAtras();
            }

            ListaContenido contenido = new ListaContenido
            {
                IdLista = idLista,
                IdPelicula = idPelicula,
                IdSerie = idSerie,
                FechaAgregado = DateTime.Now
            };

            db.ListaContenidoes.Add(contenido);
            db.SaveChanges();

            TempData["MensajeBiblioteca"] = "El contenido fue agregado a la lista correctamente.";

            return VolverAtras();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarContenido(int idListaContenido, int idLista)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            var lista = db.ListasPersonalizadas
                .FirstOrDefault(l => l.IdLista == idLista && l.IdUsuario == idUsuario);

            if (lista == null)
            {
                return RedirectToAction("Index");
            }

            var contenido = db.ListaContenidoes
                .FirstOrDefault(c => c.IdListaContenido == idListaContenido && c.IdLista == idLista);

            if (contenido == null)
            {
                TempData["MensajeBiblioteca"] = "No se encontró el contenido en la lista.";
                return RedirectToAction("Detalle", new { id = idLista });
            }

            db.ListaContenidoes.Remove(contenido);
            db.SaveChanges();

            TempData["MensajeBiblioteca"] = "El contenido fue eliminado de la lista.";

            return RedirectToAction("Detalle", new { id = idLista });
        }
        private ActionResult VolverAtras()
        {
            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }

            return RedirectToAction("Index", "Biblioteca");
        }

        private string ObtenerImagenTmdb(int idTmdb, string tipoContenido)
        {
            string endpoint = tipoContenido == "Serie"
                ? "/3/tv/" + idTmdb + "?language=es-ES"
                : "/3/movie/" + idTmdb + "?language=es-ES";

            RestResponse response = HacerRequest(endpoint);

            if (response == null || string.IsNullOrEmpty(response.Content))
            {
                return "https://placehold.co/500x750/151515/D4AF37?text=Sin+imagen";
            }

            JObject datos = JObject.Parse(response.Content);

            string posterPath = datos["poster_path"] != null
                ? datos["poster_path"].ToString()
                : "";

            if (string.IsNullOrWhiteSpace(posterPath))
            {
                return "https://placehold.co/500x750/151515/D4AF37?text=Sin+imagen";
            }

            return "https://image.tmdb.org/t/p/w500" + posterPath;
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
}