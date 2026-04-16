using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class RolServicio
    {

        private RolDatos objcd_rol = new RolDatos();


        public List<Rol> Listar()
        {
            return objcd_rol.Listar();
        }
    }
}
