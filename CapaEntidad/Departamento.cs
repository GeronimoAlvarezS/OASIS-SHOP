namespace CapaEntidad
{
    public class Departamento
    {
        public int IdDepartamento { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Codigo { get; set; } = string.Empty;

        public bool Estado { get; set; }
    }
}