namespace NotifyHub.Application.Models;

public class UserNotificationConfig
{
    public string? FilterField { get; set; }
    public string? FilterValue { get; set; }
    public bool SaveToHistory { get; set; } = true;
    public List<string> TargetGroups { get; set; } = new();
} 