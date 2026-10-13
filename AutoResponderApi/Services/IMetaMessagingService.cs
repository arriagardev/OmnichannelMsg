namespace AutoResponderApi.Services;

public interface IMetaMessagingService
{
    Task SendTextMessageAsync(string recipientId, string messageText);
}
