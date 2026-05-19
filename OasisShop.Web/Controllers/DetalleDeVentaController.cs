using CapaEntidad;
using CapaNegocio;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OasisShop.Web.Controllers
{
    public class DetalleDeVentaController : Controller
    {
        private readonly VentaServicio _ventaServicio = new VentaServicio();
        private readonly CN_Negocio _negocioServicio = new CN_Negocio();
        private readonly IWebHostEnvironment _environment;

        public DetalleDeVentaController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        #region CONSULTAR VENTA

        [HttpGet]
        public IActionResult Index(string numeroVenta)
        {
            DetalleDeVentaViewModel model = new DetalleDeVentaViewModel();

            ViewBag.NumeroVentaBuscado = numeroVenta;

            if (string.IsNullOrWhiteSpace(numeroVenta))
            {
                return View("~/Views/Venta/DetalleDeVenta.cshtml", model);
            }

            string numeroNormalizado = NormalizarNumeroVenta(numeroVenta);

            Venta venta = _ventaServicio.ObtenerVenta(numeroNormalizado);

            if (venta == null || venta.IdVenta == 0)
            {
                TempData["MensajeError"] = "No se encontró una venta registrada con el número ingresado.";
                return View("~/Views/Venta/DetalleDeVenta.cshtml", model);
            }

            model = MapearVentaAViewModel(venta);

            return View("~/Views/Venta/DetalleDeVenta.cshtml", model);
        }

        [HttpGet]
        public IActionResult ValidarFacturaElectronica(string cufe)
        {
            ViewBag.CUFE = cufe;
            return View("~/Views/Venta/ValidarFacturaElectronica.cshtml");
        }

        #endregion

        #region DESCARGAR PDF

        [HttpGet]
        public IActionResult DescargarPdf(string numeroVenta)
        {
            if (string.IsNullOrWhiteSpace(numeroVenta))
            {
                TempData["MensajeWarning"] = "Número de venta inválido.";
                return RedirectToAction("Index");
            }

            Venta venta = _ventaServicio.ObtenerVenta(numeroVenta);

            if (venta == null || venta.IdVenta == 0)
            {
                TempData["MensajeError"] = "No fue posible generar el PDF porque la venta no existe.";
                return RedirectToAction("Index");
            }

            DetalleDeVentaViewModel model = MapearVentaAViewModel(venta);
            Negocio negocio = _negocioServicio.ObtenerDatos();

            QuestPDF.Settings.License = LicenseType.Community;

            byte[] pdfBytes = model.EsFacturaElectronica
                ? GenerarFacturaElectronicaPdf(model, negocio)
                : GenerarFacturaFisicaPdf(model, negocio);

            return File(
                pdfBytes,
                "application/pdf",
                $"Comprobante_Venta_{model.NumeroDocumento}.pdf"
            );
        }

        #endregion

        #region FACTURA FÍSICA SIN QR

        //FACTURA FÍSICA
        private byte[] GenerarFacturaFisicaPdf(DetalleDeVentaViewModel model, Negocio negocio)
        {
            byte[] logo = negocio?.Logo ?? Array.Empty<byte>();

            string nombreNegocio = !string.IsNullOrWhiteSpace(negocio?.Nombre)
                ? negocio.Nombre
                : "Oasis Shop";

            string rucNegocio = negocio?.RUC ?? string.Empty;
            string ciudadNegocio = negocio?.oCiudad?.Nombre ?? string.Empty;
            string departamentoNegocio = negocio?.oDepartamento?.Nombre ?? string.Empty;
            string direccionNegocio = negocio?.Direccion ?? string.Empty;

            decimal subTotalVenta = model.Subtotal > 0
                ? model.Subtotal
                : model.Detalles.Sum(x => x.SubTotal);

            decimal descuento = model.Descuento;

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(226, 800);
                    page.Margin(10);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(8));
                    page.PageColor(Colors.White);

                    page.Content().Column(col =>
                    {
                        col.Item()
                            .AlignCenter()
                            .Width(120)
                            .Height(55)
                            .AlignMiddle()
                            .AlignCenter()
                            .Element(c =>
                            {
                                if (logo.Length > 0)
                                {
                                    c.AlignCenter().Image(logo).FitArea();
                                }
                                else
                                {
                                    c.AlignCenter().Text(nombreNegocio).Bold().FontSize(14);
                                }
                            });

                        col.Item().AlignCenter().Text(nombreNegocio).Bold().FontSize(9);
                        col.Item().AlignCenter().Text($"NIT: {rucNegocio}").FontSize(8);

                        if (!string.IsNullOrWhiteSpace(departamentoNegocio) || !string.IsNullOrWhiteSpace(ciudadNegocio))
                        {
                            col.Item().AlignCenter().Text($"{departamentoNegocio} - {ciudadNegocio}").FontSize(8);
                        }

                        if (!string.IsNullOrWhiteSpace(direccionNegocio))
                        {
                            col.Item().AlignCenter().Text(direccionNegocio).FontSize(8);
                        }

                        col.Item().PaddingVertical(5).LineHorizontal(1);

                        col.Item().AlignCenter().Text("FACTURA DE VENTA").Bold().FontSize(9);
                        col.Item().AlignCenter().Text($"No. {model.NumeroDocumento}").Bold().FontSize(8);

                        col.Item().PaddingVertical(5).LineHorizontal(1);

                        col.Item().Text($"Fecha: {model.FechaRegistro}").FontSize(8);

                        col.Item().PaddingTop(5).Text($"Cliente: {model.NombreCliente}").FontSize(8);
                        col.Item().Text($"Documento: {model.DocumentoCliente}").FontSize(8);
                        col.Item().Text($"Vendedor: {model.NombreUsuario}").FontSize(8);
                        col.Item().Text($"Tipo factura: {model.TipoFactura}").FontSize(8);

                        col.Item().PaddingVertical(5).LineHorizontal(1);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);
                                columns.RelativeColumn();
                                columns.ConstantColumn(38);
                                columns.ConstantColumn(42);
                            });

                            table.Header(header =>
                            {
                                CeldaFacturaFisicaHeader(header.Cell(), "Cant");
                                CeldaFacturaFisicaHeader(header.Cell(), "Detalle");
                                CeldaFacturaFisicaHeader(header.Cell(), "Precio");
                                CeldaFacturaFisicaHeader(header.Cell(), "SubTotal");
                            });

                            foreach (var item in model.Detalles)
                            {
                                CeldaFacturaFisicaBody(table.Cell(), item.Cantidad.ToString());
                                CeldaFacturaFisicaBody(table.Cell(), item.oProducto != null ? item.oProducto.Nombre : "N/A");
                                CeldaFacturaFisicaBody(table.Cell(), item.PrecioVenta.ToString("C0"));
                                CeldaFacturaFisicaBody(table.Cell(), item.SubTotal.ToString("C0"));
                            }
                        });

                        col.Item().PaddingVertical(8).LineHorizontal(1);

                        if (descuento > 0)
                        {
                            col.Item().AlignRight().Text($"Descuento: {descuento:C0}").FontSize(8);
                        }

                        col.Item().AlignRight().Text($"Monto pagado: {model.MontoPago:C0}").FontSize(8);
                        col.Item().AlignRight().Text($"Monto cambio: {model.MontoCambio:C0}").FontSize(8);

                        col.Item().AlignRight()
                            .Text($"TOTAL: {model.MontoTotal:C0}")
                            .Bold()
                            .FontSize(14);

                        col.Item().PaddingTop(5).AlignRight()
                            .Text($"Cantidad items: {model.Detalles.Count}")
                            .Bold()
                            .FontSize(8);

                        col.Item().PaddingVertical(8).LineHorizontal(1);

                        col.Item().PaddingTop(12).AlignCenter()
                            .Text("Documento generado automáticamente por el sistema Oasis Shop.")
                            .FontSize(7);
                    });
                });
            }).GeneratePdf();
        }

        private void CeldaFacturaFisicaHeader(IContainer container, string texto)
        {
            container
                .BorderBottom(1)
                .BorderColor(Colors.Black)
                .PaddingVertical(2)
                .AlignCenter()
                .Text(texto)
                .Bold()
                .FontSize(7);
        }

        private void CeldaFacturaFisicaBody(IContainer container, string texto)
        {
            container
                .PaddingVertical(2)
                .AlignCenter()
                .Text(texto ?? string.Empty)
                .FontSize(7);
        }

        #endregion

        #region FACTURA ELECTRÓNICA CON QR Y CUFE

        //FACTURA ELECTRÓNICA

        private byte[] GenerarFacturaElectronicaPdf(DetalleDeVentaViewModel model, Negocio negocio)
        {

            byte[] logo = negocio?.Logo ?? Array.Empty<byte>();
            byte[] qr = GenerarQrFacturaElectronica(model);

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

                            row.ConstantItem(110).AlignRight().Column(qrCol =>
                            {
                                qrCol.Item().Width(100).Height(100).Image(qr);
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

                                InfoTableRow(table, "Cliente", model.NombreCliente, "Documento Cliente", model.DocumentoCliente);
                                InfoTableRow(table, "Dirección", direccionNegocio, "Vendedor", model.NombreUsuario);
                                InfoTableRow(table, "Ciudad", $"{ciudadNegocio} - {departamentoNegocio}", "Correo", correoNegocio);
                                InfoTableRow(table, "Número de Venta", model.NumeroDocumento, "", "");
                            });

                            row.ConstantItem(200).PaddingLeft(12).Border(1).BorderColor("#D1D5DB").Padding(10).Column(box =>
                            {
                                box.Item().AlignCenter().Text("FACTURA ELECTRÓNICA").Bold().FontSize(13);
                                box.Item().PaddingTop(8).AlignCenter().Text(model.NumeroDocumento).Bold().FontSize(12);

                                box.Item().PaddingTop(10).Text("Fecha y Hora de Facturación").Bold().FontSize(8);
                                box.Item().Text(model.FechaRegistro).FontSize(8);
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

                            ElectronicHeaderCell(table.Cell(), "Código");
                            ElectronicHeaderCell(table.Cell(), "Nombre del producto");
                            ElectronicHeaderCell(table.Cell(), "Cantidad");
                            ElectronicHeaderCell(table.Cell(), "Valor Unitario");
                            ElectronicHeaderCell(table.Cell(), "Valor Total");

                            foreach (var item in model.Detalles)
                            {
                                ElectronicBodyCell(table.Cell(), item.oProducto != null ? item.oProducto.Codigo : "N/A");
                                ElectronicBodyCell(table.Cell(), item.oProducto != null ? item.oProducto.Nombre : "N/A");
                                ElectronicBodyCell(table.Cell(), item.Cantidad.ToString());
                                ElectronicBodyCell(table.Cell(), item.PrecioVenta.ToString("C0"));
                                ElectronicBodyCell(table.Cell(), item.SubTotal.ToString("C0"));
                            }
                        });

                        col.Item().PaddingTop(24).Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                left.Item().Text("CONDICIÓN DE PAGO").Bold().FontSize(9);
                                left.Item().PaddingTop(5).Text("Crédito / Contado").FontSize(8);

                                left.Item().PaddingTop(14).Text("OBSERVACIONES").Bold().FontSize(9);
                                left.Item().Text("Factura electrónica generada desde el sistema Oasis Shop.").FontSize(8);
                            });

                            row.ConstantItem(280).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                TotalRow(table, "Total Bruto", model.Subtotal.ToString("C0"), false);

                                if (model.Descuento > 0)
                                {
                                    TotalRow(table, "Descuento", model.Descuento.ToString("C0"), false);
                                }

                                TotalRow(table, "IVA", "$0", false);
                                TotalRow(table, "Monto Cambio", model.MontoCambio.ToString("C0"), false);
                                TotalRow(table, "Total a Pagar", model.MontoTotal.ToString("C0"), true);
                            });
                        });

                        col.Item().PaddingTop(24).Row(row =>
                        {
                            row.RelativeItem();

                            row.ConstantItem(360)
                                .Border(1)
                                .BorderColor("#93C5FD")
                                .Padding(10)
                                .Column(cufe =>
                                {
                                    cufe.Item().AlignCenter().Text("CUFE:").Bold().FontSize(9);
                                    cufe.Item().AlignCenter().Text(model.CUFE).FontSize(8);
                                });

                            row.RelativeItem();
                        });

                        col.Item().PaddingTop(18).AlignCenter()
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

        private byte[] GenerarQrFacturaElectronica(DetalleDeVentaViewModel model)
        {
            string contenidoQr =
                "DIAN\n" +
                "SU FACTURA ELECTRÓNICA ES VÁLIDA\n" +
                $"Número: {model.NumeroDocumento}\n" +
                $"CUFE: {model.CUFE}\n" +
                $"Fecha: {model.FechaRegistro}\n" +
                $"Cliente: {model.DocumentoCliente}\n" +
                $"Total: {model.MontoTotal:C0}";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(contenidoQr, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(20);
            }
        }

        private byte[] ObtenerImagenDian()
        {
            try
            {
                string ruta = Path.Combine(_environment.WebRootPath, "img", "Dian.png");

                if (System.IO.File.Exists(ruta))
                {
                    return System.IO.File.ReadAllBytes(ruta);
                }
            }
            catch
            {
                return Array.Empty<byte>();
            }

            return Array.Empty<byte>();
        }

        #endregion

        #region MÉTODOS AUXILIARES PDF ELECTRÓNICO

        private void InfoTableRow(TableDescriptor table, string label1, string value1, string label2, string value2)
        {
            InfoLabelCell(table.Cell(), label1);
            InfoValueCell(table.Cell(), value1);
            InfoLabelCell(table.Cell(), label2);
            InfoValueCell(table.Cell(), value2);
        }

        private void InfoLabelCell(IContainer container, string texto)
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

        private void InfoValueCell(IContainer container, string texto)
        {
            container
                .Border(1)
                .BorderColor("#D1D5DB")
                .Padding(5)
                .Text(texto ?? string.Empty)
                .FontSize(8);
        }

        private void ElectronicHeaderCell(IContainer container, string texto)
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

        private void ElectronicBodyCell(IContainer container, string texto)
        {
            container
                .Border(1)
                .BorderColor("#D1D5DB")
                .Padding(6)
                .AlignCenter()
                .Text(texto ?? string.Empty)
                .FontSize(8);
        }

        private void TotalRow(TableDescriptor table, string label, string value, bool destacado)
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

        private DetalleDeVentaViewModel MapearVentaAViewModel(Venta venta)
        {
            string tipoFactura = venta.oTipoFactura != null
                ? venta.oTipoFactura.Nombre
                : "No disponible";

            bool esFacturaElectronica = tipoFactura.ToLower().Contains("electronica")
                || tipoFactura.ToLower().Contains("electrónica");

            List<Detalle_Venta> detalles = venta.oDetalle_Venta ?? new List<Detalle_Venta>();

            decimal subTotal = 0;

            foreach (var item in detalles)
            {
                subTotal += item.SubTotal;
            }

            decimal descuento = subTotal > venta.MontoTotal
                ? subTotal - venta.MontoTotal
                : 0;

            DetalleDeVentaViewModel model = new DetalleDeVentaViewModel
            {
                IdVenta = venta.IdVenta,
                NumeroDocumento = venta.NumeroDocumento,
                TipoFactura = tipoFactura,
                FechaRegistro = venta.FechaRegistro,

                NombreUsuario = venta.oUsuario != null
                    ? venta.oUsuario.NombreCompleto
                    : "No disponible",

                DocumentoUsuario = venta.oUsuario != null
                    ? venta.oUsuario.Documento
                    : "No disponible",

                DocumentoCliente = venta.DocumentoCliente,
                NombreCliente = venta.NombreCliente,

                MontoTotal = venta.MontoTotal,
                MontoPago = venta.MontoPago,
                MontoCambio = venta.MontoCambio,

                Subtotal = subTotal,
                Descuento = descuento,

                EsFacturaElectronica = esFacturaElectronica,

                Detalles = detalles
            };

            model.CUFE = GenerarCUFE(model);

            return model;
        }

        private string GenerarCUFE(DetalleDeVentaViewModel model)
        {
            string textoBase =
                $"{model.NumeroDocumento}|{model.FechaRegistro}|{model.DocumentoCliente}|{model.MontoTotal}|OASISSHOP";

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(textoBase));
                StringBuilder builder = new StringBuilder();

                foreach (byte item in bytes)
                {
                    builder.Append(item.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private string NormalizarNumeroVenta(string numeroVenta)
        {
            numeroVenta = numeroVenta.Trim().ToUpper();

            if (numeroVenta.StartsWith("VENTA-"))
            {
                string consecutivo = numeroVenta.Replace("VENTA-", "").Trim();

                if (int.TryParse(consecutivo, out int numero))
                {
                    return "VENTA-" + numero.ToString("D6");
                }

                return numeroVenta;
            }

            if (int.TryParse(numeroVenta, out int numeroVentaEntero))
            {
                return "VENTA-" + numeroVentaEntero.ToString("D6");
            }

            return numeroVenta;
        }

        #endregion
    }
}