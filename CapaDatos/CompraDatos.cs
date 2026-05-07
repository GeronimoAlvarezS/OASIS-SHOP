using CapaDatos.Config;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CapaDatos
{
    public class CompraDatos
    {
        public int ObtenerCorrelativo()
        {
            int correlativo = 0;

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    string query = "SELECT ISNULL(MAX(IdCompra), 0) + 1 FROM COMPRA";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    correlativo = Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch
                {
                    correlativo = 0;
                }
            }

            return correlativo;
        }

        public bool Registrar(Compra obj, DataTable DetalleCompra, out string Mensaje)
        {
            bool respuesta = false;
            Mensaje = string.Empty;

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_RegistrarCompra", oconexion);

                    cmd.Parameters.AddWithValue("@IdUsuario", obj.oUsuario.IdUsuario);
                    cmd.Parameters.AddWithValue("@IdProveedor", obj.oProveedor.IdProveedor);
                    cmd.Parameters.AddWithValue("@TipoDocumento", "Factura Electrónica");
                    cmd.Parameters.AddWithValue("@NumeroDocumento", string.IsNullOrWhiteSpace(obj.NumeroDocumento) ? "AUTO" : obj.NumeroDocumento);
                    cmd.Parameters.AddWithValue("@MontoTotal", obj.MontoTotal);
                    cmd.Parameters.AddWithValue("@MontoPagado", obj.MontoPagado);
                    cmd.Parameters.AddWithValue("@MontoCambio", obj.MontoCambio);
                    cmd.Parameters.AddWithValue("@DetalleCompra", DetalleCompra);

                    cmd.Parameters.Add("@Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;

                    SqlParameter parametroNumeroDocumentoGenerado = new SqlParameter("@NumeroDocumentoGenerado", SqlDbType.VarChar, 50);
                    parametroNumeroDocumentoGenerado.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(parametroNumeroDocumentoGenerado);

                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    respuesta = Convert.ToBoolean(cmd.Parameters["@Resultado"].Value);
                    Mensaje = cmd.Parameters["@Mensaje"].Value.ToString();

                    if (respuesta && cmd.Parameters["@NumeroDocumentoGenerado"].Value != DBNull.Value)
                    {
                        obj.NumeroDocumento = cmd.Parameters["@NumeroDocumentoGenerado"].Value.ToString();
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    Mensaje = ex.Message;
                }
            }

            return respuesta;
        }

        public Compra ObtenerCompra(string numero)
        {
            Compra obj = new Compra();

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    StringBuilder query = new StringBuilder();

                    query.AppendLine("SELECT c.IdCompra,");
                    query.AppendLine("u.NombreCompleto,");
                    query.AppendLine("pr.Documento,");
                    query.AppendLine("pr.RazonSocial,");
                    query.AppendLine("c.TipoDocumento,");
                    query.AppendLine("c.NumeroDocumento,");
                    query.AppendLine("c.MontoTotal,");
                    query.AppendLine("ISNULL(c.MontoPagado, 0) AS MontoPagado,");
                    query.AppendLine("ISNULL(c.MontoCambio, 0) AS MontoCambio,");
                    query.AppendLine("CONVERT(CHAR(10), c.FechaRegistro, 103) AS FechaRegistro");
                    query.AppendLine("FROM COMPRA c");
                    query.AppendLine("INNER JOIN USUARIO u ON u.IdUsuario = c.IdUsuario");
                    query.AppendLine("INNER JOIN PROVEEDOR pr ON pr.IdProveedor = c.IdProveedor");
                    query.AppendLine("WHERE LTRIM(RTRIM(c.NumeroDocumento)) = LTRIM(RTRIM(@numero))");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            decimal montoTotal = Convert.ToDecimal(dr["MontoTotal"]);

                            obj = new Compra()
                            {
                                IdCompra = Convert.ToInt32(dr["IdCompra"]),

                                oUsuario = new Usuario()
                                {
                                    NombreCompleto = dr["NombreCompleto"].ToString()
                                },

                                oProveedor = new Proveedor()
                                {
                                    Documento = dr["Documento"].ToString(),
                                    RazonSocial = dr["RazonSocial"].ToString()
                                },

                                TipoDocumento = "Factura Electrónica",
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),

                                SubTotal = montoTotal,
                                Descuento = 0,
                                MontoTotal = montoTotal,
                                MontoPagado = Convert.ToDecimal(dr["MontoPagado"]),
                                MontoCambio = Convert.ToDecimal(dr["MontoCambio"]),

                                FechaRegistro = dr["FechaRegistro"].ToString()
                            };
                        }
                    }

                    if (obj.IdCompra > 0)
                    {
                        obj.oDetalleCompra = ObtenerDetalleCompra(obj.IdCompra);
                    }
                }
                catch
                {
                    obj = new Compra();
                }
            }

            return obj;
        }

        public List<Detalle_Compra> ObtenerDetalleCompra(int idcompra)
        {
            List<Detalle_Compra> oLista = new List<Detalle_Compra>();

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    StringBuilder query = new StringBuilder();

                    query.AppendLine("SELECT p.Codigo,");
                    query.AppendLine("p.Nombre,");
                    query.AppendLine("dc.PrecioCompra,");
                    query.AppendLine("dc.PrecioVenta,");
                    query.AppendLine("dc.Cantidad,");
                    query.AppendLine("dc.MontoTotal");
                    query.AppendLine("FROM DETALLE_COMPRA dc");
                    query.AppendLine("INNER JOIN PRODUCTO p ON p.IdProducto = dc.IdProducto");
                    query.AppendLine("WHERE dc.IdCompra = @idcompra");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@idcompra", idcompra);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            oLista.Add(new Detalle_Compra()
                            {
                                oProducto = new Producto()
                                {
                                    Codigo = dr["Codigo"].ToString(),
                                    Nombre = dr["Nombre"].ToString()
                                },

                                PrecioCompra = Convert.ToDecimal(dr["PrecioCompra"]),
                                PrecioVenta = Convert.ToDecimal(dr["PrecioVenta"]),
                                Cantidad = Convert.ToInt32(dr["Cantidad"]),
                                MontoTotal = Convert.ToDecimal(dr["MontoTotal"])
                            });
                        }
                    }
                }
                catch
                {
                    oLista = new List<Detalle_Compra>();
                }
            }

            return oLista;
        }

        public Compra ObtenerCompraDetalle(int idCompra)
        {
            Compra obj = new Compra();

            using (SqlConnection oconexion = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                try
                {
                    StringBuilder query = new StringBuilder();

                    query.AppendLine("SELECT c.IdCompra,");
                    query.AppendLine("u.NombreCompleto,");
                    query.AppendLine("u.Documento AS DocumentoUsuario,");
                    query.AppendLine("pr.Documento,");
                    query.AppendLine("pr.RazonSocial,");
                    query.AppendLine("c.TipoDocumento,");
                    query.AppendLine("c.NumeroDocumento,");
                    query.AppendLine("c.MontoTotal,");
                    query.AppendLine("ISNULL(c.MontoPagado, 0) AS MontoPagado,");
                    query.AppendLine("ISNULL(c.MontoCambio, 0) AS MontoCambio,");
                    query.AppendLine("CONVERT(CHAR(10), c.FechaRegistro, 103) AS FechaRegistro");
                    query.AppendLine("FROM COMPRA c");
                    query.AppendLine("INNER JOIN USUARIO u ON u.IdUsuario = c.IdUsuario");
                    query.AppendLine("INNER JOIN PROVEEDOR pr ON pr.IdProveedor = c.IdProveedor");
                    query.AppendLine("WHERE c.IdCompra = @idCompra");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@idCompra", idCompra);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            decimal montoTotal = Convert.ToDecimal(dr["MontoTotal"]);

                            obj = new Compra()
                            {
                                IdCompra = Convert.ToInt32(dr["IdCompra"]),

                                oUsuario = new Usuario()
                                {
                                    NombreCompleto = dr["NombreCompleto"].ToString(),
                                    Documento = dr["DocumentoUsuario"].ToString()
                                },

                                oProveedor = new Proveedor()
                                {
                                    Documento = dr["Documento"].ToString(),
                                    RazonSocial = dr["RazonSocial"].ToString()
                                },

                                TipoDocumento = "Factura Electrónica",
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),

                                SubTotal = montoTotal,
                                Descuento = 0,
                                MontoTotal = montoTotal,
                                MontoPagado = Convert.ToDecimal(dr["MontoPagado"]),
                                MontoCambio = Convert.ToDecimal(dr["MontoCambio"]),

                                FechaRegistro = dr["FechaRegistro"].ToString()
                            };
                        }
                    }

                    if (obj.IdCompra > 0)
                    {
                        obj.oDetalleCompra = ObtenerDetalleCompra(obj.IdCompra);
                    }
                }
                catch
                {
                    obj = new Compra();
                }
            }

            return obj;
        }
    }
}