using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.ViewModel;
using Microsoft.UI.Xaml.Controls;
using Moq;
using Xunit;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class BillingInfoViewModelTests
    {
        private Mock<IOrderHistoryModel> mockOrderHistoryModel;
        private Mock<IOrderSummaryModel> mockOrderSummaryModel;
        private Mock<IOrderModel> mockOrderModel;
        private Mock<IDummyProductModel> mockDummyProductModel;
        private Mock<IDummyWalletModel> mockDummyWalletModel;
        private BillingInfoViewModel billingInfoViewModel;

        [TestInitialize]
        public void Setup()
        {
            mockOrderHistoryModel = new Mock<IOrderHistoryModel>();
            mockOrderSummaryModel = new Mock<IOrderSummaryModel>();
            mockOrderModel = new Mock<IOrderModel>();
            mockDummyProductModel = new Mock<IDummyProductModel>();
            mockDummyWalletModel = new Mock<IDummyWalletModel>();

            billingInfoViewModel = new BillingInfoViewModel(1);
            var orderHistoryModelField = typeof(BillingInfoViewModel).GetField("orderHistoryModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var orderSummaryModelField = typeof(BillingInfoViewModel).GetField("orderSummaryModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var orderModelField = typeof(BillingInfoViewModel).GetField("orderModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dummyProductModelField = typeof(BillingInfoViewModel).GetField("dummyProductModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dummyWalletModelField = typeof(BillingInfoViewModel).GetField("dummyWalletModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            orderHistoryModelField.SetValue(billingInfoViewModel, mockOrderHistoryModel.Object);
            orderSummaryModelField.SetValue(billingInfoViewModel, mockOrderSummaryModel.Object);
            orderModelField.SetValue(billingInfoViewModel, mockOrderModel.Object);
            dummyProductModelField.SetValue(billingInfoViewModel, mockDummyProductModel.Object);
            dummyWalletModelField.SetValue(billingInfoViewModel, mockDummyWalletModel.Object);
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
            mockOrderHistoryModel.Setup(orderHistoryModel => orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(It.IsAny<int>())).ReturnsAsync(dummyProducts);

            // Act
            await billingInfoViewModel.InitializeViewModelAsync();

            // Assert
            Assert.AreEqual(2, billingInfoViewModel.ProductList.Count);
            Assert.AreEqual(dummyProducts, billingInfoViewModel.DummyProducts);
        }

        [TestMethod]
        public void SetVisibilityRadioButtons_ShouldSetCorrectVisibility()
        {
            // Arrange
            billingInfoViewModel.ProductList = new ObservableCollection<DummyProduct>
                {
                    new DummyProduct { ProductType = "new" }
                };

            // Act
            billingInfoViewModel.SetVisibilityRadioButtons();

            // Assert
            Assert.IsTrue(billingInfoViewModel.IsCardEnabled);
            Assert.IsTrue(billingInfoViewModel.IsCashEnabled);
            Assert.IsFalse(billingInfoViewModel.IsWalletEnabled);
        }

        [TestMethod]
        public void SetVisibilityRadioButtons_ShouldSetCorrectVisibilityForBid()
        {
            // Arrange
            billingInfoViewModel.ProductList = new ObservableCollection<DummyProduct>
                {
                    new DummyProduct { ProductType = "bid" }
                };
            // Act
            billingInfoViewModel.SetVisibilityRadioButtons();
            // Assert
            Assert.IsFalse(billingInfoViewModel.IsCardEnabled);
            Assert.IsFalse(billingInfoViewModel.IsCashEnabled);
            Assert.IsTrue(billingInfoViewModel.IsWalletEnabled);
        }

        [TestMethod]
        public void SetVisibilityRadioButtons_ShouldSetCorrectVisibilityForRefill()
        {
            // Arrange
            billingInfoViewModel.ProductList = new ObservableCollection<DummyProduct>
                {
                    new DummyProduct { ProductType = "refill" }
                };
            // Act
            billingInfoViewModel.SetVisibilityRadioButtons();
            // Assert
            Assert.IsTrue(billingInfoViewModel.IsCardEnabled);
            Assert.IsFalse(billingInfoViewModel.IsCashEnabled);
            Assert.IsFalse(billingInfoViewModel.IsWalletEnabled);
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldReturnIfDummyProductIsNull()
        {
            // Arrange
            DummyProduct dummyProduct = null;
            billingInfoViewModel.StartDate = new DateTime(2023, 1, 1);
            billingInfoViewModel.EndDate = new DateTime(2023, 3, 1);
            // Act
            await billingInfoViewModel.ApplyBorrowedTax(dummyProduct);
            // Assert
            Assert.AreEqual(0, billingInfoViewModel.WarrantyTax); // No tax should be applied
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldReturnIfProductTypeIsNotBorrowed()
        {
            // Arrange
            var dummyProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "new" };
            billingInfoViewModel.StartDate = new DateTime(2023, 1, 1);
            billingInfoViewModel.EndDate = new DateTime(2023, 3, 1);
            // Act
            await billingInfoViewModel.ApplyBorrowedTax(dummyProduct);
            // Assert
            Assert.AreEqual(0, billingInfoViewModel.WarrantyTax); // No tax should be applied
        }

        [TestMethod]
        public async Task ApplyBorrowedTaxSellerIdNull_ShouldReturnIfProductTypeIsNotBorrowed()
        {
            // Arrange
            var dummyProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "new" };
            billingInfoViewModel.StartDate = new DateTime(2023, 1, 1);
            billingInfoViewModel.EndDate = new DateTime(2023, 3, 1);
            // Act
            await billingInfoViewModel.ApplyBorrowedTax(dummyProduct);
            // Assert
            Assert.AreEqual(0, billingInfoViewModel.WarrantyTax); // No tax should be applied
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldReturnIfStartDateIsAfterEndDate()
        {
            // Arrange
            var dummyProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "borrowed" };
            billingInfoViewModel.StartDate = new DateTime(2023, 3, 1);
            billingInfoViewModel.EndDate = new DateTime(2023, 1, 1);
            // Act
            await billingInfoViewModel.ApplyBorrowedTax(dummyProduct);
            // Assert
            Assert.AreEqual(0, billingInfoViewModel.WarrantyTax); // No tax should be applied
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldCalculateCorrectTax()
        {
            // Arrange
            var dummyProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "borrowed" };
            billingInfoViewModel.StartDate = new DateTime(2023, 1, 1);
            billingInfoViewModel.EndDate = new DateTime(2023, 3, 1);

            // Act
            await billingInfoViewModel.ApplyBorrowedTax(dummyProduct);

            // Assert
            Assert.AreEqual(240, dummyProduct.Price); // 100 * 2 months + 20% tax
            Assert.AreEqual(40, billingInfoViewModel.WarrantyTax); // 20% of 200
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldCalculateCorrectTaxOver200()
        {
            // Arrange
            var dummyProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 30, ProductType = "borrowed" };
            billingInfoViewModel.DummyProducts.Add(dummyProduct);
            billingInfoViewModel.StartDate = new DateTime(2023, 1, 1);
            billingInfoViewModel.EndDate = new DateTime(2023, 1, 1);

            // Act
            await billingInfoViewModel.ApplyBorrowedTax(dummyProduct);

            // Assert
            Assert.AreEqual(36, dummyProduct.Price);
            billingInfoViewModel.DummyProducts.Remove(dummyProduct);
        }

        [TestMethod]
        public async Task ProcessWalletRefillAsync_ShouldUpdateWalletBalance()
        {
            // Arrange
            float initialBalance = 1000;
            float total = 200;
            mockDummyWalletModel.Setup(dummyWalletModel => dummyWalletModel.GetWalletBalanceAsync(1)).ReturnsAsync(initialBalance);
            mockDummyWalletModel.Setup(dummyWalletModel => dummyWalletModel.UpdateWalletBalance(1, initialBalance - total)).Returns(Task.CompletedTask);
            // Act
            await billingInfoViewModel.ProcessWalletRefillAsync();
            // Assert
            mockDummyWalletModel.Verify(dummyWalletModel => dummyWalletModel.UpdateWalletBalance(1, initialBalance - total), Times.Never);
        }

        [TestMethod]
        public async Task ProcessWalletRefillAsync_ShouldNotUpdateWalletBalance_WhenTotalIsZero()
        {
            // Arrange
            float initialBalance = 1000;
            float total = 0;
            mockDummyWalletModel.Setup(dummyWalletModel => dummyWalletModel.GetWalletBalanceAsync(1)).ReturnsAsync(initialBalance);
            // Act
            await billingInfoViewModel.ProcessWalletRefillAsync();
            // Assert
            mockDummyWalletModel.Verify(dummyWalletModel => dummyWalletModel.UpdateWalletBalance(It.IsAny<int>(), It.IsAny<float>()), Times.Once);
        }
    }
}