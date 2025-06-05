using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NotifyHub.Application.Interfaces;
using NotifyHub.Application.Models;

namespace NotifyHub.Application.Services;

public class UserContextService(
    IHttpContextAccessor httpContextAccessor,
    IGroupSubscriptionService groupSubscriptionService,
    ILogger<UserContextService> logger)
    : IUserContextService
{
    private readonly Dictionary<string, UserNotificationConfig> _userConfigs = new();

    public string GetUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        return userId;
    }

    public Task SaveUserNotificationConfigAsync(UserNotificationConfig config)
    {
        var userId = GetUserId();
        _userConfigs[userId] = config;
        logger.LogInformation("Saved notification config for user {UserId}", userId);
        return Task.CompletedTask;
    }

    public Task<List<string>> GetUsersInRoleAsync(string role)
    {
        // This would typically query a user store or database
        return Task.FromResult(new List<string>());
    }

    public Task<List<string>> GetAllUsersAsync()
    {
        // This would typically query a user store or database
        return Task.FromResult(new List<string>());
    }
} 