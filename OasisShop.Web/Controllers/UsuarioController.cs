using Microsoft.AspNetCore.Mvc;
using CapaEntidad;
using CapaNegocio;

namespace OasisShop.Web.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UsuarioServicio _usuarioServicio = new UsuarioServicio();

        [HttpGet]
        public IActionResult Index()
        {
            var lista = _usuarioServicio.Listar();
            return View("~/Views/Usuario/Usuario.cshtml", lista);
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