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

namespace OasisShop.Web.Controllers
{
    public class ReporteDeVentaController : Controller
    {
        private readonly ReporteServicio _reporteServicio = new ReporteServicio();

        [HttpGet]
        public IActionResult Index(string fechaInicio = "", string fechaFin = "", string busqueda = "", int pagina = 1)
        {
            int registrosPorPagina = 5;

            if (string.IsNullOrWhiteSpace(fechaInicio))
                fechaInicio = DateTime.Now.ToString("yyyy-MM-dd");

            if (string.IsNullOrWhiteSpace(fechaFin))
                fechaFin = DateTime.Now.ToString("yyyy-MM-dd");

            List<ReporteVenta> ventas = ObtenerVentasFiltradas(fechaInicio, fechaFin, busqueda);

            int totalRegistros = ventas.Count;
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            if (pagina < 1)
                pagina = 1;

            if (totalPaginas > 0 && pagina > totalPaginas)
                pagina = totalPaginas;

            List<ReporteVenta> ventasPaginadas = ventas
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToList();

            ReporteDeVentasViewModel model = new ReporteDeVentasViewModel
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Busqueda = busqueda,
                Ventas = ventasPaginadas,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalVentas = ventas.Count,
                MontoTotalVendido = ventas.Sum(v => ConvertirDecimal(v.SubTotal))
            };

            return View("~/Views/Reporte/ReporteDeVenta.cshtml", model);
        }

