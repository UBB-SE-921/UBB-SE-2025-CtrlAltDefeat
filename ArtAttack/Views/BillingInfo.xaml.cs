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
        /// <summary>
        /// The view model for the BillingInfo page.
        /// </summary>
        private BillingInfoViewModel viewModel;

        public BillingInfo(int orderHistoryID)
        {
            this.InitializeComponent();
            viewModel = new BillingInfoViewModel(orderHistoryID);

            DataContext = viewModel;
        }

        /// <summary>
        /// Handles the click event for the finalize button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnFinalizeButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            if (DataContext is BillingInfoViewModel viewModel)
            {
                await viewModel.OnFinalizeButtonClickedAsync();
            }
        }

        /// <summary>
        /// Handles the click event for the cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs e)
        {
            viewModel.UpdateStartDate(sender.Date);
        }

        /// <summary>
        /// Handles the click event for the end date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnEndDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs e)
        {
            viewModel.UpdateEndDate(sender.Date);
            await UpdateBorrowedProductTax(sender);
        }

        /// <summary>
        /// Handles the click event for the tax date
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private async Task UpdateBorrowedProductTax(DatePicker sender)
        {
            if (DataContext is BillingInfoViewModel viewModel && sender.DataContext is DummyProduct product)
            {
                await viewModel.ApplyBorrowedTax(product);
            }
        }
    }
}
