using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class ProveedorServicio
    {
        private ProveedorDatos objcd_Proveedor = new ProveedorDatos();

        public List<Proveedor> Listar()
        {
            return objcd_Proveedor.Listar();
        }

        public int Registrar(Proveedor obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(obj.Documento))
            {
                Mensaje += "Es necesario el documento del Proveedor\n";
            }

            if (string.IsNullOrWhiteSpace(obj.RazonSocial))
            {
                Mensaje += "Es necesario la razon social del Proveedor\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Correo))
            {
                Mensaje += "Es necesario el correo del Proveedor\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Telefono))
            {
                Mensaje += "Es necesario el telefono del Proveedor\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Direccion))
            {
                Mensaje += "Es necesaria la direccion del Proveedor\n";
            }

            if (Mensaje != string.Empty)
            {
                return 0;
            }
            else
            {
                return objcd_Proveedor.Registrar(obj, out Mensaje);
            }
        }

        public bool Editar(Proveedor obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.IdProveedor == 0)
            {
                Mensaje += "Es necesario seleccionar un Proveedor valido\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Documento))
            {
                Mensaje += "Es necesario el documento del Proveedor\n";
            }

            if (string.IsNullOrWhiteSpace(obj.RazonSocial))
            {
                Mensaje += "Es necesario la razon social del Proveedor\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Correo))
            {
                Mensaje += "Es necesario el correo del Proveedor\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Telefono))
            {
                Mensaje += "Es necesario el telefono del Proveedor\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Direccion))
            {
                Mensaje += "Es necesaria la direccion del Proveedor\n";
            }

            if (Mensaje != string.Empty)
            {
                return false;
            }
            else
            {
                return objcd_Proveedor.Editar(obj, out Mensaje);
            }
        }

        public bool Eliminar(Proveedor obj, out string Mensaje)
        {
            return objcd_Proveedor.Eliminar(obj, out Mensaje);
        }
    }
}