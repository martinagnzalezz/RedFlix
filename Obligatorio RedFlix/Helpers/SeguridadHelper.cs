using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obligatorio_RedFlix.Helpers
{
    public static class SeguridadHelper
    {
        public static bool TienePermiso(string permiso)
        {
            var permisos = HttpContext.Current.Session["Permisos"] as List<string>;

            if (permisos == null)
            {
                return false;
            }

            return permisos.Contains(permiso);
        }

        public static bool EsAdministrador()
        {
            string rol = HttpContext.Current.Session["RolNombre"] as string;

            return rol == "Administrador";
        }
    }
}