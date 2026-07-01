using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Obligatorio_RedFlix.Filters
{
    public class AutorizarRol : ActionFilterAttribute
    {
        private readonly string[] _rolesPermitidos;

        // Si no pasás roles, solo exige estar logueado
        public AutorizarRol(params string[] rolesPermitidos)
        {
            _rolesPermitidos = rolesPermitidos;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            // No logueado -> a Login
            if (session["UsuarioId"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                        { "controller", "Account" },
                        { "action", "Login" }
                    });
                return;
            }

            var rolActual = session["RolNombre"] as string;

            // Logueado pero con rol no permitido -> a Home
            if (_rolesPermitidos.Length > 0 && !_rolesPermitidos.Contains(rolActual))
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                        { "controller", "Home" },
                        { "action", "Index" }
                    });
            }
        }
    }
}