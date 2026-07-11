using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Obligatorio_RedFlix.Filters
{
    public class AutorizarPermisoAttribute : ActionFilterAttribute
    {
        private readonly string permisoNecesario;

        public AutorizarPermisoAttribute(string permiso)
        {
            permisoNecesario = permiso;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Si no hay sesión iniciada, lo manda al login
            if (filterContext.HttpContext.Session["UsuarioId"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" }
                    }
                );

                return;
            }

            // Obtiene los permisos guardados en Session
            var permisos = filterContext.HttpContext.Session["Permisos"] as List<string>;

            // Si no tiene permisos o no tiene el permiso necesario, lo manda a SinPermiso
            if (permisos == null || !permisos.Contains(permisoNecesario))
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "SinPermiso" }
                    }
                );

                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}