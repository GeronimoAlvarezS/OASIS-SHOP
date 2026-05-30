using System.Collections.Generic;

namespace OasisShop.Web.Models.ViewModels
{
    public class ChatBotViewModel
    {
        public int IdConversacion { get; set; }

        public int IdUsuario { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public string Respuesta { get; set; } = string.Empty;

        public bool Escalar { get; set; }

        public string TipoCaso { get; set; } = string.Empty;

        public List<ChatBotMensajeViewModel> Mensajes { get; set; } =
            new List<ChatBotMensajeViewModel>();
    }

    public class ChatBotMensajeViewModel
    {
        public string Remitente { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public string FechaRegistro { get; set; } = string.Empty;
    }
}