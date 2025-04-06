using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using Microsoft.UI.Xaml.Controls;

namespace ArtAttack.ViewModel
{
    public class FinalizePurchaseViewModel : IFinalizePurchaseViewModel, INotifyPropertyChanged
    {
        private readonly IOrderHistoryModel orderHistoryModel;
        private readonly OrderSummaryModel orderSummaryModel;
        private readonly IOrderModel orderModel;
        private readonly NotificationViewModel notificationViewModel;

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

        public FinalizePurchaseViewModel(int orderHistoryID)
        {
            orderHistoryModel = new OrderHistoryModel(Configuration.CONNECTION_STRING);
            orderModel = new OrderModel(Configuration.CONNECTION_STRING);
            orderSummaryModel = new OrderSummaryModel(Configuration.CONNECTION_STRING);
            notificationViewModel = new NotificationViewModel(1);
            // notificationViewModel.ShowPopup += ShowNotificationPopup;
            this.orderHistoryID = orderHistoryID;

            _ = InitializeViewModelAsync();
        }

        public async Task InitializeViewModelAsync()
        {
            DummyProducts = await GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
            ProductList = new ObservableCollection<DummyProduct>(DummyProducts);

            OnPropertyChanged(nameof(ProductList));

            OrderSummary orderSummary = await orderSummaryModel.GetOrderSummaryByIDAsync(orderHistoryID);

            await SetOrderHistoryInfo(orderSummary);
        }
        public async Task SetOrderHistoryInfo(OrderSummary orderSummary)
        {
            Orders = await orderModel.GetOrdersFromOrderHistoryAsync(orderHistoryID);
            Subtotal = orderSummary.Subtotal;
            DeliveryFee = orderSummary.DeliveryFee;
            Total = orderSummary.FinalTotal;

            FullName = orderSummary.FullName;
            Email = orderSummary.Email;
            PhoneNumber = orderSummary.PhoneNumber;
            PaymentMethod = Orders[0].PaymentMethod;
            OrderStatus = "Processing";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            return await orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
        }

        internal async void HandleFinish()
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
