using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class UsuarioServicio
    {
        // Instancia de la capa de datos para acceder a las operaciones relacionadas con usuarios.
        private UsuarioDatos objcd_usuario = new UsuarioDatos();

        public List<Usuario> Listar()
        {
            return objcd_usuario.Listar();
        }

        // Valida las credenciales del usuario para el inicio de sesión.
        // Primero busca el usuario por documento y luego verifica la clave ingresada.
        public Usuario ObtenerPorCredenciales(string documento, string clave)
        {
            Usuario usuario = objcd_usuario.ObtenerPorDocumento(documento);

            // Si no existe un usuario con el documento ingresado, retorna null.
            if (usuario == null)
            {
                return null;
            }

            // Verifica si la clave ingresada coincide con la clave almacenada en formato hash.
            bool claveValida = Seguridad.VerificarPassword(clave, usuario.Clave);

            // Compatibilidad con claves antiguas guardadas sin encriptar.
            // Si la clave no es válida como hash, pero coincide directamente con la clave almacenada,
            // se actualiza la clave guardándola ahora en formato hash.

            if (!claveValida && usuario.Clave == clave)
            {
                usuario.Clave = Seguridad.HashPassword(clave);
                objcd_usuario.ActualizarClave(usuario.IdUsuario, usuario.Clave);
                return usuario;
            }

            // Si la clave no es válida, se niega el acceso retornando null.

            if (!claveValida)
            {
                return null;
            }

            // Si las credenciales son correctas, retorna el usuario autenticado.

            return usuario;
        }

        // Registra un nuevo usuario en el sistema.
        // Antes de enviarlo a la capa de datos, valida los campos obligatorios
        // y convierte la clave a formato hash por seguridad.
        public int Registrar(Usuario obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            // Validación del documento del usuario.

            if (string.IsNullOrWhiteSpace(obj.Documento))
            {
                Mensaje += "Es necesario el documento del usuario\n";
            }

            // Validación del nombre completo del usuario.

            if (string.IsNullOrWhiteSpace(obj.NombreCompleto))
            {
                Mensaje += "Es necesario el nombre completo del usuario\n";
            }

            // Validación de la clave del usuario.

            if (string.IsNullOrWhiteSpace(obj.Clave))
            {
                Mensaje += "Es necesaria la clave del usuario\n";
            }

            // Si existen errores de validación, se cancela el registro.

            if (Mensaje != string.Empty)
            {
                return 0;
            }

            // Se encripta la clave antes de guardarla en la base de datos.

            obj.Clave = Seguridad.HashPassword(obj.Clave);

            // Envía el usuario validado a la capa de datos para su registro.

            return objcd_usuario.Registrar(obj, out Mensaje);
        }

        // Edita la información de un usuario existente.
        // Valida los campos obligatorios y vuelve a generar el hash de la clave.
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

            // Si hay errores de validación, no se realiza la edición.

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