using CapaEntidad;
using System.Collections.Generic;

namespace OasisShop.Web.Models.ViewModels
{
    public class DetalleDeVentaViewModel
    {
        // INFORMACIÓN GENERAL VENTA

        public int IdVenta { get; set; }

        public string NumeroDocumento { get; set; }

        public string TipoFactura { get; set; }

        public string FechaRegistro { get; set; }


        public string NombreUsuario { get; set; }

        public string DocumentoUsuario { get; set; }

        // INFORMACIÓN DEL CLIENTE

        public string DocumentoCliente { get; set; }

        public string NombreCliente { get; set; }

        // INFORMACIÓN MONETARIA

        public decimal MontoTotal { get; set; }

        public decimal MontoPago { get; set; }

        public decimal MontoCambio { get; set; }
        public decimal Descuento { get; set; }
        public decimal Subtotal { get; set; }

        // FACTURACIÓN ELECTRÓNICA

        // Define si la factura debe generar QR.
        public bool EsFacturaElectronica { get; set; }

        // Código CUFE único por factura electrónica.
        public string CUFE { get; set; }

        // DETALLE DE PRODUCTOS

        public List<Detalle_Venta> Detalles { get; set; } = new List<Detalle_Venta>();
    }
}