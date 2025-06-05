using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NotifyHub.Application.Interfaces;
using NotifyHub.Application.Models;

namespace NotifyHub.Application.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class NotificationHub : Hub
{
    private readonly INotificationHubService _notificationHubService;

    public NotificationHub(INotificationHubService notificationHubService)
    {
        _notificationHubService = notificationHubService;
    }

    public override async Task OnConnectedAsync()
    {
        var groups = Context.GetHttpContext()?.Request.Query["groups"].ToString()?.Split(',').ToList();
        await _notificationHubService.OnConnectedAsync(Context.ConnectionId, groups);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _notificationHubService.OnDisconnectedAsync(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToGroups(List<string> groups)
    {
        await _notificationHubService.OnConnectedAsync(Context.ConnectionId, groups);
    }

    public async Task UnsubscribeFromGroups(List<string> groups)
    {
        await _notificationHubService.UnsubscribeFromGroupsAsync(Context.ConnectionId, groups);
    }

    public async Task ConfigureUserNotifications(UserNotificationConfig config)
    {
        await _notificationHubService.ConfigureUserNotificationsAsync(Context.ConnectionId, config);
    }
} 