using Microsoft.AspNetCore.Mvc;

namespace OasisShop.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.NombreCompleto = HttpContext.Session.GetString("NombreCompleto");
            return View();
        }
    }
}