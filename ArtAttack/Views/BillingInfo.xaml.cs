using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArtAttack
{
    public sealed partial class BillingInfo : Page
    {
        private BillingInfoViewModel viewModel;

        public BillingInfo(int orderHistoryID)
        {
            this.InitializeComponent();
            viewModel = new BillingInfoViewModel(orderHistoryID);

            DataContext = viewModel;
        }
        private async void OnFinalizeButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            if (DataContext is BillingInfoViewModel viewModel)
            {
                await viewModel.OnFinalizeButtonClickedAsync();
            }
        }

        private void OnStartDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs e)
        {
            viewModel.UpdateStartDate(sender.Date);
        }

        private async void OnEndDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs e)
        {
            viewModel.UpdateEndDate(sender.Date);
            await UpdateBorrowedProductTax(sender);
        }

        private async Task UpdateBorrowedProductTax(DatePicker sender)
        {
            if (DataContext is BillingInfoViewModel viewModel && sender.DataContext is DummyProduct product)
            {
                await viewModel.ApplyBorrowedTax(product);
            }
        }
    }
}
