using CapaEntidad;
using CapaNegocio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;
using System.Collections.Generic;
using System.Data;

namespace OasisShop.Web.Controllers
{
    public class VentaController : Controller
    {
        private readonly VentaServicio _ventaServicio;
        private readonly ProductoServicio _productoServicio;
        private readonly ClienteServicio _clienteServicio;

        public VentaController()
        {
            _ventaServicio = new VentaServicio();
            _productoServicio = new ProductoServicio();
            _clienteServicio = new ClienteServicio();
        }

        // Muestra la vista principal para registrar ventas.
        // Ruta: /Venta/Index
        public IActionResult Index()
        {
            VentaViewModel model = new VentaViewModel();

            model.NumeroDocumento = GenerarNumeroVenta();
            model.TiposFactura = _ventaServicio.ListarTipoFactura();
            model.Productos = _productoServicio.Listar();
            model.Clientes = _clienteServicio.Listar();

            // Si tu vista se llama Venta.cshtml y está en Views/Venta/Venta.cshtml,
            // debes retornarla explícitamente.
            return View("Venta", model);
        }

        // Registra la venta enviada desde Venta.cshtml por fetch("/Venta/Registrar").
        [HttpPost]
        public IActionResult Registrar([FromBody] VentaRequest request)
        {
            string mensaje = string.Empty;

            if (request == null)
            {
                return Json(new { resultado = false, mensaje = "No se recibió información de la venta." });
            }

            if (request.IdTipoFactura <= 0)
            {
                return Json(new { resultado = false, mensaje = "Debe seleccionar un tipo de factura." });
            }

            if (request.DetalleVenta == null || request.DetalleVenta.Count == 0)
            {
                return Json(new { resultado = false, mensaje = "Debe agregar al menos un producto a la venta." });
            }

            if (request.MontoPago < request.MontoTotal)
            {
                return Json(new { resultado = false, mensaje = "El monto pagado no puede ser menor al total." });
            }

            DataTable detalleVenta = CrearTablaDetalleVenta(request.DetalleVenta);

            Venta venta = new Venta()
            {
                oUsuario = new Usuario()
                {
                    IdUsuario = ObtenerIdUsuarioSesion()
                },
                IdTipoFactura = request.IdTipoFactura,
                NumeroDocumento = string.IsNullOrWhiteSpace(request.NumeroDocumento)
                    ? GenerarNumeroVenta()
                    : request.NumeroDocumento,
                DocumentoCliente = request.DocumentoCliente ?? string.Empty,
                NombreCliente = request.NombreCliente ?? string.Empty,
                MontoPago = request.MontoPago,
                MontoCambio = request.MontoCambio,
                MontoTotal = request.MontoTotal
            };

            bool resultado = _ventaServicio.Registrar(venta, detalleVenta, out mensaje);

            return Json(new
            {
                resultado = resultado,
                mensaje = resultado ? "Venta registrada correctamente." : mensaje,
                numeroDocumento = venta.NumeroDocumento
            });
        }

        // Muestra la vista de detalle de ventas.
        // Ruta: /Venta/Detalle
        public IActionResult Detalle()
        {
            return View();
        }

        // Consulta una venta por número de documento.
        [HttpGet]
        public IActionResult ObtenerVenta(string numeroDocumento)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento))
            {
                return Json(new { resultado = false, mensaje = "Debe ingresar el número de venta." });
            }

            Venta venta = _ventaServicio.ObtenerVenta(numeroDocumento);

            if (venta == null || venta.IdVenta == 0)
            {
                return Json(new { resultado = false, mensaje = "No se encontró la venta." });
            }

            return Json(new { resultado = true, venta = venta });
        }

        // Convierte el detalle recibido desde JavaScript al DataTable esperado por EDetalle_Venta.
        private DataTable CrearTablaDetalleVenta(List<DetalleVentaRequest> detalle)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("IdProducto", typeof(int));
            dt.Columns.Add("PrecioVenta", typeof(decimal));
            dt.Columns.Add("Cantidad", typeof(int));
            dt.Columns.Add("SubTotal", typeof(decimal));

            foreach (DetalleVentaRequest item in detalle)
            {
                dt.Rows.Add(
                    item.IdProducto,
                    item.PrecioVenta,
                    item.Cantidad,
                    item.SubTotal
                );
            }

            return dt;
        }

        // Genera el número único de venta.
        private string GenerarNumeroVenta()
        {
            int correlativo = _ventaServicio.ObtenerCorrelativo();
            return correlativo.ToString("00000");
        }

        // Obtiene el usuario autenticado desde sesión.
        // Si no existe sesión, retorna 1 como usuario temporal.
        private int ObtenerIdUsuarioSesion()
        {
            string idUsuario = HttpContext.Session.GetString("IdUsuario");

            if (int.TryParse(idUsuario, out int id))
            {
                return id;
            }

            return 1;
        }
    }

    // DTO que recibe los datos principales de la venta desde JavaScript.
    public class VentaRequest
    {
        public int IdTipoFactura { get; set; }
        public string NumeroDocumento { get; set; }
        public string DocumentoCliente { get; set; }
        public string NombreCliente { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoPago { get; set; }
        public decimal MontoCambio { get; set; }
        public List<DetalleVentaRequest> DetalleVenta { get; set; }
    }

    // DTO que recibe cada producto agregado al detalle de la venta.
    public class DetalleVentaRequest
    {
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal SubTotal { get; set; }
    }
}