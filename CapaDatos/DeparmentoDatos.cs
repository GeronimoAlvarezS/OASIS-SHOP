using CapaDatos.Config;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class DepartamentoDatos
    {
        public List<Departamento> Listar()
        {
            List<Departamento> lista = new List<Departamento>();

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                string query = @"
                    SELECT IdDepartamento, Nombre, Codigo, Estado
                    FROM Departamento
                    WHERE Estado = 1
                    ORDER BY Nombre";

                SqlCommand cmd = new SqlCommand(query, oconexion);
                cmd.CommandType = CommandType.Text;

                oconexion.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Departamento
                        {
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