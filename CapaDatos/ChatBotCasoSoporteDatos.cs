using CapaDatos.Config;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class ChatBotCasoSoporteDatos
    {
        public bool RegistrarCaso(ChatBotCasoSoporte obj, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_RegistrarChatBotCasoSoporte", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IdConversacion", obj.IdConversacion);
                    cmd.Parameters.AddWithValue("@IdUsuario", obj.IdUsuario);
                    cmd.Parameters.AddWithValue("@Situacion", obj.Situacion);
                    cmd.Parameters.AddWithValue("@RespuestaChatBot", obj.RespuestaChatBot ?? string.Empty);
                    cmd.Parameters.AddWithValue("@TipoCaso", obj.TipoCaso);

                    SqlParameter parametroResultado = new SqlParameter("@Resultado", SqlDbType.Bit)
                    {
                        Direction = ParameterDirection.Output
                    };

                    SqlParameter parametroMensaje = new SqlParameter("@Mensaje", SqlDbType.VarChar, 500)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(parametroResultado);
                    cmd.Parameters.Add(parametroMensaje);

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    resultado = Convert.ToBoolean(parametroResultado.Value);
                    Mensaje = parametroMensaje.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                Mensaje = ex.Message;
            }

            return resultado;
        }

        public List<ChatBotCasoSoporte> Listar()
        {
            List<ChatBotCasoSoporte> lista = new List<ChatBotCasoSoporte>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    string query = @"
                    SELECT
                        IdCaso,
                        IdConversacion,
                        IdUsuario,
                        Situacion,
                        RespuestaChatBot,
                        TipoCaso,
                        Estado,
                        FechaRegistro,
                        FechaCierre
                    FROM ChatBotCasoSoporte
                    ORDER BY FechaRegistro DESC";

                    SqlCommand cmd = new SqlCommand(query, oconexion);

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ChatBotCasoSoporte()
                            {
                                IdCaso = Convert.ToInt32(dr["IdCaso"]),
                                IdConversacion = Convert.ToInt32(dr["IdConversacion"]),
                                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                                Situacion = dr["Situacion"].ToString(),
                                RespuestaChatBot = dr["RespuestaChatBot"].ToString(),
                                TipoCaso = dr["TipoCaso"].ToString(),
                                Estado = dr["Estado"].ToString(),
                                FechaRegistro = Convert.ToDateTime(dr["FechaRegistro"]),
                                FechaCierre = dr["FechaCierre"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(dr["FechaCierre"])
                            });
                        }
                    }
                }
            }
            catch
            {
                lista = new List<ChatBotCasoSoporte>();
            }

            return lista;
        }

        public bool ActualizarEstado(int idCaso, string estado, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_ActualizarEstadoChatBotCasoSoporte", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IdCaso", idCaso);
                    cmd.Parameters.AddWithValue("@Estado", estado);

                    SqlParameter parametroResultado = new SqlParameter("@Resultado", SqlDbType.Bit)
                    {
                        Direction = ParameterDirection.Output
                    };

                    SqlParameter parametroMensaje = new SqlParameter("@Mensaje", SqlDbType.VarChar, 500)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(parametroResultado);
                    cmd.Parameters.Add(parametroMensaje);

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    resultado = Convert.ToBoolean(parametroResultado.Value);
                    Mensaje = parametroMensaje.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                Mensaje = ex.Message;
            }

            return resultado;
        }
    }
}