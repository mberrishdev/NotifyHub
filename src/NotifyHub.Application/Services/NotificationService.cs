using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NotifyHub.Application.Hubs;
using NotifyHub.Application.Interfaces;
using NotifyHub.Application.Models;

namespace NotifyHub.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IUserContextService _userContextService;
    private readonly IEventHistoryService _eventHistoryService;
    private readonly IGroupSubscriptionService _groupSubscriptionService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        IUserContextService userContextService,
        IEventHistoryService eventHistoryService,
        IGroupSubscriptionService groupSubscriptionService,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _userContextService = userContextService;
        _eventHistoryService = eventHistoryService;
        _groupSubscriptionService = groupSubscriptionService;
        _logger = logger;
    }

    public async Task ProcessEventAsync(Event @event)
    {
        _logger.LogInformation("Processing event: {EventType}", @event.Type);

        // If target groups are specified, send directly to those groups
        if (@event.TargetGroups.Any())
        {
            await SendNotificationAsync(@event, @event.TargetGroups);
            return;
        }
    }

    public async Task SendNotificationAsync(Event @event, List<string> targetGroups)
    {
        try
        {
            // Save event to history only if SaveToHistory is true
            if (@event.SaveToHistory)
            {
                await _eventHistoryService.SaveEventAsync(@event);
            }

            // Get group members
            foreach (var group in targetGroups)
            {
                var groupMembers = await _groupSubscriptionService.GetGroupMembersAsync(group);
                foreach (var connectionId in groupMembers)
                {
                    var notification = new Notification
                    {
                        Type = @event.Type,
                        Data = @event.Data,
                        RecipientId = connectionId,
                        Timestamp = DateTime.UtcNow
                    };

                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
                }
                _logger.LogInformation("Notification sent to group {Group}: {EventType}", group, @event.Type);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification for event {EventType}", @event.Type);
            throw;
        }
    }

    public async Task<List<string>> GetGroupMembersAsync(string group)
    {
        return await _groupSubscriptionService.GetGroupMembersAsync(group);
    }
} 