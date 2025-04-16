using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using ArtAttack.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class CardInfoViewModelTests
    {
        private Mock<IOrderHistoryModel> mockOrderHistoryModel;
        private Mock<IOrderSummaryModel> mockOrderSummaryModel;
        private Mock<IOrderModel> mockOrderModel;
        private Mock<IDummyCardModel> mockDummyCardModel;
        private int testOrderHistoryId;
        private List<DummyProduct> testProducts;
        private OrderSummary testOrderSummary;
        private CardInfoViewModel cardInfoViewModel;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mock objects
            mockOrderHistoryModel = new Mock<IOrderHistoryModel>();
            mockOrderSummaryModel = new Mock<IOrderSummaryModel>();
            mockOrderModel = new Mock<IOrderModel>();
            mockDummyCardModel = new Mock<IDummyCardModel>();

            // Test data
            testOrderHistoryId = 123;
            testProducts = new List<DummyProduct>
            {
                new DummyProduct { ID = 1, Name = "Test Product 1", Price = 99.99f, ProductType = "new" },
                new DummyProduct { ID = 2, Name = "Test Product 2", Price = 49.99f, ProductType = "new" }
            };
            testOrderSummary = new OrderSummary
            {
                ID = testOrderHistoryId,
                Subtotal = 149.98f,
                DeliveryFee = 13.99f,
                FinalTotal = 163.97f
            };

            // Setup mock behavior
            mockOrderHistoryModel
                .Setup(orderHistoryModel => orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(testOrderHistoryId))
                .ReturnsAsync(testProducts);

            mockOrderSummaryModel
                .Setup(orderSummaryModel => orderSummaryModel.GetOrderSummaryByIDAsync(testOrderHistoryId))
                .ReturnsAsync(testOrderSummary);

            // Create view model with mocked dependencies
            cardInfoViewModel = new CardInfoViewModel(
                mockOrderHistoryModel.Object,
                mockOrderSummaryModel.Object,
                mockOrderModel.Object,
                mockDummyCardModel.Object,
                testOrderHistoryId);
        }

        [TestMethod]
        public async Task InitializeViewModelAsync_ShouldLoadProductsAndOrderSummary()
        {
            // Arrange - Create a new view model to explicitly call Initialize
            var cardInfoViewModel = new CardInfoViewModel(
                mockOrderHistoryModel.Object,
                mockOrderSummaryModel.Object,
                mockOrderModel.Object,
                mockDummyCardModel.Object,
                testOrderHistoryId);

            // Act
            await cardInfoViewModel.InitializeViewModelAsync();

            // Assert
            Assert.IsNotNull(cardInfoViewModel.ProductList);
            Assert.AreEqual(testProducts.Count, cardInfoViewModel.ProductList.Count);
            Assert.AreEqual(testOrderSummary.Subtotal, cardInfoViewModel.Subtotal);
            Assert.AreEqual(testOrderSummary.DeliveryFee, cardInfoViewModel.DeliveryFee);
            Assert.AreEqual(testOrderSummary.FinalTotal, cardInfoViewModel.Total);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_ShouldReturnProductsFromModel()
        {
            // Act
            var result = await cardInfoViewModel.GetDummyProductsFromOrderHistoryAsync(testOrderHistoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(testProducts.Count, result.Count);
            Assert.AreEqual(testProducts[0].ID, result[0].ID);
            Assert.AreEqual(testProducts[1].ID, result[1].ID);
            mockOrderHistoryModel.Verify(orderHistoryModel => orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(testOrderHistoryId), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task ProcessCardPaymentAsync_ShouldCalculateNewBalanceAndUpdateCard()
        {
            // Arrange
            string testCardNumber = "1234-5678-9012-3456";
            float initialBalance = 500.00f;
            float expectedNewBalance = initialBalance - testOrderSummary.FinalTotal;

            mockDummyCardModel
                .Setup(dummyCardModel => dummyCardModel.GetCardBalanceAsync(testCardNumber))
                .ReturnsAsync(initialBalance);

            mockOrderSummaryModel
                .Setup(orderSummaryModel => orderSummaryModel.GetOrderSummaryByIDAsync(testOrderHistoryId))
                .ReturnsAsync(testOrderSummary);

            cardInfoViewModel.CardNumber = testCardNumber;

            // Act
            await cardInfoViewModel.ProcessCardPaymentAsync();

            // Assert
            mockDummyCardModel.Verify(dummyCardModel => dummyCardModel.GetCardBalanceAsync(testCardNumber), Times.Once);
            mockOrderSummaryModel.Verify(orderSummaryModel => orderSummaryModel.GetOrderSummaryByIDAsync(testOrderHistoryId), Times.AtLeastOnce);
            mockDummyCardModel.Verify(dummyCardModel => dummyCardModel.UpdateCardBalanceAsync(testCardNumber, expectedNewBalance), Times.Once);
        }

        [TestMethod]
        public void PropertyChanged_ShouldBeRaisedWhenPropertyValueChanges()
        {
            // Arrange
            string propertyName = null;
            cardInfoViewModel.PropertyChanged += (sender, propertyChangedEventArguments) => { propertyName = propertyChangedEventArguments.PropertyName; };

            // Act & Assert for each property
            TestPropertyChanged(nameof(cardInfoViewModel.Subtotal), () => cardInfoViewModel.Subtotal = 200.0f);
            TestPropertyChanged(nameof(cardInfoViewModel.DeliveryFee), () => cardInfoViewModel.DeliveryFee = 20.0f);
            TestPropertyChanged(nameof(cardInfoViewModel.Total), () => cardInfoViewModel.Total = 220.0f);
            TestPropertyChanged(nameof(cardInfoViewModel.Email), () => cardInfoViewModel.Email = "test@example.com");
            TestPropertyChanged(nameof(cardInfoViewModel.CardHolderName), () => cardInfoViewModel.CardHolderName = "John Doe");
            TestPropertyChanged(nameof(cardInfoViewModel.CardNumber), () => cardInfoViewModel.CardNumber = "1234-5678-9012-3456");
            TestPropertyChanged(nameof(cardInfoViewModel.CardMonth), () => cardInfoViewModel.CardMonth = "12");
            TestPropertyChanged(nameof(cardInfoViewModel.CardYear), () => cardInfoViewModel.CardYear = "25");
            TestPropertyChanged(nameof(cardInfoViewModel.CardCVC), () => cardInfoViewModel.CardCVC = "123");

            // Local function to test property changed
            void TestPropertyChanged(string expected, System.Action setProperty)
            {
                propertyName = null;
                setProperty();
                Assert.AreEqual(expected, propertyName);
            }
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullExceptionForNullDependencies()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new CardInfoViewModel(
                null,
                mockOrderSummaryModel.Object,
                mockOrderModel.Object,
                mockDummyCardModel.Object,
                testOrderHistoryId));

            Assert.ThrowsException<ArgumentNullException>(() => new CardInfoViewModel(
                mockOrderHistoryModel.Object,
                null,
                mockOrderModel.Object,
                mockDummyCardModel.Object,
                testOrderHistoryId));

            Assert.ThrowsException<ArgumentNullException>(() => new CardInfoViewModel(
                mockOrderHistoryModel.Object,
                mockOrderSummaryModel.Object,
                null,
                mockDummyCardModel.Object,
                testOrderHistoryId));

            Assert.ThrowsException<ArgumentNullException>(() => new CardInfoViewModel(
                mockOrderHistoryModel.Object,
                mockOrderSummaryModel.Object,
                mockOrderModel.Object,
                null,
                testOrderHistoryId));
        }

        [TestMethod]
        public void Constructor_ShouldInitializeProperties()
        {
            // Act - the constructor was already called in Setup

            // Assert - other properties will be tested in InitializeViewModel
            Assert.IsNotNull(cardInfoViewModel);
            Assert.IsNotNull(cardInfoViewModel.ProductList);
        }

        [TestMethod]
        public async Task ProcessCardPaymentAsync_ShouldHandleNegativeBalance()
        {
            // Arrange
            string testCardNumber = "1234-5678-9012-3456";
            float initialBalance = 100.00f; // Lower than order total
            float expectedNewBalance = initialBalance - testOrderSummary.FinalTotal; // Will be negative

            mockDummyCardModel
                .Setup(dummyCardModel => dummyCardModel.GetCardBalanceAsync(testCardNumber))
                .ReturnsAsync(initialBalance);

            mockOrderSummaryModel
                .Setup(orderSummaryModel => orderSummaryModel.GetOrderSummaryByIDAsync(testOrderHistoryId))
                .ReturnsAsync(testOrderSummary);

            cardInfoViewModel.CardNumber = testCardNumber;

            // Act
            await cardInfoViewModel.ProcessCardPaymentAsync();

            // Assert
            mockDummyCardModel.Verify(dummyCardModel => dummyCardModel.UpdateCardBalanceAsync(testCardNumber, expectedNewBalance), Times.Once);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistory_ShouldHandleEmptyList()
        {
            // Arrange
            int emptyOrderHistoryId = 999;
            mockOrderHistoryModel
                .Setup(orderSummaryModel => orderSummaryModel.GetDummyProductsFromOrderHistoryAsync(emptyOrderHistoryId))
                .ReturnsAsync(new List<DummyProduct>());

            // Act
            var result = await cardInfoViewModel.GetDummyProductsFromOrderHistoryAsync(emptyOrderHistoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        // Email property tests
        [TestMethod]
        public void GetEmail_ShouldReturnCorrectValue()
        {
            // Arrange
            string expectedEmail = "test@example.com";

            // Use reflection to set the private field directly
            var emailField = typeof(CardInfoViewModel).GetField("email",
                BindingFlags.NonPublic | BindingFlags.Instance);
            emailField.SetValue(cardInfoViewModel, expectedEmail);

            // Act
            string actualEmail = cardInfoViewModel.Email;

            // Assert
            Assert.AreEqual(expectedEmail, actualEmail);
        }

        [TestMethod]
        public void SetEmail_ShouldUpdateValueAndRaisesPropertyChanged()
        {
            // Arrange
            string expectedEmail = "new@example.com";
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            cardInfoViewModel.PropertyChanged += (sender, propertyChangedEventArguments) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = propertyChangedEventArguments.PropertyName;
            };

            // Act
            cardInfoViewModel.Email = expectedEmail;

            // Assert
            Assert.AreEqual(expectedEmail, cardInfoViewModel.Email);
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(cardInfoViewModel.Email), changedPropertyName);
        }

        // CardHolderName property tests
        [TestMethod]
        public void GetCardHolderName_ShouldReturnCorrectValue()
        {
            // Arrange
            string expectedName = "John Doe";

            // Use reflection to set the private field directly
            var cardholderField = typeof(CardInfoViewModel).GetField("cardholder",
                BindingFlags.NonPublic | BindingFlags.Instance);
            cardholderField.SetValue(cardInfoViewModel, expectedName);

            // Act
            string actualName = cardInfoViewModel.CardHolderName;

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [TestMethod]
        public void SetCardHolderName_ShouldUpdateValueAndRaisesPropertyChanged()
        {
            // Arrange
            string expectedName = "Jane Smith";
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            cardInfoViewModel.PropertyChanged += (sender, propertyChangedEventArguments) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = propertyChangedEventArguments.PropertyName;
            };

            // Act
            cardInfoViewModel.CardHolderName = expectedName;

            // Assert
            Assert.AreEqual(expectedName, cardInfoViewModel.CardHolderName);
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(cardInfoViewModel.CardHolderName), changedPropertyName);
        }

        // CardNumber property tests
        [TestMethod]
        public void GetCardNumber_ShouldReturnCorrectValue()
        {
            // Arrange
            string expectedCardNumber = "1234-5678-9012-3456";

            // Use reflection to set the private field directly
            var cardnumberField = typeof(CardInfoViewModel).GetField("cardnumber",
                BindingFlags.NonPublic | BindingFlags.Instance);
            cardnumberField.SetValue(cardInfoViewModel, expectedCardNumber);

            // Act
            string actualCardNumber = cardInfoViewModel.CardNumber;

            // Assert
            Assert.AreEqual(expectedCardNumber, actualCardNumber);
        }

        [TestMethod]
        public void SetCardNumber_ShouldUpdateValueAndRaisesPropertyChanged()
        {
            // Arrange
            string expectedCardNumber = "9876-5432-1098-7654";
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            cardInfoViewModel.PropertyChanged += (sender, propertyChangedEventArguments) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = propertyChangedEventArguments.PropertyName;
            };

            // Act
            cardInfoViewModel.CardNumber = expectedCardNumber;

            // Assert
            Assert.AreEqual(expectedCardNumber, cardInfoViewModel.CardNumber);
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(cardInfoViewModel.CardNumber), changedPropertyName);
        }

        // CardMonth property tests
        [TestMethod]
        public void GetCardMonth_ShouldReturnCorrectValue()
        {
            // Arrange
            string expectedMonth = "06";

            // Use reflection to set the private field directly
            var cardMonthField = typeof(CardInfoViewModel).GetField("cardMonth",
                BindingFlags.NonPublic | BindingFlags.Instance);
            cardMonthField.SetValue(cardInfoViewModel, expectedMonth);

            // Act
            string actualMonth = cardInfoViewModel.CardMonth;

            // Assert
            Assert.AreEqual(expectedMonth, actualMonth);
        }

        [TestMethod]
        public void SetCardMonth_ShouldUpdateValueAndRaisesPropertyChanged()
        {
            // Arrange
            string expectedMonth = "12";
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            cardInfoViewModel.PropertyChanged += (sender, propertyChangedEventArguments) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = propertyChangedEventArguments.PropertyName;
            };

            // Act
            cardInfoViewModel.CardMonth = expectedMonth;

            // Assert
            Assert.AreEqual(expectedMonth, cardInfoViewModel.CardMonth);
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(cardInfoViewModel.CardMonth), changedPropertyName);
        }

        // CardYear property tests
        [TestMethod]
        public void GetCardYear_ShouldReturnCorrectValue()
        {
            // Arrange
            string expectedYear = "2025";

            // Use reflection to set the private field directly
            var cardYearField = typeof(CardInfoViewModel).GetField("cardYear",
                BindingFlags.NonPublic | BindingFlags.Instance);
            cardYearField.SetValue(cardInfoViewModel, expectedYear);

            // Act
            string actualYear = cardInfoViewModel.CardYear;

            // Assert
            Assert.AreEqual(expectedYear, actualYear);
        }

        [TestMethod]
        public void SetCardYear_ShouldUpdatesValueAndRaisesPropertyChanged()
        {
            // Arrange
            string expectedYear = "2028";
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            cardInfoViewModel.PropertyChanged += (sender, propertyChangedEventArguments) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = propertyChangedEventArguments.PropertyName;
            };

            // Act
            cardInfoViewModel.CardYear = expectedYear;

            // Assert
            Assert.AreEqual(expectedYear, cardInfoViewModel.CardYear);
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(cardInfoViewModel.CardYear), changedPropertyName);
        }

        // CardCVC property tests
        [TestMethod]
        public void GetCardCVC_ShouldReturnsCorrectValue()
        {
            // Arrange
            string expectedCardCvc = "123";

            // Use reflection to set the private field directly
            var cardCVCField = typeof(CardInfoViewModel).GetField("cardCVC",
                BindingFlags.NonPublic | BindingFlags.Instance);
            cardCVCField.SetValue(cardInfoViewModel, expectedCardCvc);

            // Act
            string actualCardCvc = cardInfoViewModel.CardCVC;

            // Assert
            Assert.AreEqual(expectedCardCvc, actualCardCvc);
        }

        [TestMethod]
        public void SetCardCVC_ShouldUpdateValueAndRaisesPropertyChanged()
        {
            // Arrange
            string expectedCardCvc = "456";
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            cardInfoViewModel.PropertyChanged += (sender, propertyChangedEventArguments) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = propertyChangedEventArguments.PropertyName;
            };

            // Act
            cardInfoViewModel.CardCVC = expectedCardCvc;

            // Assert
            Assert.AreEqual(expectedCardCvc, cardInfoViewModel.CardCVC);
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(cardInfoViewModel.CardCVC), changedPropertyName);
        }

        [TestMethod]
        public void SetToNullEmail_ShouldBeNull()
        {
            cardInfoViewModel.Email = null;
            Assert.IsNull(cardInfoViewModel.Email);
        }

        [TestMethod]
        public void SetToNullCardHolderName_ShouldBeNull()
        {
            cardInfoViewModel.CardHolderName = null;
            Assert.IsNull(cardInfoViewModel.CardHolderName);
        }

        [TestMethod]
        public void SetToNullCardNumber_ShouldBeNull()
        {
            cardInfoViewModel.CardNumber = null;
            Assert.IsNull(cardInfoViewModel.CardNumber);
        }

        [TestMethod]
        public void SetToNullCardMonth_ShouldBeNull()
        {
            cardInfoViewModel.CardMonth = null;
            Assert.IsNull(cardInfoViewModel.CardMonth);
        }

        [TestMethod]
        public void SetToNullCardYear_ShouldBeNull()
        {
            cardInfoViewModel.CardYear = null;
            Assert.IsNull(cardInfoViewModel.CardYear);
        }

        [TestMethod]
        public void SetToNullCardCVC_ShouldBeNull()
        {
            cardInfoViewModel.CardCVC = null;
            Assert.IsNull(cardInfoViewModel.CardCVC);
        }

        [TestMethod]
        public void SetToEmptyStringEmail_ShouldBeEmpty()
        {
            cardInfoViewModel.Email = string.Empty;
            Assert.AreEqual(string.Empty, cardInfoViewModel.Email);
        }

        [TestMethod]
        public void SetToEmptyStringCardHolderName_ShouldBeEmpty()
        {
            cardInfoViewModel.CardHolderName = string.Empty;
            Assert.AreEqual(string.Empty, cardInfoViewModel.CardHolderName);
        }

        [TestMethod]
        public void SetToEmptyStringCardNumber_ShouldBeEmpty()
        {
            cardInfoViewModel.CardNumber = string.Empty;
            Assert.AreEqual(string.Empty, cardInfoViewModel.CardNumber);
        }

        [TestMethod]
        public void SetToEmptyStringCardMonth_ShouldBeEmpty()
        {
            cardInfoViewModel.CardMonth = string.Empty;
            Assert.AreEqual(string.Empty, cardInfoViewModel.CardMonth);
        }

        [TestMethod]
        public void SetToEmptyStringCardYear_ShouldBeEmpty()
        {
            cardInfoViewModel.CardYear = string.Empty;
            Assert.AreEqual(string.Empty, cardInfoViewModel.CardYear);
        }

        [TestMethod]
        public void SetToEmptyStringCardCVC_ShouldBeEmpty()
        {
            cardInfoViewModel.CardCVC = string.Empty;
            Assert.AreEqual(string.Empty, cardInfoViewModel.CardCVC);
        }

        [TestMethod]
        // [ExcludeFromCodeCoverage] // Marking this test as excluded from coverage because we're testing UI-dependent code
        public async Task OnPayButtonClickedAsync_ShouldCallProcessCardPayment()
        {
            // Arrange
            // Create a mock view model that will track whether ProcessCardPaymentAsync was called
            var mockOrderHistoryModel = new Mock<IOrderHistoryModel>();
            var mockOrderSummaryModel = new Mock<IOrderSummaryModel>();
            var mockOrderModel = new Mock<IOrderModel>();
            var mockDummyCardModel = new Mock<IDummyCardModel>();

            bool processCardPaymentCalled = false;

            // Setup the mock view model
            var viewModel = new CardInfoViewModel(
                mockOrderHistoryModel.Object,
                mockOrderSummaryModel.Object,
                mockOrderModel.Object,
                mockDummyCardModel.Object,
                123);

            // Use reflection to create a way to track if ProcessCardPaymentAsync was called
            var originalMethod = typeof(CardInfoViewModel).GetMethod("ProcessCardPaymentAsync",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            mockDummyCardModel
                .Setup(dummyCardModel => dummyCardModel.GetCardBalanceAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Test marker exception"));

            viewModel.CardNumber = "test-card-number";

            try
            {
                // Act - this will throw an exception from GetCardBalanceAsync
                await viewModel.OnPayButtonClickedAsync();

                // If we get here, it means ProcessCardPayment was not called or didn't throw
                Assert.Fail("Expected exception was not thrown");
            }
            catch (InvalidOperationException ex) when (ex.Message == "Test marker exception")
            {
                // Assert - if we catch our specific exception, it means ProcessCardPayment was called
                // This confirms that part of the method worked correctly
                Assert.IsTrue(true, "ProcessCardPaymentAsync was called as expected");
            }
            catch (Exception ex)
            {
                // Any other exception is a test failure
                Assert.Fail($"Unexpected exception: {ex}");
            }
        }
    }
}
