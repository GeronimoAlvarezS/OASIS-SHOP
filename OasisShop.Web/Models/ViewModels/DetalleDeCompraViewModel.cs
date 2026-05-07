using CapaEntidad;
using System.Collections.Generic;

namespace OasisShop.Web.Models.ViewModels
{
    public class DetalleDeCompraViewModel
    {
        public int IdCompra { get; set; }
        public string NumeroDocumento { get; set; }
        public string TipoDocumento { get; set; }
        public string FechaRegistro { get; set; }

        public string NombreUsuario { get; set; }

        public string DocumentoProveedor { get; set; }
        public string RazonSocialProveedor { get; set; }

        public decimal MontoTotal { get; set; }
        public decimal MontoPago { get; set; }
        public decimal MontoCambio { get; set; }
        public string DocumentoUsuario { get; set; }

        public List<Detalle_Compra> Detalles { get; set; } = new List<Detalle_Compra>();
    }
}