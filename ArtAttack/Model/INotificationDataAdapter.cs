using ArtAttack.Domain;
using System.Collections.Generic;

public interface INotificationDataAdapter
{
    void AddNotification(Notification notification);
    void Dispose();
    List<Notification> GetNotificationsForUser(int recipientId);
    void MarkAsRead(int notificationId);
}