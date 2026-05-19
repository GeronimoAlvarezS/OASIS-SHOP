using CapaDatos.Config;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class ChatBotMensajeDatos
    {
        public bool RegistrarMensaje(ChatBotMensaje obj, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_RegistrarChatBotMensaje", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IdConversacion", obj.IdConversacion);
                    cmd.Parameters.AddWithValue("@Remitente", obj.Remitente);
                    cmd.Parameters.AddWithValue("@Mensaje", obj.Mensaje);

                    SqlParameter parametroResultado = new SqlParameter("@Resultado", SqlDbType.Bit)
                    {
                        Direction = ParameterDirection.Output
                    };

                    SqlParameter parametroMensaje = new SqlParameter("@MensajeSalida", SqlDbType.VarChar, 500)
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

        public List<ChatBotMensaje> ObtenerMensajesPorConversacion(int idConversacion)
        {
            List<ChatBotMensaje> lista = new List<ChatBotMensaje>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    string query = @"
                    SELECT
                        IdMensaje,
                        IdConversacion,
                        Remitente,
                        Mensaje,
                        FechaRegistro
                    FROM ChatBotMensaje
                    WHERE IdConversacion = @IdConversacion
                    ORDER BY FechaRegistro ASC";

                    SqlCommand cmd = new SqlCommand(query, oconexion);

                    cmd.Parameters.AddWithValue("@IdConversacion", idConversacion);

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ChatBotMensaje()
                            {
                                IdMensaje = Convert.ToInt32(dr["IdMensaje"]),
                                IdConversacion = Convert.ToInt32(dr["IdConversacion"]),
                                Remitente = dr["Remitente"].ToString(),
                                Mensaje = dr["Mensaje"].ToString(),
                                FechaRegistro = Convert.ToDateTime(dr["FechaRegistro"])
                            });
                        }
                    }
                }
            }
            catch
            {
                lista = new List<ChatBotMensaje>();
            }

            return lista;
        }
    }
}