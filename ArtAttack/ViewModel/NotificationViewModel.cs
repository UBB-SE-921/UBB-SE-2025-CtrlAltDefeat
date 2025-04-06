using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;

namespace ArtAttack.ViewModel
{
    public class NotificationViewModel : INotifyPropertyChanged
    {
        private readonly INotificationDataAdapter dataAdapter;
        private ObservableCollection<Notification> notifications;
        private int unreadCount;
        private bool isLoading;
        private int currentUserId;
        public event Action<string> ShowPopup;

        public event PropertyChangedEventHandler PropertyChanged;

        // Modified NotificationViewModel constructor for testing
        public NotificationViewModel(int currentUserId, bool autoLoad = true)
        {
            dataAdapter = new NotificationDataAdapter(Configuration.CONNECTION_STRING);
            Notifications = new ObservableCollection<Notification>();
            this.currentUserId = currentUserId;
            MarkAsReadCommand = new NotificationRelayCommand<int>(async (id) => await MarkAsReadAsync(id));

            if (autoLoad)
            {
                _ = LoadNotificationsAsync(currentUserId);
            }
        }

        public ObservableCollection<Notification> Notifications
        {
            get => notifications;
            set
            {
                notifications = value;
                OnPropertyChanged();
            }
        }

        public int UnreadCount
        {
            get => unreadCount;
            set
            {
                unreadCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UnReadNotificationsCountText));
            }
        }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged();
            }
        }
        public ICommand MarkAsReadCommand { get; }

        public async Task LoadNotificationsAsync(int recipientId)
        {
            try
            {
                IsLoading = true;
                var notifications = await Task.Run(() => dataAdapter.GetNotificationsForUser(recipientId));

                Notifications = new ObservableCollection<Notification>(notifications);
                UnreadCount = Notifications.Count(n => !n.IsRead);
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
                await Task.Run(() => dataAdapter.MarkAsRead(notificationId));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking notification as read: {ex.Message}");
            }
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            try
            {
                await Task.Run(() => dataAdapter.AddNotification(notification));

                if (notification.RecipientID == currentUserId)
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
        public string UnReadNotificationsCountText
        {
            get => "You've got #" + unreadCount + " unread notifications.";
        }
    }
}