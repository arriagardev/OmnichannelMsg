using AutoResponderApi.Models;

namespace AutoResponderApi.Services;

public interface IAutoResponderService
{
    Task ProcessMessageAsync(string senderId, MessagingItem messagingItem);
}
