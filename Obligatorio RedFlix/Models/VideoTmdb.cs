using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Obligatorio_RedFlix.Models
{
    public class ListaVideos
    {
        [JsonProperty("results")]
        public List<VideoTmdb> Results { get; set; }
    }

    public class VideoTmdb
    {
        [JsonProperty("name")]
        public string Nombre { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("type")]
        public string Tipo { get; set; }

        public string YoutubeUrl
        {
            get
            {
                return "https://www.youtube.com/embed/" + Key;
            }
        }
    }

    public class PeliculaDetalleViewModel
    {
        public Populares Pelicula { get; set; }
        public VideoTmdb Trailer { get; set; }
        public PrecioContenidoViewModel PrecioContenido { get; set; }
        public List<ActorTmdb> Actores { get; set; }
    }
}