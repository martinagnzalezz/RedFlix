using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obligatorio_RedFlix.Models
{
    public class BusquedaViewModel
    {
        public string Query { get; set; }
        public List<Populares> Peliculas { get; set; }
        public List<Populares> Series { get; set; }
        public List<ActorTmdb> Actores { get; set; }

        public BusquedaViewModel()
        {
            Peliculas = new List<Populares>();
            Series = new List<Populares>();
            Actores = new List<ActorTmdb>();
        }
    }
}