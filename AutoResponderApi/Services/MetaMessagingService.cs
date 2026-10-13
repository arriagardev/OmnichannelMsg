using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AutoResponderApi.Services;

public class MetaMessagingService : IMetaMessagingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MetaMessagingService> _logger;
    private readonly string? _pageAccessToken;

    public MetaMessagingService(HttpClient httpClient, IConfiguration configuration, ILogger<MetaMessagingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _pageAccessToken = configuration["MetaGraphApi:PageAccessToken"];
        var apiVersion = configuration["MetaGraphApi:ApiVersion"] ?? "v19.0";

        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri($"https://graph.facebook.com/{apiVersion}/");
        }
    }

    public async Task SendTextMessageAsync(string recipientId, string messageText)
    {
        if (string.IsNullOrWhiteSpace(_pageAccessToken) || _pageAccessToken == "TU_PAGE_ACCESS_TOKEN_AQUI")
        {
            _logger.LogWarning("PageAccessToken no configurado. No se pudo enviar el mensaje a {RecipientId}.", recipientId);
            return;
        }

        try
        {
            var payload = new
            {
                recipient = new { id = recipientId },
                message = new { text = messageText }
            };

            var url = $"me/messages?access_token={_pageAccessToken}";
            var response = await _httpClient.PostAsJsonAsync(url, payload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Mensaje enviado con éxito a {RecipientId}", recipientId);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Error al enviar mensaje a Meta API para {RecipientId}. Status: {StatusCode}, Error: {Error}",
                    recipientId, response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al enviar mensaje a través de Meta Graph API para {RecipientId}", recipientId);
        }
    }
}
