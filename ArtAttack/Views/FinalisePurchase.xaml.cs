using ArtAttack.Domain;
using ArtAttack.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArtAttack
{
    public sealed partial class FinalisePurchase : Page
    {
        private FinalizePurchaseViewModel viewModel;
        private NotificationViewModel notifViewModel;
        public FinalisePurchase(int orderHistoryID)
        {
            this.InitializeComponent();
            viewModel = new FinalizePurchaseViewModel(orderHistoryID);
            notifViewModel = new NotificationViewModel(1);
            DataContext = viewModel;
        }

        private void onContinueShopping_Clicked(object sender, RoutedEventArgs e)
        {
            viewModel.HandleFinish();
        }

    }
}
