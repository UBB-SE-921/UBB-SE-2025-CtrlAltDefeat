using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.ViewModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class BillingInfoViewModelTests
    {
        private  Mock<IOrderHistoryModel> mockOrderHistoryModel;
        private  Mock<IOrderSummaryModel> mockOrderSummaryModel;
        private  Mock<IOrderModel> mockOrderModel;
        private  Mock<IDummyProductModel> mockDummyProductModel;
        private  Mock<IDummyWalletModel> mockDummyWalletModel;
        private  BillingInfoViewModel viewModel;

        [TestInitialize]
        public void setup()
        {
            mockOrderHistoryModel = new Mock<IOrderHistoryModel>();
            mockOrderSummaryModel = new Mock<IOrderSummaryModel>();
            mockOrderModel = new Mock<IOrderModel>();
            mockDummyProductModel = new Mock<IDummyProductModel>();
            mockDummyWalletModel = new Mock<IDummyWalletModel>();

            viewModel = new BillingInfoViewModel(1);
            //use getField to set the private fields
            var orderHistoryModelField = typeof(BillingInfoViewModel).GetField("orderHistoryModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var orderSummaryModelField = typeof(BillingInfoViewModel).GetField("orderSummaryModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var orderModelField = typeof(BillingInfoViewModel).GetField("orderModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dummyProductModelField = typeof(BillingInfoViewModel).GetField("dummyProductModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dummyWalletModelField = typeof(BillingInfoViewModel).GetField("dummyWalletModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            orderHistoryModelField.SetValue(viewModel, mockOrderHistoryModel.Object);
            orderSummaryModelField.SetValue(viewModel, mockOrderSummaryModel.Object);
            orderModelField.SetValue(viewModel, mockOrderModel.Object);
            dummyProductModelField.SetValue(viewModel, mockDummyProductModel.Object);
            dummyWalletModelField.SetValue(viewModel, mockDummyWalletModel.Object); 

        }

        [TestMethod]
        public async Task InitializeViewModelAsync_ShouldInitializeProductList()
        {
            // Arrange
            var dummyProducts = new List<DummyProduct>
                {
                    new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "new" },
                    new DummyProduct { ID = 2, Name = "Product2", Price = 150, ProductType = "used" }
                };
            mockOrderHistoryModel.Setup(m => m.GetDummyProductsFromOrderHistoryAsync(It.IsAny<int>())).ReturnsAsync(dummyProducts);

            // Act
            await viewModel.InitializeViewModelAsync();

            // Assert
            Assert.Equals(2, viewModel.ProductList.Count);
            Assert.Equals(dummyProducts, viewModel.DummyProducts);
        }

        [TestMethod]
        public void SetVisibilityRadioButtons_ShouldSetCorrectVisibility()
        {
            // Arrange
            viewModel.ProductList = new ObservableCollection<DummyProduct>
                {
                    new DummyProduct { ProductType = "new" }
                };

            // Act
            viewModel.SetVisibilityRadioButtons();

            // Assert
            Assert.IsTrue(viewModel.IsCardEnabled);
            Assert.IsTrue(viewModel.IsCashEnabled);
            Assert.IsFalse(viewModel.IsWalletEnabled);
        }

        [TestMethod]
        public async Task OnFinalizeButtonClickedAsync_ShouldUpdateOrdersAndOrderSummary()
        {
            // Arrange
            var orders = new List<Order>
                {
                    new Order { OrderID = 1, ProductType = 1, PaymentMethod = "card" }
                };
            mockOrderModel.Setup(m => m.GetOrdersFromOrderHistoryAsync(It.IsAny<int>())).ReturnsAsync(orders);

            viewModel.SelectedPaymentMethod = "card";
            viewModel.FullName = "John Doe";
            viewModel.Email = "john.doe@example.com";
            viewModel.PhoneNumber = "1234567890";
            viewModel.Address = "123 Main St";
            viewModel.ZipCode = "12345";
            viewModel.AdditionalInfo = "None";
            viewModel.Subtotal = 100;
            viewModel.DeliveryFee = 10;
            viewModel.Total = 110;

            // Act
            await viewModel.OnFinalizeButtonClickedAsync();

            // Assert
            mockOrderModel.Verify(m => m.UpdateOrderAsync(1, 1, "card", It.IsAny<DateTime>()), Times.Once);
            mockOrderSummaryModel.Verify(m => m.UpdateOrderSummaryAsync(1, 100, 0, 10, 110, "John Doe", "john.doe@example.com", "1234567890", "123 Main St", "12345", "None", null), Times.Once);
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldCalculateCorrectTax()
        {
            // Arrange
            var dummyProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "borrowed" };
            viewModel.StartDate = new DateTime(2023, 1, 1);
            viewModel.EndDate = new DateTime(2023, 3, 1);

            // Act
            await viewModel.ApplyBorrowedTax(dummyProduct);

            // Assert
            Assert.Equals(240, dummyProduct.Price); // 100 * 2 months + 20% tax
            Assert.Equals(40, viewModel.WarrantyTax); // 20% of 200
        }
    }
}
