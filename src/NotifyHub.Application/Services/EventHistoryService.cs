using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NotifyHub.Application.Interfaces;
using NotifyHub.Application.Models;

namespace NotifyHub.Application.Services;

public class EventHistoryService : IEventHistoryService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<EventHistoryService> _logger;
    private const string UserEventsKey = "user_events_{0}";
    private const string RoleEventsKey = "role_events_{0}";
    private const string AllEventsKey = "all_events";

    public EventHistoryService(IMemoryCache cache, ILogger<EventHistoryService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task SaveEventAsync(Event @event)
    {
        try
        {
            // Save to all events
            var allEvents = await GetAllEventsAsync();
            var updatedAllEvents = new List<Event> { @event }.Concat(allEvents).Take(1000).ToList();
            _cache.Set(AllEventsKey, updatedAllEvents);

            _logger.LogInformation("Event saved: {EventType}", @event.Type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving event: {EventType}", @event.Type);
            throw;
        }
    }

    public Task<IEnumerable<Event>> GetUserEventsAsync(string userId, int limit = 50)
    {
        var key = string.Format(UserEventsKey, userId);
        var events = _cache.Get<List<Event>>(key) ?? new List<Event>();
        return Task.FromResult(events.Take(limit));
    }

    public Task<IEnumerable<Event>> GetRoleEventsAsync(string role, int limit = 50)
    {
        var key = string.Format(RoleEventsKey, role);
        var events = _cache.Get<List<Event>>(key) ?? new List<Event>();
        return Task.FromResult(events.Take(limit));
    }

    public Task<IEnumerable<Event>> GetAllEventsAsync(int limit = 50)
    {
        var events = _cache.Get<List<Event>>(AllEventsKey) ?? new List<Event>();
        return Task.FromResult(events.Take(limit));
    }

    public Task<List<Event>> GetEventsForGroupsAsync(List<string> groups)
    {
        if (groups == null || !groups.Any())
        {
            return Task.FromResult(new List<Event>());
        }

        var allEvents = _cache.Get<List<Event>>(AllEventsKey) ?? new List<Event>();
        var groupEvents = allEvents
            .Where(e => e.TargetGroups.Any(g => groups.Contains(g)))
            .OrderByDescending(e => e.Timestamp)
            .Take(50)
            .ToList();

        _logger.LogInformation("Retrieved {Count} events for groups: {Groups}", groupEvents.Count, string.Join(", ", groups));
        return Task.FromResult(groupEvents);
    }
} 