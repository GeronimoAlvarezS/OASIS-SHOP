using Microsoft.AspNetCore.Mvc;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Linq;

namespace OasisShop.Web.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UsuarioServicio _usuarioServicio = new UsuarioServicio();

        [HttpGet]
        public IActionResult Index(int pagina = 1, string busqueda = "")
        {
            int registrosPorPagina = 5;

            var lista = _usuarioServicio.Listar();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string filtro = busqueda.Trim().ToLower();

                lista = lista
                    .Where(u =>
                        (u.Documento ?? string.Empty).ToLower().Contains(filtro) ||
                        (u.NombreCompleto ?? string.Empty).ToLower().Contains(filtro) ||
                        (u.Correo ?? string.Empty).ToLower().Contains(filtro) ||
                        (u.oRol != null && (u.oRol.Descripcion ?? string.Empty).ToLower().Contains(filtro)) ||
                        (u.Estado ? "activo" : "inactivo").Contains(filtro) ||
                        u.IdUsuario.ToString().Contains(filtro)
                    )
                    .ToList();
            }

            int totalRegistros = lista.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            var usuariosPaginados = lista
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToList();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas == 0 ? 1 : totalPaginas;
            ViewBag.RegistrosPorPagina = registrosPorPagina;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.Busqueda = busqueda;

            return View("~/Views/Usuario/Usuario.cshtml", usuariosPaginados);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(int IdUsuario, string Documento, string NombreCompleto, string Correo, string Clave, int IdRol, bool Estado)
        {
            string mensaje = string.Empty;

            Usuario objUsuario = new Usuario()
            {
                IdUsuario = IdUsuario,
                Documento = Documento,
                NombreCompleto = NombreCompleto,
                Correo = Correo,
                Clave = Clave,
                Estado = Estado,
                oRol = new Rol()
                {
                    IdRol = IdRol
                }
            };

            if (IdUsuario == 0)
            {
                int idGenerado = _usuarioServicio.Registrar(objUsuario, out mensaje);

                if (idGenerado == 0)
                {
                    TempData["MensajeError"] = mensaje;
                }
                else
                {
                    TempData["MensajeOk"] = "Usuario creado correctamente.";
                }
            }
            else
            {
                bool respuesta = _usuarioServicio.Editar(objUsuario, out mensaje);

                if (!respuesta)
                {
                    TempData["MensajeError"] = mensaje;
                }
                else
                {
                    TempData["MensajeOk"] = "Usuario editado correctamente.";
                }
            }

            return RedirectToAction("Index");
        }
    }
}