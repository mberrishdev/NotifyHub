using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace NotifyHub.Application.Models;

public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public required string Type { get; set; }
    
    [Required]
    public required string Data { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool SaveToHistory { get; set; } = true; // Default to true for backward compatibility
    public List<string> TargetGroups { get; set; } = new();
} 