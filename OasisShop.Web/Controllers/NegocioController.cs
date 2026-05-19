using CapaEntidad;
using CapaNegocio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;
using System;
using System.IO;
using System.Linq;

namespace OasisShop.Web.Controllers
{
    public class NegocioController : Controller
    {
        private readonly CN_Negocio _negocioServicio = new CN_Negocio();
        private readonly DepartamentoServicio _departamentoServicio = new DepartamentoServicio();
        private readonly CiudadServicio _ciudadServicio = new CiudadServicio();

        [HttpGet]
        public IActionResult Index()
        {
            if (!EsAdministrador())
            {
                TempData["Error"] = "No tiene permisos para acceder a los datos del negocio.";
                return RedirectToAction("Index", "Home");
            }

            Negocio negocio = _negocioServicio.ObtenerDatos();

            bool logoObtenido;
            byte[] logoBytes = _negocioServicio.ObtenerLogo(out logoObtenido);

            ViewBag.Departamentos = _departamentoServicio.Listar();
            ViewBag.Ciudades = negocio.IdDepartamento > 0
                ? _ciudadServicio.ListarPorDepartamento(negocio.IdDepartamento)
                : Enumerable.Empty<Ciudad>().ToList();

            NegocioViewModel model = new NegocioViewModel
            {
                IdNegocio = negocio.IdNegocio,
                Nombre = negocio.Nombre,
                RUC = negocio.RUC,
                Direccion = negocio.Direccion,
                Correo = negocio.Correo,
                IdDepartamento = negocio.IdDepartamento,
                IdCiudad = negocio.IdCiudad,
                NombreDepartamento = negocio.oDepartamento != null ? negocio.oDepartamento.Nombre : string.Empty,
                NombreCiudad = negocio.oCiudad != null ? negocio.oCiudad.Nombre : string.Empty,
                LogoBase64 = logoObtenido && logoBytes != null && logoBytes.Length > 0
                    ? "data:image/png;base64," + Convert.ToBase64String(logoBytes)
                    : string.Empty
            };

            return View("~/Views/Negocio/Negocio.cshtml", model);
        }

        [HttpGet]
        public JsonResult ObtenerCiudadesPorDepartamento(int idDepartamento)
        {
            if (!EsAdministrador())
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "No tiene permisos para consultar esta información."
                });
            }

            if (idDepartamento <= 0)
            {
                return Json(new
                {
                    resultado = false,
                    mensaje = "Debe ingresar primero el departamento."
                });
            }

            var ciudades = _ciudadServicio.ListarPorDepartamento(idDepartamento);

            return Json(new
            {
                resultado = true,
                ciudades = ciudades.Select(c => new
                {
                    idCiudad = c.IdCiudad,
                    idDepartamento = c.IdDepartamento,
                    nombre = c.Nombre,
                    codigo = c.Codigo
                }).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarDatos(NegocioViewModel model)
        {
            if (!EsAdministrador())
            {
                TempData["Error"] = "No tiene permisos para realizar esta acción.";
                return RedirectToAction("Index", "Home");
            }

            if (model.IdDepartamento <= 0)
            {
                TempData["Error"] = "Debe ingresar primero el departamento.";
                return RedirectToAction("Index");
            }

            if (model.IdCiudad <= 0)
            {
                TempData["Error"] = "Debe ingresar un municipio.";
                return RedirectToAction("Index");
            }

            Negocio negocio = new Negocio
            {
                IdNegocio = 1,
                Nombre = model.Nombre,
                RUC = model.RUC,
                Direccion = model.Direccion,
                Correo = model.Correo,
                IdDepartamento = model.IdDepartamento,
                IdCiudad = model.IdCiudad
            };

            string mensaje;
            bool resultadoDatos = _negocioServicio.GuardarDatos(negocio, out mensaje);

            if (!resultadoDatos)
            {
                TempData["Error"] = mensaje;
                return RedirectToAction("Index");
            }

            if (model.LogoArchivo != null && model.LogoArchivo.Length > 0)
            {
                string extension = Path.GetExtension(model.LogoArchivo.FileName).ToLower();

                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                {
                    TempData["Error"] = "Datos guardados, pero el logo debe ser JPG o PNG.";
                    return RedirectToAction("Index");
                }

                if (model.LogoArchivo.Length > 2 * 1024 * 1024)
                {
                    TempData["Error"] = "Datos guardados, pero el logo no puede superar los 2MB.";
                    return RedirectToAction("Index");
                }

                byte[] imagenBytes;

                using (MemoryStream ms = new MemoryStream())
                {
                    model.LogoArchivo.CopyTo(ms);
                    imagenBytes = ms.ToArray();
                }

                bool resultadoLogo = _negocioServicio.ActualizarLogo(imagenBytes, out mensaje);

                if (!resultadoLogo)
                {
                    TempData["Error"] = "Datos guardados, pero ocurrió un error al actualizar el logo.";
                    return RedirectToAction("Index");
                }
            }

            TempData["Exito"] = "Datos del negocio guardados correctamente.";
            return RedirectToAction("Index");
        }

        private bool EsAdministrador()
        {
            string rol = HttpContext.Session.GetString("Rol") ?? string.Empty;
            string permisos = HttpContext.Session.GetString("Permisos") ?? string.Empty;

            return rol.ToLower().Contains("administrador") ||
                   permisos.ToLower().Contains("negocio");
        }
    }
}