using Microsoft.AspNetCore.Http;

namespace OasisShop.Web.Models.ViewModels
{
    public class NegocioViewModel
    {
        public int IdNegocio { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string RUC { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public int IdDepartamento { get; set; }

        public int IdCiudad { get; set; }

        public string NombreDepartamento { get; set; } = string.Empty;

        public string NombreCiudad { get; set; } = string.Empty;

        public string LogoBase64 { get; set; } = string.Empty;

        public IFormFile LogoArchivo { get; set; }
    }
}