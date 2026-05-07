using CapaEntidad;
using System.Collections.Generic;

namespace OasisShop.Web.Models.ViewModels
{
    public class CompraViewModel
    {
        public int IdUsuario { get; set; }

        public int IdProveedor { get; set; }

        public string DocumentoProveedor { get; set; } = string.Empty;

        public string TipoDocumento { get; set; } = "COMPRA";

        public string NumeroDocumento { get; set; } = "AUTO";

        public decimal SubTotal { get; set; }

        public int Descuento { get; set; }

        public decimal MontoTotal { get; set; }

        public decimal MontoPagado { get; set; }

        public decimal MontoCambio { get; set; }

        public List<Proveedor> Proveedores { get; set; } = new List<Proveedor>();

        public List<Producto> Productos { get; set; } = new List<Producto>();

        public List<DetalleCompraViewModel> DetalleCompra { get; set; } = new List<DetalleCompraViewModel>();
    }

    public class DetalleCompraViewModel
    {
        public int IdProducto { get; set; }

        public string CodigoProducto { get; set; } = string.Empty;

        public string NombreProducto { get; set; } = string.Empty;

        public decimal PrecioCompra { get; set; }

        public decimal PrecioVenta { get; set; }

        public int Cantidad { get; set; }

        public decimal MontoTotal { get; set; }
    }
}