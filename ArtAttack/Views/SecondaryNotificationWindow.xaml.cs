using ArtAttack.Domain;
using Microsoft.UI.Xaml;

namespace ArtAttack
{
    public sealed partial class SecondaryNotificationWindow : Window
    {
        public Notification SelectedNotification { get; }

        public SecondaryNotificationWindow(Notification notification)
        {
            this.InitializeComponent();
            this.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(400, 200, 1080, 600));
            this.SelectedNotification = notification;
            //contractFileButton.IsEnabled = SelectedNotification.getCategory().Equals(NotificationCategory.PAYMENT_CONFIRMATION);
            //if (!contractFileButton.IsEnabled)
            //{
            //    if (SelectedNotification.getCategory().Equals(NotificationCategory.CONTRACT_RENEWAL_ANS))
            //    {
            //        var contractRenewalAnswerNotification = (ContractRenewalAnswerNotification)SelectedNotification;
            //        contractFileButton.IsEnabled = contractRenewalAnswerNotification.getIsAccepted();
            //    }
            //}
            contractFileButton.IsEnabled = false;
            this.Populate();
        }

        private void Populate()
        {
            selectedNotificationTitle.Text = this.SelectedNotification.Title;
            selectedNotificationContent.Text = this.SelectedNotification.Content;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainNotificationWindow();
            mainWindow.Activate();
            this.Close();
        }

        private async void GoToContractFile(object sender, RoutedEventArgs e)
        {
            // AWAIT + Apelare functie Contract Darius!!!
        }
    }
}