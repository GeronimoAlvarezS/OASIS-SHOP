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

            if (obj.Stock < 0)
            {
                Mensaje += "El stock inicial no puede ser negativo\n";
            }

            if (obj.PrecioCompra <= 0)
            {
                Mensaje += "El precio de compra debe ser mayor a cero\n";
            }

            if (obj.PrecioVenta <= 0)
            {
                Mensaje += "El precio de venta debe ser mayor a cero\n";
            }

            if (obj.PrecioVenta < obj.PrecioCompra)
            {
                Mensaje += "El precio de venta no puede ser menor al precio de compra\n";
            }

            if (Mensaje != string.Empty)
            {
                return 0;
            }

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

            if (obj.Stock < 0)
            {
                Mensaje += "El stock no puede ser negativo\n";
            }

            if (obj.PrecioCompra <= 0)
            {
                Mensaje += "El precio de compra debe ser mayor a cero\n";
            }

            if (obj.PrecioVenta <= 0)
            {
                Mensaje += "El precio de venta debe ser mayor a cero\n";
            }

            if (obj.PrecioVenta < obj.PrecioCompra)
            {
                Mensaje += "El precio de venta no puede ser menor al precio de compra\n";
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