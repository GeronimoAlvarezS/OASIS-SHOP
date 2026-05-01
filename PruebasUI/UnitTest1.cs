using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;

namespace PruebasUI
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class LoginTests : PageTest
    {
        [Test]
        public async Task Login_ConCredencialesValidas_DebeIngresarAlSistema()
        {
            await Page.GotoAsync("https://localhost:7238/");

            // Usamos .First() por si hay varios elementos parecidos, así el robot elige el primero que vea
            await Page.GetByLabel("Documento de identidad").First.FillAsync("10");
            await Page.GetByLabel("Contraseña").First.FillAsync("1234");

            // Hacemos clic en el botón de Iniciar Sesión
            await Page.GetByRole(AriaRole.Button, new() { Name = "Iniciar Sesión" }).ClickAsync();

            // Esperamos a que la URL cambie o aparezca algo que confirme el éxito
            // Si después de loguear vas al Home, esto lo confirmará:
            await Page.WaitForURLAsync("**/Home**");
        }
    }
}