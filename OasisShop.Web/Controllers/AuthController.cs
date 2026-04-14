//using Microsoft.AspNetCore.Mvc;
//using OasisShop.Web.Models.ViewModels;
//using CapaEntidad;
//using CapaNegocio;
//using System.Collections.Generic;
//using System.Linq;

//namespace OasisShop.Web.Controllers
//{
//    public class AuthController : Controller
//    {
//        [HttpGet]
//        public IActionResult Login()
//        {
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Login(LoginViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }

//            string documento = model.Documento?.Trim() ?? string.Empty;
//            string clave = model.Clave?.Trim() ?? string.Empty;

//            Usuario usuario = new UsuarioServicio()
//                .ObtenerPorCredenciales(documento, clave);

//            if (usuario == null)
//            {
//                ModelState.AddModelError(string.Empty, "Documento o clave incorrectos.");
//                return View(model);
//            }

//            if (!usuario.Estado)
//            {
//                ModelState.AddModelError(string.Empty, "El usuario está inactivo.");
//                return View(model);
//            }

//            HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);
//            HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto);
//            HttpContext.Session.SetString("Documento", usuario.Documento);
//            HttpContext.Session.SetInt32("IdRol", usuario.oRol.IdRol);

//            List<Permiso> listaPermisos = new PermisoServicio().Listar(usuario.IdUsuario);
//            string permisos = string.Join(",", listaPermisos.Select(p => p.NombreMenu));

//            HttpContext.Session.SetString("Permisos", permisos);

//            return RedirectToAction("Index", "Home");
//        }

//        public IActionResult Logout()
//        {
//            HttpContext.Session.Clear();
//            return RedirectToAction("Login", "Auth");
//        }
//    }
//}

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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string documento = model.Documento?.Trim() ?? string.Empty;
            string clave = model.Clave?.Trim() ?? string.Empty;

            List<Usuario> listaUsuarios = new UsuarioServicio().Listar();

            Usuario usuario = listaUsuarios.FirstOrDefault(u =>
                (u.Documento ?? string.Empty).Trim() == documento &&
                (u.Clave ?? string.Empty).Trim() == clave);

            if (usuario == null)
            {
                string usuariosDebug = string.Join(" | ", listaUsuarios.Select(u =>
                    $"Id:{u.IdUsuario}, Doc:'{u.Documento}', Clave:'{u.Clave}', Estado:{u.Estado}"));

                ViewBag.DebugInfo = $"Documento recibido: '{documento}' | Clave recibida: '{clave}' | Total usuarios: {listaUsuarios.Count} | Datos: {usuariosDebug}";
                ModelState.AddModelError(string.Empty, "Documento o clave incorrectos.");
                return View(model);
            }

            if (!usuario.Estado)
            {
                ModelState.AddModelError(string.Empty, "El usuario está inactivo.");
                return View(model);
            }

            HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);
            HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto);
            HttpContext.Session.SetString("Documento", usuario.Documento);
            HttpContext.Session.SetInt32("IdRol", usuario.oRol.IdRol);

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