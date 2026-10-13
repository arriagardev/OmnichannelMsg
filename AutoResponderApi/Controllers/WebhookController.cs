using AutoResponderApi.Models;
using AutoResponderApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoResponderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly IAutoResponderService _autoResponderService;
    private readonly string _verifyToken;

    public WebhookController(
        ILogger<WebhookController> logger,
        IAutoResponderService autoResponderService,
        IConfiguration configuration)
    {
        _logger = logger;
        _autoResponderService = autoResponderService;
        // Obtenemos el token desde configuración o usamos uno por defecto de desarrollo
        _verifyToken = configuration["MetaWebhook:VerifyToken"] ?? "MI_TOKEN_SECRETO_PERSONALIZACION";
    }

    /// <summary>
    /// Endpoint GET para la verificación del Webhook requerida por Meta.
    /// Meta envía: hub.mode, hub.verify_token y hub.challenge como query parameters.
    /// </summary>
    [HttpGet]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? token,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        _logger.LogInformation("Verificando Webhook de Meta. Modo: {Mode}, Token recibido: {Token}", mode, token);

        if (string.Equals(mode, "subscribe", StringComparison.Ordinal) &&
            string.Equals(token, _verifyToken, StringComparison.Ordinal))
        {
            _logger.LogInformation("Webhook de Meta verificado con éxito.");
            // Meta exige responder exactamente con el hub.challenge en texto plano (HTTP 200)
            return Content(challenge ?? string.Empty, "text/plain");
        }

        _logger.LogWarning("Verificación fallida de Webhook de Meta: token o modo incorrecto.");
        return Forbid();
    }

    /// <summary>
    /// Endpoint POST para recibir eventos en tiempo real desde Facebook Messenger e Instagram Direct.
    /// Extrae el ad_title del anuncio de origen (si existe) y el texto del mensaje para logging/procesamiento.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook([FromBody] MetaPayload? payload)
    {
        if (payload == null)
        {
            _logger.LogWarning("Payload recibido nulo.");
            return BadRequest("Payload nulo");
        }

        _logger.LogInformation("Webhook recibido para objeto: {Object}", payload.Object);

        if (payload.Entry == null || payload.Entry.Count == 0)
        {
            _logger.LogInformation("El webhook no contiene elementos en 'entry'.");
            return Ok("EVENT_RECEIVED");
        }

        foreach (var entry in payload.Entry)
        {
            _logger.LogInformation("Procesando entrada de cuenta/página: {EntryId}", entry.Id);

            foreach (var messaging in entry.Messaging)
            {
                var senderId = messaging.Sender?.Id ?? "Desconocido";
                var adTitle = messaging.GetAdTitle();
                var messageText = messaging.GetMessageText();

                // Salida estructurada en consola / logs
                _logger.LogInformation("--------------------------------------------------");
                _logger.LogInformation("📥 [NUEVO MENSAJE] Remitente: {SenderId}", senderId);

                if (!string.IsNullOrWhiteSpace(adTitle))
                {
                    _logger.LogInformation("🎯 [ORIGEN ANUNCIO / CAMPAÑA] Título del Anuncio: \"{AdTitle}\"", adTitle);
                    _logger.LogInformation("ℹ️ Producto personalizado de interés: {AdTitle}", adTitle);
                }
                else
                {
                    _logger.LogInformation("ℹ️ Mensaje orgánico directo (no proviene de anuncio de Meta Ads)");
                }

                if (!string.IsNullOrWhiteSpace(messageText))
                {
                    _logger.LogInformation("💬 [CONTENIDO DEL MENSAJE]: \"{MessageText}\"", messageText);
                }
                else
                {
                    _logger.LogInformation("💬 [CONTENIDO DEL MENSAJE]: [Sin texto / Adjunto, sticker o postback]");
                }
                _logger.LogInformation("--------------------------------------------------");

                // Procesar auto-respuesta inteligente y envío por Graph API
                if (messaging.Sender?.Id != null)
                {
                    await _autoResponderService.ProcessMessageAsync(messaging.Sender.Id, messaging);
                }
            }
        }

        // Meta requiere una respuesta HTTP 200 en menos de 20s para dar por entregado el webhook y no reintentar
        return Ok("EVENT_RECEIVED");
    }
}
