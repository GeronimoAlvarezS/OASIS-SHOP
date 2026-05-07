using CapaEntidad;
using CapaNegocio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;
using System.Data;
using System.Linq;

namespace OasisShop.Web.Controllers
{
    public class CompraController : Controller
    {
        private readonly CompraServicio _compraServicio = new CompraServicio();
        private readonly ProveedorServicio _proveedorServicio = new ProveedorServicio();
        private readonly ProductoServicio _productoServicio = new ProductoServicio();

        [HttpGet]
        public IActionResult Index()
        {
            if (!UsuarioAutenticado())
            {
                return RedirectToAction("Login", "Auth");
            }

            CompraViewModel model = new CompraViewModel
            {
                Proveedores = _proveedorServicio.Listar(),
                Productos = _productoServicio.Listar(),
                TipoDocumento = "COMPRA",
                NumeroDocumento = "AUTO"
            };

            return View("~/Views/Compra/Compra.cshtml", model);
        }

        [HttpPost]
        public IActionResult Registrar([FromBody] CompraViewModel model)
        {
            if (!UsuarioAutenticado())
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "La sesión ha expirado. Inicie sesión nuevamente."
                });
            }

            if (model == null)
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "No se recibió la información de la compra."
                });
            }

            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            if (idUsuario <= 0)
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "No se encontró un usuario válido para registrar la compra."
                });
            }

            if (model.IdProveedor <= 0)
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "Debe seleccionar un proveedor."
                });
            }

            if (string.IsNullOrWhiteSpace(model.TipoDocumento))
            {
                model.TipoDocumento = "COMPRA";
            }

            if (string.IsNullOrWhiteSpace(model.NumeroDocumento))
            {
                model.NumeroDocumento = "AUTO";
            }

            if (model.Descuento < 0 || model.Descuento > 100)
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "El descuento debe estar entre 0 y 100."
                });
            }

            if (model.DetalleCompra == null || !model.DetalleCompra.Any())
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "Debe agregar al menos un producto a la compra."
                });
            }

            foreach (var item in model.DetalleCompra)
            {
                if (item.IdProducto <= 0)
                {
                    return Json(new
                    {
                        resultado = false,
                        mensaje = "Producto inválido en el detalle."
                    });
                }

                if (item.Cantidad <= 0)
                {
                    return Json(new
                    {
                        resultado = false,
                        mensaje = "La cantidad debe ser mayor a cero."
                    });
                }

                if (item.PrecioCompra <= 0)
                {
                    return Json(new
                    {
                        resultado = false,
                        mensaje = "El precio de compra debe ser mayor a cero."
                    });
                }

                if (item.PrecioVenta <= 0)
                {
                    return Json(new
                    {
                        resultado = false,
                        mensaje = "El precio de venta debe ser mayor a cero."
                    });
                }

                if (item.PrecioVenta <= item.PrecioCompra)
                {
                    return Json(new
                    {
                        resultado = false,
                        mensaje = "El precio de venta debe ser mayor al precio de compra."
                    });
                }
            }

            decimal subTotal = model.DetalleCompra.Sum(x => x.Cantidad * x.PrecioCompra);
            decimal valorDescuento = subTotal * (model.Descuento / 100m);
            decimal montoTotal = subTotal - valorDescuento;

            if (montoTotal <= 0)
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "El monto total de la compra debe ser mayor a cero."
                });
            }

            if (model.MontoPagado <= 0)
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "Debe ingresar el monto pagado."
                });
            }

            if (model.MontoPagado < montoTotal)
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "El monto pagado no puede ser menor al total a pagar."
                });
            }

            decimal montoCambio = model.MontoPagado - montoTotal;

            Compra compra = new Compra
            {
                oUsuario = new Usuario
                {
                    IdUsuario = idUsuario
                },
                oProveedor = new Proveedor
                {
                    IdProveedor = model.IdProveedor,
                    Documento = model.DocumentoProveedor ?? string.Empty
                },
                TipoDocumento = model.TipoDocumento.Trim(),
                NumeroDocumento = string.IsNullOrWhiteSpace(model.NumeroDocumento)
                    ? "AUTO"
                    : model.NumeroDocumento.Trim(),
                SubTotal = subTotal,
                Descuento = model.Descuento,
                MontoTotal = montoTotal,
                MontoPagado = model.MontoPagado,
                MontoCambio = montoCambio
            };

            DataTable detalleCompra = CrearDetalleCompraDataTable(model);

            bool resultado = _compraServicio.Registrar(compra, detalleCompra, out string mensaje);

            return Json(new
            {
                resultado = resultado,
                mensaje = resultado ? mensaje : mensaje,
                numeroDocumento = resultado ? compra.NumeroDocumento : string.Empty,
                subTotal = subTotal,
                descuento = model.Descuento,
                montoTotal = montoTotal,
                montoPagado = model.MontoPagado,
                montoCambio = montoCambio
            });
        }

        [HttpGet]
        public IActionResult Consultar(string numeroDocumento)
        {
            if (!UsuarioAutenticado())
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "La sesión ha expirado. Inicie sesión nuevamente."
                });
            }

            if (string.IsNullOrWhiteSpace(numeroDocumento))
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "Debe ingresar el número de documento de la compra."
                });
            }

            Compra compra = _compraServicio.ObtenerCompra(numeroDocumento.Trim());

            if (compra == null || compra.IdCompra == 0)
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "No se encontró la compra."
                });
            }

            return Json(new
            {
                resultado = true,
                compra = compra
            });
        }

        private DataTable CrearDetalleCompraDataTable(CompraViewModel model)
        {
            DataTable tabla = new DataTable();

            tabla.Columns.Add("IdProducto", typeof(int));
            tabla.Columns.Add("PrecioCompra", typeof(decimal));
            tabla.Columns.Add("PrecioVenta", typeof(decimal));
            tabla.Columns.Add("Cantidad", typeof(int));
            tabla.Columns.Add("MontoTotal", typeof(decimal));

            foreach (var item in model.DetalleCompra)
            {
                decimal subtotal = item.Cantidad * item.PrecioCompra;

                tabla.Rows.Add(
                    item.IdProducto,
                    item.PrecioCompra,
                    item.PrecioVenta,
                    item.Cantidad,
                    subtotal
                );
            }

            return tabla;
        }

        private bool UsuarioAutenticado()
        {
            return HttpContext.Session.GetInt32("IdUsuario") != null;
        }
    }
}