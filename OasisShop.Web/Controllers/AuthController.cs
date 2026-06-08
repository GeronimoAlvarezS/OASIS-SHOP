using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;
using CapaEntidad;
using CapaNegocio;
using System.Collections.Generic;
using System.Linq;

namespace OasisShop.Web.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Auth/Login.cshtml", model);
            }

            string documento = model.Documento?.Trim() ?? string.Empty;
            string clave = model.Clave?.Trim() ?? string.Empty;

            Usuario usuario = new UsuarioServicio().ObtenerPorCredenciales(documento, clave);

            if (usuario == null)
            {
                TempData["LoginError"] = "Documento o clave incorrectos.";
                return View("~/Views/Auth/Login.cshtml", model);
            }

            if (!usuario.Estado)
            {
                TempData["LoginWarning"] = "Usted no se encuentra activo en el sistema. Comuníquese con el administrador.";
                return View("~/Views/Auth/Login.cshtml", model);
            }

            HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);
            HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto ?? string.Empty);
            HttpContext.Session.SetString("Documento", usuario.Documento ?? string.Empty);
            HttpContext.Session.SetInt32("IdRol", usuario.oRol?.IdRol ?? 0);

            List<Permiso> listaPermisos = new PermisoServicio().Listar(usuario.IdUsuario);
            string permisos = string.Join(",", listaPermisos.Select(p => p.NombreMenu));

            HttpContext.Session.SetString("Permisos", permisos);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}