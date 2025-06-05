using NotifyHub.Application.Models;

namespace NotifyHub.Application.Interfaces;

public interface IGroupSubscriptionService
{
    Task SubscribeToGroupsAsync(string userId, string connectionId, List<string> groups);
    Task UnsubscribeFromGroupsAsync(string userId, string connectionId, List<string> groups);
    Task RemoveConnectionAsync(string connectionId);
    Task<List<string>> GetGroupMembersAsync(string group);
    Task<List<string>> GetAllGroupsAsync();
} 