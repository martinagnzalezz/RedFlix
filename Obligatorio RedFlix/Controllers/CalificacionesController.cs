using Obligatorio_RedFlix.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    public class CalificacionesController : Controller
    {
        private RedFlixDBEntities db = new RedFlixDBEntities();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Calificar(int idTmdb, string titulo, string tipoContenido, int puntaje, string comentario)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (puntaje < 1 || puntaje > 5)
            {
                TempData["MensajeCalificacion"] = "El puntaje debe estar entre 1 y 5.";
                return RedirigirDetalle(tipoContenido, idTmdb);
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);
            tipoContenido = tipoContenido.ToLower();

            if (tipoContenido == "pelicula")
            {
                var pelicula = db.Peliculas.FirstOrDefault(p => p.IdTmdb == idTmdb);

                if (pelicula == null)
                {
                    pelicula = new Pelicula
                    {
                        IdTmdb = idTmdb,
                        Titulo = titulo,
                        Precio = null
                    };

                    db.Peliculas.Add(pelicula);
                    db.SaveChanges();
                }

                var calificacion = db.Calificaciones.FirstOrDefault(c =>
                    c.IdUsuario == idUsuario &&
                    c.IdPelicula == pelicula.IdPelicula
                );

                if (calificacion == null)
                {
                    calificacion = new Calificacione
                    {
                        IdUsuario = idUsuario,
                        IdPelicula = pelicula.IdPelicula,
                        IdSerie = null,
                        Puntaje = puntaje,
                        Comentario = comentario,
                        FechaCalificacion = DateTime.Now
                    };

                    db.Calificaciones.Add(calificacion);
                }
                else
                {
                    calificacion.Puntaje = puntaje;
                    calificacion.Comentario = comentario;
                    calificacion.FechaCalificacion = DateTime.Now;
                }

                db.SaveChanges();

                TempData["MensajeCalificacion"] = "Calificación guardada correctamente.";
                return RedirectToAction("Detalle", "Pelicula", new { id = idTmdb });
            }

            if (tipoContenido == "serie")
            {
                var serie = db.Series.FirstOrDefault(s => s.IdTmdb == idTmdb);

                if (serie == null)
                {
                    serie = new Series
                    {
                        IdTmdb = idTmdb,
                        Titulo = titulo,
                        Precio = null
                    };

                    db.Series.Add(serie);
                    db.SaveChanges();
                }

                var calificacion = db.Calificaciones.FirstOrDefault(c =>
                    c.IdUsuario == idUsuario &&
                    c.IdSerie == serie.IdSerie
                );

                if (calificacion == null)
                {
                    calificacion = new Calificacione
                    {
                        IdUsuario = idUsuario,
                        IdPelicula = null,
                        IdSerie = serie.IdSerie,
                        Puntaje = puntaje,
                        Comentario = comentario,
                        FechaCalificacion = DateTime.Now
                    };

                    db.Calificaciones.Add(calificacion);
                }
                else
                {
                    calificacion.Puntaje = puntaje;
                    calificacion.Comentario = comentario;
                    calificacion.FechaCalificacion = DateTime.Now;
                }

                db.SaveChanges();

                TempData["MensajeCalificacion"] = "Calificación guardada correctamente.";
                return RedirectToAction("DetalleSerie", "Pelicula", new { id = idTmdb });
            }

            TempData["MensajeCalificacion"] = "Tipo de contenido inválido.";
            return RedirectToAction("Index", "Home");
        }

        private ActionResult RedirigirDetalle(string tipoContenido, int idTmdb)
        {
            if (tipoContenido.ToLower() == "serie")
            {
                return RedirectToAction("DetalleSerie", "Pelicula", new { id = idTmdb });
            }

            return RedirectToAction("Detalle", "Pelicula", new { id = idTmdb });
        }
    }
}