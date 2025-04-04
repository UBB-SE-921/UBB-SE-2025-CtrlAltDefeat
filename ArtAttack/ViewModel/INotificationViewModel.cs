//using ArtAttack.Domain;
//using System;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Threading.Tasks;
//using System.Windows.Input;

//namespace ArtAttack.ViewModel
//{
//    public interface INotificationViewModel
//    {
//        bool IsLoading { get; set; }
//        ICommand MarkAsReadCommand { get; }
//        ObservableCollection<INotification> Notifications { get; set; }
//        int UnreadCount { get; set; }
//        string unReadNotificationsCountText { get; }

//        event PropertyChangedEventHandler PropertyChanged;
//        event Action<string> ShowPopup;

//        Task AddNotificationAsync(INotification notification);
//        Task LoadNotificationsAsync(int recipientId);
//        Task MarkAsReadAsync(int notificationId);
//    }
//}