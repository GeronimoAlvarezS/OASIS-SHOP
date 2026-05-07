using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    public class Compra
    {
        public int IdCompra { get; set; }

        public Usuario oUsuario { get; set; } = new Usuario();

        public Proveedor oProveedor { get; set; } = new Proveedor();

        public string TipoDocumento { get; set; } = string.Empty;

        public string NumeroDocumento { get; set; } = string.Empty;

        public decimal SubTotal { get; set; }

        public int Descuento { get; set; }

        public decimal MontoTotal { get; set; }

        public decimal MontoPagado { get; set; }

        public decimal MontoCambio { get; set; }

        public List<Detalle_Compra> oDetalleCompra { get; set; } = new List<Detalle_Compra>();

        public string FechaRegistro { get; set; } = string.Empty;
    }
}