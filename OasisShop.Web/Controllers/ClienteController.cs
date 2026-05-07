using CapaEntidad;
using CapaNegocio;
using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;
using System;
using System.Linq;

namespace OasisShop.Web.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ClienteServicio _clienteServicio = new ClienteServicio();

        [HttpGet]
        public IActionResult Index(int pagina = 1, string busqueda = "")
        {
            int registrosPorPagina = 5;

            var lista = _clienteServicio.Listar();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string filtro = busqueda.Trim().ToLower();

                lista = lista
                    .Where(c =>
                        c.IdCliente.ToString().Contains(filtro) ||
                        (c.Documento ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.NombreCompleto ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.Correo ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.Telefono ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.Estado ? "activo" : "inactivo").Contains(filtro) ||
                        (c.TieneVentas ? "con ventas" : "sin ventas").Contains(filtro) ||
                        c.TotalVentas.ToString().Contains(filtro)
                    )
                    .ToList();
            }

            int totalRegistros = lista.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            var clientesPaginados = lista
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(c => new ClienteViewModel
                {
                    IdCliente = c.IdCliente,
                    Documento = c.Documento,
                    NombreCompleto = c.NombreCompleto,
                    Correo = c.Correo,
                    Telefono = c.Telefono,
                    Estado = c.Estado,
                    TieneVentas = c.TieneVentas,
                    TotalVentas = c.TotalVentas
                })
                .ToList();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas == 0 ? 1 : totalPaginas;
            ViewBag.RegistrosPorPagina = registrosPorPagina;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.Busqueda = busqueda;

            return View("~/Views/Cliente/Cliente.cshtml", clientesPaginados);
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

            string mensaje = string.Empty;

            Cliente cliente = new Cliente
            {
                IdCliente = model.IdCliente,
                Documento = model.Documento?.Trim(),
                NombreCompleto = model.NombreCompleto?.Trim(),
                Correo = model.Correo?.Trim(),
                Telefono = model.Telefono?.Trim(),
                Estado = model.Estado
            };

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

            bool respuesta = _clienteServicio.Editar(cliente, out mensaje);

            if (respuesta)
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