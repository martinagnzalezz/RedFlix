using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Obligatorio_RedFlix.Models
{
    public class ListaActores
    {
        [JsonProperty("cast")]
        public List<ActorTmdb> Cast { get; set; }
    }

    public class ListaPersonas
    {
        [JsonProperty("results")]
        public List<ActorTmdb> Results { get; set; }
    }

    public class ActorTmdb
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Nombre { get; set; }

        [JsonProperty("character")]
        public string Personaje { get; set; }

        [JsonProperty("profile_path")]
        public string FotoPath { get; set; }

        public string FotoUrl
        {
            get
            {
                if (string.IsNullOrEmpty(FotoPath))
                {
                    return "/Content/img/no-image.png";
                }

                return "https://image.tmdb.org/t/p/w185" + FotoPath;
            }
        }
    }

    public class ActorDetalleViewModel
    {
        public ActorTmdb Actor { get; set; }
        public List<Populares> Peliculas { get; set; }
        public List<Populares> Series { get; set; }

        public ActorDetalleViewModel()
        {
            Peliculas = new List<Populares>();
            Series = new List<Populares>();
        }
    }

    public class ListaCreditosActor
    {
        [JsonProperty("cast")]
        public List<Populares> Cast { get; set; }
    }
}