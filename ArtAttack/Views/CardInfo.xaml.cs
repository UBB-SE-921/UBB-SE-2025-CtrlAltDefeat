using System.Diagnostics.CodeAnalysis;
using ArtAttack.ViewModel;
using ArtAttack.Service;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ArtAttack.Model;
using ArtAttack.Repository;

namespace ArtAttack
{
    [ExcludeFromCodeCoverage]
    public sealed partial class CardInfo : Page
    {
        /// <summary>
        /// The view model for the CardInfo page
        /// </summary>
        private CardInfoViewModel viewModel;

        public CardInfo(int orderHistoryID)
        {
            this.InitializeComponent();
            string connectionString = "connection-string"; // replace with actual connection string
            var cardInfoService = new CardInfoService(
                new OrderHistoryModel(connectionString),
                new OrderSummaryModel(connectionString),
                new DummyCardRepository(connectionString));
            viewModel = new CardInfoViewModel(cardInfoService, orderHistoryID);
            DataContext = viewModel;
        }

        /// <summary>
        /// Handles the click event for the card button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPayButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            if (DataContext is CardInfoViewModel viewModel)
            {
                await viewModel.ProcessCardPaymentAsync();
            }
        }
    }
}
