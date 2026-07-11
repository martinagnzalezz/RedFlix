using Obligatorio_RedFlix.Filters;
using Obligatorio_RedFlix.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    [AutorizarRol("Administrador", "Gestor de contenido")]
    public class PreciosController : Controller
    {
        private RedFlixDBEntities db = new RedFlixDBEntities();

        private const string TmdbToken = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJkMTFhMDAwMjVlNjZkYmIxZjQ0ZmZjYzVhZWY5Nzk0OCIsIm5iZiI6MTc3OTQ5OTI4NS40MTY5OTk4LCJzdWIiOiI2YTExMDExNTE3YzM2ZjNjMTBhZWI5NjUiLCJzY29wZXMiOlsiYXBpX3JlYWQiXSwidmVyc2lvbiI6MX0.zTH2CzrPgTnbjCUv2cgxEcrTKySFdp9-ts0EWwX_ICc";

        public ActionResult Index(string q)
        {
            var precios = db.PrecioContenidoes.OrderBy(p => p.Titulo).ToList();

            ViewBag.Busqueda = q;
            ViewBag.ResultadosTmdb = new List<Populares>();
            ViewBag.PreciosPorContenido = precios
                .GroupBy(p => p.TipoContenido.ToLower() + ":" + p.TmdbId)
                .ToDictionary(g => g.Key, g => g.First().IdPrecio);

            if (!string.IsNullOrWhiteSpace(q))
            {
                try
                {
                    var client = new RestClient(new RestClientOptions("https://api.themoviedb.org"));
                    var request = new RestRequest("/3/search/multi", Method.Get);
                    request.AddHeader("Authorization", "Bearer " + TmdbToken);
                    request.AddQueryParameter("query", q.Trim());
                    request.AddQueryParameter("language", "es-ES");
                    request.AddQueryParameter("include_adult", "false");

                    RestResponse response = client.Execute(request);
                    ListaPopulares resultado = JsonConvert.DeserializeObject<ListaPopulares>(response.Content);

                    ViewBag.ResultadosTmdb = resultado != null && resultado.Results != null
                        ? resultado.Results
                            .Where(r => r.Id.HasValue &&
                                (r.MediaType == "movie" || r.MediaType == "tv") &&
                                !string.IsNullOrWhiteSpace(r.TituloMostrar))
                            .Take(12)
                            .ToList()
                        : new List<Populares>();
                }
                catch (Exception)
                {
                    ViewBag.ErrorTmdb = "No se pudo consultar TMDB. Intentá nuevamente.";
                }
            }

            return View(precios);
        }


        [HttpGet]
        public ActionResult Create(int? tmdbId, string titulo, string tipoContenido)
        {
            var precio = new PrecioContenido
            {
                TmdbId = tmdbId ?? 0,
                Titulo = titulo,
                TipoContenido = string.IsNullOrEmpty(tipoContenido) ? "pelicula" : tipoContenido,
                DiasAlquiler = 7
            };

            return View(precio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PrecioContenido precio)
        {
            bool yaExiste = db.PrecioContenidoes.Any(p =>
                p.TmdbId == precio.TmdbId && p.TipoContenido == precio.TipoContenido);

            if (yaExiste)
            {
                ModelState.AddModelError("TmdbId", "Ya existe un precio cargado para ese contenido.");
            }

            if (ModelState.IsValid)
            {
                precio.Activo = true;
                db.PrecioContenidoes.Add(precio);

                try
                {
                    db.SaveChanges();
                    TempData["Success"] = "Precio creado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var error in ex.EntityValidationErrors)
                    {
                        foreach (var subError in error.ValidationErrors)
                        {
                            ModelState.AddModelError("", $"Campo '{subError.PropertyName}': {subError.ErrorMessage}");
                        }
                    }

                }

            }

            return View(precio);
        }

        public ActionResult Edit(int id)
        {
            PrecioContenido precio = db.PrecioContenidoes.Find(id);

            if (precio == null)
                return HttpNotFound();

            return View(precio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PrecioContenido precio)
        {
            PrecioContenido precioBD = db.PrecioContenidoes.Find(precio.IdPrecio);

            if (precioBD == null)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                precioBD.Titulo = precio.Titulo;
                precioBD.PrecioCompra = precio.PrecioCompra;
                precioBD.PrecioAlquier = precio.PrecioAlquier;
                precioBD.DiasAlquiler = precio.DiasAlquiler;
                precioBD.Activo = precio.Activo;

                db.SaveChanges();

                TempData["Success"] = "Precio actualizado correctamente.";
                return RedirectToAction("Index");
            }

            return View(precio);
        }

        public ActionResult Delete(int id)
        {
            PrecioContenido precio = db.PrecioContenidoes.Find(id);

            if (precio == null)
                return HttpNotFound();

            return View(precio);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PrecioContenido precio = db.PrecioContenidoes.Find(id);

            if (precio != null)
            {
                db.PrecioContenidoes.Remove(precio);
                db.SaveChanges();
                TempData["Success"] = "Precio eliminado.";
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
