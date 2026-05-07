using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class DepartamentoServicio
    {
        private readonly DepartamentoDatos objcd_departamento = new DepartamentoDatos();

        public List<Departamento> Listar()
        {
            return objcd_departamento.Listar();
        }
    }
}