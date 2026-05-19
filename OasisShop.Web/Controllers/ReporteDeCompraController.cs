using CapaEntidad;
using CapaNegocio;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace OasisShop.Web.Controllers
{
    public class ReporteDeCompraController : Controller
    {
        private readonly ReporteServicio _reporteServicio = new ReporteServicio();
        private readonly ProveedorServicio _proveedorServicio = new ProveedorServicio();

        [HttpGet]
        public IActionResult Index(string fechaInicio = "", string fechaFin = "", string busqueda = "", int idproveedor = 0, int pagina = 1)
        {
            int registrosPorPagina = 5;

            if (string.IsNullOrWhiteSpace(fechaInicio))
                fechaInicio = DateTime.Now.ToString("yyyy-MM-dd");

            if (string.IsNullOrWhiteSpace(fechaFin))
                fechaFin = DateTime.Now.ToString("yyyy-MM-dd");

            List<ReporteCompra> compras = ObtenerComprasFiltradas(fechaInicio, fechaFin, busqueda, idproveedor);

            int totalRegistros = compras.Count;
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            if (pagina < 1)
                pagina = 1;

            if (totalPaginas > 0 && pagina > totalPaginas)
                pagina = totalPaginas;

            List<ReporteCompra> comprasPaginadas = compras
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToList();

            ReporteDeComprasViewModel model = new ReporteDeComprasViewModel
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Busqueda = busqueda,
                IdProveedor = idproveedor,
                Proveedores = _proveedorServicio.Listar(),
                Compras = comprasPaginadas,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalCompras = compras.Count
            };

            return View("~/Views/Reporte/ReporteDeCompra.cshtml", model);
        }

        [HttpGet]
        public IActionResult ExportarCsv(string fechaInicio = "", string fechaFin = "", string busqueda = "", int idproveedor = 0)
        {
            List<ReporteCompra> compras = ObtenerComprasFiltradas(fechaInicio, fechaFin, busqueda, idproveedor);

            StringBuilder csv = new StringBuilder();

            csv.AppendLine("Numero de compra;Usuario de registro;Documento del proveedor;Razon social;Codigo producto;Nombre producto;Cantidad;Categoria;Precio de compra;Precio de venta");

            foreach (var compra in compras)
            {
                csv.AppendLine(
                    $"{LimpiarCsv(compra.FechaRegistro)};" +
                    $"{LimpiarCsv(compra.NumeroCompra)};" +
                    $"{LimpiarCsv(compra.UsuarioRegistro)};" +
                    $"{LimpiarCsv(compra.DocumentoProveedor)};" +
                    $"{LimpiarCsv(compra.RazonSocial)};" +
                    $"{LimpiarCsv(compra.CodigoProducto)};" +
                    $"{LimpiarCsv(compra.NombreProducto)};" +
                    $"{LimpiarCsv(compra.Cantidad)};" +
                    $"{LimpiarCsv(compra.Categoria)};" +
                    $"{LimpiarCsv(compra.PrecioCompra)};" +
                    $"{LimpiarCsv(compra.PrecioVenta)}"
                );
            }

            byte[] bytes = Encoding.UTF8.GetBytes(csv.ToString());

            return File(bytes, "text/csv", $"ReporteCompras_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        [HttpGet]
        public IActionResult ExportarExcel(string fechaInicio = "", string fechaFin = "", string busqueda = "", int idproveedor = 0)
        {
            List<ReporteCompra> compras = ObtenerComprasFiltradas(fechaInicio, fechaFin, busqueda, idproveedor);

            using (var workbook = new XLWorkbook())
            {
                var hoja = workbook.Worksheets.Add("Reporte de compras");

                hoja.Cell(1, 1).Value = "REPORTE DE COMPRAS DE INVENTARIO";
                hoja.Range("A1:K1").Merge();

                var titulo = hoja.Range("A1:K1");
                titulo.Style.Font.Bold = true;
                titulo.Style.Font.FontSize = 18;
                titulo.Style.Font.FontColor = XLColor.White;
                titulo.Style.Fill.BackgroundColor = XLColor.FromHtml("#241E65");
                titulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                hoja.Row(1).Height = 30;

                hoja.Cell(2, 1).Value = $"Fecha de exportación: {DateTime.Now:dd/MM/yyyy HH:mm}";
                hoja.Range("A2:K2").Merge();
                hoja.Range("A2:K2").Style.Font.Italic = true;
                hoja.Range("A2:K2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                hoja.Cell(4, 1).Value = "Fecha de registro";
                hoja.Cell(4, 2).Value = "Número de compra";
                hoja.Cell(4, 3).Value = "Usuario de registro";
                hoja.Cell(4, 4).Value = "Documento del proveedor";
                hoja.Cell(4, 5).Value = "Razón social";
                hoja.Cell(4, 6).Value = "Código producto";
                hoja.Cell(4, 7).Value = "Nombre producto";
                hoja.Cell(4, 8).Value = "Cantidad";
                hoja.Cell(4, 9).Value = "Categoría";
                hoja.Cell(4, 10).Value = "Precio de compra";
                hoja.Cell(4, 11).Value = "Precio de venta";

                var encabezado = hoja.Range("A4:K4");
                encabezado.Style.Font.Bold = true;
                encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#3F3CBB");
                encabezado.Style.Font.FontColor = XLColor.White;
                encabezado.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int fila = 5;

                foreach (var item in compras)
                {
                    hoja.Cell(fila, 1).Value = item.FechaRegistro;
                    hoja.Cell(fila, 2).Value = item.NumeroCompra;
                    hoja.Cell(fila, 3).Value = item.UsuarioRegistro;
                    hoja.Cell(fila, 4).Value = item.DocumentoProveedor;
                    hoja.Cell(fila, 5).Value = item.RazonSocial;
                    hoja.Cell(fila, 6).Value = item.CodigoProducto;
                    hoja.Cell(fila, 7).Value = item.NombreProducto;
                    hoja.Cell(fila, 8).Value = ConvertirDecimal(item.Cantidad);
                    hoja.Cell(fila, 9).Value = item.Categoria;
                    hoja.Cell(fila, 10).Value = ConvertirDecimal(item.PrecioCompra);
                    hoja.Cell(fila, 11).Value = ConvertirDecimal(item.PrecioVenta);

                    fila++;
                }

                int ultimaFila = fila - 1;

                if (ultimaFila >= 5)
                {
                    var tabla = hoja.Range($"A4:K{ultimaFila}").CreateTable();
                    tabla.Theme = XLTableTheme.TableStyleMedium9;

                    hoja.Range($"J5:K{ultimaFila}").Style.NumberFormat.Format = "$ #,##0";
                    hoja.Range($"G5:G{ultimaFila}").Style.NumberFormat.Format = "#,##0";
                }

                hoja.Columns().AdjustToContents();
                hoja.SheetView.FreezeRows(4);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"ReporteCompras_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                    );
                }
            }
        }

        private List<ReporteCompra> ObtenerComprasFiltradas(string fechaInicio, string fechaFin, string busqueda, int idproveedor)
        {
            if (string.IsNullOrWhiteSpace(fechaInicio))
                fechaInicio = DateTime.Now.ToString("yyyy-MM-dd");

            if (string.IsNullOrWhiteSpace(fechaFin))
                fechaFin = DateTime.Now.ToString("yyyy-MM-dd");

            string fechaInicioConsulta = DateTime.Parse(fechaInicio).ToString("dd/MM/yyyy");
            string fechaFinConsulta = DateTime.Parse(fechaFin).ToString("dd/MM/yyyy");

            List<ReporteCompra> compras = _reporteServicio.Compra(fechaInicioConsulta, fechaFinConsulta, idproveedor);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string filtro = busqueda.Trim().ToLower();

                compras = compras
                    .Where(c =>
                        (c.FechaRegistro ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.NumeroCompra ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.UsuarioRegistro ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.DocumentoProveedor ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.RazonSocial ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.CodigoProducto ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.NombreProducto ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.Cantidad ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.Categoria ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.PrecioCompra ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.PrecioVenta ?? string.Empty).ToLower().Contains(filtro) 
                    )
                    .ToList();
            }

            return compras.ToList();
        }

        private decimal ConvertirDecimal(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return 0;

            valor = valor.Replace("$", "").Trim();

            if (decimal.TryParse(valor, NumberStyles.Any, new CultureInfo("es-CO"), out decimal resultado))
                return resultado;

            if (decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out resultado))
                return resultado;

            return 0;
        }

        private string LimpiarCsv(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            return valor.Replace(";", ",").Replace("\n", " ").Replace("\r", " ").Trim();
        }
    }
}