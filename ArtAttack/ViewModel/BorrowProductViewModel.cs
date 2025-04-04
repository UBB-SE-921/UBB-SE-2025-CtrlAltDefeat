using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ArtAttack.Services;
using ArtAttack.Model;
using ArtAttack.Domain;
using ArtAttack.Shared;
using Microsoft.UI.Xaml;

namespace ArtAttack.ViewModel
{
    internal class BorrowProductViewModel : INotifyPropertyChanged, IBorrowProductViewModel
    {
        private int currentUserId { get; set; }
        private int currentProductId { get; set; }
        private readonly IMessageDialogService _dialogService;
        private readonly IWaitListViewModel _waitListViewModel;
        private readonly NotificationViewModel _notificationViewModel;
        private string productStatus;
        private string _connectionString = Configuration._CONNECTION_STRING_;

        // Product details properties
        public string ProductName { get; set; }
        public string PriceText { get; set; }
        public string SellerText { get; set; }
        public string TypeText { get; set; }
        public string DatesText { get; set; }

        // Visibility properties
        public Visibility IsBorrowVisible { get; set; }
        public Visibility IsJoinWaitListVisible { get; set; }
        public Visibility ShowLeaveWaitlistButton { get; set; }
        public Visibility ShowUserPlaceInWaitlist { get; set; }

        // Property for the product status ("borrowed" or others)
        public string ProductStatus
        {
            get => productStatus;
            set
            {
                productStatus = value;
                OnPropertyChanged(nameof(ProductStatus));
            }
        }

        public ICommand JoinWaitlistCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BorrowProductViewModel(int currentUserId, int currentProductId, MessageDialogService dialogService)
        {
            this.currentUserId = currentUserId;
            this.currentProductId = currentProductId;
            _dialogService = dialogService;
            _waitListViewModel = new WaitListViewModel(_connectionString);
            _notificationViewModel = new NotificationViewModel(currentUserId);

            JoinWaitlistCommand = new RelayCommand(OnJoinWaitlistCommandExecuted);

            // Default visibility values
            IsBorrowVisible = Visibility.Collapsed;
            IsJoinWaitListVisible = Visibility.Visible;
            ShowLeaveWaitlistButton = Visibility.Collapsed;
            ShowUserPlaceInWaitlist = Visibility.Collapsed;
        }

        /// <summary>
        /// Join waitlist command.
        /// </summary>
        private async void OnJoinWaitlistCommandExecuted()
        {
            ShowUserPlaceInWaitlist = Visibility.Visible;
            ShowLeaveWaitlistButton = Visibility.Visible;
            ProductStatus = "borrowed"; // Just an example to toggle visibility of borrow button.
            await _dialogService.ShowMessageAsync("Success", "You've joined the waitlist!");
        }

        /// <summary>
        /// Method to load data about product.
        /// </summary>
        public async Task LoadProductDetailsAsync()
        {
            try
            {
                var product = await _waitListViewModel.GetDummyProductByIdAsync(currentProductId);

                if (product != null)
                {
                    string sellerName = await _waitListViewModel.GetSellerNameAsync(product.SellerID);

                    ProductName = product.Name;
                    PriceText = $"Price: ${product.Price:F2}";
                    SellerText = $"Seller: {sellerName}";
                    TypeText = $"Type: {product.ProductType}";
                    DatesText = $"Available until: {product.EndDate:yyyy-MM-dd}";
                    System.Diagnostics.Debug.WriteLine(ProductName, PriceText);

                    // Visibility logic based on availability
                    bool isProductAvailable = product.EndDate == DateTime.MinValue;
                    IsBorrowVisible = isProductAvailable ? Visibility.Visible : Visibility.Collapsed;
                    IsJoinWaitListVisible = !isProductAvailable ? Visibility.Visible : Visibility.Collapsed;

                    // Fire property changes for bindings; not really necesarry for now since the product is constant
                    //but you never know
                    OnPropertyChanged(nameof(ProductName));
                    OnPropertyChanged(nameof(PriceText));
                    OnPropertyChanged(nameof(SellerText));
                    OnPropertyChanged(nameof(TypeText));
                    OnPropertyChanged(nameof(DatesText));
                    OnPropertyChanged(nameof(IsBorrowVisible));
                    OnPropertyChanged(nameof(IsJoinWaitListVisible));

                    if (isProductAvailable)
                    {
                        ProductStatus = "available";
                    }
                    else
                    {
                        ProductStatus = "borrowed";
                    }

                }
                else
                {
                    await _dialogService.ShowMessageAsync("Error", "Product not found");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Error", $"Failed to load product: {ex.Message}");
            }
        }
    }
}
