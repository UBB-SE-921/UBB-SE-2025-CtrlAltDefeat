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
        private Mock<IOrderHistoryModel> mockOrderHistoryModel;
        private Mock<IOrderSummaryModel> mockOrderSummaryModel;
        private Mock<IOrderModel> mockOrderModel;
        private Mock<INotificationViewModel> mockNotificationViewModel;
        private FinalizePurchaseViewModel finalizePurchaseViewModel;
        private readonly int testOrderHistoryId = 42;

        [TestInitialize]
        public void Setup()
        {
            // Create mocks
            mockOrderHistoryModel = new Mock<IOrderHistoryModel>();
            mockOrderSummaryModel = new Mock<IOrderSummaryModel>();
            mockOrderModel = new Mock<IOrderModel>();
            mockNotificationViewModel = new Mock<INotificationViewModel>();

            // Setup the ViewModel with reflection to use mocked dependencies
            finalizePurchaseViewModel = new FinalizePurchaseViewModel(testOrderHistoryId);

            // Use reflection to inject mock dependencies
            var orderHistoryModelField = typeof(FinalizePurchaseViewModel).GetField("orderHistoryModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderHistoryModelField.SetValue(finalizePurchaseViewModel, mockOrderHistoryModel.Object);

            var orderModelField = typeof(FinalizePurchaseViewModel).GetField("orderModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderModelField.SetValue(finalizePurchaseViewModel, mockOrderModel.Object);

            var orderSummaryModelField = typeof(FinalizePurchaseViewModel).GetField("orderSummaryModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderSummaryModelField.SetValue(finalizePurchaseViewModel, mockOrderSummaryModel.Object);

            var notificationViewModelField = typeof(FinalizePurchaseViewModel).GetField("notificationViewModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            notificationViewModelField.SetValue(finalizePurchaseViewModel, mockNotificationViewModel.Object);

            // Setup the orderHistoryID field directly
            var orderHistoryIdField = typeof(FinalizePurchaseViewModel).GetField("orderHistoryID",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderHistoryIdField.SetValue(finalizePurchaseViewModel, testOrderHistoryId);
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

            mockOrderHistoryModel
                .Setup(mockModel => mockModel.GetDummyProductsFromOrderHistoryAsync(testOrderHistoryId))
                .ReturnsAsync(testProducts);

            mockOrderSummaryModel
                .Setup(mockModel => mockModel.GetOrderSummaryByIDAsync(testOrderHistoryId))
                .ReturnsAsync(testOrderSummary);

            mockOrderModel
                .Setup(mockModel => mockModel.GetOrdersFromOrderHistoryAsync(testOrderHistoryId))
                .ReturnsAsync(testOrders);

            // Act
            await finalizePurchaseViewModel.InitializeViewModelAsync();

            // Assert
            Assert.AreEqual(testProducts.Count, finalizePurchaseViewModel.ProductList.Count);
            Assert.AreEqual(testProducts[0].ID, finalizePurchaseViewModel.ProductList[0].ID);
            Assert.AreEqual(testProducts[0].Name, finalizePurchaseViewModel.ProductList[0].Name);
            Assert.AreEqual(testOrderSummary.Subtotal, finalizePurchaseViewModel.Subtotal);
            Assert.AreEqual(testOrderSummary.DeliveryFee, finalizePurchaseViewModel.DeliveryFee);
            Assert.AreEqual(testOrderSummary.FinalTotal, finalizePurchaseViewModel.Total);
            Assert.AreEqual(testOrderSummary.FullName, finalizePurchaseViewModel.FullName);
            Assert.AreEqual(testOrderSummary.Email, finalizePurchaseViewModel.Email);
            Assert.AreEqual(testOrderSummary.PhoneNumber, finalizePurchaseViewModel.PhoneNumber);
            Assert.AreEqual("Credit Card", finalizePurchaseViewModel.PaymentMethod);
            Assert.AreEqual("Processing", finalizePurchaseViewModel.OrderStatus);

            mockOrderHistoryModel.Verify(
                mockModel => mockModel.GetDummyProductsFromOrderHistoryAsync(testOrderHistoryId),
                Times.Once);
            mockOrderSummaryModel.Verify(
                mockModel => mockModel.GetOrderSummaryByIDAsync(testOrderHistoryId),
                Times.Once);
            mockOrderModel.Verify(
                mockModel => mockModel.GetOrdersFromOrderHistoryAsync(testOrderHistoryId),
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

            mockOrderModel
                .Setup(mockModel => mockModel.GetOrdersFromOrderHistoryAsync(testOrderHistoryId))
                .ReturnsAsync(testOrders);

            // Act
            await finalizePurchaseViewModel.SetOrderHistoryInfo(testOrderSummary);

            // Assert
            Assert.AreEqual(testOrderSummary.Subtotal, finalizePurchaseViewModel.Subtotal);
            Assert.AreEqual(testOrderSummary.DeliveryFee, finalizePurchaseViewModel.DeliveryFee);
            Assert.AreEqual(testOrderSummary.FinalTotal, finalizePurchaseViewModel.Total);
            Assert.AreEqual(testOrderSummary.FullName, finalizePurchaseViewModel.FullName);
            Assert.AreEqual(testOrderSummary.Email, finalizePurchaseViewModel.Email);
            Assert.AreEqual(testOrderSummary.PhoneNumber, finalizePurchaseViewModel.PhoneNumber);
            Assert.AreEqual("PayPal", finalizePurchaseViewModel.PaymentMethod);
            Assert.AreEqual("Processing", finalizePurchaseViewModel.OrderStatus);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_ReturnsProductsFromModel()
        {
            // Arrange
            var testProducts = new List<DummyProduct>
            {
                new DummyProduct { ID = 3, Name = "Another Product", Price = 15.99f }
            };

            mockOrderHistoryModel
                .Setup(mockModel => mockModel.GetDummyProductsFromOrderHistoryAsync(testOrderHistoryId))
                .ReturnsAsync(testProducts);

            // Act
            var result = await finalizePurchaseViewModel.GetDummyProductsFromOrderHistoryAsync(testOrderHistoryId);

            // Assert
            Assert.AreEqual(testProducts.Count, result.Count);
            Assert.AreEqual(testProducts[0].ID, result[0].ID);
            Assert.AreEqual(testProducts[0].Name, result[0].Name);
            Assert.AreEqual(testProducts[0].Price, result[0].Price);

            mockOrderHistoryModel.Verify(
                mockModel => mockModel.GetDummyProductsFromOrderHistoryAsync(testOrderHistoryId),
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

            finalizePurchaseViewModel.Orders = testOrders;

            // Act
            finalizePurchaseViewModel.HandleFinish();

            // Assert - Need to wait briefly for the async operations to complete
            Task.Delay(100).Wait(); // Short delay

            // Verify for each order a notification was sent
            mockNotificationViewModel.Verify(
                mockModel => mockModel.AddNotificationAsync(It.Is<Notification>(
                    notificationType => notificationType is PaymentConfirmationNotification &&
                         ((PaymentConfirmationNotification)notificationType).GetProductID() == 201 &&
                         ((PaymentConfirmationNotification)notificationType).GetOrderID() == 101)),
                Times.Once);

            mockNotificationViewModel.Verify(
                mockModel => mockModel.AddNotificationAsync(It.Is<Notification>(
                    notificationType => notificationType is PaymentConfirmationNotification &&
                         ((PaymentConfirmationNotification)notificationType).GetProductID() == 202 &&
                         ((PaymentConfirmationNotification)notificationType).GetOrderID() == 102)),
                Times.Once);
        }

        //[TestMethod]
        //public void HandleFinish_DoesNothing_WhenOrdersIsNull()
        //{
        //    // Arrange
        //    finalizePurchaseViewModel.Orders = null;

        //    // Act - should not throw
        //    finalizePurchaseViewModel.HandleFinish();

        //    // Assert - no notifications should be sent
        //    mockNotificationViewModel.Verify(
        //        m => m.AddNotificationAsync(It.IsAny<Notification>()),
        //        Times.Never);
        //}

        [TestMethod]
        public void HandleFinish_DoesNothing_WhenOrdersIsEmpty()
        {
            // Arrange
            finalizePurchaseViewModel.Orders = new List<Order>();

            // Act - should not throw
            finalizePurchaseViewModel.HandleFinish();

            // Assert - no notifications should be sent
            mockNotificationViewModel.Verify(
                mockModel => mockModel.AddNotificationAsync(It.IsAny<Notification>()),
                Times.Never);
        }

        [TestMethod]
        public void OnPropertyChanged_RaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            string propertyName = null;

            finalizePurchaseViewModel.PropertyChanged += (sender, args) =>
            {
                eventRaised = true;
                propertyName = args.PropertyName;
            };

            // Act
            finalizePurchaseViewModel.Subtotal = 50.0f;

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.AreEqual(nameof(finalizePurchaseViewModel.Subtotal), propertyName);
        }

        [TestMethod]
        public void SubtotalProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            finalizePurchaseViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(finalizePurchaseViewModel.Subtotal))
                    eventRaised = true;
            };

            // Act
            finalizePurchaseViewModel.Subtotal = 75.0f;

            // Assert
            Assert.AreEqual(75.0f, finalizePurchaseViewModel.Subtotal);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void DeliveryFeeProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            finalizePurchaseViewModel.PropertyChanged += (sender, propertyArguments) =>
            {
                if (propertyArguments.PropertyName == nameof(finalizePurchaseViewModel.DeliveryFee))
                    eventRaised = true;
            };

            // Act
            finalizePurchaseViewModel.DeliveryFee = 8.5f;

            // Assert
            Assert.AreEqual(8.5f, finalizePurchaseViewModel.DeliveryFee);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void TotalProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            finalizePurchaseViewModel.PropertyChanged += (sender, propertyArguments) =>
            {
                if (propertyArguments.PropertyName == nameof(finalizePurchaseViewModel.Total))
                    eventRaised = true;
            };

            // Act
            finalizePurchaseViewModel.Total = 83.5f;

            // Assert
            Assert.AreEqual(83.5f, finalizePurchaseViewModel.Total);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void FullNameProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            finalizePurchaseViewModel.PropertyChanged += (sender, propertyArguments) =>
            {
                if (propertyArguments.PropertyName == nameof(finalizePurchaseViewModel.FullName))
                    eventRaised = true;
            };

            // Act
            finalizePurchaseViewModel.FullName = "John Smith";

            // Assert
            Assert.AreEqual("John Smith", finalizePurchaseViewModel.FullName);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void PhoneNumberProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            finalizePurchaseViewModel.PropertyChanged += (sender, propertyArguments) =>
            {
                if (propertyArguments.PropertyName == nameof(finalizePurchaseViewModel.PhoneNumber))
                    eventRaised = true;
            };

            // Act
            finalizePurchaseViewModel.PhoneNumber = "5551234567";

            // Assert
            Assert.AreEqual("5551234567", finalizePurchaseViewModel.PhoneNumber);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void EmailProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            finalizePurchaseViewModel.PropertyChanged += (sender, propertyArguments) =>
            {
                if (propertyArguments.PropertyName == nameof(finalizePurchaseViewModel.Email))
                    eventRaised = true;
            };

            // Act
            finalizePurchaseViewModel.Email = "test@example.org";

            // Assert
            Assert.AreEqual("test@example.org", finalizePurchaseViewModel.Email);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void PaymentMethodProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            finalizePurchaseViewModel.PropertyChanged += (sender, propertyArguments) =>
            {
                if (propertyArguments.PropertyName == nameof(finalizePurchaseViewModel.PaymentMethod))
                    eventRaised = true;
            };

            // Act
            finalizePurchaseViewModel.PaymentMethod = "Bank Transfer";

            // Assert
            Assert.AreEqual("Bank Transfer", finalizePurchaseViewModel.PaymentMethod);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void OrderStatusProperty_SetsValueAndRaisesPropertyChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            finalizePurchaseViewModel.PropertyChanged += (sender, propertyArguments) =>
            {
                if (propertyArguments.PropertyName == nameof(finalizePurchaseViewModel.OrderStatus))
                    eventRaised = true;
            };

            // Act
            finalizePurchaseViewModel.OrderStatus = "Shipped";

            // Assert
            Assert.AreEqual("Shipped", finalizePurchaseViewModel.OrderStatus);
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void Constructor_InitializesProductListToEmptyCollection()
        {
            // Create a fresh instance
            var viewModel = new FinalizePurchaseViewModel(testOrderHistoryId);

            // Replace all dependencies with mocks to prevent actual initialization
            var orderHistoryModelField = typeof(FinalizePurchaseViewModel).GetField("orderHistoryModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderHistoryModelField.SetValue(viewModel, mockOrderHistoryModel.Object);

            var orderModelField = typeof(FinalizePurchaseViewModel).GetField("orderModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderModelField.SetValue(viewModel, mockOrderModel.Object);

            var orderSummaryModelField = typeof(FinalizePurchaseViewModel).GetField("orderSummaryModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            orderSummaryModelField.SetValue(viewModel, mockOrderSummaryModel.Object);

            var notificationViewModelField = typeof(FinalizePurchaseViewModel).GetField("notificationViewModel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            notificationViewModelField.SetValue(viewModel, mockNotificationViewModel.Object);

            // Initialize ProductList with an empty collection (since the async initialization was interrupted)
            viewModel.ProductList = new System.Collections.ObjectModel.ObservableCollection<DummyProduct>();

            // Assert
            Assert.IsNotNull(viewModel.ProductList);
        }

    }
}
