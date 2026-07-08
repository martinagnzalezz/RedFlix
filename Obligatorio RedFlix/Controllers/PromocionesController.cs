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
    public class PromocionesController : Controller
    {
        private RedFlixDBEntities db = new RedFlixDBEntities();

        public ActionResult Index()
        {
            var promos = db.PromocionesClimas
                .OrderByDescending(p => p.Activa)
                .ToList();
            return View(promos);
        }

        public ActionResult Create()
        {
            CargarGeneros();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PromocionesClima promo)
        {
            if (ModelState.IsValid)
            {
                promo.Activa = true;
                db.PromocionesClimas.Add(promo);
                db.SaveChanges();

                TempData["Success"] = "Promoción creada correctamente.";
                return RedirectToAction("Index");
            }

            CargarGeneros(promo.IdGenero);
            return View(promo);
        }

        public ActionResult Edit(int id)
        {
            PromocionesClima promo = db.PromocionesClimas.Find(id);

            if (promo == null)
                return HttpNotFound();

            CargarGeneros(promo.IdGenero);
            return View(promo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PromocionesClima promo)
        {
            PromocionesClima promoBD = db.PromocionesClimas.Find(promo.IdPromocion);

            if (promoBD == null)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                promoBD.Nombre = promo.Nombre;
                promoBD.Descripcion = promo.Descripcion;
                promoBD.CondicionClima = promo.CondicionClima;
                promoBD.TemperaturaMax = promo.TemperaturaMax;
                promoBD.PorcentajeDesc = promo.PorcentajeDesc;
                promoBD.IdGenero = promo.IdGenero;
                promoBD.Activa = promo.Activa;

                db.SaveChanges();

                TempData["Success"] = "Promoción actualizada correctamente.";
                return RedirectToAction("Index");
            }

            CargarGeneros(promo.IdGenero);
            return View(promo);
        }

        private void CargarGeneros(int? idSeleccionado = null)
        {
            if (!db.Generos.Any())
            {
                string[] nombres = {
                    "Acción",
                    "Aventura",
                    "Animación",
                    "Comedia",
                    "Crimen",
                    "Drama",
                    "Familia",
                    "Fantasía",
                    "Misterio",
                    "Romance",
                    "Suspenso",
                    "Terror",
                    "Ciencia ficción"
                };

                foreach (string nombre in nombres)
                {
                    db.Generos.Add(new Genero { Nombre = nombre });
                }

                db.SaveChanges();
            }

            ViewBag.IdGenero = new SelectList(db.Generos.OrderBy(g => g.Nombre), "IdGenero", "Nombre", idSeleccionado);
        }

        // Activar/Desactivar rápido desde el listado
        public ActionResult CambiarEstado(int id)
        {
            PromocionesClima promo = db.PromocionesClimas.Find(id);

            if (promo != null)
            {
                promo.Activa = !promo.Activa;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            PromocionesClima promo = db.PromocionesClimas.Find(id);

            if (promo == null)
                return HttpNotFound();

            return View(promo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PromocionesClima promo = db.PromocionesClimas.Find(id);

            if (promo != null)
            {
                db.PromocionesClimas.Remove(promo);
                db.SaveChanges();
                TempData["Success"] = "Promoción eliminada.";
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
