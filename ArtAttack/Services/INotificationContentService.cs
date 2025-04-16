using ArtAttack.Domain;

namespace ArtAttack.Services
{
    public interface INotificationContentService
    {
        string GetContent(Notification notification);
        string GetTitle(Notification notification);
        string GetSubtitle(Notification notification);
    }
}