using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NotifyHub.Application.Interfaces;
using NotifyHub.Application.Models;

namespace NotifyHub.Application.Services;

public class GroupSubscriptionService(IMemoryCache cache, ILogger<GroupSubscriptionService> logger)
    : IGroupSubscriptionService
{
    private const string SubscriptionsKey = "subscriptions";
    private const string UserGroupsKey = "user_groups_{0}";
    private const string GroupMembersKey = "group_members_{0}";

    private Dictionary<string, GroupSubscription>? GetSubscriptions()
    {
        return cache.GetOrCreate(SubscriptionsKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(24);
            return new Dictionary<string, GroupSubscription>();
        });
    }

    private void SaveSubscriptions(Dictionary<string, GroupSubscription>? subscriptions)
    {
        cache.Set(SubscriptionsKey, subscriptions, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(24)
        });
    }

    public Task SubscribeToGroupsAsync(string userId, string connectionId, List<string> groups)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(connectionId))
        {
            return Task.CompletedTask;
        }

        var subscriptions = GetSubscriptions();
        var subscription = new GroupSubscription
        {
            UserId = userId,
            ConnectionId = connectionId,
            Groups = groups ?? new List<string>()
        };

        subscriptions[connectionId] = subscription;
        SaveSubscriptions(subscriptions);

        // Update user groups cache
        var userGroupsKey = string.Format(UserGroupsKey, userId);
        cache.Set(userGroupsKey, groups, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(24)
        });

        // Update group members cache
        foreach (var group in groups)
        {
            var groupMembersKey = string.Format(GroupMembersKey, group);
            var groupMembers = cache.GetOrCreate(groupMembersKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(24);
                return new HashSet<string>();
            });
            groupMembers?.Add(connectionId);
            cache.Set(groupMembersKey, groupMembers, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(24)
            });
        }

        logger.LogInformation("User {UserId} subscribed to groups: {Groups}", userId, string.Join(", ", groups));
        return Task.CompletedTask;
    }

    public Task UnsubscribeFromGroupsAsync(string userId, string connectionId, List<string> groups)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(connectionId))
        {
            return Task.CompletedTask;
        }

        var subscriptions = GetSubscriptions();
        if (subscriptions.TryGetValue(connectionId, out var subscription))
        {
            subscription.Groups.RemoveAll(g => groups.Contains(g));
            SaveSubscriptions(subscriptions);

            // Update user groups cache
            var userGroupsKey = string.Format(UserGroupsKey, userId);
            var userGroups = cache.Get<List<string>>(userGroupsKey) ?? new List<string>();
            userGroups.RemoveAll(g => groups.Contains(g));
            cache.Set(userGroupsKey, userGroups, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(24)
            });

            // Update group members cache
            foreach (var group in groups)
            {
                var groupMembersKey = string.Format(GroupMembersKey, group);
                var groupMembers = cache.Get<HashSet<string>>(groupMembersKey);
                if (groupMembers != null)
                {
                    groupMembers.Remove(userId);
                    cache.Set(groupMembersKey, groupMembers, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromHours(24)
                    });
                }
            }

            logger.LogInformation("User {UserId} unsubscribed from groups: {Groups}", userId, string.Join(", ", groups));
        }

        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(string connectionId)
    {
        if (string.IsNullOrEmpty(connectionId))
        {
            return Task.CompletedTask;
        }

        var subscriptions = GetSubscriptions();
        if (subscriptions.TryGetValue(connectionId, out var subscription))
        {
            var userId = subscription.UserId;
            var groups = subscription.Groups;

            // Remove from subscriptions
            subscriptions.Remove(connectionId);
            SaveSubscriptions(subscriptions);

            // Update user groups cache
            var userGroupsKey = string.Format(UserGroupsKey, userId);
            var userGroups = cache.Get<List<string>>(userGroupsKey) ?? new List<string>();
            userGroups.RemoveAll(g => groups.Contains(g));
            cache.Set(userGroupsKey, userGroups, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(24)
            });

            // Update group members cache
            foreach (var group in groups)
            {
                var groupMembersKey = string.Format(GroupMembersKey, group);
                var groupMembers = cache.Get<HashSet<string>>(groupMembersKey);
                if (groupMembers != null)
                {
                    groupMembers.Remove(userId);
                    cache.Set(groupMembersKey, groupMembers, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromHours(24)
                    });
                }
            }

            logger.LogInformation("Removed connection {ConnectionId} for user {UserId}", connectionId, userId);
        }

        return Task.CompletedTask;
    }

    public Task<List<string>> GetGroupMembersAsync(string group)
    {
        if (string.IsNullOrEmpty(group))
        {
            return Task.FromResult(new List<string>());
        }

        var groupMembersKey = string.Format(GroupMembersKey, group);
        var groupMembers = cache.Get<HashSet<string>>(groupMembersKey) ?? new HashSet<string>();
        return Task.FromResult(groupMembers.ToList());
    }

    public Task<List<string>> GetAllGroupsAsync()
    {
        var subscriptions = GetSubscriptions();
        var groups = subscriptions.Values
            .SelectMany(s => s.Groups)
            .Distinct()
            .ToList();

        return Task.FromResult(groups);
    }
} 