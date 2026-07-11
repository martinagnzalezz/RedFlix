using Obligatorio_RedFlix.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Obligatorio_RedFlix.Data
{
    public class AdoNetService
    {
        private readonly string _connectionString;

        public AdoNetService()
        {
            string entityConnection = ConfigurationManager.ConnectionStrings["RedFlixDBEntities"].ConnectionString;
            _connectionString = new EntityConnectionStringBuilder(entityConnection).ProviderConnectionString;
        }

        // guardar el clima actual de una ciudad en la base de datos
        public void GuardarClima(string ciudad, decimal temperatura, string estado, int humedad)
        {
            const string sql = @"INSERT INTO Clima
                (Ciudad, Temperatura, EstadoClima, Humedad, FechaConsulta)
                VALUES (@Ciudad, @Temperatura, @EstadoClima, @Humedad, @FechaConsulta);";

            using (var conexion = new SqlConnection(_connectionString))
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Ciudad", SqlDbType.VarChar, 100).Value = ciudad;
                var temperaturaParam = comando.Parameters.Add("@Temperatura", SqlDbType.Decimal);
                temperaturaParam.Precision = 5;
                temperaturaParam.Scale = 2;
                temperaturaParam.Value = temperatura;
                comando.Parameters.Add("@EstadoClima", SqlDbType.VarChar, 100).Value = estado;
                comando.Parameters.Add("@Humedad", SqlDbType.Int).Value = humedad;
                comando.Parameters.Add("@FechaConsulta", SqlDbType.DateTime).Value = DateTime.Now;
                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        // inserta o actualiza la cotización de una moneda en la base de datos
        public void GuardarOActualizarCotizacion(string origen, string destino, decimal valor)
        {
            const string sql = @"UPDATE Cotizaciones
                SET Valor = @Valor, FechaActualizacion = @FechaActualizacion
                WHERE MonedaOrigen = @Origen AND MonedaDestino = @Destino;
                IF @@ROWCOUNT = 0
                    INSERT INTO Cotizaciones (MonedaOrigen, MonedaDestino, Valor, FechaActualizacion)
                    VALUES (@Origen, @Destino, @Valor, @FechaActualizacion);";

            using (var conexion = new SqlConnection(_connectionString))
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Origen", SqlDbType.VarChar, 10).Value = origen;
                comando.Parameters.Add("@Destino", SqlDbType.VarChar, 10).Value = destino;
                var valorParam = comando.Parameters.Add("@Valor", SqlDbType.Decimal);
                valorParam.Precision = 18;
                valorParam.Scale = 4;
                valorParam.Value = valor;
                comando.Parameters.Add("@FechaActualizacion", SqlDbType.DateTime).Value = DateTime.Now;
                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        // consulta la mejor promoción activa para el clima actual 
        public PromocionesClima ObtenerPromocionClimatica(string categoria, string descripcion, decimal temperatura)
        {
            const string sql = @"SELECT TOP 1 IdPromocion, Nombre, Descripcion, CondicionClima,
                    TemperaturaMax, PorcentajeDesc, IdGenero, Activa
                FROM PromocionesClima
                WHERE Activa = 1
                  AND (TemperaturaMax IS NULL OR @Temperatura <= TemperaturaMax)
                  AND (LTRIM(RTRIM(CondicionClima)) = ''
                       OR LOWER(CondicionClima) = @Categoria
                       OR @Descripcion LIKE '%' + LOWER(CondicionClima) + '%')
                ORDER BY PorcentajeDesc DESC;";

            using (var conexion = new SqlConnection(_connectionString))
            using (var comando = new SqlCommand(sql, conexion))
            {
                var temperaturaParam = comando.Parameters.Add("@Temperatura", SqlDbType.Decimal);
                temperaturaParam.Precision = 5;
                temperaturaParam.Scale = 2;
                temperaturaParam.Value = temperatura;
                comando.Parameters.Add("@Categoria", SqlDbType.VarChar, 100).Value = Normalizar(categoria);
                comando.Parameters.Add("@Descripcion", SqlDbType.VarChar, 300).Value = Normalizar(descripcion);
                conexion.Open();

                using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!lector.Read()) return null;
                    return new PromocionesClima
                    {
                        IdPromocion = lector.GetInt32(0),
                        Nombre = lector.GetString(1),
                        Descripcion = lector.IsDBNull(2) ? null : lector.GetString(2),
                        CondicionClima = lector.GetString(3),
                        TemperaturaMax = lector.IsDBNull(4) ? (decimal?)null : lector.GetDecimal(4),
                        PorcentajeDesc = lector.GetDecimal(5),
                        IdGenero = lector.IsDBNull(6) ? (int?)null : lector.GetInt32(6),
                        Activa = lector.GetBoolean(7)
                    };
                }
            }
        }

        private static string Normalizar(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? "" : texto.Trim().ToLower()
                .Replace("í", "i").Replace("á", "a").Replace("é", "e")
                .Replace("ó", "o").Replace("ú", "u");
        }
    }
}