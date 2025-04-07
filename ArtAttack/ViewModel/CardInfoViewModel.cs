using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;

namespace ArtAttack.ViewModel
{
    public class CardInfoViewModel : ICardInfoViewModel, INotifyPropertyChanged
    {
        private readonly IOrderHistoryModel orderHistoryModel;
        private readonly IOrderSummaryModel orderSummaryModel;
        private readonly IOrderModel orderModel;
        private readonly IDummyCardModel dummyCardModel;

        private int orderHistoryID;

        private float subtotal;
        private float deliveryFee;
        private float total;

        private string email;
        private string cardholder;
        private string cardnumber;
        private string cardMonth;
        private string cardYear;
        private string cardCVC;
        public ObservableCollection<DummyProduct> ProductList { get; set; }
        public List<DummyProduct> DummyProducts;
        public CardInfoViewModel(
            IOrderHistoryModel orderHistoryModel,
            IOrderSummaryModel orderSummaryModel,
            IOrderModel orderModel,
            IDummyCardModel dummyCardModel,
            int orderHistoryID)
        {
            this.orderHistoryModel = orderHistoryModel ?? throw new ArgumentNullException(nameof(orderHistoryModel));
            this.orderSummaryModel = orderSummaryModel ?? throw new ArgumentNullException(nameof(orderSummaryModel));
            this.orderModel = orderModel ?? throw new ArgumentNullException(nameof(orderModel));
            this.dummyCardModel = dummyCardModel ?? throw new ArgumentNullException(nameof(dummyCardModel));
            this.orderHistoryID = orderHistoryID;

            _ = InitializeViewModelAsync();
        }

        [ExcludeFromCodeCoverage]
        public CardInfoViewModel(int orderHistoryID)
        {
            orderHistoryModel = new OrderHistoryModel(Configuration.CONNECTION_STRING);
            orderModel = new OrderModel(Configuration.CONNECTION_STRING);
            orderSummaryModel = new OrderSummaryModel(Configuration.CONNECTION_STRING);
            dummyCardModel = new DummyCardModel(Configuration.CONNECTION_STRING);

            this.orderHistoryID = orderHistoryID;

            _ = InitializeViewModelAsync();
        }

        public async Task InitializeViewModelAsync()
        {
            DummyProducts = await GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
            ProductList = new ObservableCollection<DummyProduct>(DummyProducts);

            OnPropertyChanged(nameof(ProductList));

            OrderSummary orderSummary = await orderSummaryModel.GetOrderSummaryByIDAsync(orderHistoryID);

            Subtotal = orderSummary.Subtotal;
            DeliveryFee = orderSummary.DeliveryFee;
            Total = orderSummary.FinalTotal;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string CardHolderName
        {
            get => cardholder;
            set
            {
                cardholder = value;
                OnPropertyChanged(nameof(CardHolderName));
            }
        }

        public string CardNumber
        {
            get => cardnumber;
            set
            {
                cardnumber = value;
                OnPropertyChanged(nameof(CardNumber));
            }
        }

        public string CardMonth
        {
            get => cardMonth;
            set
            {
                cardMonth = value;
                OnPropertyChanged(nameof(CardMonth));
            }
        }

        public string CardYear
        {
            get => cardYear;
            set
            {
                cardYear = value;
                OnPropertyChanged(nameof(CardYear));
            }
        }

        public string CardCVC
        {
            get => cardCVC;
            set
            {
                cardCVC = value;
                OnPropertyChanged(nameof(CardCVC));
            }
        }

        public async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            return await orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
        }
        public async Task ProcessCardPaymentAsync()
        {
            float balance = await dummyCardModel.GetCardBalanceAsync(CardNumber);

            OrderSummary orderSummary = await orderSummaryModel.GetOrderSummaryByIDAsync(orderHistoryID);

            float totalSum = orderSummary.FinalTotal;

            float newBalance = balance - totalSum;

            await dummyCardModel.UpdateCardBalanceAsync(CardNumber, newBalance);
        }

        [ExcludeFromCodeCoverage]
        public async Task OnPayButtonClickedAsync()
        {
            await ProcessCardPaymentAsync();

            var b_window = new BillingInfoWindow();
            var finalisePurchasePage = new FinalisePurchase(orderHistoryID);
            b_window.Content = finalisePurchasePage;

            b_window.Activate();

            // Some validation of the fields is required to make sure they are actually filled.
            // Will update later
        }
    }
}
