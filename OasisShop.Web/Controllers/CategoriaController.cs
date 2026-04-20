using CapaDatos;
using CapaEntidad;
using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;

namespace OasisShop.Web.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly CategoriaDatos _categoriaServicio = new CategoriaDatos();

        [HttpGet]
        public IActionResult Index(int pagina = 1, string busqueda = "")
        {
            int registrosPorPagina = 5;

            var listaEntidad = _categoriaServicio.Listar();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string filtro = busqueda.Trim().ToLower();

                listaEntidad = listaEntidad
                    .Where(c =>
                        (c.Descripcion ?? string.Empty).ToLower().Contains(filtro) ||
                        (c.Estado ? "activo" : "inactivo").Contains(filtro) ||
                        c.IdCategoria.ToString().Contains(filtro)
                    )
                    .ToList();
            }

            int totalRegistros = listaEntidad.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            var categoriasPaginadas = listaEntidad
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(c => new CategoriaViewModel
                {
                    IdCategoria = c.IdCategoria,
                    Descripcion = c.Descripcion,
                    Estado = c.Estado
                })
                .ToList();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas == 0 ? 1 : totalPaginas;
            ViewBag.RegistrosPorPagina = registrosPorPagina;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.Busqueda = busqueda;

            return View("~/Views/Categoria/Categoria.cshtml", categoriasPaginadas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(CategoriaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Verifica los campos obligatorios.";
                return RedirectToAction("Index");
            }

            string mensaje;
            var categoria = new Categoria
            {
                IdCategoria = model.IdCategoria,
                Descripcion = model.Descripcion,
                Estado = model.Estado
            };

            if (model.IdCategoria == 0)
            {
                int idGenerado = _categoriaServicio.Registrar(categoria, out mensaje);

                if (idGenerado == 0)
                {
                    TempData["MensajeError"] = string.IsNullOrWhiteSpace(mensaje)
                        ? "No se pudo registrar la categoría."
                        : mensaje;
                }
                else
                {
                    TempData["MensajeOk"] = "Categoría creada correctamente.";
                }
            }
            else
            {
                bool respuesta = _categoriaServicio.Editar(categoria, out mensaje);

                if (!respuesta)
                {
                    TempData["MensajeError"] = string.IsNullOrWhiteSpace(mensaje)
                        ? "No se pudo editar la categoría."
                        : mensaje;
                }
                else
                {
                    TempData["MensajeOk"] = "Categoría editada correctamente.";
                }
            }

            return RedirectToAction("Index");
        }
    }
}