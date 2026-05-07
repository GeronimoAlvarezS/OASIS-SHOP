using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;
using System.Data;

namespace CapaNegocio
{
    public class VentaServicio
    {
        // Objeto de acceso a datos encargado de ejecutar las operaciones contra SQL Server.
        private VentaDatos objcd_venta = new VentaDatos();

        // Resta stock a un producto.
        // Nota: en el flujo de registrar venta, lo recomendable es que el stock
        // se descuente dentro del procedimiento almacenado usp_RegistrarVenta,
        // para mantener la transacción completa.
        public bool RestarStock(int idproducto, int cantidad)
        {
            return objcd_venta.RestarStock(idproducto, cantidad);
        }

        // Suma stock a un producto.
        // Este método puede ser útil para anulaciones, devoluciones o ajustes.
        public bool SumarStock(int idproducto, int cantidad)
        {
            return objcd_venta.SumarStock(idproducto, cantidad);
        }

        // Obtiene el consecutivo para generar el número único de venta.
        public int ObtenerCorrelativo()
        {
            return objcd_venta.ObtenerCorrelativo();
        }

        // Registra una venta junto con su detalle.
        // Recibe:
        // - obj: datos principales de la venta.
        // - DetalleVenta: tabla temporal con los productos vendidos.
        // - Mensaje: devuelve el mensaje generado por el procedimiento almacenado.
        public bool Registrar(Venta obj, DataTable DetalleVenta, out string Mensaje)
        {
            return objcd_venta.Registrar(obj, DetalleVenta, out Mensaje);
        }

        // Obtiene una venta por su número de documento o número de venta.
        // Si la venta existe, también consulta y asigna su detalle.
        public Venta ObtenerVenta(string numero)
        {
            Venta oVenta = objcd_venta.ObtenerVenta(numero);

            if (oVenta.IdVenta != 0)
            {
                List<Detalle_Venta> oDetalleVenta = objcd_venta.ObtenerDetalleVenta(oVenta.IdVenta);
                oVenta.oDetalle_Venta = oDetalleVenta;
            }

            return oVenta;
        }

        // Lista los tipos de factura activos desde la tabla maestra TIPO_FACTURA.
        // Este método se usa para llenar el ComboBox del formulario de ventas.
        public List<TipoFactura> ListarTipoFactura()
        {
            return objcd_venta.ListarTipoFactura();
        }
    }
}