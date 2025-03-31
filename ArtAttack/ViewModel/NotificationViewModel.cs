using ArtAttack.Domain;
using ArtAttack.Shared;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ArtAttack.ViewModel
{
    public class NotificationViewModel : INotifyPropertyChanged
    {
        private readonly NotificationDataAdapter _dataAdapter;
        private ObservableCollection<Notification> _notifications;
        private int _unreadCount;
        private bool _isLoading;
        private int currentUserId;
        public event Action<string> ShowPopup;

        public event PropertyChangedEventHandler PropertyChanged;

        public NotificationViewModel(int currentUserId)
        {
            _dataAdapter = new NotificationDataAdapter(Configuration._CONNECTION_STRING_);
            Notifications = new ObservableCollection<Notification>();
            this.currentUserId = currentUserId;
            MarkAsReadCommand = new NotificationRelayCommand<int>(async (id) => await MarkAsReadAsync(id));
            _ = LoadNotificationsAsync(currentUserId);
        }

        public ObservableCollection<Notification> Notifications
        {
            get => _notifications;
            set
            {
                _notifications = value;
                OnPropertyChanged();
            }
        }

        public int UnreadCount
        {
            get => _unreadCount;
            set
            {
                _unreadCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(unReadNotificationsCountText));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        public ICommand MarkAsReadCommand { get; }

        public async Task LoadNotificationsAsync(int recipientId)
        {
            try
            {
                IsLoading = true;
                var notifications = await Task.Run(() => _dataAdapter.GetNotificationsForUser(recipientId));

                Notifications = new ObservableCollection<Notification>(notifications);
                UnreadCount = Notifications.Count(n => !n.getIsRead());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading notifications: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            try
            {
                await Task.Run(() => _dataAdapter.MarkAsRead(notificationId));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking notification as read: {ex.Message}");
            }
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            try
            {
                await Task.Run(() => _dataAdapter.AddNotification(notification));

                if (notification.getRecipientID() == currentUserId)
                {
                    Notifications.Insert(0, notification);
                    UnreadCount++;
                    ShowPopup?.Invoke("Notification sent!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding notification: {ex.Message}");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string unReadNotificationsCountText
        {
            get => "You've got #" + _unreadCount + " unread notifications.";
        }

        private void UpdateUnreadCount()
        {
            UnreadCount = Notifications.Count(n => !n.getIsRead());
            OnPropertyChanged(nameof(unReadNotificationsCountText));
        }

    }
}
