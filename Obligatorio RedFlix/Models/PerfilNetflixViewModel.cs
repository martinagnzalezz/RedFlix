using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obligatorio_RedFlix.Models
{
    public class PerfilNetflixViewModel
    {
        public string Nombre { get; set; }
        public string Color { get; set; }
        public string Inicial { get; set; }

        public static List<PerfilNetflixViewModel> CrearPerfiles(string nombreUsuario)
        {
            string nombre = string.IsNullOrWhiteSpace(nombreUsuario) ? "Usuario" : nombreUsuario;

            return new List<PerfilNetflixViewModel>
            {
                new PerfilNetflixViewModel { Nombre = nombre, Color = "#D4AF37", Inicial = nombre.Substring(0, 1).ToUpper() },
                new PerfilNetflixViewModel { Nombre = "Invitado", Color = "#2D9CDB", Inicial = "I" },
                new PerfilNetflixViewModel { Nombre = "Kids", Color = "#EB5757", Inicial = "K" },
                new PerfilNetflixViewModel { Nombre = "Familia", Color = "#27AE60", Inicial = "F" }
            };
        }
    }
}