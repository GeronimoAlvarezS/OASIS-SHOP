using System;

namespace CapaEntidad
{
    public class ChatBotConversacion
    {
        public int IdConversacion { get; set; }
        public int IdUsuario { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}