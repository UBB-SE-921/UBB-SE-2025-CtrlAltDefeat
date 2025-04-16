using System.Diagnostics.CodeAnalysis;
using ArtAttack.Domain;
using Microsoft.UI.Xaml;
using ArtAttack.Services;

namespace ArtAttack
{
    [ExcludeFromCodeCoverage]
    public sealed partial class SecondaryNotificationWindow : Window
    {
        /// <summary>
        /// The window for the secondary notification view
        /// </summary>
        public Notification SelectedNotification { get; }
        public INotificationContentService NotificationContentService { get; } = new NotificationContentService();

        public SecondaryNotificationWindow(Notification notification)
        {
            this.InitializeComponent();
            this.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(400, 200, 1080, 600));
            this.SelectedNotification = notification;
            contractFileButton.IsEnabled = false;
            this.Populate();
        }

        /// <summary>
        /// Populates the secondary notification window with the selected notification's details
        /// </summary>
        private void Populate()
        {
            selectedNotificationTitle.Text = NotificationContentService.GetTitle(this.SelectedNotification);
            selectedNotificationContent.Text = NotificationContentService.GetContent(this.SelectedNotification);
        }

        /// <summary>
        /// Handles the click event for the back button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainNotificationWindow();
            mainWindow.Activate();
            this.Close();
        }

        private void GoToContractFile(object sender, RoutedEventArgs e)
        {
        }
    }
}