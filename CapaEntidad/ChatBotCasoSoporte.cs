using System;

namespace CapaEntidad
{
    public class ChatBotCasoSoporte
    {
        public int IdCaso { get; set; }
        public int IdConversacion { get; set; }
        public int IdUsuario { get; set; }
        public string Situacion { get; set; } = string.Empty;
        public string RespuestaChatBot { get; set; } = string.Empty;
        public string TipoCaso { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaCierre { get; set; }
    }
}