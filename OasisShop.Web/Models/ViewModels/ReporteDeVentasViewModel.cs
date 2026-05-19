using CapaEntidad;
using System;
using System.Collections.Generic;

namespace OasisShop.Web.Models.ViewModels
{
    public class ReporteDeVentasViewModel
    {
        public string FechaInicio { get; set; } =
            DateTime.Now.ToString("yyyy-MM-dd");

        public string FechaFin { get; set; } =
            DateTime.Now.ToString("yyyy-MM-dd");

        public string Busqueda { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public string DocumentoCliente { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int PaginaActual { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 5;
        public int TotalPaginas { get; set; }
        public int TotalVentas { get; set; }
        public decimal MontoTotalVendido { get; set; }
        public List<ReporteVenta> Ventas { get; set; } =
            new List<ReporteVenta>();
    }
}