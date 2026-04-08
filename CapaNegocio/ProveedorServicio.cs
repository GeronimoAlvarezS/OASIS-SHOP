using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class ProveedorServicio
    {

        private CD_Proveedor objcd_Proveedor = new CD_Proveedor();


        public List<Proveedor> Listar()
        {
            return objcd_Proveedor.Listar();
        }

        public int Registrar(Proveedor obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.Documento == "")
            {
                Mensaje += "Es necesario el documento del Proveedor\n";
            }

            if (obj.RazonSocial == "")
            {
                Mensaje += "Es necesario la razon social del Proveedor\n";
            }

            if (obj.Correo == "")
            {
                Mensaje += "Es necesario la correo del Proveedor\n";
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

            if (obj.Documento == "")
            {
                Mensaje += "Es necesario el documento del Proveedor\n";
            }

            if (obj.RazonSocial == "")
            {
                Mensaje += "Es necesario la razon social del Proveedor\n";
            }

            if (obj.Correo == "")
            {
                Mensaje += "Es necesario la correo del Proveedor\n";
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
