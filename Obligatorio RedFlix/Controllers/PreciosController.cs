using Obligatorio_RedFlix.Filters;
using Obligatorio_RedFlix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    [AutorizarRol("Administrador")]
    public class PreciosController : Controller
    {
        private RedFlixDBEntities db = new RedFlixDBEntities();

        public ActionResult Index()
        {
            var precios = db.PrecioContenidoes.OrderBy(p => p.Titulo).ToList();
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

                // IMPORTANTE:
                // Al editar, lo dejamos activo para que se siga usando en la película
                precioBD.Activo = true;

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