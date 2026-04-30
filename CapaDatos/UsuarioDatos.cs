using CapaDatos.Config;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CapaDatos
{
    public class UsuarioDatos
    {
        public List<Usuario> Listar()
        {
            List<Usuario> lista = new List<Usuario>();

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select u.IdUsuario,u.Documento,u.NombreCompleto,u.Correo,u.Clave,u.Estado,r.IdRol,r.Descripcion from usuario u");
                    query.AppendLine("inner join rol r on r.IdRol = u.IdRol");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(MapearUsuario(dr));
                        }
                    }
                }
                catch (Exception)
                {
                    lista = new List<Usuario>();
                }
            }

            return lista;
        }

        public Usuario ObtenerPorDocumento(string documento)
        {
            Usuario objUsuario = null;

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select top 1 u.IdUsuario,u.Documento,u.NombreCompleto,u.Correo,u.Clave,u.Estado,r.IdRol,r.Descripcion");
                    query.AppendLine("from usuario u");
                    query.AppendLine("inner join rol r on r.IdRol = u.IdRol");
                    query.AppendLine("where u.Documento = @Documento");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@Documento", documento);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            objUsuario = MapearUsuario(dr);
                        }
                    }
                }
                catch (Exception)
                {
                    objUsuario = null;
                }
            }

            return objUsuario;
        }

        public Usuario ObtenerPorCredenciales(string documento, string clave)
        {
            Usuario objUsuario = null;

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select top 1 u.IdUsuario,u.Documento,u.NombreCompleto,u.Correo,u.Clave,u.Estado,r.IdRol,r.Descripcion");
                    query.AppendLine("from usuario u");
                    query.AppendLine("inner join rol r on r.IdRol = u.IdRol");
                    query.AppendLine("where u.Documento = @Documento and u.Clave = @Clave");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@Documento", documento);
                    cmd.Parameters.AddWithValue("@Clave", clave);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            objUsuario = MapearUsuario(dr);
                        }
                    }
                }
                catch (Exception)
                {
                    objUsuario = null;
                }
            }

            return objUsuario;
        }

        public bool ActualizarClave(int idUsuario, string claveHash)
        {
            bool respuesta = false;

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("update usuario set Clave = @Clave where IdUsuario = @IdUsuario", oconexion);
                    cmd.Parameters.AddWithValue("@Clave", claveHash);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    respuesta = cmd.ExecuteNonQuery() > 0;
                }
                catch (Exception)
                {
                    respuesta = false;
                }
            }

            return respuesta;
        }

        public int Registrar(Usuario obj, out string Mensaje)
        {
            int idusuariogenerado = 0;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_REGISTRARUSUARIO", oconexion);
                    cmd.Parameters.AddWithValue("Documento", obj.Documento);
                    cmd.Parameters.AddWithValue("NombreCompleto", obj.NombreCompleto);
                    cmd.Parameters.AddWithValue("Correo", obj.Correo);
                    cmd.Parameters.AddWithValue("Clave", obj.Clave);
                    cmd.Parameters.AddWithValue("IdRol", obj.oRol.IdRol);
                    cmd.Parameters.AddWithValue("Estado", obj.Estado);
                    cmd.Parameters.Add("IdUsuarioResultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();

                    cmd.ExecuteNonQuery();

                    idusuariogenerado = Convert.ToInt32(cmd.Parameters["IdUsuarioResultado"].Value);
                    Mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                idusuariogenerado = 0;
                Mensaje = ex.Message;
            }

            return idusuariogenerado;
        }

        public bool Editar(Usuario obj, out string Mensaje)
        {
            bool respuesta = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_EDITARUSUARIO", oconexion);
                    cmd.Parameters.AddWithValue("IdUsuario", obj.IdUsuario);
                    cmd.Parameters.AddWithValue("Documento", obj.Documento);
                    cmd.Parameters.AddWithValue("NombreCompleto", obj.NombreCompleto);
                    cmd.Parameters.AddWithValue("Correo", obj.Correo);
                    cmd.Parameters.AddWithValue("Clave", obj.Clave);
                    cmd.Parameters.AddWithValue("IdRol", obj.oRol.IdRol);
                    cmd.Parameters.AddWithValue("Estado", obj.Estado);
                    cmd.Parameters.Add("Respuesta", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();

                    cmd.ExecuteNonQuery();

                    respuesta = Convert.ToBoolean(cmd.Parameters["Respuesta"].Value);
                    Mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                respuesta = false;
                Mensaje = ex.Message;
            }

            return respuesta;
        }

        public bool Eliminar(Usuario obj, out string Mensaje)
        {
            bool respuesta = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_ELIMINARUSUARIO", oconexion);
                    cmd.Parameters.AddWithValue("IdUsuario", obj.IdUsuario);
                    cmd.Parameters.Add("Respuesta", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();

                    cmd.ExecuteNonQuery();

                    respuesta = Convert.ToBoolean(cmd.Parameters["Respuesta"].Value);
                    Mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                respuesta = false;
                Mensaje = ex.Message;
            }

            return respuesta;
        }

        private Usuario MapearUsuario(SqlDataReader dr)
        {
            return new Usuario()
            {
                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                Documento = dr["Documento"].ToString(),
                NombreCompleto = dr["NombreCompleto"].ToString(),
                Correo = dr["Correo"].ToString(),
                Clave = dr["Clave"].ToString(),
                Estado = Convert.ToBoolean(dr["Estado"]),
                oRol = new Rol()
                {
                    IdRol = Convert.ToInt32(dr["IdRol"]),
                    Descripcion = dr["Descripcion"].ToString()
                }
            };
        }
    }
}