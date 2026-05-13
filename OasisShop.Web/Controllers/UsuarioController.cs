using Microsoft.AspNetCore.Mvc;
using CapaEntidad;
using CapaNegocio;

namespace OasisShop.Web.Controllers
{
    public class UsuarioController : Controller
    {
        // Instancia de la capa de negocio encargada de la lógica de usuarios.
        private readonly UsuarioServicio _usuarioServicio = new UsuarioServicio();

        // Método GET que carga la vista principal de usuarios.
        // Incluye funcionalidades de búsqueda y paginación.

        [HttpGet]
        public IActionResult Index()
        {
<<<<<<< HEAD
            // Cantidad de registros que se mostrarán por página.

            int registrosPorPagina = 5;

            // Obtiene la lista completa de usuarios desde la capa de negocio.

            var lista = _usuarioServicio.Listar();

            // Verifica si el usuario ingresó un texto de búsqueda.

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

            // Obtiene el total de registros después del filtro.

            int totalRegistros = lista.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            var usuariosPaginados = lista
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToList();

            // Variables enviadas a la vista mediante ViewBag.

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas == 0 ? 1 : totalPaginas;
            ViewBag.RegistrosPorPagina = registrosPorPagina;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.Busqueda = busqueda;

            // Retorna la vista Usuario.cshtml junto con la lista paginada.

            return View("~/Views/Usuario/Usuario.cshtml", usuariosPaginados);
=======
            var lista = _usuarioServicio.Listar();
            return View("~/Views/Usuario/Usuario.cshtml", lista);
>>>>>>> parent of 234fe1e (feature/LogicaDeGestionDeUsuario)
        }

        // Método POST encargado de registrar o editar usuarios.
        // ValidateAntiForgeryToken ayuda a prevenir ataques CSRF.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(int IdUsuario, string Documento, string NombreCompleto, string Correo, string Clave, int IdRol, bool Estado)
        {
            string mensaje = string.Empty;

            // Construcción del objeto Usuario con los datos recibidos del formulario.

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

            // Si el IdUsuario es 0 significa que es un nuevo registro.

            if (IdUsuario == 0)
            {
                // Llama al servicio para registrar el usuario.

                int idGenerado = _usuarioServicio.Registrar(objUsuario, out mensaje);

                if (idGenerado == 0)
                {
                    TempData["MensajeError"] = mensaje;
                }
                else
                {
                    // Mensaje de éxito.
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
                    // Mensaje de éxito.
                    TempData["MensajeOk"] = "Usuario editado correctamente.";
                }
            }

            // Redirecciona nuevamente al método Index después de guardar.

            return RedirectToAction("Index");
        }
    }
}