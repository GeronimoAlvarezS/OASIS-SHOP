using CapaDatos.Config;
using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class ChatBotConversacionDatos
    {
        public int CrearConversacion(ChatBotConversacion obj, out string Mensaje)
        {
            int idConversacionGenerada = 0;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_CrearChatBotConversacion", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IdUsuario", obj.IdUsuario);

                    SqlParameter parametroResultado = new SqlParameter("@IdConversacion", SqlDbType.Int)
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

                    idConversacionGenerada = Convert.ToInt32(parametroResultado.Value);
                    Mensaje = parametroMensaje.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                idConversacionGenerada = 0;
                Mensaje = ex.Message;
            }

            return idConversacionGenerada;
        }

        public bool FinalizarConversacion(int idConversacion, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_FinalizarChatBotConversacion", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IdConversacion", idConversacion);

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