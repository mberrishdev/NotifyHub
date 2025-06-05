using NotifyHub.Application.Models;

namespace NotifyHub.Application.Interfaces;

public interface INotificationService
{
    Task ProcessEventAsync(Event @event);
    Task SendNotificationAsync(Event @event, List<string> targetGroups);
    Task<List<string>> GetGroupMembersAsync(string group);
} 