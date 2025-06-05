using NotifyHub.Application.Models;

namespace NotifyHub.Application.Interfaces;

public interface INotificationHubService
{
    Task OnConnectedAsync(string connectionId, List<string> groups);
    Task OnDisconnectedAsync(string connectionId);
    Task UnsubscribeFromGroupsAsync(string connectionId, List<string> groups);
    Task ConfigureUserNotificationsAsync(string connectionId, UserNotificationConfig config);
} 