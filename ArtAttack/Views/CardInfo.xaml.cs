using System.Diagnostics.CodeAnalysis;
using ArtAttack.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArtAttack
{
    [ExcludeFromCodeCoverage]
    public sealed partial class CardInfo : Page
    {
        private CardInfoViewModel viewModel;

        public CardInfo(int orderHistoryID)
        {
            this.InitializeComponent();
            viewModel = new CardInfoViewModel(orderHistoryID);
            DataContext = viewModel;
        }
        private async void OnPayButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            if (DataContext is CardInfoViewModel viewModel)
            {
                await viewModel.OnPayButtonClickedAsync();
            }
        }
    }
}
