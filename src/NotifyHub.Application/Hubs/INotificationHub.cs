using NotifyHub.Application.Models;

namespace NotifyHub.Application.Hubs;
 
public interface INotificationHub
{
    Task ReceiveNotification(Notification notification);
} 