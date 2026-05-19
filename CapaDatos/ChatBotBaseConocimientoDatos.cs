using CapaDatos.Config;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace CapaDatos
{
    public class ChatBotBaseConocimientoDatos
    {
        public List<ChatBotBaseConocimiento> Listar()
        {
            List<ChatBotBaseConocimiento> lista = new List<ChatBotBaseConocimiento>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    string query = @"
                    SELECT
                        IdConocimiento,
                        Problema,
                        PalabrasClave,
                        Solucion,
                        TipoCaso,
                        Estado,
                        FechaRegistro
                    FROM ChatBotBaseConocimiento
                    WHERE Estado = 1";

                    SqlCommand cmd = new SqlCommand(query, oconexion);

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ChatBotBaseConocimiento()
                            {
                                IdConocimiento = Convert.ToInt32(dr["IdConocimiento"]),
                                Problema = dr["Problema"].ToString(),
                                PalabrasClave = dr["PalabrasClave"].ToString(),
                                Solucion = dr["Solucion"].ToString(),
                                TipoCaso = dr["TipoCaso"].ToString(),
                                Estado = Convert.ToBoolean(dr["Estado"]),
                                FechaRegistro = Convert.ToDateTime(dr["FechaRegistro"])
                            });
                        }
                    }
                }
            }
            catch
            {
                lista = new List<ChatBotBaseConocimiento>();
            }

            return lista;
        }

        public ChatBotBaseConocimiento BuscarCoincidencia(string mensajeUsuario)
        {
            ChatBotBaseConocimiento resultado = null;

            try
            {
                mensajeUsuario = mensajeUsuario.ToLower().Trim();

                List<ChatBotBaseConocimiento> conocimientos = Listar();

                foreach (var item in conocimientos)
                {
                    string[] palabras = item.PalabrasClave
                        .ToLower()
                        .Split(',');

                    foreach (string palabra in palabras)
                    {
                        if (mensajeUsuario.Contains(palabra.Trim()))
                        {
                            resultado = item;
                            return resultado;
                        }
                    }
                }
            }
            catch
            {
                resultado = null;
            }

            return resultado;
        }

        public bool RegistrarConocimiento(ChatBotBaseConocimiento obj, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_RegistrarChatBotConocimiento", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Problema", obj.Problema);
                    cmd.Parameters.AddWithValue("@PalabrasClave", obj.PalabrasClave);
                    cmd.Parameters.AddWithValue("@Solucion", obj.Solucion);
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
    }
}