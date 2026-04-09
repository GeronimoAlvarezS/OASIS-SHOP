using System.ComponentModel.DataAnnotations;

namespace OasisShop.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El documento es obligatorio.")]
        public string Documento { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria.")]
        [DataType(DataType.Password)]
        public string Clave { get; set; } = string.Empty;
    }
}