using System;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;
using ArtAttack.ViewModel;
using ArtAttack.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuestPDF.Infrastructure;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace ArtAttack
{
    public sealed partial class MainWindow : Window
    {
        private IContract contract;
        private IContractViewModel contractViewModel;
        private ITrackedOrderViewModel trackedOrderViewModel;

        public MainWindow()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            this.InitializeComponent();
            contract = new Contract();
            contractViewModel = new ContractViewModel(Configuration.CONNECTION_STRING);
            trackedOrderViewModel = new TrackedOrderViewModel(Configuration.CONNECTION_STRING);
        }

        // This event handler is called when the Grid (root element) is loaded.
        private async void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // Asynchronously fetch the contract after the UI is ready.
            contract = await contractViewModel.GetContractByIdAsync(2);
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Now you await the async method.
            contract = await contractViewModel.GetContractByIdAsync(1);
        }

        private void PurchaseButton_Clicked(object sender, RoutedEventArgs e)
        {
            BillingInfoWindow billingInfoWindow = new BillingInfoWindow();
            var bp = new BillingInfo(1);
            billingInfoWindow.Content = bp;
            billingInfoWindow.Activate();
        }

        private void OrderHitoryButton_Clicked(object sender, RoutedEventArgs e)
        {
            int user_id = 1;
            var orderhistorywindow = new OrderHistoryUI(Configuration.CONNECTION_STRING, user_id);
            orderhistorywindow.Activate();
        }

        private void BidProductButton_Clicked(object sender, RoutedEventArgs e)
        {
            BillingInfoWindow billingInfoWindow = new BillingInfoWindow();
            var bp = new BillingInfo(2);
            billingInfoWindow.Content = bp;
            billingInfoWindow.Activate();
        }

        private void NotificationButton_Clicked(object sender, RoutedEventArgs e)
        {
            MainNotificationWindow mainNotificationWindow = new MainNotificationWindow();
            mainNotificationWindow.Activate();
        }
        private void WalletRefillButton_Clicked(object sender, RoutedEventArgs e)
        {
            BillingInfoWindow billingInfoWindow = new BillingInfoWindow();
            var bp = new BillingInfo(3);
            billingInfoWindow.Content = bp;
            billingInfoWindow.Activate();
        }

        private async void GenerateContractButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (contract != null)
            {
                await contractViewModel.GenerateAndSaveContractAsync(contract, PredefinedContractType.BorrowContract);

                // Optionally, show a success dialog after generating the contract.
                var successDialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Contract generated and saved successfully.",
                    CloseButtonText = "OK",
                    XamlRoot = RootGrid.XamlRoot
                };
                await successDialog.ShowAsync();
            }
            else
            {
                await ShowNoContractDialogAsync();
            }
        }

        private async Task ShowNoContractDialogAsync()
        {
            var contentDialog = new ContentDialog
            {
                Title = "Error",
                Content = "No Contract has been found with ID 1.",
                CloseButtonText = "OK",
                XamlRoot = RootGrid.XamlRoot
            };

            await contentDialog.ShowAsync();
        }

        private async void BorrowButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                int productId = 2;

                var borrowWindow = new BorrowProductWindow(Configuration.CONNECTION_STRING, productId);
                borrowWindow.Activate();
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("Failed to open Borrow Product", ex.Message);
            }
        }
        private async void RenewContractButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a new instance of the RenewContractView window
                var renewContractWindow = new RenewContractView();

                // Show (activate) the window to the user
                renewContractWindow.Activate();
            }
            catch (Exception ex)
            {
                // If an error occurs while opening the window, show an error dialog with the message
                await ShowErrorDialogAsync("Error opening Renew Contract", ex.Message);
            }
        }
        private async Task ShowErrorDialogAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK"
            };

            // Apply default WinUI styling
            dialog.XamlRoot = this.Content.XamlRoot;
            await dialog.ShowAsync();
        }

        private async void TrackOrderButton_Clicked(object sender, RoutedEventArgs e)
        {
            var inputID = await ShowTrackedOrderInputDialogAsync();
            if (inputID == null)
            {
                return;
            }
            if (inputID == -1)
            {
                await ShowNoTrackedOrderDialogAsync("Please enter an integer!");
            }
            else
            {
                int trackedOrderID = (int)inputID.Value;
                try
                {
                    var order = await trackedOrderViewModel.GetTrackedOrderByIDAsync(trackedOrderID);

                    // false=readonly, true=sudomode; Modify according to the current user privileges
                    bool hasControlAccess = true;

                    TrackedOrderWindow trackedOrderWindow = new TrackedOrderWindow();
                    if (hasControlAccess)
                    {
                        var controlp = new TrackedOrderControlPage(trackedOrderViewModel, trackedOrderID);
                        trackedOrderWindow.Content = controlp;
                    }
                    else
                    {
                        var buyerp = new TrackedOrderBuyerPage(trackedOrderViewModel, trackedOrderID);
                        trackedOrderWindow.Content = buyerp;
                    }

                    trackedOrderWindow.Activate();
                }
                catch (Exception)
                {
                    await ShowNoTrackedOrderDialogAsync("No TrackedOrder has been found with ID " + trackedOrderID.ToString());
                }
            }
        }

        private async Task<int?> ShowTrackedOrderInputDialogAsync()
        {
            var contentDialog = new ContentDialog
            {
                Title = "Enter Tracked Order ID",
                PrimaryButtonText = "Confirm",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = RootGrid.XamlRoot
            };

            TextBox inputTextBox = new TextBox { PlaceholderText = "Enter Tracked Order ID" };
            contentDialog.Content = inputTextBox;

            var result = await contentDialog.ShowAsync();
            bool parseSuccessful = int.TryParse(inputTextBox.Text, out int trackedOrderID);

            if (result == ContentDialogResult.Primary && parseSuccessful)
            {
                return trackedOrderID;
            }

            if (result == ContentDialogResult.Primary && !parseSuccessful)
            {
                return -1;
            }

            return null;
        }

        private async Task ShowNoTrackedOrderDialogAsync(string message)
        {
            var contentDialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = RootGrid.XamlRoot
            };

            await contentDialog.ShowAsync();
        }
    }
}