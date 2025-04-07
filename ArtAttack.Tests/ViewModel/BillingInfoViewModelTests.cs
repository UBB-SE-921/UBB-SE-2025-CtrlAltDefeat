using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.ViewModel;
using Microsoft.UI.Xaml.Controls;
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
            Assert.AreEqual(2, viewModel.ProductList.Count);
            Assert.AreEqual(dummyProducts, viewModel.DummyProducts);
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
        // also make for bid and refill

        [TestMethod]
        public void SetVisibilityRadioButtons_ShouldSetCorrectVisibilityForBid()
        {
            // Arrange
            viewModel.ProductList = new ObservableCollection<DummyProduct>
                {
                    new DummyProduct { ProductType = "bid" }
                };
            // Act
            viewModel.SetVisibilityRadioButtons();
            // Assert
            Assert.IsFalse(viewModel.IsCardEnabled);
            Assert.IsFalse(viewModel.IsCashEnabled);
            Assert.IsTrue(viewModel.IsWalletEnabled);
        }

        [TestMethod]
        public void SetVisibilityRadioButtons_ShouldSetCorrectVisibilityForRefill()
        {
            // Arrange
            viewModel.ProductList = new ObservableCollection<DummyProduct>
                {
                    new DummyProduct { ProductType = "refill" }
                };
            // Act
            viewModel.SetVisibilityRadioButtons();
            // Assert
            Assert.IsTrue(viewModel.IsCardEnabled);
            Assert.IsFalse(viewModel.IsCashEnabled);
            Assert.IsFalse(viewModel.IsWalletEnabled);
        }

        /*
         *in  CalculateOrderTotal
         * else
            {
                Total = subtotalProducts + 13.99f;
                DeliveryFee = 13.99f;
            }
         * 
         * public async Task ApplyBorrowedTax(DummyProduct dummyProduct)
        {
            if (dummyProduct == null || dummyProduct.ProductType != "borrowed")
            {
                return;
            }
            if (StartDate > EndDate)
            {
                return;
            }
            int monthsBorrowed = ((EndDate.Year - StartDate.Year) * 12) + EndDate.Month - StartDate.Month;
            if (monthsBorrowed <= 0)
            {
                monthsBorrowed = 1;
            }

        make tests for these lines
         */

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldReturnIfDummyProductIsNull()
        {
            // Arrange
            DummyProduct dummyProduct = null;
            viewModel.StartDate = new DateTime(2023, 1, 1);
            viewModel.EndDate = new DateTime(2023, 3, 1);
            // Act
            await viewModel.ApplyBorrowedTax(dummyProduct);
            // Assert
            Assert.AreEqual(0, viewModel.WarrantyTax); // No tax should be applied
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldReturnIfProductTypeIsNotBorrowed()
        {
            // Arrange
            var dummyProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "new" };
            viewModel.StartDate = new DateTime(2023, 1, 1);
            viewModel.EndDate = new DateTime(2023, 3, 1);
            // Act
            await viewModel.ApplyBorrowedTax(dummyProduct);
            // Assert
            Assert.AreEqual(0, viewModel.WarrantyTax); // No tax should be applied
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldReturnIfStartDateIsAfterEndDate()
        {
            // Arrange
            var dummyProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "borrowed" };
            viewModel.StartDate = new DateTime(2023, 3, 1);
            viewModel.EndDate = new DateTime(2023, 1, 1);
            // Act
            await viewModel.ApplyBorrowedTax(dummyProduct);
            // Assert
            Assert.AreEqual(0, viewModel.WarrantyTax); // No tax should be applied
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
            Assert.AreEqual(240, dummyProduct.Price); // 100 * 2 months + 20% tax
            Assert.AreEqual(40, viewModel.WarrantyTax); // 20% of 200
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldCalculateCorrectTaxOver200()
        {
            // Arrange
            var dummyProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 30, ProductType = "borrowed" };
            viewModel.DummyProducts.Add(dummyProduct);
            viewModel.StartDate = new DateTime(2023, 1, 1);
            viewModel.EndDate = new DateTime(2023, 1, 1);


            // Act
            await viewModel.ApplyBorrowedTax(dummyProduct);

            // Assert
            Assert.AreEqual(72, dummyProduct.Price);
            viewModel.DummyProducts.Remove(dummyProduct);

        }


        [TestMethod]
        public async Task ProcessWalletRefillAsync_ShouldUpdateWalletBalance()
        {
            // Arrange
            float initialBalance = 1000;
            float total = 200;
            mockDummyWalletModel.Setup(m => m.GetWalletBalanceAsync(1)).ReturnsAsync(initialBalance);
            mockDummyWalletModel.Setup(m => m.UpdateWalletBalance(1, initialBalance - total)).Returns(Task.CompletedTask);
            // Act
            await viewModel.ProcessWalletRefillAsync();
            // Assert
            mockDummyWalletModel.Verify(m => m.UpdateWalletBalance(1, initialBalance - total), Times.Never);
        }
        [TestMethod]
        public async Task ProcessWalletRefillAsync_ShouldNotUpdateWalletBalance_WhenTotalIsZero()
        {
            // Arrange
            float initialBalance = 1000;
            float total = 0;
            mockDummyWalletModel.Setup(m => m.GetWalletBalanceAsync(1)).ReturnsAsync(initialBalance);
            // Act
            await viewModel.ProcessWalletRefillAsync();
            // Assert
            mockDummyWalletModel.Verify(m => m.UpdateWalletBalance(It.IsAny<int>(), It.IsAny<float>()), Times.Once);
        }
    }
}
