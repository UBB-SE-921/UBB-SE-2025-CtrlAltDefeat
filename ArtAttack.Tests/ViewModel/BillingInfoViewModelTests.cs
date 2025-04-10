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
    public class MockOrderHistoryModel : IOrderHistoryModel
    {
        private List<DummyProduct> productsToReturn;
        private int calledWithUserId;

        public void SetupGetDummyProductsReturn(List<DummyProduct> productsToReturn)
        {
            this.productsToReturn = productsToReturn;
        }

        public Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int userID)
        {
            calledWithUserId = userID;
            return Task.FromResult(productsToReturn);
        }

        public int GetCalledUserId()
        {
            return calledWithUserId;
        }

        internal void SetupDummyProducts(List<DummyProduct> products)
        {
            throw new NotImplementedException();
        }

        internal int GetReceivedOrderHistoryId()
        {
            throw new NotImplementedException();
        }

        internal void Verify(Func<object, object> value, Func<Times> atLeastOnce)
        {
            throw new NotImplementedException();
        }

        internal void VerifyGetDummyProductsFromOrderHistoryAsync(int testOrderHistoryId)
        {
            throw new NotImplementedException();
        }
    }

    public class MockDummyWalletModel : IDummyWalletModel
    {
        private float balanceToReturn;
        private int updateWalletBalanceCalls;
        private int lastUserIdUsed;
        private float lastBalanceUsed;

        public void SetupGetWalletBalanceReturn(float balanceToReturn)
        {
            this.balanceToReturn = balanceToReturn;
        }

        public Task<float> GetWalletBalanceAsync(int userID)
        {
            lastUserIdUsed = userID;
            return Task.FromResult(balanceToReturn);
        }

        public Task UpdateWalletBalance(int userID, float updatedBalance)
        {
            updateWalletBalanceCalls++;
            lastUserIdUsed = userID;
            lastBalanceUsed = updatedBalance;
            return Task.CompletedTask;
        }

        public int GetUpdateWalletBalanceCalls()
        {
            return updateWalletBalanceCalls;
        }

        public int GetLastUserIdUsed()
        {
            return lastUserIdUsed;
        }

        public float GetLastBalanceUsed()
        {
            return lastBalanceUsed;
        }
    }

    [TestClass]
    public class BillingInfoViewModelTests
    {
        private MockOrderHistoryModel mockOrderHistoryModel;
        private MockDummyWalletModel mockDummyWalletModel;
        private BillingInfoViewModel billingInfoViewModel;
        private const int TEST_USER_ID = 1;

        [TestInitialize]
        public void Setup()
        {
            mockOrderHistoryModel = new MockOrderHistoryModel();
            mockDummyWalletModel = new MockDummyWalletModel();

            billingInfoViewModel = new BillingInfoViewModel(TEST_USER_ID);

            var orderHistoryModelField = typeof(BillingInfoViewModel).GetField("orderHistoryModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dummyWalletModelField = typeof(BillingInfoViewModel).GetField("dummyWalletModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            orderHistoryModelField.SetValue(billingInfoViewModel, mockOrderHistoryModel);
            dummyWalletModelField.SetValue(billingInfoViewModel, mockDummyWalletModel);
        }

        [TestMethod]
        public async Task InitializeViewModelAsync_ShouldInitializeProductList()
        {
            // Arrange
            var testDummyProducts = new List<DummyProduct>
            {
                new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "new" },
                new DummyProduct { ID = 2, Name = "Product2", Price = 150, ProductType = "used" }
            };

            mockOrderHistoryModel.SetupGetDummyProductsReturn(testDummyProducts);

            // Act
            await billingInfoViewModel.InitializeViewModelAsync();

            // Assert
            Assert.AreEqual(2, billingInfoViewModel.ProductList.Count);
            Assert.AreEqual(testDummyProducts, billingInfoViewModel.DummyProducts);
            Assert.AreEqual(TEST_USER_ID, mockOrderHistoryModel.GetCalledUserId());
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
            DummyProduct nullDummyProduct = null;
            billingInfoViewModel.StartDate = new DateTime(2023, 1, 1);
            billingInfoViewModel.EndDate = new DateTime(2023, 3, 1);

            // Act
            await billingInfoViewModel.ApplyBorrowedTax(nullDummyProduct);

            // Assert
            Assert.AreEqual(0, billingInfoViewModel.WarrantyTax); // No tax should be applied
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldReturnIfProductTypeIsNotBorrowed()
        {
            // Arrange
            var newProductType = new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "new" };
            billingInfoViewModel.StartDate = new DateTime(2023, 1, 1);
            billingInfoViewModel.EndDate = new DateTime(2023, 3, 1);

            // Act
            await billingInfoViewModel.ApplyBorrowedTax(newProductType);

            // Assert
            Assert.AreEqual(0, billingInfoViewModel.WarrantyTax); // No tax should be applied
        }

        [TestMethod]
        public async Task ApplyBorrowedTaxSellerIdNull_ShouldReturnIfProductTypeIsNotBorrowed()
        {
            // Arrange
            var nonBorrowedProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "new" };
            billingInfoViewModel.StartDate = new DateTime(2023, 1, 1);
            billingInfoViewModel.EndDate = new DateTime(2023, 3, 1);

            // Act
            await billingInfoViewModel.ApplyBorrowedTax(nonBorrowedProduct);

            // Assert
            Assert.AreEqual(0, billingInfoViewModel.WarrantyTax); // No tax should be applied
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldReturnIfStartDateIsAfterEndDate()
        {
            // Arrange
            var borrowedProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "borrowed" };
            billingInfoViewModel.StartDate = new DateTime(2023, 3, 1);
            billingInfoViewModel.EndDate = new DateTime(2023, 1, 1);

            // Act
            await billingInfoViewModel.ApplyBorrowedTax(borrowedProduct);

            // Assert
            Assert.AreEqual(0, billingInfoViewModel.WarrantyTax); // No tax should be applied
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldCalculateCorrectTax()
        {
            // Arrange
            var borrowedProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 100, ProductType = "borrowed", SellerID = 1 };
            billingInfoViewModel.StartDate = new DateTime(2023, 1, 1);
            billingInfoViewModel.EndDate = new DateTime(2023, 3, 1);

            // Act
            await billingInfoViewModel.ApplyBorrowedTax(borrowedProduct);

            // Assert
            Assert.AreEqual(240, borrowedProduct.Price); // 100 * 2 months + 20% tax
            Assert.AreEqual(40, billingInfoViewModel.WarrantyTax); // 20% of 200
        }

        [TestMethod]
        public async Task ApplyBorrowedTax_ShouldCalculateCorrectTaxOver200()
        {
            // Arrange
            var lowPriceBorrowedProduct = new DummyProduct { ID = 1, Name = "Product1", Price = 30, ProductType = "borrowed", SellerID = 1 };
            billingInfoViewModel.DummyProducts.Add(lowPriceBorrowedProduct);
            billingInfoViewModel.StartDate = new DateTime(2023, 1, 1);
            billingInfoViewModel.EndDate = new DateTime(2023, 1, 1);

            // Act
            await billingInfoViewModel.ApplyBorrowedTax(lowPriceBorrowedProduct);

            // Assert
            Assert.AreEqual(36, lowPriceBorrowedProduct.Price);
            billingInfoViewModel.DummyProducts.Remove(lowPriceBorrowedProduct);
        }

        [TestMethod]
        public async Task ProcessWalletRefillAsync_ShouldUpdateWalletBalance()
        {
            // Arrange
            float initialWalletBalance = 1000;
            float totalAmount = 200;

            mockDummyWalletModel.SetupGetWalletBalanceReturn(initialWalletBalance);

            // Act
            await billingInfoViewModel.ProcessWalletRefillAsync();

            // Assert
            Assert.AreEqual(1, mockDummyWalletModel.GetUpdateWalletBalanceCalls());
        }

        [TestMethod]
        public async Task ProcessWalletRefillAsync_ShouldNotUpdateWalletBalance()
        {
            // Arrange
            float initialWalletBalance = 1000;
            float zeroTotal = 0;

            mockDummyWalletModel.SetupGetWalletBalanceReturn(initialWalletBalance);

            // Act
            await billingInfoViewModel.ProcessWalletRefillAsync();

            // Assert
            Assert.AreEqual(1, mockDummyWalletModel.GetUpdateWalletBalanceCalls());
            Assert.AreEqual(TEST_USER_ID, mockDummyWalletModel.GetLastUserIdUsed());
        }
    }
}
