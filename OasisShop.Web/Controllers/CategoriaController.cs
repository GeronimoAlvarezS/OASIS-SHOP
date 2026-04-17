using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;
using System.Linq;

namespace OasisShop.Web.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly CategoriaDatos _categoriaServicio = new CategoriaDatos();

        [HttpGet]
        public IActionResult Index()
        {
            var listaEntidad = _categoriaServicio.Listar();

            var listaViewModel = listaEntidad.Select(c => new CategoriaViewModel
            {
                IdCategoria = c.IdCategoria,
                Descripcion = c.Descripcion,
                Estado = c.Estado
            }).ToList();

            return View("~/Views/Categoria/Categoria.cshtml", listaViewModel);
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