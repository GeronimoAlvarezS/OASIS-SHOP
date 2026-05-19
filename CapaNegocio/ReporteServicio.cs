using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class ReporteServicio
    {
        private readonly ReporteDatos objcd_reporte = new ReporteDatos();

        public List<ReporteCompra> Compra(string fechainicio, string fechafin, int idproveedor)
        {
            if (string.IsNullOrWhiteSpace(fechainicio))
            {
                fechainicio = DateTime.Now.ToString("dd/MM/yyyy");
            }

            if (string.IsNullOrWhiteSpace(fechafin))
            {
                fechafin = DateTime.Now.ToString("dd/MM/yyyy");
            }

            return objcd_reporte.Compra(fechainicio, fechafin, idproveedor);
        }

        public List<ReporteVenta> Venta(string fechainicio, string fechafin)
        {
            if (string.IsNullOrWhiteSpace(fechainicio))
            {
                fechainicio = DateTime.Now.ToString("yyyy-MM-dd");
            }

            if (string.IsNullOrWhiteSpace(fechafin))
            {
                fechafin = DateTime.Now.ToString("yyyy-MM-dd");
            }

            return objcd_reporte.Venta(fechainicio, fechafin);
        }
    }
}