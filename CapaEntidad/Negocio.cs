using System;

namespace CapaEntidad
{
    public class Negocio
    {
        public int IdNegocio { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string RUC { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public int IdDepartamento { get; set; }

        public int IdCiudad { get; set; }

        public Departamento oDepartamento { get; set; } = new Departamento();

        public Ciudad oCiudad { get; set; } = new Ciudad();

        public byte[] Logo { get; set; } = Array.Empty<byte>();
    }
}