using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using ArtAttack.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ArtAttack.ViewModel
{
    public class BillingInfoModelView : IBillingInfoModelView, INotifyPropertyChanged
    {
        private readonly OrderHistoryModel orderHistoryModel;
        private readonly OrderSummaryModel orderSummaryModel;
        private readonly OrderModel orderModel;
        private readonly DummyProductModel dummyProductModel;
        private readonly DummyWalletModel dummyWalletModel;

        private int orderHistoryID;

        private bool _isWalletEnabled;
        private bool _isCashEnabled;
        private bool _isCardEnabled;

        private string _selectedPaymentMethod;

        private string _fullName;
        private string _email;
        private string _phoneNumber;
        private string _address;
        private string _zipCode;
        private string _additionalInfo;

        private DateTime _startDate;
        private DateTime _endDate;

        private float _subtotal;
        private float _deliveryFee;
        private float _total;
        private float _warrantyTax;

        public ObservableCollection<DummyProduct> ProductList { get; set; }
        public List<DummyProduct> dummyProducts;

        public BillingInfoModelView(int orderHistoryID)
        {
            orderHistoryModel = new OrderHistoryModel(Configuration._CONNECTION_STRING_);
            orderModel = new OrderModel(Configuration._CONNECTION_STRING_);
            orderSummaryModel = new OrderSummaryModel(Configuration._CONNECTION_STRING_);
            dummyWalletModel = new DummyWalletModel(Configuration._CONNECTION_STRING_);
            dummyProductModel = new DummyProductModel(Configuration._CONNECTION_STRING_);

            this.orderHistoryID = orderHistoryID;

            _ = InitializeViewModelAsync();

            _warrantyTax = 0;
        }

        public async Task InitializeViewModelAsync()
        {
            dummyProducts = await GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
            ProductList = new ObservableCollection<DummyProduct>(dummyProducts);

            OnPropertyChanged(nameof(ProductList));

            SetVisibilityRadioButtons();

            CalculateOrderTotal(orderHistoryID);
        }

        public void SetVisibilityRadioButtons()
        {

            if (ProductList.Count > 0)
            {
                // For simplicity, assume that products won in a bid can only be bought separately. 
                // Same for wallet refills

                // Based on the type of the first product in the list, we set the visibility
                // of the payment method choices

                string FirstProductType = ProductList[0].ProductType;

                if (FirstProductType == "new" || FirstProductType == "used" || FirstProductType == "borrowed")
                {
                    IsCardEnabled = true;
                    IsCashEnabled = true;
                    IsWalletEnabled = false;
                }
                else if (FirstProductType == "bid")
                {
                    IsCardEnabled = false;
                    IsCashEnabled = false;
                    IsWalletEnabled = true;
                }
                else if (FirstProductType == "refill")
                {
                    IsCardEnabled = true;
                    IsCashEnabled = false;
                    IsWalletEnabled = false;
                }

            }
        }

        public async Task onFinalizeButtonClickedAsync()
        {
            string paymentMethod = SelectedPaymentMethod;

            // This is subject to change, as the orderModel is to be switched to asynchronous architecture
            List<Order> orderList = await orderModel.GetOrdersFromOrderHistoryAsync(orderHistoryID);

            foreach (var order in orderList)
            {
                await orderModel.UpdateOrderAsync(order.OrderID, order.ProductType, SelectedPaymentMethod, DateTime.Now);
            }

            // Currently, an order summary has the same ID as the order history for simplicity
            await orderSummaryModel.UpdateOrderSummaryAsync(orderHistoryID, Subtotal, _warrantyTax, DeliveryFee, Total, FullName, Email, PhoneNumber, Address, ZipCode, AdditionalInfo, null);

            await openNextWindowAsync(SelectedPaymentMethod);
        }

        public async Task openNextWindowAsync(string SelectedPaymentMethod)
        {
            if (SelectedPaymentMethod == "card")
            {
                var b_window = new BillingInfoWindow();
                var cardInfoPage = new CardInfo(orderHistoryID);
                b_window.Content = cardInfoPage;

                b_window.Activate();

                // This is just a workaround until I figure out how to switch between pages

            }
            else
            {
                if (SelectedPaymentMethod == "wallet")
                    await processWalletRefillAsync();
                var b_window = new BillingInfoWindow();
                var finalisePurchasePage = new FinalisePurchase(orderHistoryID);
                b_window.Content = finalisePurchasePage;

                b_window.Activate();
            }
        }

        private async Task processWalletRefillAsync()
        {
            // There is only one wallet, with the ID 1

            float walletBalance = await dummyWalletModel.GetWalletBalanceAsync(1);

            float newBalance = walletBalance - Total;

            await dummyWalletModel.UpdateWalletBalance(1, newBalance);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void CalculateOrderTotal(int orderHistoryID)
        {
            float subtotalProducts = 0;
            foreach (var product in dummyProducts)
                subtotalProducts += product.Price;

            // For orders over 200 RON, a fixed delivery fee of 13.99 will be added
            // (this is only for orders of new, used or borrowed products)

            Subtotal = subtotalProducts;
            if (subtotalProducts >= 200 || dummyProducts[0].ProductType == "refill" || dummyProducts[0].ProductType == "bid")
                Total = subtotalProducts;
            else
            {
                Total = subtotalProducts + 13.99f;
                DeliveryFee = 13.99f;
            }

        }

        public async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            return await orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
        }


        public async Task ApplyBorrowedTax(DummyProduct dummyProduct)
        {
            if (dummyProduct == null || dummyProduct.ProductType != "borrowed")
                return;
            if (StartDate > EndDate)
                return;
            int monthsBorrowed = ((EndDate.Year - StartDate.Year) * 12) + EndDate.Month - StartDate.Month;
            if (monthsBorrowed <= 0)
                monthsBorrowed = 1;

            float warrantyTaxAmount = 0.2f;

            float finalPrice = dummyProduct.Price * monthsBorrowed;

            _warrantyTax += finalPrice * warrantyTaxAmount;

            WarrantyTax = _warrantyTax;

            dummyProduct.Price = finalPrice;

            CalculateOrderTotal(orderHistoryID);

            DateTime newStartDate = _startDate.Date;
            DateTime newEndDate = _endDate.Date;

            dummyProduct.StartDate = newStartDate;
            dummyProduct.EndDate = newEndDate;

            await dummyProductModel.UpdateDummyProductAsync(dummyProduct.ID, dummyProduct.Name, dummyProduct.Price, dummyProduct.SellerID ?? 0, dummyProduct.ProductType, newStartDate, newEndDate);
        }

        internal void UpdateStartDate(DateTimeOffset date)
        {
            _startDate = date.DateTime;
            StartDate = date.DateTime;
        }

        internal void UpdateEndDate(DateTimeOffset date)
        {
            _endDate = date.DateTime;
            EndDate = date.DateTime;
        }

        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged(nameof(SelectedPaymentMethod));
            }
        }

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(nameof(FullName)); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); }
        }
        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(nameof(Address)); }
        }

        public string ZipCode
        {
            get => _zipCode;
            set { _zipCode = value; OnPropertyChanged(nameof(ZipCode)); }
        }

        public string AdditionalInfo
        {
            get => _additionalInfo;
            set { _additionalInfo = value; OnPropertyChanged(nameof(AdditionalInfo)); }
        }

        public bool IsWalletEnabled
        {
            get => _isWalletEnabled;
            set { _isWalletEnabled = value; OnPropertyChanged(nameof(IsWalletEnabled)); }
        }

        public bool IsCashEnabled
        {
            get => _isCashEnabled;
            set { _isCashEnabled = value; OnPropertyChanged(nameof(IsCashEnabled)); }
        }

        public bool IsCardEnabled
        {
            get => _isCardEnabled;
            set { _isCardEnabled = value; OnPropertyChanged(nameof(IsCardEnabled)); }
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
        public float WarrantyTax
        {
            get => _warrantyTax;
            set { _warrantyTax = value; OnPropertyChanged(nameof(_warrantyTax)); }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set { 
                _startDate = value; OnPropertyChanged(nameof(StartDate));
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value; OnPropertyChanged(nameof(EndDate));
            }
        }


    }
}
