using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    public class CardInfoViewModel : ICardInfoViewModel, INotifyPropertyChanged
    {
        private readonly OrderHistoryModel orderHistoryModel;
        private readonly OrderSummaryModel orderSummaryModel;
        private readonly OrderModel orderModel;
        private readonly DummyCardModel dummyCardModel;

        private int orderHistoryID;

        private float _subtotal;
        private float _deliveryFee;
        private float _total;

        private string _email;
        private string _cardholder;
        private string _cardnumber;
        private string _cardMonth;
        private string _cardYear;
        private string _cardCVC;
        public ObservableCollection<DummyProduct> ProductList { get; set; }
        public List<DummyProduct> dummyProducts;


        public CardInfoViewModel(int orderHistoryID)
        {
            orderHistoryModel = new OrderHistoryModel(Configuration._CONNECTION_STRING_);
            orderModel = new OrderModel(Configuration._CONNECTION_STRING_);
            orderSummaryModel = new OrderSummaryModel(Configuration._CONNECTION_STRING_);
            dummyCardModel = new DummyCardModel(Configuration._CONNECTION_STRING_);

            this.orderHistoryID = orderHistoryID;

            _ = InitializeViewModelAsync();

        }

        public async Task InitializeViewModelAsync()
        {
            dummyProducts = await GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
            ProductList = new ObservableCollection<DummyProduct>(dummyProducts);

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
            get => _subtotal;
            set { _subtotal = value; OnPropertyChanged(nameof(Subtotal)); }
        }

        public float DeliveryFee
        {
            get => _deliveryFee;
            set { _deliveryFee = value; OnPropertyChanged(nameof(DeliveryFee)); }
        }

        public float Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged(nameof(Total)); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        public string CardHolderName
        {
            get => _cardholder;
            set { _cardholder = value; OnPropertyChanged(nameof(CardHolderName)); }
        }

        public string CardNumber
        {
            get => _cardnumber;
            set { _cardnumber = value; OnPropertyChanged(nameof(CardNumber)); }
        }

        public string CardMonth
        {
            get => _cardMonth;
            set { _cardMonth = value; OnPropertyChanged(nameof(CardMonth)); }
        }

        public string CardYear
        {
            get => _cardYear;
            set { _cardYear = value; OnPropertyChanged(nameof(CardYear)); }
        }

        public string CardCVC
        {
            get => _cardCVC;
            set { _cardCVC = value; OnPropertyChanged(nameof(CardCVC)); }
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

        internal async Task onPayButtonClickedAsync()
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
