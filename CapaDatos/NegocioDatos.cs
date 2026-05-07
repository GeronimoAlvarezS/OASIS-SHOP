using CapaDatos.Config;
using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CapaDatos
{
    public class NegocioDatos
    {
        private void InicializarNegocio()
        {
            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                oconexion.Open();

                string query = @"
                IF NOT EXISTS (SELECT 1 FROM NEGOCIO WHERE IdNegocio = 1)
                BEGIN
                    SET IDENTITY_INSERT NEGOCIO ON;

                    INSERT INTO NEGOCIO (IdNegocio, Nombre, RUC, Direccion, IdDepartamento, IdCiudad)
                    VALUES (1, '', '', '', NULL, NULL);

                    SET IDENTITY_INSERT NEGOCIO OFF;
                END";

                SqlCommand cmd = new SqlCommand(query, oconexion);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public Negocio ObtenerDatos()
        {
            Negocio obj = new Negocio();

            try
            {
                InicializarNegocio();

                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    oconexion.Open();

                    string query = @"
                    SELECT 
                        n.IdNegocio, 
                        n.Nombre, 
                        n.RUC, 
                        n.Direccion,
                        n.IdDepartamento,
                        n.IdCiudad,
                        d.Nombre AS NombreDepartamento,
                        d.Codigo AS CodigoDepartamento,
                        c.Nombre AS NombreCiudad,
                        c.Codigo AS CodigoCiudad,
                        n.Logo
                    FROM NEGOCIO n
                    LEFT JOIN Departamento d ON d.IdDepartamento = n.IdDepartamento
                    LEFT JOIN Ciudad c ON c.IdCiudad = n.IdCiudad
                    WHERE n.IdNegocio = 1";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            obj = new Negocio()
                            {
                                IdNegocio = Convert.ToInt32(dr["IdNegocio"]),
                                Nombre = dr["Nombre"] == DBNull.Value ? string.Empty : dr["Nombre"].ToString(),
                                RUC = dr["RUC"] == DBNull.Value ? string.Empty : dr["RUC"].ToString(),
                                Direccion = dr["Direccion"] == DBNull.Value ? string.Empty : dr["Direccion"].ToString(),

                                IdDepartamento = dr["IdDepartamento"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdDepartamento"]),
                                IdCiudad = dr["IdCiudad"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdCiudad"]),

                                oDepartamento = new Departamento()
                                {
                                    IdDepartamento = dr["IdDepartamento"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdDepartamento"]),
                                    Nombre = dr["NombreDepartamento"] == DBNull.Value ? string.Empty : dr["NombreDepartamento"].ToString(),
                                    Codigo = dr["CodigoDepartamento"] == DBNull.Value ? string.Empty : dr["CodigoDepartamento"].ToString()
                                },

                                oCiudad = new Ciudad()
                                {
                                    IdCiudad = dr["IdCiudad"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdCiudad"]),
                                    IdDepartamento = dr["IdDepartamento"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdDepartamento"]),
                                    Nombre = dr["NombreCiudad"] == DBNull.Value ? string.Empty : dr["NombreCiudad"].ToString(),
                                    Codigo = dr["CodigoCiudad"] == DBNull.Value ? string.Empty : dr["CodigoCiudad"].ToString()
                                },

                                Logo = dr["Logo"] == DBNull.Value ? Array.Empty<byte>() : (byte[])dr["Logo"]
                            };
                        }
                    }
                }
            }
            catch
            {
                obj = new Negocio();
            }

            return obj;
        }

        public bool GuardarDatos(Negocio objeto, out string mensaje)
        {
            mensaje = string.Empty;
            bool respuesta = true;

            try
            {
                InicializarNegocio();

                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    oconexion.Open();

                    StringBuilder query = new StringBuilder();
                    query.AppendLine("UPDATE NEGOCIO SET");
                    query.AppendLine("Nombre = @nombre,");
                    query.AppendLine("RUC = @ruc,");
                    query.AppendLine("Direccion = @direccion,");
                    query.AppendLine("IdDepartamento = @idDepartamento,");
                    query.AppendLine("IdCiudad = @idCiudad");
                    query.AppendLine("WHERE IdNegocio = 1;");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@nombre", objeto.Nombre ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ruc", objeto.RUC ?? string.Empty);
                    cmd.Parameters.AddWithValue("@direccion", objeto.Direccion ?? string.Empty);

                    if (objeto.IdDepartamento > 0)
                    {
                        cmd.Parameters.AddWithValue("@idDepartamento", objeto.IdDepartamento);
                    }
                    else
                    {
                        cmd.Parameters.Add("@idDepartamento", SqlDbType.Int).Value = DBNull.Value;
                    }

                    if (objeto.IdCiudad > 0)
                    {
                        cmd.Parameters.AddWithValue("@idCiudad", objeto.IdCiudad);
                    }
                    else
                    {
                        cmd.Parameters.Add("@idCiudad", SqlDbType.Int).Value = DBNull.Value;
                    }

                    cmd.CommandType = CommandType.Text;

                    if (cmd.ExecuteNonQuery() < 1)
                    {
                        mensaje = "No se pudo guardar los datos del negocio.";
                        respuesta = false;
                    }
                    else
                    {
                        mensaje = "Datos del negocio guardados correctamente.";
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                respuesta = false;
            }

            return respuesta;
        }

        public byte[] ObtenerLogo(out bool obtenido)
        {
            obtenido = true;
            byte[] logoBytes = Array.Empty<byte>();

            try
            {
                InicializarNegocio();

                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    oconexion.Open();

                    string query = "SELECT Logo FROM NEGOCIO WHERE IdNegocio = 1";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    object resultado = cmd.ExecuteScalar();

                    if (resultado != null && resultado != DBNull.Value)
                    {
                        logoBytes = (byte[])resultado;
                    }
                }
            }
            catch
            {
                obtenido = false;
                logoBytes = Array.Empty<byte>();
            }

            return logoBytes;
        }

        public bool ActualizarLogo(byte[] imagen, out string mensaje)
        {
            mensaje = string.Empty;
            bool respuesta = true;

            try
            {
                InicializarNegocio();

                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    oconexion.Open();

                    StringBuilder query = new StringBuilder();
                    query.AppendLine("UPDATE NEGOCIO SET");
                    query.AppendLine("Logo = @imagen");
                    query.AppendLine("WHERE IdNegocio = 1;");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.Add("@imagen", SqlDbType.VarBinary).Value = imagen;
                    cmd.CommandType = CommandType.Text;

                    if (cmd.ExecuteNonQuery() < 1)
                    {
                        mensaje = "No se pudo actualizar el logo.";
                        respuesta = false;
                    }
                    else
                    {
                        mensaje = "Logo actualizado correctamente.";
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                respuesta = false;
            }

            return respuesta;
        }
    }
}