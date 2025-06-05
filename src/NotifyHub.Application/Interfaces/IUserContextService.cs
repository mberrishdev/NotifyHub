using NotifyHub.Application.Models;

namespace NotifyHub.Application.Interfaces;

public interface IUserContextService
{
    string GetUserId();
    Task SaveUserNotificationConfigAsync(UserNotificationConfig config);
    Task<List<string>> GetUsersInRoleAsync(string role);
    Task<List<string>> GetAllUsersAsync();
} 