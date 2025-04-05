using System;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ArtAttack.Services;
using ArtAttack.ViewModel;

namespace ArtAttack
{
    public sealed partial class BorrowProductWindow : Window
    {
        private readonly string connectionString;
        private readonly int currentProductId;
        private readonly WaitListViewModel waitListViewModel;
        private readonly NotificationViewModel notificationVM;

        public BorrowProductWindow(string connectionString, int productId)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            currentProductId = productId;
            waitListViewModel = new WaitListViewModel(connectionString);
            notificationVM = new NotificationViewModel(GetCurrentUserId());
            this.Activated += Window_Activated;
        }

        private async void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            this.Activated -= Window_Activated;
            await LoadProductDetails();
        }

        private async Task LoadProductDetails()
        {
            try
            {
                var product = await waitListViewModel.GetDummyProductByIdAsync(currentProductId);
                if (product != null)
                {
                    string sellerName = await waitListViewModel.GetSellerNameAsync(product.SellerID);
                    DisplayProduct(product, sellerName);

                    int currentUserId = GetCurrentUserId();
                    bool isOnWaitlist = waitListViewModel.IsUserInWaitlist(currentUserId, currentProductId);

                    UpdateWaitlistUI(isOnWaitlist);
                }
                else
                {
                    await ShowMessageAsync("Error", "Product not found");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error", $"Failed to load product: {ex.Message}");
            }
        }

        private void UpdateWaitlistUI(bool isOnWaitlist)
        {
            btnJoinWaitList.Visibility = isOnWaitlist ? Visibility.Collapsed : Visibility.Visible;
            waitlistActionsPanel.Visibility = isOnWaitlist ? Visibility.Visible : Visibility.Collapsed;
            txtPositionInQueue.Visibility = Visibility.Collapsed;
        }

        private void DisplayProduct(DummyProduct product, string sellerName)
        {
            txtProductName.Text = product.Name;
            txtPrice.Text = $"Price: ${product.Price}";
            txtSeller.Text = $"Seller: {sellerName}";
            txtType.Text = $"Type: {product.ProductType}";

            bool isAvailable = product.EndDate == DateTime.MinValue;

            if (isAvailable)
            {
                txtDates.Text = product.StartDate == DateTime.MinValue
                    ? "Availability: Now"
                    : $"Available after: {product.StartDate:yyyy-MM-dd}";

                btnBorrow.Visibility = Visibility.Visible;
                btnJoinWaitList.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtDates.Text = $"Unavailable until: {product.EndDate:yyyy-MM-dd}";
                btnBorrow.Visibility = Visibility.Collapsed;
                btnJoinWaitList.Visibility = Visibility.Visible;
            }
        }

        private async Task ShowMessageAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void BtnJoinWaitList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                waitListViewModel.AddUserToWaitlist(currentUserId, currentProductId);

                UpdateWaitlistUI(true);

                await ShowMessageAsync("Success", "You've joined the waitlist!");
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error", $"Failed to join waitlist: {ex.Message}");
            }
        }

        private int GetCurrentUserId()
        {
            return 1;
        }

        private async void BtnLeaveWaitList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                waitListViewModel.RemoveUserFromWaitlist(currentUserId, currentProductId);

                UpdateWaitlistUI(false);

                await ShowMessageAsync("Success", "You've left the waitlist");
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error", $"Failed to leave waitlist: {ex.Message}");
            }
        }

        private async void BtnViewPosition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                int position = waitListViewModel.GetUserWaitlistPosition(currentUserId, currentProductId);

                if (position > 0)
                {
                    txtPositionInQueue.Text = $"Your position: #{position}";
                    txtPositionInQueue.Visibility = Visibility.Visible;
                }
                else
                {
                    await ShowMessageAsync("Position", "You are not currently on the waitlist");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error", $"Failed to get waitlist position: {ex.Message}");
            }
        }

        private async void BtnNotifications_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create and show a simple notifications popup
                var notificationPopup = new ContentDialog
                {
                    Title = "Your Notifications",
                    Content = new ScrollViewer
                    {
                        Content = new StackPanel
                        {
                            Children =
                    {
                        new TextBlock { Text = notificationVM.UnReadNotificationsCountText },
                        // Add more notification items here if needed
                    }
                        }
                    },
                    CloseButtonText = "Close",
                    XamlRoot = this.Content.XamlRoot
                };

                await notificationPopup.ShowAsync();
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error", $"Couldn't load notifications: {ex.Message}");
            }
        }
    }
}