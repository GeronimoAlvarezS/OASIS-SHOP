using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class ChatBotServicio
    {
        private readonly ChatBotBaseConocimientoDatos _baseConocimientoDatos =
            new ChatBotBaseConocimientoDatos();

        private readonly ChatBotMensajeDatos _mensajeDatos =
            new ChatBotMensajeDatos();

        private readonly ChatBotCasoSoporteDatos _casoDatos =
            new ChatBotCasoSoporteDatos();

        private readonly ChatGptServicio _chatGptServicio;

        public ChatBotServicio()
        {
            _chatGptServicio = null;
        }

        public ChatBotServicio(string apiKey)
        {
            _chatGptServicio = new ChatGptServicio(apiKey);
        }

        public async Task<ChatBotRespuesta> ProcesarMensajeAsync(
            int idConversacion,
            int idUsuario,
            string mensajeUsuario)
        {
            ChatBotRespuesta respuesta = new ChatBotRespuesta();

            try
            {
                if (string.IsNullOrWhiteSpace(mensajeUsuario))
                {
                    return CrearRespuesta(
                        "Cuéntame qué problema estás presentando en el sistema para poder ayudarte.",
                        false,
                        "Soporte básico"
                    );
                }

                mensajeUsuario = mensajeUsuario.Trim();

                _mensajeDatos.RegistrarMensaje(
                    new ChatBotMensaje()
                    {
                        IdConversacion = idConversacion,
                        Remitente = "Usuario",
                        Mensaje = mensajeUsuario
                    },
                    out string mensajeRegistroUsuario
                );

                List<ChatBotMensaje> historial =
                    _mensajeDatos.ObtenerMensajesPorConversacion(idConversacion);

                int cantidadMensajesUsuario = historial
                    .Where(m => m.Remitente == "Usuario")
                    .Count();

                if (EsSaludoOMensajeGeneral(mensajeUsuario))
                {
                    respuesta = CrearRespuesta(
                        "¡Hola! Soy Oasis Assistant. Cuéntame qué problema tienes en el sistema y te ayudaré paso a paso.",
                        false,
                        "Soporte básico"
                    );
                }
                else if (UsuarioSolicitaSoporteHumano(mensajeUsuario))
                {
                    respuesta = CrearRespuesta(
                        "Entiendo. Voy a escalar tu caso al equipo de desarrollo para que puedan revisarlo con mayor detalle.",
                        true,
                        "Escalar a desarrollo"
                    );
                }
                else if (EsPreguntaPasoAPaso(mensajeUsuario))
                {
                    respuesta = CrearRespuesta(
                        GenerarPasoAPasoPorContexto(mensajeUsuario, historial),
                        false,
                        "Soporte básico"
                    );
                }
                else
                {
                    ChatBotBaseConocimiento conocimiento =
                        _baseConocimientoDatos.BuscarCoincidencia(mensajeUsuario);

                    if (conocimiento != null)
                    {
                        respuesta = CrearRespuesta(
                            conocimiento.Solucion,
                            DebeEscalarPorTipo(conocimiento.TipoCaso),
                            conocimiento.TipoCaso
                        );
                    }
                    else if (_chatGptServicio != null)
                    {
                        respuesta = await _chatGptServicio.ProcesarMensajeAsync(
                            mensajeUsuario,
                            historial
                        );
                    }
                    else
                    {
                        respuesta = GenerarRespuestaPredictiva(
                            mensajeUsuario,
                            historial,
                            cantidadMensajesUsuario
                        );
                    }
                }

                if (respuesta == null || string.IsNullOrWhiteSpace(respuesta.Respuesta))
                {
                    respuesta = GenerarRespuestaPredictiva(
                        mensajeUsuario,
                        historial,
                        cantidadMensajesUsuario
                    );
                }

                respuesta.Respuesta = LimpiarRespuestaNoPermitida(
                    respuesta.Respuesta,
                    mensajeUsuario,
                    historial
                );

                respuesta = ControlarEscalamientoPrematuro(
                    respuesta,
                    cantidadMensajesUsuario,
                    historial,
                    mensajeUsuario
                );

                respuesta.Respuesta = EvitarRespuestaRepetida(
                    respuesta.Respuesta,
                    historial,
                    mensajeUsuario
                );

                _mensajeDatos.RegistrarMensaje(
                    new ChatBotMensaje()
                    {
                        IdConversacion = idConversacion,
                        Remitente = "ChatBot",
                        Mensaje = respuesta.Respuesta
                    },
                    out string mensajeRegistroChatBot
                );

                if (respuesta.Escalar)
                {
                    _casoDatos.RegistrarCaso(
                        new ChatBotCasoSoporte()
                        {
                            IdConversacion = idConversacion,
                            IdUsuario = idUsuario,
                            Situacion = mensajeUsuario,
                            RespuestaChatBot = respuesta.Respuesta,
                            TipoCaso = respuesta.TipoCaso
                        },
                        out string mensajeCaso
                    );
                }
            }
            catch
            {
                respuesta = CrearRespuesta(
                    "Se presentó un inconveniente interno al procesar tu solicitud. Voy a escalar el caso al equipo de desarrollo para que pueda ser revisado.",
                    true,
                    "Escalar a desarrollo"
                );
            }

            return respuesta;
        }

        private ChatBotRespuesta GenerarRespuestaPredictiva(
            string mensajeUsuario,
            List<ChatBotMensaje> historial,
            int cantidadMensajesUsuario)
        {
            string mensaje = mensajeUsuario?.Trim().ToLower() ?? string.Empty;
            string contexto = ObtenerContextoHistorial(historial);

            if (EsPreguntaPasoAPaso(mensaje))
            {
                return CrearRespuesta(
                    GenerarPasoAPasoPorContexto(mensajeUsuario, historial),
                    false,
                    "Soporte básico"
                );
            }

            if (EsRespuestaCortaDeContinuacion(mensaje))
            {
                if (contexto.Contains("negocio") ||
                    contexto.Contains("rut") ||
                    contexto.Contains("logo") ||
                    contexto.Contains("dirección") ||
                    contexto.Contains("direccion"))
                {
                    return CrearRespuesta(
                        "Perfecto. Si ya verificaste que el nombre, RUT, dirección y logo están completos, el problema puede estar relacionado con el formato del RUT, el tipo de archivo del logo o que los datos no se estén guardando correctamente. Intenta guardar nuevamente y revisa si aparece algún mensaje de validación.",
                        false,
                        "Parametrización humana"
                    );
                }

                if (contexto.Contains("producto") ||
                    contexto.Contains("inventario") ||
                    contexto.Contains("stock"))
                {
                    return CrearRespuesta(
                        "Entiendo. Si ya verificaste los datos del producto y el problema continúa, revisa que tenga categoría, marca, precio y stock válidos. También confirma si el producto está activo y si el cambio se refleja después de guardar.",
                        false,
                        "Soporte básico"
                    );
                }

                if (contexto.Contains("venta") ||
                    contexto.Contains("factura") ||
                    contexto.Contains("pdf") ||
                    contexto.Contains("qr"))
                {
                    return CrearRespuesta(
                        "Perfecto. Si la venta ya está registrada y el problema continúa, revisa que tenga cliente, productos, cantidades y totales válidos. Para generar PDF o QR también deben estar completos los datos del negocio.",
                        false,
                        "Soporte básico"
                    );
                }

                if (contexto.Contains("compra") ||
                    contexto.Contains("proveedor"))
                {
                    return CrearRespuesta(
                        "Entiendo. Si ya verificaste la compra, revisa que el proveedor esté seleccionado, que los productos tengan cantidad válida y que el monto pagado sea suficiente respecto al total de la compra.",
                        false,
                        "Soporte básico"
                    );
                }

                if (cantidadMensajesUsuario >= 4)
                {
                    return CrearRespuesta(
                        "Con la información disponible no encuentro una solución definitiva en este momento. Voy a escalar el caso al equipo de desarrollo para que puedan revisarlo con mayor detalle.",
                        true,
                        "Escalar a desarrollo"
                    );
                }

                return CrearRespuesta(
                    "Perfecto. Si ya verificaste esa información y el problema continúa, revisa si aparece algún mensaje de error o validación. También confirma en qué módulo ocurre para guiarte con mayor precisión.",
                    false,
                    "Soporte básico"
                );
            }

            if (mensaje.Contains("no funciona") ||
                mensaje.Contains("error") ||
                mensaje.Contains("falla") ||
                mensaje.Contains("no guarda") ||
                mensaje.Contains("no carga") ||
                mensaje.Contains("no aparece"))
            {
                if (cantidadMensajesUsuario >= 4 || UsuarioConfirmaQueNadaFunciono(historial))
                {
                    return CrearRespuesta(
                        "Ya se realizaron varias validaciones y el problema continúa. Voy a escalar el caso al equipo de desarrollo para que revisen la causa técnica.",
                        true,
                        "Escalar a desarrollo"
                    );
                }

                return CrearRespuesta(
                    "Entiendo el problema. Primero verifica que todos los campos obligatorios estén completos y que los datos tengan el formato correcto. Luego intenta guardar nuevamente. Si aparece un mensaje de error, escríbelo exactamente para identificar la causa.",
                    false,
                    "Soporte básico"
                );
            }

            if (cantidadMensajesUsuario <= 2 && EsMensajeSinDetalle(mensajeUsuario, cantidadMensajesUsuario))
            {
                return CrearRespuesta(
                    "Para ayudarte mejor, dime en qué módulo ocurre el problema, qué acción estabas realizando y si aparece algún mensaje de error.",
                    false,
                    "Soporte básico"
                );
            }

            if (cantidadMensajesUsuario >= 5)
            {
                return CrearRespuesta(
                    "No encuentro una solución clara con la información disponible en este momento. Voy a escalar el caso al equipo de desarrollo para que puedan analizarlo.",
                    true,
                    "Escalar a desarrollo"
                );
            }

            return CrearRespuesta(
                "Con la información que me das, lo más recomendable es revisar el módulo donde ocurre el problema, validar los campos obligatorios y confirmar si el sistema muestra algún mensaje. Indícame el módulo exacto para darte una solución más precisa.",
                false,
                "Soporte básico"
            );
        }

        private string GenerarPasoAPasoPorContexto(
            string mensajeUsuario,
            List<ChatBotMensaje> historial)
        {
            string texto = (mensajeUsuario + " " + ObtenerContextoHistorial(historial)).ToLower();

            if (texto.Contains("venta"))
            {
                return "Para registrar una venta, sigue estos pasos:\n\n" +
                       "Paso 1: Ingresa al módulo de Ventas.\n" +
                       "Paso 2: Da clic en agregar producto.\n" +
                       "Paso 3: Ingresa los datos del cliente.\n" +
                       "Paso 4: Busca el producto que deseas vender.\n" +
                       "Paso 5: Agrega la cantidad del producto.\n" +
                       "Paso 6: Da clic en el botón de crear\n" +
                       "Paso 7: Selecciona el tipo de factura\n" +
                       "Paso 8: Ingresa el monto pagado por el cliente.\n" +
                       "Paso 9: Confirma la venta.\n" +
                       "Paso 10: Genera el comprobante en PDF si lo necesitas a través del módulo de Detalle de Venta.";
            }

            if (texto.Contains("compra"))
            {
                return "Para registrar una compra de inventario, sigue estos pasos:\n\n" +
                       "Paso 1: Ingresa al módulo de Inventario.\n" +
                       "Paso 2: Selecciona el proveedor.\n" +
                       "Paso 3: Busca los productos comprados.\n" +
                       "Paso 4: Ingresa la cantidad de cada producto.\n" +
                       "Paso 5: Ingresa el precio de compra.\n" +
                       "Paso 6: Verifica el total de la compra.\n" +
                       "Paso 7: Ingresa el monto pagado.\n" +
                       "Paso 8: Guarda la compra.\n" +
                       "Paso 9: Verifica que el stock se haya actualizado correctamente.";
            }

            if (texto.Contains("producto"))
            {
                return "Para registrar un producto, sigue estos pasos:\n\n" +
                       "Paso 1: Ingresa al módulo de Productos.\n" +
                       "Paso 2: Haz clic en Nuevo o Registrar producto.\n" +
                       "Paso 3: Completa el nombre del producto.\n" +
                       "Paso 4: Selecciona la categoría y la marca.\n" +
                       "Paso 5: Ingresa el precio de compra o venta según corresponda.\n" +
                       "Paso 6: Define el stock inicial.\n" +
                       "Paso 7: Verifica que todos los campos obligatorios estén completos.\n" +
                       "Paso 8: Guarda el producto.\n" +
                       "Paso 9: Confirma que el producto aparezca en la lista.";
            }

            if (texto.Contains("reporte"))
            {
                return "Para generar un reporte, sigue estos pasos:\n\n" +
                       "Paso 1: Ingresa al módulo de Reportes.\n" +
                       "Paso 2: Selecciona el tipo de reporte: ventas o compras.\n" +
                       "Paso 3: Define el rango de fechas.\n" +
                       "Paso 4: Haz clic en Buscar o Filtrar.\n" +
                       "Paso 5: Revisa los resultados en pantalla.\n" +
                       "Paso 6: Usa la barra de búsqueda si necesitas filtrar información específica.\n" +
                       "Paso 7: Exporta el reporte a Excel si necesitas descargarlo.";
            }

            if (texto.Contains("usuario"))
            {
                return "Para gestionar un usuario, sigue estos pasos:\n\n" +
                       "Paso 1: Ingresa al módulo de Usuarios.\n" +
                       "Paso 2: Haz clic en Nuevo usuario.\n" +
                       "Paso 3: Completa los datos personales del usuario.\n" +
                       "Paso 4: Asigna el rol correspondiente.\n" +
                       "Paso 5: Define si el usuario estará activo o inactivo.\n" +
                       "Paso 6: Guarda los cambios.\n" +
                       "Paso 7: Verifica que el usuario aparezca correctamente en la lista.";
            }

            if (texto.Contains("permiso") || texto.Contains("rol"))
            {
                return "Para revisar permisos o roles, sigue estos pasos:\n\n" +
                       "Paso 1: Ingresa al módulo de Usuarios o Roles.\n" +
                       "Paso 2: Identifica el usuario que presenta el problema.\n" +
                       "Paso 3: Verifica qué rol tiene asignado.\n" +
                       "Paso 4: Revisa si ese rol tiene acceso al módulo correspondiente.\n" +
                       "Paso 5: Si el rol no tiene permisos, asigna un rol adecuado.\n" +
                       "Paso 6: Guarda los cambios.\n" +
                       "Paso 7: Cierra sesión e inicia nuevamente para validar los permisos.";
            }

            if (texto.Contains("negocio") ||
                texto.Contains("rut") ||
                texto.Contains("logo") ||
                texto.Contains("dirección") ||
                texto.Contains("direccion"))
            {
                return "Para configurar los datos del negocio, sigue estos pasos:\n\n" +
                       "Paso 1: Ingresa al módulo de Negocio o Configuración del negocio.\n" +
                       "Paso 2: Completa el nombre del negocio.\n" +
                       "Paso 3: Ingresa el RUT.\n" +
                       "Paso 4: Selecciona el departamento y la ciudad.\n" +
                       "Paso 5: Escribe la dirección.\n" +
                       "Paso 6: Carga el logo del negocio en un formato válido.\n" +
                       "Paso 7: Guarda los cambios.\n" +
                       "Paso 8: Verifica que la información aparezca correctamente.";
            }

            if (texto.Contains("pdf") || texto.Contains("factura") || texto.Contains("comprobante"))
            {
                return "Para generar un comprobante o PDF, sigue estos pasos:\n\n" +
                       "Paso 1: Verifica que la venta o compra esté registrada correctamente.\n" +
                       "Paso 2: Ingresa al detalle del registro.\n" +
                       "Paso 3: Revisa que los datos del cliente o proveedor estén completos.\n" +
                       "Paso 4: Verifica que los productos, cantidades y totales sean correctos.\n" +
                       "Paso 5: Haz clic en Generar PDF o Descargar comprobante.\n" +
                       "Paso 6: Abre el archivo descargado y valida que la información sea correcta.";
            }

            return "Claro. Para darte el paso a paso necesito saber qué proceso deseas realizar: venta, compra, producto, reporte, usuario, permisos, negocio o generación de PDF.";
        }

        private ChatBotRespuesta CrearRespuesta(
            string mensaje,
            bool escalar,
            string tipoCaso)
        {
            return new ChatBotRespuesta()
            {
                Respuesta = mensaje,
                Escalar = escalar,
                TipoCaso = tipoCaso
            };
        }

        private bool DebeEscalarPorTipo(string tipoCaso)
        {
            return tipoCaso == "Error del sistema" ||
                   tipoCaso == "Escalar a desarrollo" ||
                   tipoCaso == "Nuevo problema";
        }

        private ChatBotRespuesta ControlarEscalamientoPrematuro(
            ChatBotRespuesta respuesta,
            int cantidadMensajesUsuario,
            List<ChatBotMensaje> historial,
            string mensajeUsuario)
        {
            if (respuesta == null)
            {
                return CrearRespuesta(
                    "Cuéntame un poco más del problema para poder ayudarte.",
                    false,
                    "Soporte básico"
                );
            }

            if (EsPreguntaPasoAPaso(mensajeUsuario))
            {
                respuesta.Escalar = false;
                respuesta.TipoCaso = "Soporte básico";
                return respuesta;
            }

            if (UsuarioSolicitaSoporteHumano(mensajeUsuario))
            {
                respuesta.Escalar = true;
                respuesta.TipoCaso = "Escalar a desarrollo";
                return respuesta;
            }

            if (respuesta.Escalar && cantidadMensajesUsuario < 4)
            {
                respuesta.Escalar = false;
                respuesta.TipoCaso = "Soporte básico";

                respuesta.Respuesta =
                    "Antes de escalar el caso, intentemos una validación adicional. Revisa si los campos obligatorios están completos, si el formato de los datos es correcto y si aparece algún mensaje de error en pantalla.";
            }

            if (respuesta.Escalar && !UsuarioConfirmaQueNadaFunciono(historial))
            {
                respuesta.Escalar = false;
                respuesta.TipoCaso = "Soporte básico";

                respuesta.Respuesta =
                    "Todavía podemos intentar una solución antes de escalar el caso. Confirma qué pasos ya realizaste y qué mensaje aparece exactamente en pantalla.";
            }

            return respuesta;
        }

        private bool UsuarioConfirmaQueNadaFunciono(List<ChatBotMensaje> historial)
        {
            if (historial == null)
            {
                return false;
            }

            string texto = string.Join(" ",
                historial
                    .Where(h => h.Remitente == "Usuario")
                    .OrderByDescending(h => h.FechaRegistro)
                    .Take(5)
                    .Select(h => h.Mensaje ?? string.Empty)
            ).ToLower();

            return texto.Contains("no funcionó") ||
                   texto.Contains("no funciono") ||
                   texto.Contains("sigue igual") ||
                   texto.Contains("ya hice eso") ||
                   texto.Contains("ya lo hice") ||
                   texto.Contains("persiste") ||
                   texto.Contains("continúa") ||
                   texto.Contains("continua") ||
                   texto.Contains("nada funciona") ||
                   texto.Contains("no se solucionó") ||
                   texto.Contains("no se soluciono");
        }

        private bool UsuarioSolicitaSoporteHumano(string mensaje)
        {
            string texto = mensaje.ToLower().Trim();

            return texto.Contains("soporte en persona") ||
                   texto.Contains("soporte humano") ||
                   texto.Contains("pasame a soporte") ||
                   texto.Contains("pásame a soporte") ||
                   texto.Contains("equipo de desarrollo") ||
                   texto.Contains("escalar") ||
                   texto.Contains("escalalo") ||
                   texto.Contains("escálalo") ||
                   texto.Contains("no encuentro solución") ||
                   texto.Contains("no encuentro solucion") ||
                   texto.Contains("no hay solución") ||
                   texto.Contains("no hay solucion");
        }

        private bool EsPreguntaPasoAPaso(string mensaje)
        {
            string texto = mensaje.ToLower().Trim();

            return texto.Contains("paso a paso") ||
                   texto.Contains("como hago") ||
                   texto.Contains("cómo hago") ||
                   texto.Contains("como registro") ||
                   texto.Contains("cómo registro") ||
                   texto.Contains("como crear") ||
                   texto.Contains("cómo crear") ||
                   texto.Contains("como generar") ||
                   texto.Contains("cómo generar") ||
                   texto.Contains("como puedo") ||
                   texto.Contains("cómo puedo") ||
                   texto.Contains("que pasos") ||
                   texto.Contains("qué pasos") ||
                   texto.Contains("procedimiento") ||
                   texto.Contains("guia") ||
                   texto.Contains("guía");
        }

        private string EvitarRespuestaRepetida(
            string nuevaRespuesta,
            List<ChatBotMensaje> historial,
            string mensajeUsuario)
        {
            if (historial == null || historial.Count == 0)
            {
                return nuevaRespuesta;
            }

            ChatBotMensaje ultimaRespuestaBot = historial
                .Where(m => m.Remitente == "ChatBot")
                .OrderByDescending(m => m.FechaRegistro)
                .FirstOrDefault();

            if (ultimaRespuestaBot == null)
            {
                return nuevaRespuesta;
            }

            string anterior = (ultimaRespuestaBot.Mensaje ?? string.Empty).Trim().ToLower();
            string actual = (nuevaRespuesta ?? string.Empty).Trim().ToLower();

            if (anterior == actual)
            {
                string contexto = ObtenerContextoHistorial(historial);

                if (contexto.Contains("negocio") ||
                    contexto.Contains("rut") ||
                    contexto.Contains("logo"))
                {
                    return "Avancemos con una validación más específica: revisa que el RUT no tenga espacios, que el logo sea una imagen válida y que los datos del negocio se estén enviando correctamente al guardar.";
                }

                if (contexto.Contains("venta") ||
                    contexto.Contains("factura"))
                {
                    return "Avancemos con una validación más específica: confirma si la venta se registra correctamente antes de generar el comprobante, y revisa que el cliente, productos y totales estén completos.";
                }

                if (contexto.Contains("compra"))
                {
                    return "Avancemos con una validación más específica: confirma si el proveedor está seleccionado, si los productos tienen cantidades válidas y si el total de la compra se calcula correctamente.";
                }

                if (UsuarioConfirmaQueNadaFunciono(historial))
                {
                    return "Como el problema continúa después de las validaciones, voy a escalar el caso al equipo de desarrollo para su revisión.";
                }

                return "Con la información adicional que me das, avancemos un paso más: confirma el módulo exacto, la acción realizada y el mensaje que aparece en pantalla.";
            }

            return nuevaRespuesta;
        }

        private string LimpiarRespuestaNoPermitida(
            string respuesta,
            string mensajeUsuario,
            List<ChatBotMensaje> historial)
        {
            if (string.IsNullOrWhiteSpace(respuesta))
            {
                return GenerarRespuestaPredictiva(mensajeUsuario, historial, 1).Respuesta;
            }

            string texto = respuesta.ToLower();

            if (texto.Contains("inteligencia artificial no está configurada") ||
                texto.Contains("inteligencia artificial no esta configurada") ||
                texto.Contains("no fue posible obtener respuesta") ||
                texto.Contains("no encontré una solución registrada") ||
                texto.Contains("no encontre una solución registrada") ||
                texto.Contains("no pude generar una respuesta clara") ||
                texto.Contains("ocurrió un error interno") ||
                texto.Contains("ocurrio un error interno"))
            {
                return GenerarRespuestaPredictiva(mensajeUsuario, historial, 1).Respuesta;
            }

            return respuesta;
        }

        private bool EsSaludoOMensajeGeneral(string mensaje)
        {
            string texto = mensaje.ToLower().Trim();

            return texto == "hola" ||
                   texto == "buenas" ||
                   texto == "buenos dias" ||
                   texto == "buenos días" ||
                   texto == "buenas tardes" ||
                   texto == "buenas noches" ||
                   texto == "necesito soporte" ||
                   texto == "necesito soporte tecnico" ||
                   texto == "necesito soporte técnico" ||
                   texto == "ayuda" ||
                   texto == "me ayudas" ||
                   texto == "puedes ayudarme";
        }

        private bool EsMensajeSinDetalle(string mensaje, int cantidadMensajesUsuario)
        {
            string texto = mensaje.ToLower();

            if (cantidadMensajesUsuario >= 3)
            {
                return false;
            }

            if (EsRespuestaCortaDeContinuacion(texto))
            {
                return false;
            }

            if (texto.Length < 15)
            {
                return true;
            }

            bool tienePalabrasDeError =
                texto.Contains("error") ||
                texto.Contains("no guarda") ||
                texto.Contains("no carga") ||
                texto.Contains("no aparece") ||
                texto.Contains("no genera") ||
                texto.Contains("no puedo") ||
                texto.Contains("falla") ||
                texto.Contains("venta") ||
                texto.Contains("compra") ||
                texto.Contains("producto") ||
                texto.Contains("inventario") ||
                texto.Contains("factura") ||
                texto.Contains("pdf") ||
                texto.Contains("excel") ||
                texto.Contains("usuario") ||
                texto.Contains("permiso") ||
                texto.Contains("reporte") ||
                texto.Contains("negocio") ||
                texto.Contains("rut") ||
                texto.Contains("logo");

            return !tienePalabrasDeError;
        }

        private bool EsRespuestaCortaDeContinuacion(string mensaje)
        {
            string texto = mensaje.ToLower().Trim();

            return texto == "si" ||
                   texto == "sí" ||
                   texto == "ok" ||
                   texto == "listo" ||
                   texto == "ya" ||
                   texto == "ya está" ||
                   texto == "ya esta" ||
                   texto == "correcto" ||
                   texto == "entendido" ||
                   texto == "verificado" ||
                   texto.Contains("ya lo hice") ||
                   texto.Contains("ya revise") ||
                   texto.Contains("ya revisé") ||
                   texto.Contains("ya verifique") ||
                   texto.Contains("ya verifiqué") ||
                   texto.Contains("están todos") ||
                   texto.Contains("estan todos") ||
                   texto.Contains("todos los campos");
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
                    .Take(8)
                    .Select(h => h.Mensaje ?? string.Empty)
            ).ToLower();
        }
    }
}