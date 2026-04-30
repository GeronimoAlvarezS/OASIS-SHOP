using CapaEntidad;
using CapaNegocio;
using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;
using System.Linq;

namespace OasisShop.Web.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ClienteServicio _clienteServicio = new ClienteServicio();

        [HttpGet]
        public IActionResult Index()
        {
            var listaClientes = _clienteServicio.Listar();

            var listaViewModel = listaClientes.Select(c => new ClienteViewModel
            {
                IdCliente = c.IdCliente,
                Documento = c.Documento,
                NombreCompleto = c.NombreCompleto,
                Correo = c.Correo,
                Telefono = c.Telefono,
                Estado = c.Estado,

                TieneVentas = c.TieneVentas,
                TotalVentas = c.TotalVentas

            }).ToList();

            return View("~/Views/Cliente/Cliente.cshtml", listaViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(ClienteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    mensaje = "Debe completar correctamente los campos obligatorios."
                });
            }

            Cliente cliente = new Cliente
            {
                IdCliente = model.IdCliente,
                Documento = model.Documento?.Trim(),
                NombreCompleto = model.NombreCompleto?.Trim(),
                Correo = model.Correo?.Trim(),
                Telefono = model.Telefono?.Trim(),
                Estado = model.Estado
            };

            string mensaje;

            if (cliente.IdCliente == 0)
            {
                int resultado = _clienteServicio.Registrar(cliente, out mensaje);

                if (resultado > 0)
                {
                    return Json(new
                    {
                        success = true,
                        mensaje = "Cliente registrado correctamente."
                    });
                }

                return Json(new
                {
                    success = false,
                    mensaje = string.IsNullOrEmpty(mensaje)
                        ? "No se pudo registrar el cliente."
                        : mensaje
                });
            }
            else
            {
                bool resultado = _clienteServicio.Editar(cliente, out mensaje);

                if (resultado)
                {
                    return Json(new
                    {
                        success = true,
                        mensaje = "Cliente editado correctamente."
                    });
                }

                return Json(new
                {
                    success = false,
                    mensaje = string.IsNullOrEmpty(mensaje)
                        ? "No se pudo editar el cliente."
                        : mensaje
                });
            }
        }
    }
}