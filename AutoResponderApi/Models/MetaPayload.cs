using System.Text.Json.Serialization;

namespace AutoResponderApi.Models;

/// <summary>
/// Representa el payload raíz enviado por los Webhooks de Meta (Facebook Messenger e Instagram).
/// </summary>
public class MetaPayload
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("entry")]
    public List<MetaEntry> Entry { get; set; } = [];
}

/// <summary>
/// Representa una entrada de evento asociada a una Página de Facebook o Cuenta de Instagram.
/// </summary>
public class MetaEntry
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("time")]
    public long? Time { get; set; }

    [JsonPropertyName("messaging")]
    public List<MessagingItem> Messaging { get; set; } = [];
}

/// <summary>
/// Representa un evento individual dentro de messaging (mensaje, referral, postback, etc.).
/// </summary>
public class MessagingItem
{
    [JsonPropertyName("sender")]
    public MessagingParticipant? Sender { get; set; }

    [JsonPropertyName("recipient")]
    public MessagingParticipant? Recipient { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }

    [JsonPropertyName("message")]
    public MessageData? Message { get; set; }

    [JsonPropertyName("postback")]
    public PostbackData? Postback { get; set; }

    /// <summary>
    /// Referral directo a nivel de messaging (ocurre cuando un usuario abre una conversación desde un anuncio de Meta Ads).
    /// </summary>
    [JsonPropertyName("referral")]
    public ReferralData? Referral { get; set; }

    /// <summary>
    /// Helper para obtener el título del anuncio (ad_title), extrayéndolo de donde sea que Meta lo envíe:
    /// messaging.referral, messaging.message.referral o messaging.postback.referral.
    /// </summary>
    public string? GetAdTitle() =>
        Referral?.AdsContextData?.AdTitle
        ?? Message?.Referral?.AdsContextData?.AdTitle
        ?? Postback?.Referral?.AdsContextData?.AdTitle;

    /// <summary>
    /// Helper para obtener el texto del mensaje o del postback asociado.
    /// </summary>
    public string? GetMessageText() =>
        Message?.Text ?? Postback?.Title;
}

public class MessagingParticipant
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class MessageData
{
    [JsonPropertyName("mid")]
    public string? Mid { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Referral anidado dentro de message (común en mensajes iniciales desde 'Click to Messenger / Instagram Direct').
    /// </summary>
    [JsonPropertyName("referral")]
    public ReferralData? Referral { get; set; }

    [JsonPropertyName("quick_reply")]
    public QuickReplyData? QuickReply { get; set; }

    [JsonPropertyName("reply_to")]
    public ReplyToData? ReplyTo { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachmentData>? Attachments { get; set; }
}

/// <summary>
/// Contiene la información de procedencia / referencia del anuncio de Meta Ads.
/// </summary>
public class ReferralData
{
    [JsonPropertyName("ref")]
    public string? Ref { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("ad_id")]
    public string? AdId { get; set; }

    /// <summary>
    /// Contexto crítico del anuncio publicitario.
    /// </summary>
    [JsonPropertyName("ads_context_data")]
    public AdsContextData? AdsContextData { get; set; }
}

/// <summary>
/// Contiene los detalles específicos del anuncio publicitario de Meta.
/// </summary>
public class AdsContextData
{
    [JsonPropertyName("ad_title")]
    public string? AdTitle { get; set; }

    [JsonPropertyName("photo_url")]
    public string? PhotoUrl { get; set; }

    [JsonPropertyName("video_url")]
    public string? VideoUrl { get; set; }

    [JsonPropertyName("post_id")]
    public string? PostId { get; set; }

    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }
}

public class PostbackData
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("payload")]
    public string? Payload { get; set; }

    [JsonPropertyName("referral")]
    public ReferralData? Referral { get; set; }
}

public class QuickReplyData
{
    [JsonPropertyName("payload")]
    public string? Payload { get; set; }
}

public class ReplyToData
{
    [JsonPropertyName("mid")]
    public string? Mid { get; set; }
}

public class AttachmentData
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("payload")]
    public AttachmentPayload? Payload { get; set; }
}

public class AttachmentPayload
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}
