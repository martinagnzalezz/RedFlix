using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obligatorio_RedFlix.Models
{
    public class Populares
    {
        [JsonProperty("adult", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Adult { get; set; }

        [JsonProperty("backdrop_path", NullValueHandling = NullValueHandling.Ignore)]
        public string BackdropPath { get; set; }

        [JsonProperty("genre_ids", NullValueHandling = NullValueHandling.Ignore)]
        public List<long> GenreIds { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("original_language", NullValueHandling = NullValueHandling.Ignore)]
        public string OriginalLanguage { get; set; }

        [JsonProperty("original_title", NullValueHandling = NullValueHandling.Ignore)]
        public string OriginalTitle { get; set; }

        [JsonProperty("overview", NullValueHandling = NullValueHandling.Ignore)]
        public string Overview { get; set; }

        [JsonProperty("popularity", NullValueHandling = NullValueHandling.Ignore)]
        public double? Popularity { get; set; }

        [JsonProperty("poster_path", NullValueHandling = NullValueHandling.Ignore)]
        public string PosterPath { get; set; }

        [JsonProperty("release_date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ReleaseDate { get; set; }

        [JsonProperty("softcore", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Softcore { get; set; }

        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Video { get; set; }

        [JsonProperty("vote_average", NullValueHandling = NullValueHandling.Ignore)]
        public double? VoteAverage { get; set; }

        [JsonProperty("vote_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? VoteCount { get; set; }
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("original_name", NullValueHandling = NullValueHandling.Ignore)]
        public string OriginalName { get; set; }

        [JsonProperty("first_air_date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? FirstAirDate { get; set; }

        [JsonProperty("media_type", NullValueHandling = NullValueHandling.Ignore)]
        public string MediaType { get; set; }

        public string TituloMostrar
        {
            get
            {
                if (!string.IsNullOrEmpty(Title))
                {
                    return Title;
                }

                return Name;
            }
        }

        public string FechaMostrar
        {
            get
            {
                if (ReleaseDate.HasValue)
                {
                    return ReleaseDate.Value.ToString("dd/MM/yyyy");
                }

                if (FirstAirDate.HasValue)
                {
                    return FirstAirDate.Value.ToString("dd/MM/yyyy");
                }

                return "Sin fecha";
            }
        }
        public string ImagenUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PosterPath))
                {
                    return "https://placehold.co/500x750/151515/D4AF37?text=Sin+imagen";
                }

                return "https://image.tmdb.org/t/p/w500" + PosterPath;
            }
        }
        public string TipoContenido { get; set; }
    }
}
