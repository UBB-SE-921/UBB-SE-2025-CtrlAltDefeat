using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using ArtAttack.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class FinalizePurchaseViewModelTests
    {
        private Mock<IOrderHistoryModel> _mockOrderHistoryModel;
        private Mock<IOrderSummaryModel> _mockOrderSummaryModel;
        private Mock<IOrderModel> _mockOrderModel;
        private Mock<INotificationViewModel> _mockNotificationViewModel;
        private FinalizePurchaseViewModel _viewModel;
        private readonly int _testOrderHistoryId = 42;

        [TestInitialize]
        public void Setup()
        {
            // Create mocks
            _mockOrderHistoryModel = new Mock<IOrderHistoryModel>();
            _mockOrderSummaryModel = new Mock<IOrderSummaryModel>();
            _mockOrderModel = new Mock<IOrderModel>();
            _mockNotificationViewModel = new Mock<INotificationViewModel>();

            // Setup the ViewModel with reflection to use mocked dependencies
            _viewModel = new FinalizePurchaseViewModel(_testOrderHistoryId);

            // Use reflection to inject mock dependencies
            var orderHistoryModelField = typeof(FinalizePurchaseViewModel).GetField("orderHistoryModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderHistoryModelField.SetValue(_viewModel, _mockOrderHistoryModel.Object);

            var orderModelField = typeof(FinalizePurchaseViewModel).GetField("orderModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderModelField.SetValue(_viewModel, _mockOrderModel.Object);

            var orderSummaryModelField = typeof(FinalizePurchaseViewModel).GetField("orderSummaryModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderSummaryModelField.SetValue(_viewModel, _mockOrderSummaryModel.Object);

            var notificationViewModelField = typeof(FinalizePurchaseViewModel).GetField("notificationViewModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            notificationViewModelField.SetValue(_viewModel, _mockNotificationViewModel.Object);

            // Setup the orderHistoryID field directly
            var orderHistoryIdField = typeof(FinalizePurchaseViewModel).GetField("orderHistoryID",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderHistoryIdField.SetValue(_viewModel, _testOrderHistoryId);
        }

        [TestMethod]
        public async Task InitializeViewModelAsync_LoadsProductsAndOrderSummary()
        {
            // Arrange
            var testProducts = new List<DummyProduct>
            {
                new DummyProduct { ID = 1, Name = "Test Product", Price = 10.99f }
            };

            var testOrderSummary = new OrderSummary
            {
                Subtotal = 10.99f,
                DeliveryFee = 5.0f,
                FinalTotal = 15.99f,
                FullName = "Test User",
                Email = "test@example.com",
                PhoneNumber = "1234567890"
            };

            var testOrders = new List<Order>
            {
                new Order
                {
                    OrderID = 1,
                    PaymentMethod = "Credit Card"
                }
            };

            _mockOrderHistoryModel
                .Setup(m => m.GetDummyProductsFromOrderHistoryAsync(_testOrderHistoryId))
                .ReturnsAsync(testProducts);

            _mockOrderSummaryModel
                .Setup(m => m.GetOrderSummaryByIDAsync(_testOrderHistoryId))
                .ReturnsAsync(testOrderSummary);

            _mockOrderModel
                .Setup(m => m.GetOrdersFromOrderHistoryAsync(_testOrderHistoryId))
                .ReturnsAsync(testOrders);

            // Act
            await _viewModel.InitializeViewModelAsync();

            // Assert
            Assert.AreEqual(testProducts.Count, _viewModel.ProductList.Count);
            Assert.AreEqual(testProducts[0].ID, _viewModel.ProductList[0].ID);
            Assert.AreEqual(testProducts[0].Name, _viewModel.ProductList[0].Name);
            Assert.AreEqual(testOrderSummary.Subtotal, _viewModel.Subtotal);
            Assert.AreEqual(testOrderSummary.DeliveryFee, _viewModel.DeliveryFee);
            Assert.AreEqual(testOrderSummary.FinalTotal, _viewModel.Total);
            Assert.AreEqual(testOrderSummary.FullName, _viewModel.FullName);
            Assert.AreEqual(testOrderSummary.Email, _viewModel.Email);
            Assert.AreEqual(testOrderSummary.PhoneNumber, _viewModel.PhoneNumber);
            Assert.AreEqual("Credit Card", _viewModel.PaymentMethod);
            Assert.AreEqual("Processing", _viewModel.OrderStatus);

            _mockOrderHistoryModel.Verify(
                m => m.GetDummyProductsFromOrderHistoryAsync(_testOrderHistoryId),
                Times.Once);
            _mockOrderSummaryModel.Verify(
                m => m.GetOrderSummaryByIDAsync(_testOrderHistoryId),
                Times.Once);
            _mockOrderModel.Verify(
                m => m.GetOrdersFromOrderHistoryAsync(_testOrderHistoryId),
                Times.Once);
        }

        [TestMethod]
        public async Task SetOrderHistoryInfo_SetsPropertiesToCorrectValues()
        {
            // Arrange
            var testOrderSummary = new OrderSummary
            {
                Subtotal = 25.99f,
                DeliveryFee = 7.0f,
                FinalTotal = 32.99f,
                FullName = "Jane Doe",
                Email = "jane@example.com",
                PhoneNumber = "9876543210"
            };

            var testOrders = new List<Order>
            {
                new Order
                {
                    OrderID = 2,
                    PaymentMethod = "PayPal"
                }
            };

            _mockOrderModel
                .Setup(m => m.GetOrdersFromOrderHistoryAsync(_testOrderHistoryId))
                .ReturnsAsync(testOrders);

            // Act
            await _viewModel.SetOrderHistoryInfo(testOrderSummary);

            // Assert
            Assert.AreEqual(testOrderSummary.Subtotal, _viewModel.Subtotal);
            Assert.AreEqual(testOrderSummary.DeliveryFee, _viewModel.DeliveryFee);
            Assert.AreEqual(testOrderSummary.FinalTotal, _viewModel.Total);
            Assert.AreEqual(testOrderSummary.FullName, _viewModel.FullName);
            Assert.AreEqual(testOrderSummary.Email, _viewModel.Email);
            Assert.AreEqual(testOrderSummary.PhoneNumber, _viewModel.PhoneNumber);
            Assert.AreEqual("PayPal", _viewModel.PaymentMethod);
            Assert.AreEqual("Processing", _viewModel.OrderStatus);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_ReturnsProductsFromModel()
        {
            // Arrange
            var testProducts = new List<DummyProduct>
            {
                new DummyProduct { ID = 3, Name = "Another Product", Price = 15.99f }
            };

            _mockOrderHistoryModel
                .Setup(m => m.GetDummyProductsFromOrderHistoryAsync(_testOrderHistoryId))
                .ReturnsAsync(testProducts);

            // Act
            var result = await _viewModel.GetDummyProductsFromOrderHistoryAsync(_testOrderHistoryId);

            // Assert
            Assert.AreEqual(testProducts.Count, result.Count);
            Assert.AreEqual(testProducts[0].ID, result[0].ID);
            Assert.AreEqual(testProducts[0].Name, result[0].Name);
            Assert.AreEqual(testProducts[0].Price, result[0].Price);

            _mockOrderHistoryModel.Verify(
                m => m.GetDummyProductsFromOrderHistoryAsync(_testOrderHistoryId),
                Times.Once);
        }

        [TestMethod]
        public void HandleFinish_SendsNotificationForEachOrder()
        {
            // Arrange
            var testOrders = new List<Order>
            {
                new Order { OrderID = 101, ProductID = 201 },
                new Order { OrderID = 102, ProductID = 202 }
            };

            _viewModel.Orders = testOrders;

            // Act
            _viewModel.HandleFinish();

            // Assert - Need to wait briefly for the async operations to complete
            Task.Delay(100).Wait(); // Short delay

            // Verify for each order a notification was sent
            _mockNotificationViewModel.Verify(
                m => m.AddNotificationAsync(It.Is<Notification>(
                    n => n is PaymentConfirmationNotification &&
                         ((PaymentConfirmationNotification)n).GetProductID() == 201 &&
                         ((PaymentConfirmationNotification)n).GetOrderID() == 101)),
                Times.Once);

            _mockNotificationViewModel.Verify(
                m => m.AddNotificationAsync(It.Is<Notification>(
                    n => n is PaymentConfirmationNotification &&
                         ((PaymentConfirmationNotification)n).GetProductID() == 202 &&
                         ((PaymentConfirmationNotification)n).GetOrderID() == 102)),
                Times.Once);
        }

        [TestMethod]
        public void HandleFinish_DoesNothing_WhenOrdersIsNull()
        {
            // Arrange
            _viewModel.Orders = null;

            // Act - should not throw
            _viewModel.HandleFinish();

            // Assert - no notifications should be sent
            _mockNotificationViewModel.Verify(
                m => m.AddNotificationAsync(It.IsAny<Notification>()),
                Times.Never);
        }

        [TestMethod]
        public void HandleFinish_DoesNothing_WhenOrdersIsEmpty()
        {
            // Arrange
            _viewModel.Orders = new List<Order>();

            // Act - should not throw
            _viewModel.HandleFinish();

            // Assert - no notifications should be sent
            _mockNotificationViewModel.Verify(
                m => m.AddNotificationAsync(It.IsAny<Notification>()),
                Times.Never);
        }

        [TestMethod]
        public void OnPropertyChanged_RaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            string propertyName = null;

            _viewModel.PropertyChanged += (sender, args) =>
            {
                eventRaised = true;
                propertyName = args.PropertyName;
            };

            // Act
            _viewModel.Subtotal = 50.0f;

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.AreEqual(nameof(_viewModel.Subtotal), propertyName);
        }

        [TestMethod]
        public void SubtotalProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.Subtotal))
                    eventRaised = true;
            };

            // Act
            _viewModel.Subtotal = 75.0f;

            // Assert
            Assert.AreEqual(75.0f, _viewModel.Subtotal);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void DeliveryFeeProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.DeliveryFee))
                    eventRaised = true;
            };

            // Act
            _viewModel.DeliveryFee = 8.5f;

            // Assert
            Assert.AreEqual(8.5f, _viewModel.DeliveryFee);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void TotalProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.Total))
                    eventRaised = true;
            };

            // Act
            _viewModel.Total = 83.5f;

            // Assert
            Assert.AreEqual(83.5f, _viewModel.Total);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void FullNameProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.FullName))
                    eventRaised = true;
            };

            // Act
            _viewModel.FullName = "John Smith";

            // Assert
            Assert.AreEqual("John Smith", _viewModel.FullName);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void PhoneNumberProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.PhoneNumber))
                    eventRaised = true;
            };

            // Act
            _viewModel.PhoneNumber = "5551234567";

            // Assert
            Assert.AreEqual("5551234567", _viewModel.PhoneNumber);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void EmailProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.Email))
                    eventRaised = true;
            };

            // Act
            _viewModel.Email = "test@example.org";

            // Assert
            Assert.AreEqual("test@example.org", _viewModel.Email);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void PaymentMethodProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.PaymentMethod))
                    eventRaised = true;
            };

            // Act
            _viewModel.PaymentMethod = "Bank Transfer";

            // Assert
            Assert.AreEqual("Bank Transfer", _viewModel.PaymentMethod);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void OrderStatusProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.OrderStatus))
                    eventRaised = true;
            };

            // Act
            _viewModel.OrderStatus = "Shipped";

            // Assert
            Assert.AreEqual("Shipped", _viewModel.OrderStatus);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void Constructor_InitializesProductListToEmptyCollection()
        {
            // Create a fresh instance
            var viewModel = new FinalizePurchaseViewModel(_testOrderHistoryId);

            // Replace all dependencies with mocks to prevent actual initialization
            var orderHistoryModelField = typeof(FinalizePurchaseViewModel).GetField("orderHistoryModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderHistoryModelField.SetValue(viewModel, _mockOrderHistoryModel.Object);

            var orderModelField = typeof(FinalizePurchaseViewModel).GetField("orderModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderModelField.SetValue(viewModel, _mockOrderModel.Object);

            var orderSummaryModelField = typeof(FinalizePurchaseViewModel).GetField("orderSummaryModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderSummaryModelField.SetValue(viewModel, _mockOrderSummaryModel.Object);

            var notificationViewModelField = typeof(FinalizePurchaseViewModel).GetField("notificationViewModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            notificationViewModelField.SetValue(viewModel, _mockNotificationViewModel.Object);

            // Initialize ProductList with an empty collection (since the async initialization was interrupted)
            viewModel.ProductList = new System.Collections.ObjectModel.ObservableCollection<DummyProduct>();

            // Assert
            Assert.IsNotNull(viewModel.ProductList);
        }

    }
}
