using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Service;
using ArtAttack.Shared;
using Microsoft.UI.Xaml.Controls;

namespace ArtAttack.ViewModel
{
    /// <summary>
    /// Represents the view model for finalizing a purchase and updating order history and notifications.
    /// </summary>
    public class FinalizePurchaseViewModel : IFinalizePurchaseViewModel, INotifyPropertyChanged
    {
        private readonly IOrderHistoryService orderHistoryService;
        private readonly IOrderSummaryService orderSummaryService;
        private readonly IOrderService orderService;
        private readonly INotificationViewModel notificationViewModel;

        private int orderHistoryID;

        private float subtotal;
        private float deliveryFee;
        private float total;

        private string fullname;
        private string phone;
        private string email;
        private string paymentMethod;
        private string orderStatus;
        public ObservableCollection<DummyProduct> ProductList { get; set; }
        public List<DummyProduct> DummyProducts;
        public List<Order> Orders;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinalizePurchaseViewModel"/> class and begins loading order details for finalization.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        public FinalizePurchaseViewModel(int orderHistoryID)
        {
            string connectionString = Configuration.CONNECTION_STRING;
            IDatabaseProvider databaseProvider = new SqlDatabaseProvider();
            
            orderHistoryService = new OrderHistoryService(connectionString, databaseProvider);
            orderService = new OrderService(connectionString, databaseProvider);
            orderSummaryService = new OrderSummaryService(connectionString, databaseProvider);
            notificationViewModel = new NotificationViewModel(1);
            
            this.orderHistoryID = orderHistoryID;

            _ = InitializeViewModelAsync();
        }

        /// <summary>
        /// Asynchronously initializes the view model by loading dummy products, order summary details, and setting order history information.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeViewModelAsync()
        {
            DummyProducts = await GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
            ProductList = new ObservableCollection<DummyProduct>(DummyProducts);

            OnPropertyChanged(nameof(ProductList));

            OrderSummary orderSummary = await orderSummaryService.GetOrderSummaryByIdAsync(orderHistoryID);

            await SetOrderHistoryInfo(orderSummary);
        }

        /// <summary>
        /// Sets the order history information by updating order summary details and order status.
        /// </summary>
        /// <param name="orderSummary">The order summary object containing order details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SetOrderHistoryInfo(OrderSummary orderSummary)
        {
            Orders = await orderService.GetOrdersFromOrderHistoryAsync(orderHistoryID);
            Subtotal = orderSummary.Subtotal;
            DeliveryFee = orderSummary.DeliveryFee;
            Total = orderSummary.FinalTotal;

            FullName = orderSummary.FullName;
            Email = orderSummary.Email;
            PhoneNumber = orderSummary.PhoneNumber;
            PaymentMethod = Orders[0].PaymentMethod;
            OrderStatus = "Processing";
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Asynchronously retrieves dummy products associated with the specified order history.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A task that returns a list of <see cref="DummyProduct"/> objects.</returns>
        public async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            return await orderHistoryService.GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
        }

        /// <summary>
        /// Handles the finish process by sending payment confirmation notifications for each order.
        /// </summary>
        public async void HandleFinish()
        {
            foreach (var order in Orders)
            {
                await notificationViewModel.AddNotificationAsync(new PaymentConfirmationNotification(1, System.DateTime.Now, order.ProductID, order.OrderID));
            }
        }

        public float Subtotal
        {
            get => subtotal;
            set
            {
                subtotal = value;
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public float DeliveryFee
        {
            get => deliveryFee;
            set
            {
                deliveryFee = value;
                OnPropertyChanged(nameof(DeliveryFee));
            }
        }

        public float Total
        {
            get => total;
            set
            {
                total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        public string FullName
        {
            get => fullname;
            set
            {
                fullname = value;
                OnPropertyChanged(nameof(FullName));
            }
        }

        public string PhoneNumber
        {
            get => phone;
            set
            {
                phone = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }

        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string PaymentMethod
        {
            get => paymentMethod;
            set
            {
                paymentMethod = value;
                OnPropertyChanged(nameof(PaymentMethod));
            }
        }

        public string OrderStatus
        {
            get => orderStatus;
            set
            {
                orderStatus = value;
                OnPropertyChanged(nameof(OrderStatus));
            }
        }
    }
}
