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

        /// <summary>
        /// Constructor for the NotificationViewModel class.
        /// </summary>
        /// <param name="currentUserId" The id of the current user></param>
        public NotificationViewModel(int currentUserId)
        {
            _dataAdapter = new NotificationDataAdapter(Configuration._CONNECTION_STRING_);
            Notifications = new ObservableCollection<Notification>();
            this.currentUserId = currentUserId;
            MarkAsReadCommand = new NotificationRelayCommand<int>(async (id) => await MarkAsReadAsync(id));
            _ = LoadNotificationsAsync(currentUserId);
        }

        /// <summary>
        /// The collection of notifications.
        /// </summary>
        public ObservableCollection<Notification> Notifications
        {
            get => _notifications;
            set
            {
                _notifications = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The number of unread notifications.
        /// </summary>
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

        /// <summary>
        /// A boolean value indicating whether the notifications are being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Command to mark a notification as read.
        /// </summary>
        public ICommand MarkAsReadCommand { get; }

        /// <summary>
        /// Loads the notifications for a given user.
        /// </summary>
        /// <param name="recipientId" The id of the recipient></param>
        /// <returns A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        /// <param name="notificationId" The id of the notification></param>
        /// <returns A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Adds a new notification.
        /// </summary>
        /// <param name="notification" The notification to be added></param>
        /// <returns A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException" Thrown when the notification is null></exception>
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

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName" The name of the property></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// The text for the unread notifications count.
        /// </summary>
        public string unReadNotificationsCountText
        {
            get => "You've got #" + _unreadCount + " unread notifications.";
        }

        /// <summary>
        /// Updates the unread count.
        /// </summary>
        private void UpdateUnreadCount()
        {
            UnreadCount = Notifications.Count(n => !n.getIsRead());
            OnPropertyChanged(nameof(unReadNotificationsCountText));
        }

    }
}
