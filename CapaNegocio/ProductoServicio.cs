using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class ProductoServicio
    {
        private ProductoDatos objcd_Producto = new ProductoDatos();

        public List<Producto> Listar()
        {
            return objcd_Producto.Listar();
        }

        public int Registrar(Producto obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(obj.Codigo))
            {
                Mensaje += "Es necesario el código del producto\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Nombre))
            {
                Mensaje += "Es necesario el nombre del producto\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Descripcion))
            {
                Mensaje += "Es necesario la descripción del producto\n";
            }

            if (obj.oCategoria == null || obj.oCategoria.IdCategoria == 0)
            {
                Mensaje += "Es necesario seleccionar una categoría válida\n";
            }

            if (Mensaje != string.Empty)
            {
                return 0;
            }

            obj.Stock = 0;
            obj.PrecioCompra = 0;
            obj.PrecioVenta = 0;

            return objcd_Producto.Registrar(obj, out Mensaje);
        }

        public bool Editar(Producto obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.IdProducto == 0)
            {
                Mensaje += "No se encontró el producto a editar\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Codigo))
            {
                Mensaje += "Es necesario el código del producto\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Nombre))
            {
                Mensaje += "Es necesario el nombre del producto\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Descripcion))
            {
                Mensaje += "Es necesario la descripción del producto\n";
            }

            if (obj.oCategoria == null || obj.oCategoria.IdCategoria == 0)
            {
                Mensaje += "Es necesario seleccionar una categoría válida\n";
            }

            if (Mensaje != string.Empty)
            {
                return false;
            }

            return objcd_Producto.Editar(obj, out Mensaje);
        }

        public bool Eliminar(Producto obj, out string Mensaje)
        {
            return objcd_Producto.Eliminar(obj, out Mensaje);
        }
    }
}