using System.ComponentModel.DataAnnotations;

namespace OasisShop.Web.Models.ViewModels
{
    public class CategoriaViewModel
    {
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(100, ErrorMessage = "La descripción no debe superar los 100 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado es obligatorio.")]
        public bool Estado { get; set; }
    }
}