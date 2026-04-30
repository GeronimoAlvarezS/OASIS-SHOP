using System.ComponentModel.DataAnnotations;

namespace OasisShop.Web.Models.ViewModels
{
    public class ClienteViewModel
    {
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [Display(Name = "Número de documento")]
        public string Documento { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; }

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        public bool Estado { get; set; }

        public bool TieneVentas { get; set; }

        public int TotalVentas { get; set; }
    }
}