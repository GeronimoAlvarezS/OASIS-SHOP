using System;

namespace CapaEntidad
{
    public class ChatBotMensaje
    {
        public int IdMensaje { get; set; }
        public int IdConversacion { get; set; }
        public string Remitente { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
    }
}