namespace CapaEntidad
{
    public class Producto
    {
        public int IdProducto { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public Categoria oCategoria { get; set; } = new Categoria();

        public int Stock { get; set; }

        public decimal PrecioCompra { get; set; }

        public decimal PrecioVenta { get; set; }

        public bool Estado { get; set; }

        public string FechaRegistro { get; set; } = string.Empty;
    }
}