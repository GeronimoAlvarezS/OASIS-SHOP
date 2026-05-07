using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;
using System.Data;

namespace CapaNegocio
{
    public class CompraServicio
    {
        private readonly CompraDatos objcd_compra = new CompraDatos();

        public int ObtenerCorrelativo()
        {
            return objcd_compra.ObtenerCorrelativo();
        }

        public bool Registrar(Compra obj, DataTable DetalleCompra, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj == null)
            {
                Mensaje = "La información de la compra no puede estar vacía.";
                return false;
            }

            if (obj.oUsuario == null || obj.oUsuario.IdUsuario <= 0)
            {
                Mensaje = "Debe existir un usuario válido para registrar la compra.";
                return false;
            }

            if (obj.oProveedor == null || obj.oProveedor.IdProveedor <= 0)
            {
                Mensaje = "Debe seleccionar un proveedor.";
                return false;
            }

            obj.TipoDocumento = "Factura Electrónica";

            if (string.IsNullOrWhiteSpace(obj.NumeroDocumento))
            {
                obj.NumeroDocumento = "AUTO";
            }

            if (DetalleCompra == null || DetalleCompra.Rows.Count == 0)
            {
                Mensaje = "Debe agregar al menos un producto a la compra.";
                return false;
            }

            if (obj.SubTotal < 0)
            {
                Mensaje = "El subtotal no puede ser negativo.";
                return false;
            }

            if (obj.Descuento < 0 || obj.Descuento > 100)
            {
                Mensaje = "El descuento debe estar entre 0 y 100.";
                return false;
            }

            if (obj.MontoTotal <= 0)
            {
                Mensaje = "El monto total de la compra debe ser mayor a cero.";
                return false;
            }

            if (obj.MontoPagado <= 0)
            {
                Mensaje = "Debe ingresar el monto pagado.";
                return false;
            }

            if (obj.MontoPagado < obj.MontoTotal)
            {
                Mensaje = "El monto pagado no puede ser menor al total a pagar.";
                return false;
            }

            obj.MontoCambio = obj.MontoPagado - obj.MontoTotal;

            bool resultado = objcd_compra.Registrar(obj, DetalleCompra, out Mensaje);

            if (resultado && !string.IsNullOrWhiteSpace(obj.NumeroDocumento))
            {
                Mensaje = string.IsNullOrWhiteSpace(Mensaje)
                    ? "Compra registrada correctamente con identificador: " + obj.NumeroDocumento
                    : Mensaje;
            }

            return resultado;
        }

        public Compra ObtenerCompra(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
            {
                return new Compra();
            }

            Compra oCompra = objcd_compra.ObtenerCompra(numero.Trim());

            if (oCompra != null && oCompra.IdCompra != 0)
            {
                if (oCompra.oDetalleCompra == null)
                {
                    List<Detalle_Compra> oDetalleCompra = objcd_compra.ObtenerDetalleCompra(oCompra.IdCompra);
                    oCompra.oDetalleCompra = oDetalleCompra;
                }

                oCompra.TipoDocumento = "Factura Electrónica";
            }

            return oCompra;
        }

        public Compra ObtenerCompraDetalle(int idCompra)
        {
            if (idCompra <= 0)
            {
                return new Compra();
            }

            Compra oCompra = objcd_compra.ObtenerCompraDetalle(idCompra);

            if (oCompra != null && oCompra.IdCompra != 0)
            {
                if (oCompra.oDetalleCompra == null)
                {
                    List<Detalle_Compra> oDetalleCompra = objcd_compra.ObtenerDetalleCompra(oCompra.IdCompra);
                    oCompra.oDetalleCompra = oDetalleCompra;
                }

                oCompra.TipoDocumento = "Factura Electrónica";
            }

            return oCompra;
        }
    }
}