using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using Microsoft.AspNetCore.Mvc;
using OasisShop.Web.Models.ViewModels;

namespace OasisShop.Web.Controllers
{
    public class ChatBotController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ChatBotConversacionDatos _conversacionDatos = new ChatBotConversacionDatos();
        private readonly ChatBotMensajeDatos _mensajeDatos = new ChatBotMensajeDatos();

        public ChatBotController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            int idUsuario = ObtenerIdUsuario();

            if (idUsuario == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            string mensaje;
            int idConversacion = _conversacionDatos.CrearConversacion(
                new ChatBotConversacion()
                {
                    IdUsuario = idUsuario
                },
                out mensaje
            );

            ChatBotViewModel model = new ChatBotViewModel()
            {
                IdUsuario = idUsuario,
                IdConversacion = idConversacion
            };

            return View("~/Views/ChatBot/ChatBot.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> EnviarMensaje([FromBody] ChatBotViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Mensaje))
            {
                return Json(new
                {
                    respuesta = "Debe escribir una situación para poder brindarle soporte.",
                    escalar = false,
                    tipoCaso = "Soporte básico"
                });
            }

            int idUsuario = ObtenerIdUsuario();

            if (idUsuario == 0)
            {
                return Json(new
                {
                    respuesta = "La sesión del usuario ha expirado. Inicie sesión nuevamente.",
                    escalar = true,
                    tipoCaso = "Error del sistema"
                });
            }

            if (model.IdConversacion == 0)
            {
                string mensaje;
                model.IdConversacion = _conversacionDatos.CrearConversacion(
                    new ChatBotConversacion()
                    {
                        IdUsuario = idUsuario
                    },
                    out mensaje
                );
            }

            string apiKey = _configuration["OpenAI:ApiKey"];

            ChatBotServicio chatBotServicio = new ChatBotServicio(apiKey);

            ChatBotRespuesta respuesta = await chatBotServicio.ProcesarMensajeAsync(
                model.IdConversacion,
                idUsuario,
                model.Mensaje
            );

            return Json(new
            {
                respuesta = respuesta.Respuesta,
                escalar = respuesta.Escalar,
                tipoCaso = respuesta.TipoCaso,
                idConversacion = model.IdConversacion
            });
        }

        [HttpGet]
        public IActionResult ObtenerMensajes(int idConversacion)
        {
            var mensajes = _mensajeDatos.ObtenerMensajesPorConversacion(idConversacion);

            var lista = mensajes.Select(m => new ChatBotMensajeViewModel()
            {
                Remitente = m.Remitente,
                Mensaje = m.Mensaje,
                FechaRegistro = m.FechaRegistro.ToString("dd/MM/yyyy HH:mm")
            }).ToList();

            return Json(lista);
        }

        [HttpPost]
        public IActionResult FinalizarConversacion(int idConversacion)
        {
            string mensaje;

            bool resultado = _conversacionDatos.FinalizarConversacion(
                idConversacion,
                out mensaje
            );

            return Json(new
            {
                resultado = resultado,
                mensaje = mensaje
            });
        }

        private int ObtenerIdUsuario()
        {
            try
            {
                if (HttpContext.Session.GetInt32("IdUsuario") != null)
                {
                    return Convert.ToInt32(HttpContext.Session.GetInt32("IdUsuario"));
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}