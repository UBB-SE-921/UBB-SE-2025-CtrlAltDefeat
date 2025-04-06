using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArtAttack
{
    [ExcludeFromCodeCoverage]
    public sealed partial class BillingInfo : Page
    {
        private BillingInfoModelView viewModel;

        public BillingInfo(int orderHistoryID)
        {
            this.InitializeComponent();
            viewModel = new BillingInfoModelView(orderHistoryID);

            DataContext = viewModel;
        }
        private async void OnFinalizeButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            if (DataContext is BillingInfoModelView viewModel)
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
            if (DataContext is BillingInfoModelView viewModel && sender.DataContext is DummyProduct product)
            {
                await viewModel.ApplyBorrowedTax(product);
            }
        }
    }
}
