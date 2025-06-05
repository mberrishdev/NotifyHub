using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NotifyHub.Application.Interfaces;
using NotifyHub.Application.Models;

namespace NotifyHub.Application.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications.
/// This hub handles all real-time communication between the server and clients,
/// including connection management, group subscriptions, and notification delivery.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class NotificationHub(INotificationHubService notificationHubService) : Hub
{
    /// <summary>
    /// Handles new client connections to the hub.
    /// This method is called automatically when a client connects and:
    /// 1. Extracts group subscriptions from query parameters
    /// 2. Processes the connection through the notification service
    /// 3. Sets up initial group memberships
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        // Extract groups from query parameters (comma-separated list)
        var groups = Context.GetHttpContext()?.Request.Query["groups"].ToString()?.Split(',').ToList();
        
        // Process the connection and group subscriptions
        await notificationHubService.OnConnectedAsync(Context.ConnectionId, groups);
        
        // Call base implementation
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handles client disconnections from the hub.
    /// This method is called automatically when a client disconnects and:
    /// 1. Cleans up group subscriptions
    /// 2. Removes the connection from tracking
    /// 3. Handles any disconnection errors
    /// </summary>
    /// <param name="exception">The exception that caused the disconnection, if any</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up the connection and its subscriptions
        await notificationHubService.OnDisconnectedAsync(Context.ConnectionId);
        
        // Call base implementation
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Allows clients to subscribe to specific notification groups.
    /// This method can be called multiple times to update group memberships.
    /// </summary>
    /// <param name="groups">List of group names to subscribe to</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SubscribeToGroups(List<string> groups)
    {
        // Process group subscriptions through the notification service
        await notificationHubService.OnConnectedAsync(Context.ConnectionId, groups);
    }

    /// <summary>
    /// Allows clients to unsubscribe from specific notification groups.
    /// This method removes the client from the specified groups.
    /// </summary>
    /// <param name="groups">List of group names to unsubscribe from</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task UnsubscribeFromGroups(List<string> groups)
    {
        // Process group unsubscriptions through the notification service
        await notificationHubService.UnsubscribeFromGroupsAsync(Context.ConnectionId, groups);
    }

    /// <summary>
    /// Configures user-specific notification settings.
    /// This method allows clients to set their notification preferences, including:
    /// - Event type filters
    /// - Data field filters
    /// - Notification delivery preferences
    /// </summary>
    /// <param name="config">The notification configuration to apply</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task ConfigureUserNotifications(UserNotificationConfig config)
    {
        // Apply the notification configuration through the notification service
        await notificationHubService.ConfigureUserNotificationsAsync(Context.ConnectionId, config);
    }
} 