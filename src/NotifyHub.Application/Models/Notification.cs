using System.ComponentModel.DataAnnotations;

namespace NotifyHub.Application.Models;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public required string Type { get; set; }
    
    [Required]
    public required string Data { get; set; }
    
    [Required]
    public required string RecipientId { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Username { get; set; }
} 