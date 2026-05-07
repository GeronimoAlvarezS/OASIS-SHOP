using CapaDatos.Config;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class CiudadDatos
    {
        public List<Ciudad> ListarPorDepartamento(int idDepartamento)
        {
            List<Ciudad> lista = new List<Ciudad>();

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                string query = @"
                    SELECT IdCiudad, IdDepartamento, Nombre, Codigo, Estado
                    FROM Ciudad
                    WHERE Estado = 1
                    AND IdDepartamento = @IdDepartamento
                    ORDER BY Nombre";

                SqlCommand cmd = new SqlCommand(query, oconexion);
                cmd.Parameters.AddWithValue("@IdDepartamento", idDepartamento);
                cmd.CommandType = CommandType.Text;

                oconexion.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Ciudad
                        {
                            IdCiudad = Convert.ToInt32(dr["IdCiudad"]),
                            IdDepartamento = Convert.ToInt32(dr["IdDepartamento"]),
                            Nombre = dr["Nombre"].ToString(),
                            Codigo = dr["Codigo"].ToString(),
                            Estado = Convert.ToBoolean(dr["Estado"])
                        });
                    }
                }
            }

            return lista;
        }
    }
}