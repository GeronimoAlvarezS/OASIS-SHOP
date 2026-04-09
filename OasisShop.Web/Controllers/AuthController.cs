using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;
using CapaEntidad;
using CapaNegocio;

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

            Usuario usuario = new UsuarioServicio()
                .Listar()
                .FirstOrDefault(u => u.Documento == model.Documento && u.Clave == model.Clave);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Documento o clave incorrectos.");
                return View(model);
            }

            HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);
            HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto);
            HttpContext.Session.SetString("Documento", usuario.Documento);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}