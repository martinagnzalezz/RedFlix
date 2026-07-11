using Obligatorio_RedFlix.Models;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Collections.Generic;

namespace Obligatorio_RedFlix.Data
{
    public class AdoNetService
    {
        private readonly string connectionString;

        public AdoNetService()
        {
            connectionString = ObtenerConnectionString();
        }

        private string ObtenerConnectionString()
        {
            string cs = ConfigurationManager.ConnectionStrings["RedFlixDBEntities"].ConnectionString;

            if (cs.Contains("metadata="))
            {
                EntityConnectionStringBuilder builder = new EntityConnectionStringBuilder(cs);
                return builder.ProviderConnectionString;
            }

            return cs;
        }

        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(connectionString);
        }

        public void GuardarClima(string ciudad, decimal temperatura, string descripcion, int humedad)
        {
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                {
                    conexion.Open();

                    if (!ExisteTabla(conexion, "ClimaHistorial"))
                    {
                        return;
                    }

                    string sql = @"
                        INSERT INTO ClimaHistorial
                        (Ciudad, Temperatura, Descripcion, Humedad, FechaConsulta)
                        VALUES
                        (@Ciudad, @Temperatura, @Descripcion, @Humedad, GETDATE())";

                    using (SqlCommand comando = new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@Ciudad", ciudad ?? "");
                        comando.Parameters.AddWithValue("@Temperatura", temperatura);
                        comando.Parameters.AddWithValue("@Descripcion", descripcion ?? "");
                        comando.Parameters.AddWithValue("@Humedad", humedad);

                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Si la tabla no existe o hay algún problema, no rompe la aplicación.
            }
        }

        public void GuardarOActualizarCotizacion(string monedaBase, string monedaDestino, decimal valor)
        {
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                {
                    conexion.Open();

                    if (!ExisteTabla(conexion, "Cotizaciones"))
                    {
                        return;
                    }

                    string sql = @"
                        MERGE Cotizaciones AS destino
                        USING (
                            SELECT 
                                @MonedaBase AS MonedaBase,
                                @MonedaDestino AS MonedaDestino
                        ) AS origen
                        ON destino.MonedaBase = origen.MonedaBase
                        AND destino.MonedaDestino = origen.MonedaDestino

                        WHEN MATCHED THEN
                            UPDATE SET 
                                Valor = @Valor,
                                FechaActualizacion = GETDATE()

                        WHEN NOT MATCHED THEN
                            INSERT (MonedaBase, MonedaDestino, Valor, FechaActualizacion)
                            VALUES (@MonedaBase, @MonedaDestino, @Valor, GETDATE());";

                    using (SqlCommand comando = new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@MonedaBase", monedaBase);
                        comando.Parameters.AddWithValue("@MonedaDestino", monedaDestino);
                        comando.Parameters.AddWithValue("@Valor", valor);

                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Si la tabla no existe o hay algún problema, no rompe la aplicación.
            }
        }

        public PromocionesClima ObtenerPromocionClimatica(string categoria, string descripcion, decimal temperatura)
        {
            try
            {
                string categoriaNormalizada = NormalizarTexto(categoria);
                string descripcionNormalizada = NormalizarTexto(descripcion);

                using (RedFlixDBEntities db = new RedFlixDBEntities())
                {
                    var promociones = db.PromocionesClimas
                        .Where(p => p.Activa)
                        .ToList();

                    foreach (var promo in promociones)
                    {
                        string condicion = NormalizarTexto(promo.CondicionClima);

                        if (condicion == categoriaNormalizada ||
                            descripcionNormalizada.Contains(condicion) ||
                            categoriaNormalizada.Contains(condicion))
                        {
                            return promo;
                        }
                    }
                }
            }
            catch
            {
                // Si falla, simplemente no aplica promoción.
            }

            return null;
        }

        private bool ExisteTabla(SqlConnection conexion, string nombreTabla)
        {
            string sql = @"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_NAME = @NombreTabla";

            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@NombreTabla", nombreTabla);

                int cantidad = Convert.ToInt32(comando.ExecuteScalar());

                return cantidad > 0;
            }
        }

        private string NormalizarTexto(string texto)
        {
            return string.IsNullOrWhiteSpace(texto)
                ? ""
                : texto.Trim().ToLower()
                    .Replace("í", "i")
                    .Replace("á", "a")
                    .Replace("é", "e")
                    .Replace("ó", "o")
                    .Replace("ú", "u");
        }

        public bool GuardarReporteUsuario(int idUsuario, string tipoReporte, string titulo, string descripcion)
        {
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                {
                    conexion.Open();

                    string sql = @"
                INSERT INTO ReportesUsuarios
                (IdUsuario, TipoReporte, Titulo, Descripcion, Estado, FechaReporte)
                VALUES
                (@IdUsuario, @TipoReporte, @Titulo, @Descripcion, 'Pendiente', GETDATE())";

                    using (SqlCommand comando = new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        comando.Parameters.AddWithValue("@TipoReporte", tipoReporte);
                        comando.Parameters.AddWithValue("@Titulo", titulo);
                        comando.Parameters.AddWithValue("@Descripcion", descripcion);

                        int filas = comando.ExecuteNonQuery();

                        return filas > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public List<ReporteUsuarioViewModel> ListarReportesUsuarios()
        {
            List<ReporteUsuarioViewModel> reportes = new List<ReporteUsuarioViewModel>();

            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                {
                    conexion.Open();

                    string sql = @"
                SELECT 
                    r.IdReporte,
                    r.IdUsuario,
                    u.Nombre,
                    u.Email,
                    r.TipoReporte,
                    r.Titulo,
                    r.Descripcion,
                    r.Estado,
                    r.FechaReporte,
                    r.FechaResolucion
                FROM ReportesUsuarios r
                INNER JOIN Usuarios u ON r.IdUsuario = u.IdUsuario
                ORDER BY r.FechaReporte DESC";

                    using (SqlCommand comando = new SqlCommand(sql, conexion))
                    {
                        using (SqlDataReader reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ReporteUsuarioViewModel reporte = new ReporteUsuarioViewModel
                                {
                                    IdReporte = Convert.ToInt32(reader["IdReporte"]),
                                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                    NombreUsuario = reader["Nombre"].ToString(),
                                    EmailUsuario = reader["Email"].ToString(),
                                    TipoReporte = reader["TipoReporte"].ToString(),
                                    Titulo = reader["Titulo"].ToString(),
                                    Descripcion = reader["Descripcion"].ToString(),
                                    Estado = reader["Estado"].ToString(),
                                    FechaReporte = Convert.ToDateTime(reader["FechaReporte"]),
                                    FechaResolucion = reader["FechaResolucion"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(reader["FechaResolucion"])
                                };

                                reportes.Add(reporte);
                            }
                        }
                    }
                }
            }
            catch
            {
                return new List<ReporteUsuarioViewModel>();
            }

            return reportes;
        }

        public bool MarcarReporteComoResuelto(int idReporte)
        {
            try
            {
                using (SqlConnection conexion = ObtenerConexion())
                {
                    conexion.Open();

                    string sql = @"
                UPDATE ReportesUsuarios
                SET Estado = 'Resuelto',
                    FechaResolucion = GETDATE()
                WHERE IdReporte = @IdReporte";

                    using (SqlCommand comando = new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@IdReporte", idReporte);

                        int filas = comando.ExecuteNonQuery();

                        return filas > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}