using NotifyHub.Application.Models;

namespace NotifyHub.Application.Interfaces;

public interface IEventHistoryService
{
    Task SaveEventAsync(Event @event);
    Task<IEnumerable<Event>> GetUserEventsAsync(string userId, int limit = 50);
    Task<IEnumerable<Event>> GetRoleEventsAsync(string role, int limit = 50);
    Task<IEnumerable<Event>> GetAllEventsAsync(int limit = 50);
    Task<List<Event>> GetEventsForGroupsAsync(List<string> groups);
} 