using CapaEntidad;
using CapaNegocio;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace OasisShop.Web.Controllers
{
    public class ProductoController : Controller
    {
        private readonly ProductoServicio _productoServicio = new ProductoServicio();
        private readonly CN_Categoria _categoriaServicio = new CN_Categoria();

        [HttpGet]
        public IActionResult Index(string busqueda = "", int pagina = 1)
        {
            int registrosPorPagina = 10;

            var listaProductos = _productoServicio.Listar();

            var listaCategorias = _categoriaServicio.Listar()
                .Where(c => c.Estado == true)
                .ToList();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string textoBusqueda = busqueda.Trim().ToLower();

                listaProductos = listaProductos.Where(p =>
                    (p.Codigo ?? "").ToLower().Contains(textoBusqueda) ||
                    (p.Nombre ?? "").ToLower().Contains(textoBusqueda) ||
                    (p.Descripcion ?? "").ToLower().Contains(textoBusqueda) ||
                    (p.oCategoria?.Nombre ?? "").ToLower().Contains(textoBusqueda) ||
                    (p.oCategoria?.Descripcion ?? "").ToLower().Contains(textoBusqueda)
                ).ToList();
            }

            int totalRegistros = listaProductos.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            if (pagina < 1)
                pagina = 1;

            if (totalPaginas > 0 && pagina > totalPaginas)
                pagina = totalPaginas;

            var productosPaginados = listaProductos
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToList();

            ViewBag.Categorias = listaCategorias;
            ViewBag.Busqueda = busqueda;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View("~/Views/Producto/Producto.cshtml", productosPaginados);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(Producto obj, int IdCategoria)
        {
            string mensaje = string.Empty;

            obj.oCategoria = new Categoria()
            {
                IdCategoria = IdCategoria
            };

            if (obj.IdProducto == 0)
            {
                int idProductoGenerado = _productoServicio.Registrar(obj, out mensaje);

                if (idProductoGenerado != 0)
                    TempData["MensajeOk"] = "Producto registrado correctamente.";
                else
                    TempData["MensajeError"] = mensaje;
            }
            else
            {
                bool resultado = _productoServicio.Editar(obj, out mensaje);

                if (resultado)
                    TempData["MensajeOk"] = "Producto actualizado correctamente.";
                else
                    TempData["MensajeError"] = mensaje;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int idProducto)
        {
            string mensaje = string.Empty;

            Producto producto = new Producto()
            {
                IdProducto = idProducto
            };

            bool resultado = _productoServicio.Eliminar(producto, out mensaje);

            if (resultado)
                TempData["MensajeOk"] = "Producto eliminado correctamente.";
            else
                TempData["MensajeError"] = mensaje;

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ExportarExcel(string busqueda = "")
        {
            var listaProductos = _productoServicio.Listar();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string textoBusqueda = busqueda.Trim().ToLower();

                listaProductos = listaProductos.Where(p =>
                    (p.Codigo ?? "").ToLower().Contains(textoBusqueda) ||
                    (p.Nombre ?? "").ToLower().Contains(textoBusqueda) ||
                    (p.Descripcion ?? "").ToLower().Contains(textoBusqueda) ||
                    (p.oCategoria?.Nombre ?? "").ToLower().Contains(textoBusqueda) ||
                    (p.oCategoria?.Descripcion ?? "").ToLower().Contains(textoBusqueda)
                ).ToList();
            }

            using (var workbook = new XLWorkbook())
            {
                var hoja = workbook.Worksheets.Add("Productos");

                // Título
                hoja.Cell(1, 1).Value = "LISTADO DE PRODUCTOS";
                hoja.Range("A1:H1").Merge();

                var titulo = hoja.Range("A1:H1");
                titulo.Style.Font.Bold = true;
                titulo.Style.Font.FontSize = 18;
                titulo.Style.Font.FontColor = XLColor.White;
                titulo.Style.Fill.BackgroundColor = XLColor.FromHtml("#241E65");
                titulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                hoja.Row(1).Height = 30;

                hoja.Cell(2, 1).Value = $"Fecha de exportación: {DateTime.Now:dd/MM/yyyy HH:mm}";
                hoja.Range("A2:H2").Merge();
                hoja.Range("A2:H2").Style.Font.Italic = true;
                hoja.Range("A2:H2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // Encabezados
                hoja.Cell(4, 1).Value = "Código";
                hoja.Cell(4, 2).Value = "Nombre";
                hoja.Cell(4, 3).Value = "Descripción";
                hoja.Cell(4, 4).Value = "Categoría";
                hoja.Cell(4, 5).Value = "Stock";
                hoja.Cell(4, 6).Value = "Precio de compra";
                hoja.Cell(4, 7).Value = "Precio de venta";
                hoja.Cell(4, 8).Value = "Estado";

                var encabezado = hoja.Range("A4:H4");
                encabezado.Style.Font.Bold = true;
                encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#3F3CBB");
                encabezado.Style.Font.FontColor = XLColor.White;
                encabezado.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int fila = 5;

                foreach (var item in listaProductos)
                {
                    hoja.Cell(fila, 1).Value = item.Codigo;
                    hoja.Cell(fila, 2).Value = item.Nombre;
                    hoja.Cell(fila, 3).Value = item.Descripcion;

                    // 🔥 MODIFICACIÓN CLAVE
                    hoja.Cell(fila, 4).Value =
                        !string.IsNullOrWhiteSpace(item.oCategoria?.Nombre)
                            ? item.oCategoria.Nombre
                            : "Sin categoría";

                    hoja.Cell(fila, 5).Value = item.Stock;
                    hoja.Cell(fila, 6).Value = item.PrecioCompra;
                    hoja.Cell(fila, 7).Value = item.PrecioVenta;
                    hoja.Cell(fila, 8).Value = item.Estado ? "Activo" : "Inactivo";

                    fila++;
                }

                int ultimaFila = fila - 1;

                if (ultimaFila >= 5)
                {
                    var tabla = hoja.Range($"A4:H{ultimaFila}").CreateTable();
                    tabla.Theme = XLTableTheme.TableStyleMedium9;

                    hoja.Range($"F5:G{ultimaFila}").Style.NumberFormat.Format = "$ #,##0";
                }

                hoja.Columns().AdjustToContents();
                hoja.SheetView.FreezeRows(4);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Productos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                    );
                }
            }
        }
    }
}