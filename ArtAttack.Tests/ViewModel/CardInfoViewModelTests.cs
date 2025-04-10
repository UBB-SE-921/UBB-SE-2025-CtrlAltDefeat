using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using ArtAttack.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class CardInfoViewModelTests
    {
        private Mock<IOrderHistoryModel> _mockOrderHistoryModel;
        private Mock<IOrderSummaryModel> _mockOrderSummaryModel;
        private Mock<IOrderModel> _mockOrderModel;
        private Mock<IDummyCardModel> _mockDummyCardModel;
        private int _testOrderHistoryId;
        private List<DummyProduct> _testProducts;
        private OrderSummary _testOrderSummary;
        private CardInfoViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mock objects
            _mockOrderHistoryModel = new Mock<IOrderHistoryModel>();
            _mockOrderSummaryModel = new Mock<IOrderSummaryModel>();
            _mockOrderModel = new Mock<IOrderModel>();
            _mockDummyCardModel = new Mock<IDummyCardModel>();

            // Test data
            _testOrderHistoryId = 123;
            _testProducts = new List<DummyProduct>
            {
                new DummyProduct { ID = 1, Name = "Test Product 1", Price = 99.99f, ProductType = "new" },
                new DummyProduct { ID = 2, Name = "Test Product 2", Price = 49.99f, ProductType = "new" }
            };
            _testOrderSummary = new OrderSummary
            {
                ID = _testOrderHistoryId,
                Subtotal = 149.98f,
                DeliveryFee = 13.99f,
                FinalTotal = 163.97f
            };

            // Setup mock behavior
            _mockOrderHistoryModel
                .Setup(m => m.GetDummyProductsFromOrderHistoryAsync(_testOrderHistoryId))
                .ReturnsAsync(_testProducts);

            _mockOrderSummaryModel
                .Setup(m => m.GetOrderSummaryByIDAsync(_testOrderHistoryId))
                .ReturnsAsync(_testOrderSummary);

            // Create view model with mocked dependencies
            _viewModel = new CardInfoViewModel(
                _mockOrderHistoryModel.Object,
                _mockOrderSummaryModel.Object,
                _mockOrderModel.Object,
                _mockDummyCardModel.Object,
                _testOrderHistoryId);
        }

        [TestMethod]
        public async Task InitializeViewModelAsync_ShouldLoadProductsAndOrderSummary()
        {
            // Arrange - Create a new view model to explicitly call Initialize
            var viewModel = new CardInfoViewModel(
                _mockOrderHistoryModel.Object,
                _mockOrderSummaryModel.Object,
                _mockOrderModel.Object,
                _mockDummyCardModel.Object,
                _testOrderHistoryId);

            // Act
            await viewModel.InitializeViewModelAsync();

            // Assert
            Assert.IsNotNull(viewModel.ProductList);
            Assert.AreEqual(_testProducts.Count, viewModel.ProductList.Count);
            Assert.AreEqual(_testOrderSummary.Subtotal, viewModel.Subtotal);
            Assert.AreEqual(_testOrderSummary.DeliveryFee, viewModel.DeliveryFee);
            Assert.AreEqual(_testOrderSummary.FinalTotal, viewModel.Total);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_ShouldReturnProductsFromModel()
        {
            // Act
            var result = await _viewModel.GetDummyProductsFromOrderHistoryAsync(_testOrderHistoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_testProducts.Count, result.Count);
            Assert.AreEqual(_testProducts[0].ID, result[0].ID);
            Assert.AreEqual(_testProducts[1].ID, result[1].ID);
            _mockOrderHistoryModel.Verify(m => m.GetDummyProductsFromOrderHistoryAsync(_testOrderHistoryId), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task ProcessCardPaymentAsync_ShouldCalculateNewBalanceAndUpdateCard()
        {
            // Arrange
            string testCardNumber = "1234-5678-9012-3456";
            float initialBalance = 500.00f;
            float expectedNewBalance = initialBalance - _testOrderSummary.FinalTotal;

            _mockDummyCardModel
                .Setup(m => m.GetCardBalanceAsync(testCardNumber))
                .ReturnsAsync(initialBalance);

            _mockOrderSummaryModel
                .Setup(m => m.GetOrderSummaryByIDAsync(_testOrderHistoryId))
                .ReturnsAsync(_testOrderSummary);

            _viewModel.CardNumber = testCardNumber;

            // Act
            await _viewModel.ProcessCardPaymentAsync();

            // Assert
            _mockDummyCardModel.Verify(m => m.GetCardBalanceAsync(testCardNumber), Times.Once);
            _mockOrderSummaryModel.Verify(m => m.GetOrderSummaryByIDAsync(_testOrderHistoryId), Times.AtLeastOnce);
            _mockDummyCardModel.Verify(m => m.UpdateCardBalanceAsync(testCardNumber, expectedNewBalance), Times.Once);
        }

        [TestMethod]
        public void PropertyChanged_ShouldBeRaisedWhenPropertyValueChanges()
        {
            // Arrange
            string propertyName = null;
            _viewModel.PropertyChanged += (sender, e) => { propertyName = e.PropertyName; };

            // Act & Assert for each property
            TestPropertyChanged(nameof(_viewModel.Subtotal), () => _viewModel.Subtotal = 200.0f);
            TestPropertyChanged(nameof(_viewModel.DeliveryFee), () => _viewModel.DeliveryFee = 20.0f);
            TestPropertyChanged(nameof(_viewModel.Total), () => _viewModel.Total = 220.0f);
            TestPropertyChanged(nameof(_viewModel.Email), () => _viewModel.Email = "test@example.com");
            TestPropertyChanged(nameof(_viewModel.CardHolderName), () => _viewModel.CardHolderName = "John Doe");
            TestPropertyChanged(nameof(_viewModel.CardNumber), () => _viewModel.CardNumber = "1234-5678-9012-3456");
            TestPropertyChanged(nameof(_viewModel.CardMonth), () => _viewModel.CardMonth = "12");
            TestPropertyChanged(nameof(_viewModel.CardYear), () => _viewModel.CardYear = "25");
            TestPropertyChanged(nameof(_viewModel.CardCVC), () => _viewModel.CardCVC = "123");

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
                _mockOrderSummaryModel.Object,
                _mockOrderModel.Object,
                _mockDummyCardModel.Object,
                _testOrderHistoryId));

            Assert.ThrowsException<ArgumentNullException>(() => new CardInfoViewModel(
                _mockOrderHistoryModel.Object,
                null,
                _mockOrderModel.Object,
                _mockDummyCardModel.Object,
                _testOrderHistoryId));

            Assert.ThrowsException<ArgumentNullException>(() => new CardInfoViewModel(
                _mockOrderHistoryModel.Object,
                _mockOrderSummaryModel.Object,
                null,
                _mockDummyCardModel.Object,
                _testOrderHistoryId));

            Assert.ThrowsException<ArgumentNullException>(() => new CardInfoViewModel(
                _mockOrderHistoryModel.Object,
                _mockOrderSummaryModel.Object,
                _mockOrderModel.Object,
                null,
                _testOrderHistoryId));
        }

        [TestMethod]
        public void Constructor_ShouldInitializeProperties()
        {
            // Act - the constructor was already called in Setup

            // Assert - other properties will be tested in InitializeViewModel
            Assert.IsNotNull(_viewModel);
            Assert.IsNotNull(_viewModel.ProductList);
        }

        [TestMethod]
        public async Task ProcessCardPaymentAsync_ShouldHandleNegativeBalance()
        {
            // Arrange
            string testCardNumber = "1234-5678-9012-3456";
            float initialBalance = 100.00f; // Lower than order total
            float expectedNewBalance = initialBalance - _testOrderSummary.FinalTotal; // Will be negative

            _mockDummyCardModel
                .Setup(m => m.GetCardBalanceAsync(testCardNumber))
                .ReturnsAsync(initialBalance);

            _mockOrderSummaryModel
                .Setup(m => m.GetOrderSummaryByIDAsync(_testOrderHistoryId))
                .ReturnsAsync(_testOrderSummary);

            _viewModel.CardNumber = testCardNumber;

            // Act
            await _viewModel.ProcessCardPaymentAsync();

            // Assert
            _mockDummyCardModel.Verify(m => m.UpdateCardBalanceAsync(testCardNumber, expectedNewBalance), Times.Once);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistory_ShouldHandleEmptyList()
        {
            // Arrange
            int emptyOrderHistoryId = 999;
            _mockOrderHistoryModel
                .Setup(m => m.GetDummyProductsFromOrderHistoryAsync(emptyOrderHistoryId))
                .ReturnsAsync(new List<DummyProduct>());

            // Act
            var result = await _viewModel.GetDummyProductsFromOrderHistoryAsync(emptyOrderHistoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        // Email property tests
        [TestMethod]
        public void Email_Get_ReturnsCorrectValue()
        {
            // Arrange
            string expectedEmail = "test@example.com";

            // Use reflection to set the private field directly
            var emailField = typeof(CardInfoViewModel).GetField("email",
                BindingFlags.NonPublic | BindingFlags.Instance);
            emailField.SetValue(_viewModel, expectedEmail);

            // Act
            string actualEmail = _viewModel.Email;

            // Assert
            Assert.AreEqual(expectedEmail, actualEmail);
        }

        [TestMethod]
        public void Email_Set_UpdatesValueAndRaisesPropertyChanged()
        {
            // Arrange
            string expectedEmail = "new@example.com";
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            _viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            };

            // Act
            _viewModel.Email = expectedEmail;

            // Assert
            Assert.AreEqual(expectedEmail, _viewModel.Email);
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(_viewModel.Email), changedPropertyName);
        }

        // CardHolderName property tests
        [TestMethod]
        public void CardHolderName_Get_ReturnsCorrectValue()
        {
            // Arrange
            string expectedName = "John Doe";

            // Use reflection to set the private field directly
            var cardholderField = typeof(CardInfoViewModel).GetField("cardholder",
                BindingFlags.NonPublic | BindingFlags.Instance);
            cardholderField.SetValue(_viewModel, expectedName);

            // Act
            string actualName = _viewModel.CardHolderName;

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [TestMethod]
        public void CardHolderName_Set_UpdatesValueAndRaisesPropertyChanged()
        {
            // Arrange
            string expectedName = "Jane Smith";
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            _viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            };

            // Act
            _viewModel.CardHolderName = expectedName;

            // Assert
            Assert.AreEqual(expectedName, _viewModel.CardHolderName);
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(_viewModel.CardHolderName), changedPropertyName);
        }

        // CardNumber property tests
        [TestMethod]
        public void CardNumber_Get_ReturnsCorrectValue()
        {
            // Arrange
            string expectedCardNumber = "1234-5678-9012-3456";

            // Use reflection to set the private field directly
            var cardnumberField = typeof(CardInfoViewModel).GetField("cardnumber",
                BindingFlags.NonPublic | BindingFlags.Instance);
            cardnumberField.SetValue(_viewModel, expectedCardNumber);

            // Act
            string actualCardNumber = _viewModel.CardNumber;

            // Assert
            Assert.AreEqual(expectedCardNumber, actualCardNumber);
        }

        [TestMethod]
        public void CardNumber_Set_UpdatesValueAndRaisesPropertyChanged()
        {
            // Arrange
            string expectedCardNumber = "9876-5432-1098-7654";
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            _viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            };

            // Act
            _viewModel.CardNumber = expectedCardNumber;

            // Assert
            Assert.AreEqual(expectedCardNumber, _viewModel.CardNumber);
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(_viewModel.CardNumber), changedPropertyName);
        }

        // CardMonth property tests
        [TestMethod]
        public void CardMonth_Get_ReturnsCorrectValue()
        {
            // Arrange
            string expectedMonth = "06";

            // Use reflection to set the private field directly
            var cardMonthField = typeof(CardInfoViewModel).GetField("cardMonth",
                BindingFlags.NonPublic | BindingFlags.Instance);
            cardMonthField.SetValue(_viewModel, expectedMonth);

            // Act
            string actualMonth = _viewModel.CardMonth;

            // Assert
            Assert.AreEqual(expectedMonth, actualMonth);
        }

        [TestMethod]
        public void CardMonth_Set_UpdatesValueAndRaisesPropertyChanged()
        {
            // Arrange
            string expectedMonth = "12";
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            _viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            };

            // Act
            _viewModel.CardMonth = expectedMonth;

            // Assert
            Assert.AreEqual(expectedMonth, _viewModel.CardMonth);
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(_viewModel.CardMonth), changedPropertyName);
        }

        // CardYear property tests
        [TestMethod]
        public void CardYear_Get_ReturnsCorrectValue()
        {
            // Arrange
            string expectedYear = "2025";

            // Use reflection to set the private field directly
            var cardYearField = typeof(CardInfoViewModel).GetField("cardYear",
                BindingFlags.NonPublic | BindingFlags.Instance);
            cardYearField.SetValue(_viewModel, expectedYear);

            // Act
            string actualYear = _viewModel.CardYear;

            // Assert
            Assert.AreEqual(expectedYear, actualYear);
        }

        [TestMethod]
        public void CardYear_Set_UpdatesValueAndRaisesPropertyChanged()
        {
            // Arrange
            string expectedYear = "2028";
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            _viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            };

            // Act
            _viewModel.CardYear = expectedYear;

            // Assert
            Assert.AreEqual(expectedYear, _viewModel.CardYear);
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(_viewModel.CardYear), changedPropertyName);
        }

        // CardCVC property tests
        [TestMethod]
        public void CardCVC_Get_ReturnsCorrectValue()
        {
            // Arrange
            string expectedCvc = "123";

            // Use reflection to set the private field directly
            var cardCVCField = typeof(CardInfoViewModel).GetField("cardCVC",
                BindingFlags.NonPublic | BindingFlags.Instance);
            cardCVCField.SetValue(_viewModel, expectedCvc);

            // Act
            string actualCvc = _viewModel.CardCVC;

            // Assert
            Assert.AreEqual(expectedCvc, actualCvc);
        }

        [TestMethod]
        public void CardCVC_Set_UpdatesValueAndRaisesPropertyChanged()
        {
            // Arrange
            string expectedCvc = "456";
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            _viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            };

            // Act
            _viewModel.CardCVC = expectedCvc;

            // Assert
            Assert.AreEqual(expectedCvc, _viewModel.CardCVC);
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(_viewModel.CardCVC), changedPropertyName);
        }

        // Tests for edge cases
        [TestMethod]
        public void Properties_SetNullValues_HandlesCorrectly()
        {
            // Act
            _viewModel.Email = null;
            _viewModel.CardHolderName = null;
            _viewModel.CardNumber = null;
            _viewModel.CardMonth = null;
            _viewModel.CardYear = null;
            _viewModel.CardCVC = null;

            // Assert
            Assert.IsNull(_viewModel.Email);
            Assert.IsNull(_viewModel.CardHolderName);
            Assert.IsNull(_viewModel.CardNumber);
            Assert.IsNull(_viewModel.CardMonth);
            Assert.IsNull(_viewModel.CardYear);
            Assert.IsNull(_viewModel.CardCVC);
        }

        [TestMethod]
        public void Properties_SetEmptyStrings_HandlesCorrectly()
        {
            // Act
            _viewModel.Email = string.Empty;
            _viewModel.CardHolderName = string.Empty;
            _viewModel.CardNumber = string.Empty;
            _viewModel.CardMonth = string.Empty;
            _viewModel.CardYear = string.Empty;
            _viewModel.CardCVC = string.Empty;

            // Assert
            Assert.AreEqual(string.Empty, _viewModel.Email);
            Assert.AreEqual(string.Empty, _viewModel.CardHolderName);
            Assert.AreEqual(string.Empty, _viewModel.CardNumber);
            Assert.AreEqual(string.Empty, _viewModel.CardMonth);
            Assert.AreEqual(string.Empty, _viewModel.CardYear);
            Assert.AreEqual(string.Empty, _viewModel.CardCVC);
        }

        [TestMethod]
        //[ExcludeFromCodeCoverage] // Marking this test as excluded from coverage because we're testing UI-dependent code
        public async Task OnPayButtonClickedAsync_CallsProcessCardPayment()
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

            // The issue is we can't easily replace ProcessCardPaymentAsync. 
            // In a real test, we'd introduce abstraction to make this testable.

            // Instead, let's try to use a structured exception approach
            mockDummyCardModel
                .Setup(m => m.GetCardBalanceAsync(It.IsAny<string>()))
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

            // Note: We can't easily verify the UI part (BillingInfoWindow creation and activation)
            // without introducing abstraction or using a UI automation framework
        }

    }
}
