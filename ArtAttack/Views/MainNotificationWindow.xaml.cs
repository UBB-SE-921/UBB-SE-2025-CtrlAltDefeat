using ArtAttack.Domain;
using ArtAttack.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArtAttack
{
    public sealed partial class MainNotificationWindow : Window
    {
        private const int CurrentUserId = 1; // Replace with your actual user ID

        public NotificationViewModel ViewModel = new NotificationViewModel(CurrentUserId);

        public MainNotificationWindow()
        {
            this.InitializeComponent();
            this.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(400, 200, 1080, 600));

            RootPanel.DataContext = ViewModel;

            Activated += MainNotificationWindow_Activated;
        }

        private async void MainNotificationWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState != WindowActivationState.Deactivated)
            {
                await ViewModel.LoadNotificationsAsync(CurrentUserId);
            }
        }
        private async void notificationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (notificationList.SelectedItem is Notification selectedNotification)
            {
                var secondaryWindow = new SecondaryNotificationWindow(selectedNotification);
                secondaryWindow.Activate();

                await ViewModel.MarkAsReadAsync(selectedNotification.getNotificationID());

                notificationList.SelectedItem = null;
            }

            this.Close();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            //Need to create main Window and activate it;
            this.Close();
        }
    }
}