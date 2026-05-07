using CapaEntidad;
using CapaNegocio;
using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OasisShop.Web.Controllers
{
    public class ProveedorController : Controller
    {
        private readonly ProveedorServicio _proveedorServicio = new ProveedorServicio();

        [HttpGet]
        public IActionResult Index(int pagina = 1, string busqueda = "")
        {
            int registrosPorPagina = 5;

            List<Proveedor> lista = _proveedorServicio.Listar();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string filtro = busqueda.Trim().ToLower();

                lista = lista
                    .Where(p =>
                        (p.Documento ?? string.Empty).ToLower().Contains(filtro) ||
                        (p.RazonSocial ?? string.Empty).ToLower().Contains(filtro) ||
                        (p.Correo ?? string.Empty).ToLower().Contains(filtro)
                    )
                    .ToList();
            }

            int totalRegistros = lista.Count;
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            if (pagina < 1)
            {
                pagina = 1;
            }

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            List<ProveedorViewModel> proveedores = lista
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(p => new ProveedorViewModel
                {
                    IdProveedor = p.IdProveedor,
                    Documento = p.Documento,
                    RazonSocial = p.RazonSocial,
                    Correo = p.Correo,
                    Telefono = p.Telefono,
                    Direccion = p.Direccion,
                    Estado = p.Estado,
                    TieneComprasAsociadas = p.TieneComprasAsociadas
                })
                .ToList();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.Busqueda = busqueda;

            return View("~/Views/Proveedor/Proveedor.cshtml", proveedores);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(ProveedorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    mensaje = "Debe completar correctamente todos los campos obligatorios."
                });
            }

            Proveedor proveedor = new Proveedor
            {
                IdProveedor = model.IdProveedor,
                Documento = model.Documento?.Trim(),
                RazonSocial = model.RazonSocial?.Trim(),
                Correo = model.Correo?.Trim(),
                Telefono = model.Telefono?.Trim(),
                Direccion = model.Direccion?.Trim(),
                Estado = model.Estado
            };

            string mensaje = string.Empty;

            if (proveedor.IdProveedor == 0)
            {
                int resultado = _proveedorServicio.Registrar(proveedor, out mensaje);

                return Json(new
                {
                    success = resultado > 0,
                    mensaje = string.IsNullOrWhiteSpace(mensaje)
                        ? resultado > 0
                            ? "Proveedor registrado correctamente."
                            : "No fue posible registrar el proveedor."
                        : mensaje
                });
            }
            else
            {
                bool resultado = _proveedorServicio.Editar(proveedor, out mensaje);

                return Json(new
                {
                    success = resultado,
                    mensaje = string.IsNullOrWhiteSpace(mensaje)
                        ? resultado
                            ? "Proveedor actualizado correctamente."
                            : "No fue posible actualizar el proveedor."
                        : mensaje
                });
            }
        }
    }
}