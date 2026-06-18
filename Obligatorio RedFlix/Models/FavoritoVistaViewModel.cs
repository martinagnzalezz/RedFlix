using System;

namespace Obligatorio_RedFlix.Models
{
    public class FavoritoVistaViewModel
    {
        public int IdFavorito { get; set; }

        public int IdTmdb { get; set; }

        public string Titulo { get; set; }

        public string Tipo { get; set; }

        public string ImagenUrl { get; set; }

        public string Overview { get; set; }

        public double? VoteAverage { get; set; }

        public DateTime FechaAgregado { get; set; }
    }
}