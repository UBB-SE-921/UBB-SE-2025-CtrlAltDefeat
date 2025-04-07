using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Data.SqlClient;
using ArtAttack.Domain;
using ArtAttack.Services;
using ArtAttack.ViewModel;

namespace ArtAttack
{
    [ExcludeFromCodeCoverage]
    public sealed partial class BorrowProductWindow : Window
    {
        private readonly string connectionString;
        private readonly int currentProductId;
        private readonly IWaitListViewModel waitListViewModel;
        private readonly NotificationViewModel notificationVM;

        public BorrowProductWindow(string connectionString, int productId)
        {
            this.InitializeComponent();
            this.connectionString = connectionString;
            this.currentProductId = productId;
            this.waitListViewModel = new WaitListViewModel(connectionString);
            this.notificationVM = new NotificationViewModel(this.GetCurrentUserId());
            this.Activated += this.Window_Activated;
        }

        private async void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            await this.LoadProductDetails();
        }

        private async Task LoadProductDetails()
        {
            try
            {
                var product = await this.waitListViewModel.GetDummyProductByIdAsync(this.currentProductId);
                if (product != null)
                {
                    string sellerName = await this.waitListViewModel.GetSellerNameAsync(product.SellerID);
                    this.DisplayProduct(product, sellerName);

                    int currentUserId = this.GetCurrentUserId();
                    bool isOnWaitlist = this.waitListViewModel.IsUserInWaitlist(currentUserId, this.currentProductId);

                    this.UpdateWaitlistUI(isOnWaitlist);
                }
                else
                {
                    await this.ShowMessageAsync("Error", "Product not found");
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", $"Failed to load product: {ex.Message}");
            }
        }

        private void UpdateWaitlistUI(bool isOnWaitlist)
        {
            this.ButtonJoinWaitList.Visibility = isOnWaitlist ? Visibility.Collapsed : Visibility.Visible;
            this.waitlistActionsPanel.Visibility = isOnWaitlist ? Visibility.Visible : Visibility.Collapsed;
            this.txtPositionInQueue.Visibility = Visibility.Collapsed;
        }

        private void DisplayProduct(DummyProduct product, string sellerName)
        {
            this.txtProductName.Text = product.Name;
            this.txtPrice.Text = $"Price: ${product.Price}";
            this.txtSeller.Text = $"Seller: {sellerName}";
            this.txtType.Text = $"Type: {product.ProductType}";

            bool isAvailable = product.EndDate == DateTime.MinValue;

            if (isAvailable)
            {
                this.txtDates.Text = product.StartDate == DateTime.MinValue
                    ? "Availability: Now"
                    : $"Available after: {product.StartDate:yyyy-MM-dd}";

                this.ButtonBorrow.Visibility = Visibility.Visible;
                this.ButtonJoinWaitList.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.txtDates.Text = $"Unavailable until: {product.EndDate:yyyy-MM-dd}";
                this.ButtonBorrow.Visibility = Visibility.Collapsed;
                this.ButtonJoinWaitList.Visibility = Visibility.Visible;
            }
        }

        private async Task ShowMessageAsync(string title, string message)
        {
            try
            {
                if (this.Content.XamlRoot == null)
                {
                    throw new InvalidOperationException("XamlRoot is not initialized.");
                }

                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            catch (Exception e)
            {
                // Handle the exception here
                Debug.WriteLine($"Failed to show dialog: {e.Message}");
            }
        }

        private async void ButtonJoinWaitList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentUserId = this.GetCurrentUserId();

                this.waitListViewModel.AddUserToWaitlist(currentUserId, this.currentProductId);

                this.UpdateWaitlistUI(true);

                await this.ShowMessageAsync("Success", "You've joined the waitlist!");
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", $"Failed to join waitlist: {ex.Message}");
            }
        }

        private int GetCurrentUserId()
        {
            return 1;
        }

        private async void ButtonLeaveWaitList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentUserId = this.GetCurrentUserId();

                this.waitListViewModel.RemoveUserFromWaitlist(currentUserId, this.currentProductId);

                this.UpdateWaitlistUI(false);

                await this.ShowMessageAsync("Success", "You've left the waitlist");
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", $"Failed to leave waitlist: {ex.Message}");
            }
        }

        private async void ButtonViewPosition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentUserId = this.GetCurrentUserId();
                int position = this.waitListViewModel.GetUserWaitlistPosition(currentUserId, this.currentProductId);

                if (position > 0)
                {
                    this.txtPositionInQueue.Text = $"Your position: #{position}";
                    this.txtPositionInQueue.Visibility = Visibility.Visible;
                }
                else
                {
                    await this.ShowMessageAsync("Position", "You are not currently on the waitlist");
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", $"Failed to get waitlist position: {ex.Message}");
            }
        }

        private async void ButtonNotifications_Click(object sender, RoutedEventArgs e)
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
                    new TextBlock { Text = this.notificationVM.UnReadNotificationsCountText },
                },
                        },
                    },
                    CloseButtonText = "Close",
                    XamlRoot = this.Content.XamlRoot,
                };

                await notificationPopup.ShowAsync();
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", $"Couldn't load notifications: {ex.Message}");
            }
        }
    }
}