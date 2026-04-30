using System;
using System.Security.Cryptography;

namespace CapaNegocio
{
    public static class Seguridad
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía.", nameof(password));

            byte[] salt = new byte[SaltSize];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                hash = pbkdf2.GetBytes(HashSize);
            }

            return Iterations + "." +
                   Convert.ToBase64String(salt) + "." +
                   Convert.ToBase64String(hash);
        }

        public static bool VerificarPassword(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
                return false;

            string[] partes = passwordHash.Split('.');

            if (partes.Length != 3)
                return false;

            int iteraciones;

            if (!int.TryParse(partes[0], out iteraciones))
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(partes[1]);
                byte[] hashGuardado = Convert.FromBase64String(partes[2]);

                byte[] hashIngresado;

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iteraciones))
                {
                    hashIngresado = pbkdf2.GetBytes(hashGuardado.Length);
                }

                return CompararHashes(hashGuardado, hashIngresado);
            }
            catch
            {
                return false;
            }
        }

        private static bool CompararHashes(byte[] hashGuardado, byte[] hashIngresado)
        {
            if (hashGuardado == null || hashIngresado == null)
                return false;

            if (hashGuardado.Length != hashIngresado.Length)
                return false;

            int diferencias = 0;

            for (int i = 0; i < hashGuardado.Length; i++)
            {
                diferencias |= hashGuardado[i] ^ hashIngresado[i];
            }

            return diferencias == 0;
        }
    }
}