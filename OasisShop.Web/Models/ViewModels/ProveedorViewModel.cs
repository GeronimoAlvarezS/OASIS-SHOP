using System.ComponentModel.DataAnnotations;

namespace OasisShop.Web.Models.ViewModels
{
    public class ProveedorViewModel
    {
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El documento o NIT es obligatorio.")]
        public string Documento { get; set; }

        [Required(ErrorMessage = "La razón social es obligatoria.")]
        public string RazonSocial { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public string Direccion { get; set; }

        public bool Estado { get; set; }

        public bool TieneComprasAsociadas { get; set; }
    }
}