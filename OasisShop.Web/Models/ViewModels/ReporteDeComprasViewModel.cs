using System.Collections.Generic;
using CapaEntidad;

namespace OasisShop.Web.Models.ViewModels
{
    public class ReporteDeComprasViewModel
    {
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 5;
        public int TotalPaginas { get; set; }
        public int TotalCompras { get; set; }
        public string Busqueda { get; set; } = string.Empty;
        public int IdProveedor { get; set; }
        public List<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
        public List<ReporteCompra> Compras { get; set; } = new List<ReporteCompra>();
    }
}