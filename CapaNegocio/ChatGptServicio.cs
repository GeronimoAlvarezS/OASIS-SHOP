using CapaEntidad;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class ChatGptServicio
    {
        private readonly string _apiKey;
        private readonly string _modelo;

        public ChatGptServicio(string apiKey, string modelo = "gpt-4.1-mini")
        {
            _apiKey = apiKey;
            _modelo = modelo;
        }

        public async Task<ChatBotRespuesta> ProcesarMensajeAsync(
            string mensajeUsuario,
            List<ChatBotMensaje> historial)
        {
            ChatBotRespuesta respuestaFinal = new ChatBotRespuesta();

            try
            {
                string apiKey = _apiKey;
                string modelo = string.IsNullOrWhiteSpace(_modelo)
                    ? "gpt-4.1-mini"
                    : _modelo;

                if (string.IsNullOrWhiteSpace(mensajeUsuario))
                {
                    return CrearRespuestaBasica(
                        "Por favor, describe brevemente el problema que estás presentando en el sistema para poder ayudarte.",
                        "Soporte básico",
                        false
                    );
                }

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return GenerarRespuestaPredictiva(mensajeUsuario, historial);
                }

                string instrucciones = @"
Eres Oasis Assistant, un chatbot de soporte técnico experto en el sistema web de inventario OasisShop.

Tu objetivo principal es ayudar al usuario con respuestas útiles, lógicas y predictivas, incluso cuando el mensaje sea corto.

Contexto del sistema OasisShop:
- Usuarios
- Roles y permisos
- Productos
- Categorías
- Marcas
- Clientes
- Proveedores
- Compras
- Ventas
- Inventario
- Reportes
- PDF
- Excel
- Facturación electrónica
- Código QR
- Configuración del negocio
- Logo, RUT, nombre, dirección y datos del negocio

Reglas obligatorias:
1. Analiza siempre el historial de conversación antes de responder.
2. Si el usuario responde con frases cortas como ""sí"", ""ya está"", ""ya lo hice"", ""no funciona"", debes deducir el contexto usando el historial.
3. No digas nunca que la inteligencia artificial no está configurada.
4. No digas que no puedes ayudar por falta de configuración.
5. No escales saludos, respuestas cortas ni mensajes incompletos.
6. Si el usuario saluda, responde amablemente y pide que describa el problema.
7. Si el usuario da poca información, haz una pregunta de aclaración útil.
8. Si el usuario ya dio contexto antes, no vuelvas a pedir lo mismo.
9. Cada respuesta debe avanzar la conversación.
10. No repitas la misma pregunta.
11. No te quedes en bucle.
12. Da soluciones prácticas y concretas.
13. Si el problema parece de datos, configuración o uso del sistema, guía al usuario.
14. Si el problema parece técnico, indica una posible causa.
15. Solo escala cuando:
   - ya se intentaron varias soluciones,
   - el usuario confirma que no funcionaron,
   - existe un error técnico claro,
   - se requiere revisar código fuente o base de datos.

Formato obligatorio:
Devuelve únicamente JSON válido con esta estructura:

{
  ""Respuesta"": ""respuesta clara para el usuario"",
  ""Escalar"": false,
  ""TipoCaso"": ""Soporte básico""
}

Tipos permitidos:
- Soporte básico
- Parametrización humana
- Error del sistema
- Escalar a desarrollo
- Nuevo problema
";

                List<object> mensajes = new List<object>();

                mensajes.Add(new
                {
                    role = "system",
                    content = instrucciones
                });

                List<ChatBotMensaje> historialReciente = historial == null
                    ? new List<ChatBotMensaje>()
                    : historial
                        .OrderByDescending(m => m.FechaRegistro)
                        .Take(10)
                        .OrderBy(m => m.FechaRegistro)
                        .ToList();

                foreach (ChatBotMensaje item in historialReciente)
                {
                    string rol = item.Remitente == "Usuario" ? "user" : "assistant";

                    mensajes.Add(new
                    {
                        role = rol,
                        content = item.Mensaje ?? string.Empty
                    });
                }

                mensajes.Add(new
                {
                    role = "user",
                    content = mensajeUsuario
                });

                var cuerpo = new
                {
                    model = modelo,
                    input = mensajes.ToArray(),
                    store = false
                };

                string json = JsonConvert.SerializeObject(cuerpo);

                using (HttpClient cliente = new HttpClient())
                {
                    cliente.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiKey);

                    StringContent contenido = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"
                    );

                    HttpResponseMessage response = await cliente.PostAsync(
                        "https://api.openai.com/v1/responses",
                        contenido
                    );

                    string resultado = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return GenerarRespuestaPredictiva(mensajeUsuario, historialReciente);
                    }

                    string textoIA = ObtenerTextoRespuesta(resultado);

                    if (string.IsNullOrWhiteSpace(textoIA))
                    {
                        return GenerarRespuestaPredictiva(mensajeUsuario, historialReciente);
                    }

                    ChatBotRespuesta respuestaIA = null;

                    try
                    {
                        respuestaIA = JsonConvert.DeserializeObject<ChatBotRespuesta>(textoIA);
                    }
                    catch
                    {
                        return CrearRespuestaBasica(
                            textoIA,
                            "Soporte básico",
                            false
                        );
                    }

                    if (respuestaIA == null || string.IsNullOrWhiteSpace(respuestaIA.Respuesta))
                    {
                        return GenerarRespuestaPredictiva(mensajeUsuario, historialReciente);
                    }

                    respuestaIA.Respuesta = LimpiarRespuestaProhibida(respuestaIA.Respuesta);

                    if (string.IsNullOrWhiteSpace(respuestaIA.TipoCaso))
                    {
                        respuestaIA.TipoCaso = "Soporte básico";
                    }

                    return respuestaIA;
                }
            }
            catch
            {
                return GenerarRespuestaPredictiva(mensajeUsuario, historial);
            }
        }

        public async Task<ChatBotRespuesta> ProcesarMensajeAsync(string mensajeUsuario)
        {
            return await ProcesarMensajeAsync(
                mensajeUsuario,
                new List<ChatBotMensaje>()
            );
        }

        private ChatBotRespuesta GenerarRespuestaPredictiva(
            string mensajeUsuario,
            List<ChatBotMensaje> historial)
        {
            string mensaje = mensajeUsuario?.Trim().ToLower() ?? string.Empty;
            string contexto = ObtenerContextoHistorial(historial);

            if (EsSaludo(mensaje))
            {
                return CrearRespuestaBasica(
                    "Hola, soy Oasis Assistant. Cuéntame qué problema tienes en el sistema y te ayudaré paso a paso.",
                    "Soporte básico",
                    false
                );
            }

            if (mensaje.Contains("ya") || mensaje.Contains("sí") || mensaje.Contains("si") || mensaje.Contains("listo") || mensaje.Contains("verificado"))
            {
                return CrearRespuestaBasica(
                    "Perfecto. Si ya verificaste esa información y el problema continúa, es posible que el inconveniente esté relacionado con una validación interna, datos incompletos en otro campo o un error al guardar la información. Revisa si el sistema muestra algún mensaje al guardar y confirma si el problema ocurre en negocio, productos, compras, ventas o reportes.",
                    "Soporte básico",
                    false
                );
            }

            if (mensaje.Contains("no funciona") || mensaje.Contains("error") || mensaje.Contains("falla") || mensaje.Contains("problema"))
            {
                return CrearRespuestaBasica(
                    "Entiendo. Para ayudarte mejor, revisa primero si todos los campos obligatorios están completos, si el formato de los datos es correcto y si el sistema muestra algún mensaje de error. Indícame en qué módulo ocurre el problema y qué acción estabas realizando.",
                    "Soporte básico",
                    false
                );
            }

            if (contexto.Contains("negocio") || contexto.Contains("rut") || contexto.Contains("logo") || contexto.Contains("dirección") || contexto.Contains("direccion"))
            {
                return CrearRespuestaBasica(
                    "Por el contexto, el problema parece estar relacionado con la configuración del negocio. Verifica que el nombre, RUT, dirección y logo estén completos, que el logo tenga un formato válido y que al guardar no aparezca ningún mensaje de validación. Si todo está correcto y no guarda, puede tratarse de una falla en la actualización de datos.",
                    "Parametrización humana",
                    false
                );
            }

            if (contexto.Contains("producto") || contexto.Contains("stock") || contexto.Contains("inventario"))
            {
                return CrearRespuestaBasica(
                    "Por el contexto, el problema parece estar relacionado con productos o inventario. Verifica que el producto esté activo, tenga categoría y marca asignadas, y que el stock se esté actualizando correctamente después de compras o ventas.",
                    "Soporte básico",
                    false
                );
            }

            if (contexto.Contains("venta") || contexto.Contains("factura") || contexto.Contains("qr") || contexto.Contains("pdf"))
            {
                return CrearRespuestaBasica(
                    "Por el contexto, el problema parece estar relacionado con ventas, facturación o generación de PDF. Verifica que la venta esté registrada correctamente, que tenga productos asociados y que los datos del negocio estén completos antes de generar el comprobante.",
                    "Soporte básico",
                    false
                );
            }

            if (contexto.Contains("compra") || contexto.Contains("proveedor"))
            {
                return CrearRespuestaBasica(
                    "Por el contexto, el problema parece estar relacionado con compras. Verifica que el proveedor esté seleccionado, que los productos tengan cantidades válidas y que el monto pagado sea correcto antes de registrar la compra.",
                    "Soporte básico",
                    false
                );
            }

            return CrearRespuestaBasica(
                "Entiendo. Con la información que me das, puedo ayudarte a revisar el problema. Indícame en qué módulo ocurre, qué acción estabas realizando y si aparece algún mensaje en pantalla.",
                "Soporte básico",
                false
            );
        }

        private string ObtenerContextoHistorial(List<ChatBotMensaje> historial)
        {
            if (historial == null || historial.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(" ",
                historial
                    .OrderByDescending(h => h.FechaRegistro)
                    .Take(6)
                    .Select(h => h.Mensaje ?? string.Empty)
            ).ToLower();
        }

        private bool EsSaludo(string mensaje)
        {
            return mensaje == "hola" ||
                   mensaje == "buenas" ||
                   mensaje == "buenos días" ||
                   mensaje == "buenos dias" ||
                   mensaje == "buenas tardes" ||
                   mensaje == "buenas noches" ||
                   mensaje.Contains("hola");
        }

        private ChatBotRespuesta CrearRespuestaBasica(
            string respuesta,
            string tipoCaso,
            bool escalar)
        {
            return new ChatBotRespuesta
            {
                Respuesta = respuesta,
                TipoCaso = tipoCaso,
                Escalar = escalar
            };
        }

        private string LimpiarRespuestaProhibida(string respuesta)
        {
            if (string.IsNullOrWhiteSpace(respuesta))
            {
                return respuesta;
            }

            string texto = respuesta.ToLower();

            if (texto.Contains("inteligencia artificial no está configurada") ||
                texto.Contains("inteligencia artificial no esta configurada") ||
                texto.Contains("no fue posible obtener respuesta de la inteligencia artificial") ||
                texto.Contains("ocurrió un error al conectar") ||
                texto.Contains("ocurrio un error al conectar"))
            {
                return "Puedo ayudarte con el problema. Describe qué estabas intentando hacer, en qué módulo ocurrió y si el sistema mostró algún mensaje en pantalla.";
            }

            return respuesta;
        }

        private string ObtenerTextoRespuesta(string jsonRespuesta)
        {
            try
            {
                JObject root = JObject.Parse(jsonRespuesta);
                JToken output = root["output"];

                if (output == null)
                {
                    return string.Empty;
                }

                foreach (JToken item in output)
                {
                    JToken content = item["content"];

                    if (content == null)
                    {
                        continue;
                    }

                    foreach (JToken contenido in content)
                    {
                        JToken text = contenido["text"];

                        if (text != null)
                        {
                            return text.ToString();
                        }
                    }
                }
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }
    }
}