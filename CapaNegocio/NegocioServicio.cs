using CapaDatos;
using CapaEntidad;
using System;
using System.Text.RegularExpressions;

namespace CapaNegocio
{
    public class CN_Negocio
    {
        private NegocioDatos objcd_negocio = new NegocioDatos();

        public Negocio ObtenerDatos()
        {
            return objcd_negocio.ObtenerDatos();
        }

        public bool GuardarDatos(Negocio obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj == null)
            {
                Mensaje = "No se recibieron los datos del negocio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(obj.Nombre))
            {
                Mensaje += "Es necesario el nombre del negocio\n";
            }

            if (string.IsNullOrWhiteSpace(obj.RUC))
            {
                Mensaje += "Es necesario el número del RUT\n";
            }
            else if (!Regex.IsMatch(obj.RUC.Trim(), @"^\d{1,3}(\.\d{3})*-[0-9A-Za-z]$"))
            {
                Mensaje += "El formato del RUT no es válido. Ejemplo válido: 800.244.387-4\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Direccion))
            {
                Mensaje += "Es necesario la dirección\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Correo))
            {
                Mensaje += "Es necesario el correo electrónico\n";
            }
            else if (!Regex.IsMatch(obj.Correo.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                Mensaje += "El formato del correo electrónico no es válido\n";
            }

            if (obj.IdDepartamento <= 0)
            {
                Mensaje += "Debe seleccionar un departamento válido\n";
            }

            if (obj.IdCiudad <= 0)
            {
                Mensaje += "Debe seleccionar una ciudad válida\n";
            }

            if (Mensaje != string.Empty)
            {
                return false;
            }

            obj.Nombre = obj.Nombre.Trim();
            obj.RUC = obj.RUC.Trim().ToUpper();
            obj.Direccion = obj.Direccion.Trim();
            obj.Correo = obj.Correo.Trim().ToLower();

            if (obj.oDepartamento == null)
            {
                obj.oDepartamento = new Departamento();
            }

            if (obj.oCiudad == null)
            {
                obj.oCiudad = new Ciudad();
            }

            obj.oDepartamento.IdDepartamento = obj.IdDepartamento;
            obj.oCiudad.IdCiudad = obj.IdCiudad;

            return objcd_negocio.GuardarDatos(obj, out Mensaje);
        }

        public byte[] ObtenerLogo(out bool obtenido)
        {
            return objcd_negocio.ObtenerLogo(out obtenido);
        }

        public bool ActualizarLogo(byte[] imagen, out string mensaje)
        {
            mensaje = string.Empty;

            if (imagen == null || imagen.Length == 0)
            {
                mensaje = "Debe seleccionar una imagen válida.";
                return false;
            }

            if (imagen.Length > 2 * 1024 * 1024)
            {
                mensaje = "El logo no puede superar los 2MB.";
                return false;
            }

            return objcd_negocio.ActualizarLogo(imagen, out mensaje);
        }
    }
}