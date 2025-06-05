using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NotifyHub.Application.Hubs;
using NotifyHub.Application.Interfaces;
using NotifyHub.Application.Models;

namespace NotifyHub.Application.Services;

public class NotificationHubService(
    IGroupSubscriptionService groupSubscriptionService,
    IUserContextService userContextService,
    IHubContext<NotificationHub> hubContext,
    IEventHistoryService eventHistoryService,
    ILogger<NotificationHubService> logger)
    : INotificationHubService
{
    public async Task OnConnectedAsync(string connectionId, List<string> groups)
    {
        var userId = userContextService.GetUserId();
        logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, connectionId);

        await groupSubscriptionService.SubscribeToGroupsAsync(userId, connectionId, groups);

        // Add the connection to the SignalR groups
        foreach (var group in groups)
        {
            await hubContext.Groups.AddToGroupAsync(connectionId, group);
        }

        // Load and send event history for user's groups
        var history = await eventHistoryService.GetEventsForGroupsAsync(groups);
        if (history.Count != 0)
        {
            await hubContext.Clients.Client(connectionId).SendAsync("ReceiveHistory", history);
            logger.LogInformation("Sent {Count} historical events to user {UserId}", history.Count, userId);
        }
    }

    public async Task OnDisconnectedAsync(string connectionId)
    {
        var userId = userContextService.GetUserId();
        logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", userId, connectionId);

        // Remove the connection and its subscriptions
        await groupSubscriptionService.RemoveConnectionAsync(connectionId);
    }


    public async Task UnsubscribeFromGroupsAsync(string connectionId, List<string> groups)
    {
        var userId = userContextService.GetUserId();
        logger.LogInformation("User {UserId} unsubscribing from groups: {Groups}", userId, string.Join(", ", groups));

        await groupSubscriptionService.UnsubscribeFromGroupsAsync(userId, connectionId, groups);

        // Remove the connection from the SignalR groups
        foreach (var group in groups)
        {
            await hubContext.Groups.RemoveFromGroupAsync(connectionId, group);
        }
    }

    public async Task ConfigureUserNotificationsAsync(string connectionId, UserNotificationConfig config)
    {
        var userId = userContextService.GetUserId();
        logger.LogInformation("User {UserId} configuring notifications", userId);

        // Store user's notification preferences
        await userContextService.SaveUserNotificationConfigAsync(config);
    }
}