        [HttpGet]
        public IActionResult ExportarCsv(string fechaInicio = "", string fechaFin = "", string busqueda = "")
        {
            List<ReporteVenta> ventas = ObtenerVentasFiltradas(fechaInicio, fechaFin, busqueda);

            StringBuilder csv = new StringBuilder();

            csv.AppendLine("Fecha de registro;Numero de venta;Tipo de factura;Documento cliente;Nombre cliente;Codigo producto;Nombre producto;Categoria;Precio de venta");

            foreach (var venta in ventas)
            {
                csv.AppendLine(
                    $"{LimpiarCsv(venta.FechaRegistro)};" +
                    $"{LimpiarCsv(venta.NumeroDocumento)};" +
                    $"{LimpiarCsv(venta.Nombre)};" +
                    $"{LimpiarCsv(venta.DocumentoCliente)};" +
                    $"{LimpiarCsv(venta.NombreCliente)};" +
                    $"{LimpiarCsv(venta.CodigoProducto)};" +
                    $"{LimpiarCsv(venta.NombreProducto)};" +
                    $"{LimpiarCsv(venta.Categoria)};" +
                    $"{LimpiarCsv(venta.PrecioVenta)}"
                );
            }

            byte[] bytes = Encoding.UTF8.GetBytes(csv.ToString());

            return File(bytes, "text/csv", $"ReporteVentas_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        [HttpGet]
        public IActionResult ExportarExcel(string fechaInicio = "", string fechaFin = "", string busqueda = "")
        {
            List<ReporteVenta> ventas = ObtenerVentasFiltradas(fechaInicio, fechaFin, busqueda);

            using (var workbook = new XLWorkbook())
            {
                var hoja = workbook.Worksheets.Add("Reporte de ventas");

                hoja.Cell(1, 1).Value = "REPORTE DE VENTAS";
                hoja.Range("A1:I1").Merge();

                var titulo = hoja.Range("A1:I1");
                titulo.Style.Font.Bold = true;
                titulo.Style.Font.FontSize = 18;
                titulo.Style.Font.FontColor = XLColor.White;
                titulo.Style.Fill.BackgroundColor = XLColor.FromHtml("#241E65");
                titulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                hoja.Row(1).Height = 30;

                hoja.Cell(2, 1).Value = $"Fecha de exportación: {DateTime.Now:dd/MM/yyyy HH:mm}";
                hoja.Range("A2:I2").Merge();
                hoja.Range("A2:I2").Style.Font.Italic = true;
                hoja.Range("A2:I2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                hoja.Cell(4, 1).Value = "Fecha de registro";
                hoja.Cell(4, 2).Value = "Número de venta";
                hoja.Cell(4, 3).Value = "Tipo de factura";
                hoja.Cell(4, 4).Value = "Documento cliente";
                hoja.Cell(4, 5).Value = "Nombre cliente";
                hoja.Cell(4, 6).Value = "Código producto";
                hoja.Cell(4, 7).Value = "Nombre producto";
                hoja.Cell(4, 8).Value = "Categoría";
                hoja.Cell(4, 9).Value = "Precio de venta";

                var encabezado = hoja.Range("A4:I4");
                encabezado.Style.Font.Bold = true;
                encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#3F3CBB");
                encabezado.Style.Font.FontColor = XLColor.White;
                encabezado.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int fila = 5;

                foreach (var item in ventas)
                {
                    hoja.Cell(fila, 1).Value = item.FechaRegistro;
                    hoja.Cell(fila, 2).Value = item.NumeroDocumento;
                    hoja.Cell(fila, 3).Value = item.Nombre;
                    hoja.Cell(fila, 4).Value = item.DocumentoCliente;
                    hoja.Cell(fila, 5).Value = item.NombreCliente;
                    hoja.Cell(fila, 6).Value = item.CodigoProducto;
                    hoja.Cell(fila, 7).Value = item.NombreProducto;
                    hoja.Cell(fila, 8).Value = item.Categoria;
                    hoja.Cell(fila, 9).Value = ConvertirDecimal(item.PrecioVenta);

                    fila++;
                }

                int ultimaFila = fila - 1;

                if (ultimaFila >= 5)
                {
                    var tabla = hoja.Range($"A4:I{ultimaFila}").CreateTable();
                    tabla.Theme = XLTableTheme.TableStyleMedium9;

                    hoja.Range($"I5:I{ultimaFila}").Style.NumberFormat.Format = "$ #,##0";
                }

                hoja.Columns().AdjustToContents();
                hoja.SheetView.FreezeRows(4);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"ReporteVentas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                    );
                }
            }
        }

        private List<ReporteVenta> ObtenerVentasFiltradas(string fechaInicio, string fechaFin, string busqueda)
        {
            if (string.IsNullOrWhiteSpace(fechaInicio))
                fechaInicio = DateTime.Now.ToString("yyyy-MM-dd");

            if (string.IsNullOrWhiteSpace(fechaFin))
                fechaFin = DateTime.Now.ToString("yyyy-MM-dd");

            string fechaInicioConsulta = DateTime.Parse(fechaInicio).ToString("yyyy-MM-dd");
            string fechaFinConsulta = DateTime.Parse(fechaFin).ToString("yyyy-MM-dd");

            List<ReporteVenta> ventas = _reporteServicio.Venta(fechaInicioConsulta, fechaFinConsulta);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string filtro = busqueda.Trim().ToLower();

                ventas = ventas
                    .Where(v =>
                        (v.NumeroDocumento ?? string.Empty).ToLower().Contains(filtro) ||
                        (v.Nombre ?? string.Empty).ToLower().Contains(filtro) ||
                        (v.DocumentoCliente ?? string.Empty).ToLower().Contains(filtro) ||
                        (v.NombreCliente ?? string.Empty).ToLower().Contains(filtro) ||
                        (v.CodigoProducto ?? string.Empty).ToLower().Contains(filtro) ||
                        (v.NombreProducto ?? string.Empty).ToLower().Contains(filtro) ||
                        (v.Categoria ?? string.Empty).ToLower().Contains(filtro) ||
                        (v.FechaRegistro ?? string.Empty).ToLower().Contains(filtro)
                    )
                    .ToList();
            }

            return ventas
                .OrderByDescending(v => ConvertirFecha(v.FechaRegistro))
                .ToList();
        }

        private DateTime ConvertirFecha(string fecha)
        {
            if (DateTime.TryParseExact(fecha, "dd/MM/yyyy", new CultureInfo("es-CO"), DateTimeStyles.None, out DateTime resultado))
                return resultado;

            if (DateTime.TryParse(fecha, out resultado))
                return resultado;

            return DateTime.MinValue;
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