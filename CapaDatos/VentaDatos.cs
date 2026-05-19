using CapaDatos.Config;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CapaDatos
{
    public class VentaDatos
    {
        public int ObtenerCorrelativo()
        {
            int idcorrelativo = 0;

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select count(*) + 1 from VENTA");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    idcorrelativo = Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch
                {
                    idcorrelativo = 0;
                }
            }

            return idcorrelativo;
        }

        public bool RestarStock(int idproducto, int cantidad)
        {
            bool respuesta = true;

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("update PRODUCTO set Stock = Stock - @cantidad where IdProducto = @idproducto");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@idproducto", idproducto);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    respuesta = cmd.ExecuteNonQuery() > 0;
                }
                catch
                {
                    respuesta = false;
                }
            }

            return respuesta;
        }

        public bool SumarStock(int idproducto, int cantidad)
        {
            bool respuesta = true;

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("update PRODUCTO set Stock = Stock + @cantidad where IdProducto = @idproducto");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@idproducto", idproducto);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    respuesta = cmd.ExecuteNonQuery() > 0;
                }
                catch
                {
                    respuesta = false;
                }
            }

            return respuesta;
        }

        public bool Registrar(Venta obj, DataTable DetalleVenta, out string Mensaje)
        {
            bool Respuesta = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("usp_RegistrarVenta", oconexion);

                    cmd.Parameters.AddWithValue("@IdUsuario", obj.oUsuario.IdUsuario);
                    cmd.Parameters.AddWithValue("@IdTipoFactura", obj.IdTipoFactura);
                    cmd.Parameters.AddWithValue("@NumeroDocumento", obj.NumeroDocumento ?? string.Empty);
                    cmd.Parameters.AddWithValue("@DocumentoCliente", obj.DocumentoCliente ?? string.Empty);
                    cmd.Parameters.AddWithValue("@NombreCliente", obj.NombreCliente ?? string.Empty);
                    cmd.Parameters.AddWithValue("@MontoPago", obj.MontoPago);
                    cmd.Parameters.AddWithValue("@MontoCambio", obj.MontoCambio);
                    cmd.Parameters.AddWithValue("@MontoTotal", obj.MontoTotal);
                    cmd.Parameters.AddWithValue("@DetalleVenta", DetalleVenta);

                    cmd.Parameters.Add("@Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@NumeroDocumentoGenerado", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;

                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    Respuesta = Convert.ToBoolean(cmd.Parameters["@Resultado"].Value);
                    Mensaje = cmd.Parameters["@Mensaje"].Value.ToString();

                    if (cmd.Parameters["@NumeroDocumentoGenerado"].Value != DBNull.Value)
                    {
                        obj.NumeroDocumento = cmd.Parameters["@NumeroDocumentoGenerado"].Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Respuesta = false;
                Mensaje = ex.Message;
            }

            return Respuesta;
        }

        public Venta ObtenerVenta(string numero)
        {
            Venta obj = new Venta();

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    oconexion.Open();

                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select v.IdVenta,");
                    query.AppendLine("u.NombreCompleto,");
                    query.AppendLine("u.Documento as DocumentoUsuario,");
                    query.AppendLine("v.DocumentoCliente,");
                    query.AppendLine("v.NombreCliente,");
                    query.AppendLine("tf.IdTipoFactura,");
                    query.AppendLine("tf.Nombre as TipoFactura,");
                    query.AppendLine("v.NumeroDocumento,");
                    query.AppendLine("v.MontoPago,");
                    query.AppendLine("v.MontoCambio,");
                    query.AppendLine("v.MontoTotal,");
                    query.AppendLine("convert(char(10), v.FechaRegistro, 103) as FechaRegistro");
                    query.AppendLine("from VENTA v");
                    query.AppendLine("inner join USUARIO u on u.IdUsuario = v.IdUsuario");
                    query.AppendLine("inner join TIPO_FACTURA tf on tf.IdTipoFactura = v.IdTipoFactura");
                    query.AppendLine("where v.NumeroDocumento = @numero");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            obj = new Venta()
                            {
                                IdVenta = Convert.ToInt32(dr["IdVenta"]),

                                oUsuario = new Usuario()
                                {
                                    NombreCompleto = dr["NombreCompleto"].ToString(),
                                    Documento = dr["DocumentoUsuario"].ToString()
                                },

                                DocumentoCliente = dr["DocumentoCliente"].ToString(),
                                NombreCliente = dr["NombreCliente"].ToString(),
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),

                                MontoPago = Convert.ToDecimal(dr["MontoPago"]),
                                MontoCambio = Convert.ToDecimal(dr["MontoCambio"]),
                                MontoTotal = Convert.ToDecimal(dr["MontoTotal"]),
                                FechaRegistro = dr["FechaRegistro"].ToString(),

                                IdTipoFactura = Convert.ToInt32(dr["IdTipoFactura"]),

                                oTipoFactura = new TipoFactura()
                                {
                                    IdTipoFactura = Convert.ToInt32(dr["IdTipoFactura"]),
                                    Nombre = dr["TipoFactura"].ToString()
                                }
                            };
                        }
                    }
                }
                catch
                {
                    obj = new Venta();
                }
            }

            return obj;
        }

        public List<Detalle_Venta> ObtenerDetalleVenta(int idVenta)
        {
            List<Detalle_Venta> oLista = new List<Detalle_Venta>();

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    oconexion.Open();

                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select p.IdProducto,");
                    query.AppendLine("p.Codigo,");
                    query.AppendLine("p.Nombre,");
                    query.AppendLine("dv.PrecioVenta,");
                    query.AppendLine("dv.Cantidad,");
                    query.AppendLine("dv.SubTotal");
                    query.AppendLine("from DETALLE_VENTA dv");
                    query.AppendLine("inner join PRODUCTO p on p.IdProducto = dv.IdProducto");
                    query.AppendLine("where dv.IdVenta = @idventa");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@idventa", idVenta);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            oLista.Add(new Detalle_Venta()
                            {
                                oProducto = new Producto()
                                {
                                    IdProducto = Convert.ToInt32(dr["IdProducto"]),
                                    Codigo = dr["Codigo"].ToString(),
                                    Nombre = dr["Nombre"].ToString()
                                },

                                PrecioVenta = Convert.ToDecimal(dr["PrecioVenta"]),
                                Cantidad = Convert.ToInt32(dr["Cantidad"]),
                                SubTotal = Convert.ToDecimal(dr["SubTotal"])
                            });
                        }
                    }
                }
                catch
                {
                    oLista = new List<Detalle_Venta>();
                }
            }

            return oLista;
        }

        public List<TipoFactura> ListarTipoFactura()
        {
            List<TipoFactura> lista = new List<TipoFactura>();

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    string query = "SELECT IdTipoFactura, Nombre, Descripcion, Estado FROM TIPO_FACTURA WHERE Estado = 1";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new TipoFactura()
                            {
                                IdTipoFactura = Convert.ToInt32(dr["IdTipoFactura"]),
                                Nombre = dr["Nombre"].ToString(),
                                Descripcion = dr["Descripcion"].ToString(),
                                Estado = Convert.ToBoolean(dr["Estado"])
                            });
                        }
                    }
                }
                catch
                {
                    lista = new List<TipoFactura>();
                }
            }

            return lista;
        }
    }
}