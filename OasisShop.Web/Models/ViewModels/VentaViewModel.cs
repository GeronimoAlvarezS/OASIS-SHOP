using CapaEntidad;
using System.Collections.Generic;

namespace OasisShop.Web.Models.ViewModels
{
    /// <summary>
    /// ViewModel utilizado por la vista Venta/Index.cshtml.
    /// Su responsabilidad es transportar los datos necesarios
    /// para pintar la pantalla de registrar ventas.
    /// </summary>
    public class VentaViewModel
    {
        /// <summary>
        /// Número único de venta generado desde el controlador.
        /// </summary>
        public string NumeroDocumento { get; set; }

        /// <summary>
        /// Lista de tipos de factura disponibles:
        /// Física, Electrónica, etc.
        /// </summary>
        public List<TipoFactura> TiposFactura { get; set; }

        /// <summary>
        /// Lista de productos activos que se muestran en el dropdown.
        /// </summary>
        public List<Producto> Productos { get; set; }

        /// <summary>
        /// Lista de clientes disponibles para autocompletar
        /// el nombre al ingresar el documento.
        /// </summary>
        public List<Cliente> Clientes { get; set; }

        /// <summary>
        /// Constructor del ViewModel.
        /// Inicializa las listas para evitar errores por valores nulos en Razor.
        /// </summary>
        public VentaViewModel()
        {
            NumeroDocumento = string.Empty;
            TiposFactura = new List<TipoFactura>();
            Productos = new List<Producto>();
            Clientes = new List<Cliente>();
        }
    }
}