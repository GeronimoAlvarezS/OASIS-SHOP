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

            if (usuario == null)
            {
                return null;
            }

            string claveIngresada = clave?.Trim() ?? string.Empty;
            string claveGuardada = usuario.Clave?.Trim() ?? string.Empty;

            // Caso 1: clave antigua guardada en texto plano, ejemplo: 1234
            if (claveGuardada == claveIngresada)
            {
                string claveHash = Seguridad.HashPassword(claveIngresada);

                usuario.Clave = claveHash;
                objcd_usuario.ActualizarClave(usuario.IdUsuario, claveHash);

                return usuario;
            }

            // Caso 2: clave ya guardada con hash
            bool claveValida = Seguridad.VerificarPassword(claveIngresada, claveGuardada);

            if (!claveValida)
            {
                return null;
            }

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