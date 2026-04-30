using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class UsuarioServicio
    {
        private UsuarioDatos objcd_usuario = new UsuarioDatos();

        public List<Usuario> Listar()
        {
            return objcd_usuario.Listar();
        }

        public Usuario ObtenerPorCredenciales(string documento, string clave)
        {
            Usuario usuario = objcd_usuario.ObtenerPorDocumento(documento);

            if (usuario == null)
            {
                return null;
            }

            bool claveValida = Seguridad.VerificarPassword(clave, usuario.Clave);

            if (!claveValida && usuario.Clave == clave)
            {
                usuario.Clave = Seguridad.HashPassword(clave);
                objcd_usuario.ActualizarClave(usuario.IdUsuario, usuario.Clave);
                return usuario;
            }

            if (!claveValida)
            {
                return null;
            }

            return usuario;
        }

        public int Registrar(Usuario obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(obj.Documento))
            {
                Mensaje += "Es necesario el documento del usuario\n";
            }

            if (string.IsNullOrWhiteSpace(obj.NombreCompleto))
            {
                Mensaje += "Es necesario el nombre completo del usuario\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Clave))
            {
                Mensaje += "Es necesaria la clave del usuario\n";
            }

            if (Mensaje != string.Empty)
            {
                return 0;
            }

            obj.Clave = Seguridad.HashPassword(obj.Clave);

            return objcd_usuario.Registrar(obj, out Mensaje);
        }

        public bool Editar(Usuario obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(obj.Documento))
            {
                Mensaje += "Es necesario el documento del usuario\n";
            }

            if (string.IsNullOrWhiteSpace(obj.NombreCompleto))
            {
                Mensaje += "Es necesario el nombre completo del usuario\n";
            }

            if (string.IsNullOrWhiteSpace(obj.Clave))
            {
                Mensaje += "Es necesaria la clave del usuario\n";
            }

            if (Mensaje != string.Empty)
            {
                return false;
            }

            obj.Clave = Seguridad.HashPassword(obj.Clave);

            return objcd_usuario.Editar(obj, out Mensaje);
        }

        public bool Eliminar(Usuario obj, out string Mensaje)
        {
            return objcd_usuario.Eliminar(obj, out Mensaje);
        }
    }
}