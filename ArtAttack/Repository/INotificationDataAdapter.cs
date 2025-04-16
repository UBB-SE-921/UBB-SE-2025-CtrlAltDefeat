using System.Collections.Generic;
using ArtAttack.Domain;

public interface INotificationDataAdapter
{
    void AddNotification(Notification notification);
    void Dispose();
    List<Notification> GetNotificationsForUser(int recipientId);
    void MarkAsRead(int notificationId);
}