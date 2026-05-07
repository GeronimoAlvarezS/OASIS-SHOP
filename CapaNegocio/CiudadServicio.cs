using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CiudadServicio
    {
        private readonly CiudadDatos objcd_ciudad = new CiudadDatos();

        public List<Ciudad> ListarPorDepartamento(int idDepartamento)
        {
            if (idDepartamento <= 0)
            {
                return new List<Ciudad>();
            }

            return objcd_ciudad.ListarPorDepartamento(idDepartamento);
        }
    }
}