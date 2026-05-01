using NUnit.Framework;
using CapaEntidad;

namespace PruebasUI
{
    [TestFixture]
    public class UsuarioDatosTests
    {
        [Test]
        public void CrearUsuario_ConDatosValidos_DebeAsignarPropiedadesCorrectamente()
        {
            // 1. Preparar
            Usuario usuarioPrueba = new Usuario
            {
                Documento = "10",
                NombreCompleto = "Geronimo Administrador",
                Clave = "1234"
            };

            // 2. Verificar (Usando la sintaxis moderna Assert.That)
            Assert.That(usuarioPrueba, Is.Not.Null, "El objeto usuario no debería ser nulo.");
            Assert.That(usuarioPrueba.Documento, Is.EqualTo("10"), "El documento no coincide.");
            Assert.That(usuarioPrueba.NombreCompleto, Is.EqualTo("Geronimo Administrador"), "El nombre no coincide.");
        }
    }
}