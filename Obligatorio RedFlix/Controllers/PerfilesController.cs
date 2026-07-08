using Obligatorio_RedFlix.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    public class PerfilesController : Controller
    {
        public ActionResult Seleccionar()
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<PerfilNetflixViewModel> perfiles = PerfilNetflixViewModel.CrearPerfiles(Session["UsuarioNombre"] as string);
            return View(perfiles);
        }

        public ActionResult Usar(string nombre)
        {
            if (Session["UsuarioId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<PerfilNetflixViewModel> perfiles = PerfilNetflixViewModel.CrearPerfiles(Session["UsuarioNombre"] as string);
            PerfilNetflixViewModel perfil = perfiles.FirstOrDefault(p => p.Nombre == nombre) ?? perfiles.First();

            Session["PerfilNombre"] = perfil.Nombre;
            Session["PerfilColor"] = perfil.Color;
            Session["PerfilInicial"] = perfil.Inicial;

            return RedirectToAction("Index", "Home");
        }
    }
}
