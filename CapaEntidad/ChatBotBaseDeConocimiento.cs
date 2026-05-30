using System;

namespace CapaEntidad
{
    public class ChatBotBaseConocimiento
    {
        public int IdConocimiento { get; set; }
        public string Problema { get; set; } = string.Empty;
        public string PalabrasClave { get; set; } = string.Empty;
        public string Solucion { get; set; } = string.Empty;
        public string TipoCaso { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}