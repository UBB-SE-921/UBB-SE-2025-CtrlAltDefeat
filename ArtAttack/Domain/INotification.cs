using System;

namespace ArtAttack.Domain
{
    public interface INotification
    {
        int NotificationID { get; set; }
        int RecipientID { get; set; }
        NotificationCategory Category { get; set; }
        bool IsRead { get; set; }
        string Title { get; }
        string Subtitle { get; }
        string Content { get; }
        DateTime Timestamp { get; }
        bool IsNotRead { get; set; }

        void MarkAsRead();
    }
}