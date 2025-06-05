using System;
using System.Collections.Generic;

namespace NotifyHub.Application.Models;

public class GroupSubscription
{
    public required string UserId { get; set; }
    public required string ConnectionId { get; set; }
    public List<string> Groups { get; set; } = new();
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
} 