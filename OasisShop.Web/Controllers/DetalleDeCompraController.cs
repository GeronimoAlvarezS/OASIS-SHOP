using CapaEntidad;
using CapaNegocio;
using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;

namespace OasisShop.Web.Controllers
{
    public class DetalleDeCompraController : Controller
    {
        private readonly CompraServicio _compraServicio = new CompraServicio();
        private readonly CN_Negocio _negocioServicio = new CN_Negocio();

        #region CONSULTAR COMPRA

        [HttpGet]
        public IActionResult Index(string numeroCompra)
        {
            DetalleDeCompraViewModel model = new DetalleDeCompraViewModel();

            ViewBag.NumeroCompraBuscado = numeroCompra;

            if (string.IsNullOrWhiteSpace(numeroCompra))
            {
                return View("~/Views/Compra/DetalleDeCompra.cshtml", model);
            }

            string numeroNormalizado = NormalizarNumeroCompra(numeroCompra);

            Compra compra = _compraServicio.ObtenerCompra(numeroNormalizado);

            if (compra == null || compra.IdCompra == 0)
            {
                TempData["MensajeError"] = "No se encontró una compra registrada con el número ingresado.";
                return View("~/Views/Compra/DetalleDeCompra.cshtml", model);
            }

            model = MapearCompraAViewModel(compra);

            return View("~/Views/Compra/DetalleDeCompra.cshtml", model);
        }

        #endregion

        #region DESCARGAR PDF

        [HttpGet]
        public IActionResult DescargarPdf(int idCompra)
        {
            if (idCompra <= 0)
            {
                TempData["MensajeWarning"] = "Identificador de compra inválido.";
                return RedirectToAction("Index");
            }

            Compra compra = _compraServicio.ObtenerCompraDetalle(idCompra);

            if (compra == null || compra.IdCompra == 0)
            {
                TempData["MensajeError"] = "No fue posible generar el PDF porque la compra no existe.";
                return RedirectToAction("Index");
            }

            DetalleDeCompraViewModel model = MapearCompraAViewModel(compra);
            Negocio negocio = _negocioServicio.ObtenerDatos();

            QuestPDF.Settings.License = LicenseType.Community;

            byte[] pdfBytes = GenerarFacturaCompraPdf(model, negocio);

            return File(
                pdfBytes,
                "application/pdf",
                $"Comprobante_Compra_{model.NumeroDocumento}.pdf"
            );
        }

        #endregion

        #region PDF FACTURA COMPRA

        private byte[] GenerarFacturaCompraPdf(DetalleDeCompraViewModel model, Negocio negocio)
        {
            byte[] logo = negocio?.Logo ?? Array.Empty<byte>();

            string nombreNegocio = !string.IsNullOrWhiteSpace(negocio?.Nombre)
                ? negocio.Nombre
                : "Oasis Shop";

            string rucNegocio = negocio?.RUC ?? string.Empty;
            string direccionNegocio = negocio?.Direccion ?? string.Empty;
            string ciudadNegocio = negocio?.oCiudad?.Nombre ?? string.Empty;
            string departamentoNegocio = negocio?.oDepartamento?.Nombre ?? string.Empty;
            string correoNegocio = negocio?.Correo ?? "No registrado";

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9));
                    page.PageColor(Colors.White);

                    page.Content().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(170).Height(75).Element(c =>
                            {
                                if (logo.Length > 0)
                                    c.Image(logo).FitArea();
                                else
                                    c.Text(nombreNegocio).Bold().FontSize(18);
                            });

                            row.RelativeItem().AlignCenter().Column(info =>
                            {
                                info.Item().AlignCenter().Text(nombreNegocio.ToUpper()).Bold().FontSize(16);
                                info.Item().AlignCenter().Text($"NIT: {rucNegocio}").FontSize(9);
                                info.Item().AlignCenter().Text($"{ciudadNegocio} - {departamentoNegocio}").FontSize(9);
                                info.Item().AlignCenter().Text(direccionNegocio).FontSize(9);
                                info.Item().AlignCenter().Text(correoNegocio).FontSize(9);
                            });

