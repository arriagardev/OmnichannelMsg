using AutoResponderApi.Models;

namespace AutoResponderApi.Services;

public class AutoResponderService : IAutoResponderService
{
    private readonly IMetaMessagingService _messagingService;
    private readonly ILogger<AutoResponderService> _logger;

    public AutoResponderService(IMetaMessagingService messagingService, ILogger<AutoResponderService> messagingLogger)
    {
        _messagingService = messagingService;
        _logger = messagingLogger;
    }

    public async Task ProcessMessageAsync(string senderId, MessagingItem messagingItem)
    {
        var adTitle = messagingItem.GetAdTitle();
        var messageText = messagingItem.GetMessageText()?.Trim() ?? string.Empty;

        _logger.LogInformation("🤖 Analizando mensaje para auto-respuesta. Remitente: {SenderId}, AdTitle: {AdTitle}, Texto: {Text}",
            senderId, adTitle, messageText);

        string responseMessage;

        // 1. Lógica basada en el anuncio de origen (ad_title)
        if (!string.IsNullOrWhiteSpace(adTitle))
        {
            var lowerAd = adTitle.ToLower();
            if (lowerAd.Contains("taza") || lowerAd.Contains("mug"))
            {
                responseMessage = "¡Hola! Gracias por tu interés en nuestras ☕ Tazas Personalizadas. Tenemos diseños increíbles para cualquier ocasión y con nombre o frase gratis. ¿Qué cantidad te gustaría cotizar o qué diseño tienes en mente?";
            }
            else if (lowerAd.Contains("termo") || lowerAd.Contains("vaso") || lowerAd.Contains("tumbler"))
            {
                responseMessage = "¡Hola! Qué gusto saludarte. Vi que te interesaron nuestros 🧊 Termos y Vasos Grabados láser. Mantienen tus bebidas frías o calientes por horas. ¿Te gustaría ver nuestro catálogo de colores y fuentes disponibles?";
            }
            else if (lowerAd.Contains("playera") || lowerAd.Contains("camisa") || lowerAd.Contains("shirt"))
            {
                responseMessage = "¡Hola! Gracias por ver nuestro anuncio de 👕 Playeras Personalizadas. Contamos con tallas desde infantil hasta XXL en varios colores. ¿Para qué fecha o evento las necesitas?";
            }
            else
            {
                responseMessage = $"¡Hola! Gracias por contactarnos desde nuestro anuncio \"{adTitle}\". ¿En qué te podemos ayudar hoy con tu regalo o artículo personalizado?";
            }
        }
        // 2. Lógica basada en palabras clave en el mensaje de texto si no viene de anuncio
        else if (!string.IsNullOrEmpty(messageText))
        {
            var lowerText = messageText.ToLower();
            if (lowerText.Contains("precio") || lowerText.Contains("costo") || lowerText.Contains("cuanto") || lowerText.Contains("catalogo"))
            {
                responseMessage = "¡Hola! Con gusto te compartimos información. Nos especializamos en artículos personalizados hechos a mano/láser. ¿Podrías indicarnos qué producto te interesa (tazas, termos, playeras, llaveros, etc.) para darte precio exacto?";
            }
            else if (lowerText.Contains("hola") || lowerText.Contains("buenas") || lowerText.Contains("info"))
            {
                responseMessage = "¡Hola! Bienvenido(a) a nuestro taller de personalización 🎁. ¿Qué artículo te gustaría personalizar hoy?";
            }
            else
            {
                responseMessage = "¡Hola! Hemos recibido tu mensaje. Un asesor de nuestro equipo de personalización te responderá en breve. Si buscas algún artículo en especial (tazas, termos, etc.), cuéntanos cuál es.";
            }
        }
        else
        {
            responseMessage = "¡Hola! Gracias por escribirnos a nuestro taller de artículos personalizados. ¿En qué te podemos ayudar hoy?";
        }

        // Enviar la respuesta automática a través de la API de Meta
        await _messagingService.SendTextMessageAsync(senderId, responseMessage);
    }
}
