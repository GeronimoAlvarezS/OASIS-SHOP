using System;

namespace CapaEntidad
{
    public class Cliente
    {
        public int IdCliente { get; set; }

        public string Documento { get; set; }

        public string NombreCompleto { get; set; }

        public string Correo { get; set; }

        public string Telefono { get; set; }

        public bool Estado { get; set; }

        public string FechaRegistro { get; set; }

        public bool TieneVentas { get; set; }

        public int TotalVentas { get; set; }
    }
}