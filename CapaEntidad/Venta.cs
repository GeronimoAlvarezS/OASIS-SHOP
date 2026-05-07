using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    public class Venta
    {
        public int IdVenta { get; set; }

        public Usuario oUsuario { get; set; }

        public int IdTipoFactura { get; set; }

        public TipoFactura oTipoFactura { get; set; }

        public string NumeroDocumento { get; set; }

        public string DocumentoCliente { get; set; }

        public string NombreCliente { get; set; }

        public decimal MontoPago { get; set; }

        public decimal MontoCambio { get; set; }

        public decimal MontoTotal { get; set; }

        public List<Detalle_Venta> oDetalle_Venta { get; set; }

        public string FechaRegistro { get; set; }
    }
}