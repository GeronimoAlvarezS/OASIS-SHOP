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

        #region DESCARGAR PDF QUESTPDF

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

        #region PDF QUESTPDF

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

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.PageColor("#F4F6FB");
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(12));

                    page.Content().Background(Colors.White).Column(col =>
                    {
                        col.Item().Element(c => HeaderFactura(
                            c,
                            model,
                            logo,
                            nombreNegocio,
                            rucNegocio,
                            direccionNegocio,
                            ciudadNegocio,
                            departamentoNegocio
                        ));

                        col.Item().Padding(25).Column(contenido =>
                        {
                            contenido.Item().Element(c => TituloSeccion(c, "Información general"));

                            contenido.Item().Row(row =>
                            {
                                row.RelativeItem().Element(c =>
                                {
                                    TarjetaInfo(c, new List<(string, string)>
                                    {
                                        ("Número de compra", model.NumeroDocumento),
                                        ("Tipo de factura", model.TipoDocumento),
                                        ("Fecha", model.FechaRegistro)
                                    }, 90);
                                });

                                row.RelativeItem().PaddingLeft(12).Element(c =>
                                {
                                    TarjetaInfo(c, new List<(string, string)>
                                    {
                                        ("Usuario comprador", model.NombreUsuario),
                                        ("Documento", model.DocumentoUsuario)
                                    }, 90);
                                });
                            });

                            contenido.Item().PaddingTop(22).Element(c => TituloSeccion(c, "Información del proveedor"));

                            contenido.Item().Row(row =>
                            {
                                row.RelativeItem().Element(c =>
                                {
                                    TarjetaInfo(c, new List<(string, string)>
                                    {
                                        ("Documento / NIT", model.DocumentoProveedor)
                                    }, 60);
                                });

                                row.RelativeItem().PaddingLeft(12).Element(c =>
                                {
                                    TarjetaInfo(c, new List<(string, string)>
                                    {
                                        ("Nombre / Razón social", model.RazonSocialProveedor)
                                    }, 60);
                                });
                            });

                            contenido.Item().PaddingTop(22).Element(c => TituloSeccion(c, "Productos comprados"));

                            contenido.Item().Element(c => TablaProductos(c, model));

                            contenido.Item().PaddingTop(28).Row(row =>
                            {
                                row.RelativeItem();

                                row.RelativeItem().Element(c =>
                                {
                                    TotalesCompacto(c, model);
                                });
                            });
                        });

                        col.Item()
                        .AlignCenter()
                        .Element(container =>
                        {
                            container
                                .Border(1)
                                .BorderColor("#E5E7EB")
                                .Background("#F3F4F6")
                                .CornerRadius(10)
                                .Padding(12)
                                .MaxWidth(400)
                                .AlignCenter()
                                .Text("Este documento fue generado automáticamente y sirve como respaldo formal de la transacción registrada en el sistema.")
                                .FontSize(11)
                                .FontColor("#4B5563")
                                .AlignCenter();
                        });
                    });
                });
            }).GeneratePdf();
        }

        // ===========================
        // NUEVOS MÉTODOS (TOTALES COMPACTOS)
        // ===========================

        private void TotalesCompacto(IContainer container, DetalleDeCompraViewModel model)
        {
            container
                .Border(1)
                .BorderColor("#E5E7EB")
                .CornerRadius(12)
                .Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Element(c =>
                            CajaTotalSimple(c, "Monto pagado", model.MontoPago.ToString("C0"))
                        );

                        row.RelativeItem().Element(c =>
                            CajaTotalSimple(c, "Monto cambio", model.MontoCambio.ToString("C0"))
                        );
                    });

                    col.Item().Element(c =>
                        CajaTotalFinal(c, "TOTAL COMPRA", model.MontoTotal.ToString("C0"))
                    );
                });
        }

        private void CajaTotalSimple(IContainer container, string etiqueta, string valor)
        {
            container
                .BorderRight(1)
                .BorderColor("#E5E7EB")
                .Padding(10)
                .Column(col =>
                {
                    col.Item().Text(etiqueta)
                        .FontSize(11)
                        .FontColor("#6B7280");

                    col.Item().PaddingTop(2).Text(valor)
                        .FontSize(13)
                        .Bold()
                        .FontColor("#111827");
                });
        }

        private void CajaTotalFinal(IContainer container, string etiqueta, string valor)
        {
            container
                .Background("#4B43D1")
                .Padding(12)
                .Row(row =>
                {
                    row.RelativeItem().Text(etiqueta)
                        .FontSize(13)
                        .Bold()
                        .FontColor(Colors.White);

                    row.RelativeItem().AlignRight().Text(valor)
                        .FontSize(13)
                        .Bold()
                        .FontColor(Colors.White);
                });
        }

        // ===========================
        // MÉTODOS ORIGINALES
        // ===========================

        private void HeaderFactura(
            IContainer container,
            DetalleDeCompraViewModel model,
            byte[] logo,
            string nombreNegocio,
            string rucNegocio,
            string direccionNegocio,
            string ciudadNegocio,
            string departamentoNegocio)
        {
            container
                .Background("#241E65")
                .CornerRadius(18)
                .Padding(28)
                .Row(row =>
                {
                    row.ConstantItem(90)
                       .Height(90)
                       .Background(Colors.White)
                       .CornerRadius(12)
                       .Padding(8)
                       .AlignMiddle()
                       .AlignCenter()
                       .Element(c =>
                       {
                           if (logo.Length > 0)
                           {
                               c.Image(logo).FitArea();
                           }
                           else
                           {
                               c.Text("LOGO")
                                .FontSize(11)
                                .Bold()
                                .FontColor("#4B43D1");
                           }
                       });

                    row.RelativeItem()
                       .PaddingLeft(18)
                       .AlignMiddle()
                       .Column(info =>
                       {
                           info.Item().Text(nombreNegocio)
                               .FontSize(28)
                               .Bold()
                               .FontColor(Colors.White);

                           if (!string.IsNullOrWhiteSpace(rucNegocio))
                           {
                               info.Item().PaddingTop(6).Text($"RUT: {rucNegocio}")
                                   .FontSize(13)
                                   .FontColor("#F3F4F6");
                           }

                           if (!string.IsNullOrWhiteSpace(direccionNegocio))
                           {
                               info.Item().Text($"Dirección: {direccionNegocio}")
                                   .FontSize(13)
                                   .FontColor("#F3F4F6");
                           }

                           if (!string.IsNullOrWhiteSpace(ciudadNegocio) || !string.IsNullOrWhiteSpace(departamentoNegocio))
                           {
                               info.Item().Text($"Ubicación: {ciudadNegocio} - {departamentoNegocio}")
                                   .FontSize(13)
                                   .FontColor("#F3F4F6");
                           }
                       });

                    row.ConstantItem(190)
                       .AlignRight()
                       .Column(col =>
                       {
                           col.Item()
                              .AlignRight()
                              .Text("FACTURA\nDE COMPRA")
                              .FontSize(28)
                              .Bold()
                              .FontColor(Colors.White)
                              .LineHeight(1.05f);

                           col.Item()
                              .PaddingTop(12)
                              .AlignRight()
                              .Width(135)
                              .Background(Colors.White)
                              .CornerRadius(6)
                              .PaddingVertical(8)
                              .AlignCenter()
                              .Text(model.NumeroDocumento)
                              .FontSize(12)
                              .Bold()
                              .FontColor("#241E65");
                       });
                });
        }

        private void TituloSeccion(IContainer container, string titulo)
        {
            container
                .PaddingBottom(10)
                .Row(row =>
                {
                    row.ConstantItem(5)
                       .Height(22)
                       .Background("#4B43D1");

                    row.RelativeItem()
                       .PaddingLeft(8)
                       .AlignMiddle()
                       .Text(titulo)
                       .FontSize(18)
                       .Bold()
                       .FontColor("#241E65");
                });
        }

        private void TarjetaInfo(IContainer container, List<(string Etiqueta, string Valor)> datos, float altoMinimo)
        {
            container
                .Background("#F9FAFB")
                .Border(1)
                .BorderColor("#E5E7EB")
                .CornerRadius(12)
                .Padding(14)
                .MinHeight(altoMinimo)
                .Column(col =>
                {
                    foreach (var item in datos)
                    {
                        col.Item().PaddingVertical(4).Row(row =>
                        {
                            row.RelativeItem()
                               .Text(item.Etiqueta)
                               .FontSize(12)
                               .FontColor("#6B7280");

                            row.RelativeItem()
                               .AlignRight()
                               .Text(item.Valor ?? string.Empty)
                               .FontSize(12)
                               .Bold()
                               .FontColor("#111827");
                        });
                    }
                });
        }

        private void TablaProductos(IContainer container, DetalleDeCompraViewModel model)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50);
                    columns.RelativeColumn();
                    columns.ConstantColumn(120);
                    columns.ConstantColumn(90);
                    columns.ConstantColumn(120);
                });

                table.Header(header =>
                {
                    HeaderCelda(header.Cell(), "Item");
                    HeaderCelda(header.Cell(), "Producto");
                    HeaderCelda(header.Cell(), "Precio compra");
                    HeaderCelda(header.Cell(), "Cantidad");
                    HeaderCelda(header.Cell(), "Subtotal");
                });

                int contador = 1;

                foreach (var item in model.Detalles)
                {
                    BodyCelda(table.Cell(), contador.ToString());
                    BodyCelda(table.Cell(), item.oProducto != null ? item.oProducto.Nombre : "Producto no disponible");
                    BodyCelda(table.Cell(), item.PrecioCompra.ToString("C0"));
                    BodyCelda(table.Cell(), item.Cantidad.ToString());
                    BodyCelda(table.Cell(), item.MontoTotal.ToString("C0"));

                    contador++;
                }
            });
        }

        private void HeaderCelda(IContainer container, string texto)
        {
            container
                .Background("#241E65")
                .PaddingVertical(12)
                .PaddingHorizontal(8)
                .AlignCenter()
                .Text(texto)
                .FontSize(12)
                .Bold()
                .FontColor(Colors.White);
        }

        private void BodyCelda(IContainer container, string texto)
        {
            container
                .BorderBottom(1)
                .BorderColor("#E5E7EB")
                .PaddingVertical(12)
                .PaddingHorizontal(8)
                .AlignCenter()
                .Text(texto ?? string.Empty)
                .FontSize(12)
                .FontColor("#111827");
        }

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