                            row.ConstantItem(200)
                                .Border(1)
                                .BorderColor("#D1D5DB")
                                .Padding(10)
                                .Column(box =>
                                {
                                    box.Item().AlignCenter().Text("FACTURA DE COMPRA DE INVENTARIO").Bold().FontSize(13);
                                    box.Item().PaddingTop(8).AlignCenter().Text(model.NumeroDocumento).Bold().FontSize(12);

                                    box.Item().PaddingTop(10).Text("Fecha de Compra").Bold().FontSize(8);
                                    box.Item().Text(model.FechaRegistro).FontSize(8);
                                });
                        });

                        col.Item().PaddingTop(16).Row(row =>
                        {
                            row.RelativeItem().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(120);
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(120);
                                    columns.RelativeColumn();
                                });

                                InfoTableRowCompra(table, "Proveedor", model.RazonSocialProveedor, "Documento / NIT", model.DocumentoProveedor);
                                InfoTableRowCompra(table, "Dirección", direccionNegocio, "Comprador", model.NombreUsuario);
                                InfoTableRowCompra(table, "Ciudad", $"{ciudadNegocio} - {departamentoNegocio}", "Correo", correoNegocio);
                                InfoTableRowCompra(table, "Número de Compra", model.NumeroDocumento, "Tipo Documento", model.TipoDocumento);
                            });
                        });

                        col.Item().PaddingTop(24).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(110);
                                columns.ConstantColumn(110);
                            });

                            CompraHeaderCell(table.Cell(), "Código");
                            CompraHeaderCell(table.Cell(), "Nombre del producto");
                            CompraHeaderCell(table.Cell(), "Cantidad");
                            CompraHeaderCell(table.Cell(), "Valor Unitario");
                            CompraHeaderCell(table.Cell(), "Valor Total");

                            foreach (var item in model.Detalles)
                            {
                                CompraBodyCell(table.Cell(), item.oProducto != null ? item.oProducto.Codigo : "N/A");
                                CompraBodyCell(table.Cell(), item.oProducto != null ? item.oProducto.Nombre : "N/A");
                                CompraBodyCell(table.Cell(), item.Cantidad.ToString());
                                CompraBodyCell(table.Cell(), item.PrecioCompra.ToString("C0"));
                                CompraBodyCell(table.Cell(), item.MontoTotal.ToString("C0"));
                            }
                        });

                        col.Item().PaddingTop(24).Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                left.Item().Text("CONDICIÓN DE PAGO").Bold().FontSize(9);
                                left.Item().PaddingTop(5).Text("Crédito / Contado").FontSize(8);

                                left.Item().PaddingTop(14).Text("OBSERVACIONES").Bold().FontSize(9);
                                left.Item().Text("Factura de compra generada desde el sistema Oasis Shop.").FontSize(8);
                            });

                            row.ConstantItem(280).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                TotalRowCompra(table, "Total Bruto", model.MontoTotal.ToString("C0"), false);
                                TotalRowCompra(table, "IVA", "$0", false);
                                TotalRowCompra(table, "Monto Pagado", model.MontoPago.ToString("C0"), false);
                                TotalRowCompra(table, "Monto Cambio", model.MontoCambio.ToString("C0"), false);
                                TotalRowCompra(table, "Total Compra", model.MontoTotal.ToString("C0"), true);
                            });
                        });

                        col.Item().PaddingTop(22).AlignCenter()
                            .Text("Documento generado automáticamente por el sistema Oasis Shop.")
                            .FontSize(8)
                            .FontColor("#4B5563");

                        col.Item().PaddingTop(4).AlignCenter()
                            .Text($"{nombreNegocio} - NIT: {rucNegocio}")
                            .Bold()
                            .FontSize(8);
                    });
                });
            }).GeneratePdf();
        }

        #endregion

        #region MÉTODOS AUXILIARES PDF

        private void InfoTableRowCompra(TableDescriptor table, string label1, string value1, string label2, string value2)
        {
            InfoLabelCellCompra(table.Cell(), label1);
            InfoValueCellCompra(table.Cell(), value1);
            InfoLabelCellCompra(table.Cell(), label2);
            InfoValueCellCompra(table.Cell(), value2);
        }

        private void InfoLabelCellCompra(IContainer container, string texto)
        {
            container
                .Background("#E5E7EB")
                .Border(1)
                .BorderColor("#D1D5DB")
                .Padding(5)
                .Text(texto)
                .Bold()
                .FontSize(8);
        }

        private void InfoValueCellCompra(IContainer container, string texto)
        {
            container
                .Border(1)
                .BorderColor("#D1D5DB")
                .Padding(5)
                .Text(texto ?? string.Empty)
                .FontSize(8);
        }

        private void CompraHeaderCell(IContainer container, string texto)
        {
            container
                .Background("#E5E7EB")
                .Border(1)
                .BorderColor("#D1D5DB")
                .Padding(6)
                .AlignCenter()
                .Text(texto)
                .Bold()
                .FontSize(8);
        }

        private void CompraBodyCell(IContainer container, string texto)
        {
            container
                .Border(1)
                .BorderColor("#D1D5DB")
                .Padding(6)
                .AlignCenter()
                .Text(texto ?? string.Empty)
                .FontSize(8);
        }

        private void TotalRowCompra(TableDescriptor table, string label, string value, bool destacado)
        {
            table.Cell()
                .Background(destacado ? "#E5E7EB" : Colors.White)
                .Border(1)
                .BorderColor("#D1D5DB")
                .Padding(6)
                .Text(label)
                .Bold()
                .FontSize(destacado ? 10 : 8);

            table.Cell()
                .Background(destacado ? "#E5E7EB" : Colors.White)
                .Border(1)
                .BorderColor("#D1D5DB")
                .Padding(6)
                .AlignRight()
                .Text(value)
                .Bold()
                .FontSize(destacado ? 10 : 8);
        }

        #endregion

        #region MAPEO Y NORMALIZACIÓN

        private DetalleDeCompraViewModel MapearCompraAViewModel(Compra compra)
        {
            return new DetalleDeCompraViewModel
            {
                IdCompra = compra.IdCompra,
                NumeroDocumento = compra.NumeroDocumento,
                TipoDocumento = "Factura Electrónica",
                FechaRegistro = compra.FechaRegistro,

                NombreUsuario = compra.oUsuario != null
                    ? compra.oUsuario.NombreCompleto
                    : "No disponible",

                DocumentoUsuario = compra.oUsuario != null
                    ? compra.oUsuario.Documento
                    : "No disponible",

                DocumentoProveedor = compra.oProveedor != null
                    ? compra.oProveedor.Documento
                    : "No disponible",

                RazonSocialProveedor = compra.oProveedor != null
                    ? compra.oProveedor.RazonSocial
                    : "No disponible",

                MontoTotal = compra.MontoTotal,
                MontoPago = compra.MontoPagado,
                MontoCambio = compra.MontoCambio,

                Detalles = compra.oDetalleCompra ?? new List<Detalle_Compra>()
            };
        }

        private string NormalizarNumeroCompra(string numeroCompra)
        {
            numeroCompra = numeroCompra.Trim().ToUpper();

            if (numeroCompra.StartsWith("COMPRA-"))
            {
                string consecutivo = numeroCompra.Replace("COMPRA-", "").Trim();

                if (int.TryParse(consecutivo, out int numero))
                {
                    return "COMPRA-" + numero.ToString("D6");
                }

                return numeroCompra;
            }

            if (int.TryParse(numeroCompra, out int numeroCompraEntero))
            {
                return "COMPRA-" + numeroCompraEntero.ToString("D6");
            }

            return numeroCompra;
        }

        #endregion
    }
}