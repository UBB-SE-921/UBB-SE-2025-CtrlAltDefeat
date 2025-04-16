using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Service;

namespace ArtAttack.ViewModel
{
    public class CardInfoViewModel : ICardInfoViewModel, INotifyPropertyChanged
    {
        private readonly CardInfoService cardInfoService;
        private readonly int orderHistoryID;

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

        public CardInfoViewModel(CardInfoService cardInfoService, int orderHistoryID)
        {
            this.cardInfoService = cardInfoService ?? throw new ArgumentNullException(nameof(cardInfoService));
            this.orderHistoryID = orderHistoryID;

            _ = InitializeViewModelAsync();
        }

        public async Task InitializeViewModelAsync()
        {
            DummyProducts = await cardInfoService.GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
            ProductList = new ObservableCollection<DummyProduct>(DummyProducts);

            OnPropertyChanged(nameof(ProductList));

            OrderSummary orderSummary = await cardInfoService.GetOrderSummaryAsync(orderHistoryID);

            Subtotal = orderSummary.Subtotal;
            DeliveryFee = orderSummary.DeliveryFee;
            Total = orderSummary.FinalTotal;
        }

        public async Task ProcessCardPaymentAsync()
        {
            await cardInfoService.ProcessCardPaymentAsync(CardNumber, orderHistoryID);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task OnPayButtonClickedAsync()
        {
            await ProcessCardPaymentAsync();

            var billingInfoWindow = new BillingInfoWindow();
            var finalisePurchasePage = new FinalisePurchase(orderHistoryID);
            billingInfoWindow.Content = finalisePurchasePage;

            billingInfoWindow.Activate();
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
    }
}
