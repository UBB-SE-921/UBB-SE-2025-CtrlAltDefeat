using System;

namespace ArtAttack.Domain
{
    public interface INotification
    {
        string Content { get; }
        bool IsNotRead { get; }
        string Subtitle { get; }
        DateTime timestamp { get; set; }
        string Title { get; }

        NotificationCategory getCategory();
        bool getIsRead();
        int getNotificationID();
        int getRecipientID();
        void markAsRead();
        void setIsRead(bool value);
    }
}