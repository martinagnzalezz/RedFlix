using Obligatorio_RedFlix.Data;
using Obligatorio_RedFlix.Filters;
using Obligatorio_RedFlix.Models;
using System;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    public class ReportesController : Controller
    {
        private readonly AdoNetService adoNet = new AdoNetService();

        // LISTADO DE REPORTES - SOLO GESTOR DE REPORTES / ADMIN
        [AutorizarPermiso("Reportes.Ver")]
        public ActionResult Index()
        {
            var reportes = adoNet.ListarReportesUsuarios();

            return View(reportes);
        }

        // CREAR REPORTE - USUARIO COMÚN LOGUEADO
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
        public ActionResult Crear(string tipoReporte, string titulo, string descripcion)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(tipoReporte) ||
                string.IsNullOrWhiteSpace(titulo) ||
                string.IsNullOrWhiteSpace(descripcion))
            {
                ViewBag.Error = "Por favor completá todos los campos.";
                return View();
            }

            int idUsuario = Convert.ToInt32(Session["UsuarioId"]);

            bool guardado = adoNet.GuardarReporteUsuario(
                idUsuario,
                tipoReporte,
                titulo,
                descripcion
            );

            if (!guardado)
            {
                ViewBag.Error = "No se pudo guardar el reporte.";
                return View();
            }

            TempData["Success"] = "Reporte enviado correctamente.";

            return RedirectToAction("Index", "Home");
        }

        // MARCAR COMO RESUELTO - SOLO GESTOR DE REPORTES / ADMIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AutorizarPermiso("Reportes.Ver")]
        public ActionResult Resolver(int id)
        {
            bool resuelto = adoNet.MarcarReporteComoResuelto(id);

            if (resuelto)
            {
                TempData["Success"] = "Reporte marcado como resuelto.";
            }
            else
            {
                TempData["Error"] = "No se pudo actualizar el reporte.";
            }

            return RedirectToAction("Index");
        }
    }
}