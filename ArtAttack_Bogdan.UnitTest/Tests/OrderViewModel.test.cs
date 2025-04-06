using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using ArtAttack.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtAttack.Tests
{
    [TestClass]
    public class OrderViewModelTests
    {
        private Mock<IOrderModel> _mockOrderModel;
        private Mock<IOrderSummaryService> _mockOrderSummaryService;
        private OrderViewModel _orderViewModel;
        private const string ConnectionString = "test_connection_string";
        private const int ALL_ORDERS_DEFAULT_BUYER_ID = 0;

        [TestInitialize]
        public void TestInitialize()
        {
            // Create mocks for dependencies
            _mockOrderModel = new Mock<IOrderModel>();
            _mockOrderSummaryService = new Mock<IOrderSummaryService>();

            // Create the view model with mocked dependencies
            _orderViewModel = new OrderViewModel(_mockOrderModel.Object, _mockOrderSummaryService.Object);
        }

        [TestMethod]
        public async Task AddOrderAsync_ValidParameters_CallsModelMethod()
        {
            // Arrange
            int productId = 123;
            int buyerId = 456;
            int productType = OrderViewModel.ProductType.New;
            string paymentMethod = "Credit Card";
            int orderSummaryId = 789;
            DateTime orderDate = new DateTime(2025, 4, 1);

            _mockOrderModel.Setup(m => m.AddOrderAsync(
                productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate))
                .Returns(Task.CompletedTask);

            // Act
            await _orderViewModel.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate);

            // Assert
            _mockOrderModel.Verify(m => m.AddOrderAsync(
                productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate),
                Times.Once);
        }

        [TestMethod]
        public async Task UpdateOrderAsync_ValidParameters_CallsModelMethod()
        {
            // Arrange
            int orderId = 123;
            int productType = OrderViewModel.ProductType.Used;
            string paymentMethod = "PayPal";
            DateTime orderDate = new DateTime(2025, 4, 1);

            _mockOrderModel.Setup(m => m.UpdateOrderAsync(
                orderId, productType, paymentMethod, orderDate))
                .Returns(Task.CompletedTask);

            // Act
            await _orderViewModel.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate);

            // Assert
            _mockOrderModel.Verify(m => m.UpdateOrderAsync(
                orderId, productType, paymentMethod, orderDate),
                Times.Once);
        }

        [TestMethod]
        public async Task DeleteOrderAsync_ValidOrderId_CallsModelMethod()
        {
            // Arrange
            int orderId = 123;

            _mockOrderModel.Setup(m => m.DeleteOrderAsync(orderId))
                .Returns(Task.CompletedTask);

            // Act
            await _orderViewModel.DeleteOrderAsync(orderId);

            // Assert
            _mockOrderModel.Verify(m => m.DeleteOrderAsync(orderId), Times.Once);
        }

        [TestMethod]
        public async Task GetBorrowedOrderHistoryAsync_ValidBuyerId_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            var expectedOrders = new List<Order>
            {
                new Order { OrderID = 101, ProductID = 201, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetBorrowedOrderHistoryAsync(buyerId))
                .ReturnsAsync(expectedOrders);

            // Act
            var result = await _orderViewModel.GetBorrowedOrderHistoryAsync(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(101, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetNewOrUsedOrderHistoryAsync_ValidBuyerId_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            var expectedOrders = new List<Order>
            {
                new Order { OrderID = 102, ProductID = 202, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetNewOrUsedOrderHistoryAsync(buyerId))
                .ReturnsAsync(expectedOrders);

            // Act
            var result = await _orderViewModel.GetNewOrUsedOrderHistoryAsync(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(102, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromLastThreeMonthsAsync_ValidBuyerId_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            var expectedOrders = new List<Order>
            {
                new Order { OrderID = 103, ProductID = 203, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetOrdersFromLastThreeMonths(buyerId))
                .Returns(expectedOrders);

            // Act
            var result = await _orderViewModel.GetOrdersFromLastThreeMonthsAsync(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(103, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFromLastThreeMonths(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromLastSixMonthsAsync_ValidBuyerId_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            var expectedOrders = new List<Order>
            {
                new Order { OrderID = 104, ProductID = 204, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetOrdersFromLastSixMonths(buyerId))
                .Returns(expectedOrders);

            // Act
            var result = await _orderViewModel.GetOrdersFromLastSixMonthsAsync(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(104, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFromLastSixMonths(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFrom2024Async_ValidBuyerId_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            var expectedOrders = new List<Order>
            {
                new Order { OrderID = 105, ProductID = 205, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetOrdersFrom2024(buyerId))
                .Returns(expectedOrders);

            // Act
            var result = await _orderViewModel.GetOrdersFrom2024Async(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(105, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFrom2024(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFrom2025Async_ValidBuyerId_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            var expectedOrders = new List<Order>
            {
                new Order { OrderID = 106, ProductID = 206, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetOrdersFrom2025(buyerId))
                .Returns(expectedOrders);

            // Act
            var result = await _orderViewModel.GetOrdersFrom2025Async(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(106, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFrom2025(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersByNameAsync_ValidParameters_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            string searchText = "Art";
            var expectedOrders = new List<Order>
            {
                new Order { OrderID = 107, ProductID = 207, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetOrdersByName(buyerId, searchText))
                .Returns(expectedOrders);

            // Act
            var result = await _orderViewModel.GetOrdersByNameAsync(buyerId, searchText);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(107, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersByName(buyerId, searchText), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromOrderHistoryAsync_ValidOrderHistoryId_ReturnsOrderList()
        {
            // Arrange
            int orderHistoryId = 789;
            var expectedOrders = new List<Order>
            {
                new Order { OrderID = 108, ProductID = 208, OrderHistoryID = orderHistoryId }
            };

            _mockOrderModel.Setup(m => m.GetOrdersFromOrderHistoryAsync(orderHistoryId))
                .ReturnsAsync(expectedOrders);

            // Act
            var result = await _orderViewModel.GetOrdersFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(108, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFromOrderHistoryAsync(orderHistoryId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderSummaryAsync_ValidId_ReturnsOrderSummary()
        {
            // Arrange
            int orderSummaryId = 501;
            var expectedOrderSummary = new OrderSummary
            {
                ID = orderSummaryId,
                Subtotal = 100.0f,
                WarrantyTax = 10.0f,
                DeliveryFee = 5.0f,
                FinalTotal = 115.0f,
                FullName = "John Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "123-456-7890",
                Address = "123 Main St",
                PostalCode = "12345",
                AdditionalInfo = "Leave at the door",
                ContractDetails = "Standard terms"
            };

            _mockOrderSummaryService.Setup(s => s.GetOrderSummaryAsync(orderSummaryId))
                .ReturnsAsync(expectedOrderSummary);

            // Act
            var result = await _orderViewModel.GetOrderSummaryAsync(orderSummaryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(orderSummaryId, result.ID);
            Assert.AreEqual("John Doe", result.FullName);
            Assert.AreEqual(115.0f, result.FinalTotal);
            _mockOrderSummaryService.Verify(s => s.GetOrderSummaryAsync(orderSummaryId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetOrderSummaryAsync_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int invalidOrderSummaryId = 999;
            _mockOrderSummaryService.Setup(s => s.GetOrderSummaryAsync(invalidOrderSummaryId))
                .ThrowsAsync(new KeyNotFoundException($"OrderSummary with ID {invalidOrderSummaryId} not found"));

            // Act
            await _orderViewModel.GetOrderSummaryAsync(invalidOrderSummaryId);

            // Assert is handled by the ExpectedException attribute
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_ExistingOrderId_ReturnsOrder()
        {
            // Arrange
            int orderId = 101;
            int buyerId = 456;
            var borrowedOrders = new List<Order>
            {
                new Order { OrderID = 101, ProductID = 201, BuyerID = buyerId, ProductType = OrderViewModel.ProductType.Borrowed }
            };
            var newOrUsedOrders = new List<Order>
            {
                new Order { OrderID = 102, ProductID = 202, BuyerID = buyerId, ProductType = OrderViewModel.ProductType.New }
            };

            _mockOrderModel.Setup(m => m.GetBorrowedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID))
                .ReturnsAsync(borrowedOrders);
            _mockOrderModel.Setup(m => m.GetNewOrUsedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID))
                .ReturnsAsync(newOrUsedOrders);

            // Act
            var result = await _orderViewModel.GetOrderByIdAsync(orderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(orderId, result.OrderID);
            Assert.AreEqual(OrderViewModel.ProductType.Borrowed, result.ProductType);
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID), Times.Once);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID), Times.Never);
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_OrderNotInBorrowedButInNewOrUsed_ReturnsOrder()
        {
            // Arrange
            int orderId = 102;
            int buyerId = 456;
            var borrowedOrders = new List<Order>
            {
                new Order { OrderID = 101, ProductID = 201, BuyerID = buyerId, ProductType = OrderViewModel.ProductType.Borrowed }
            };
            var newOrUsedOrders = new List<Order>
            {
                new Order { OrderID = 102, ProductID = 202, BuyerID = buyerId, ProductType = OrderViewModel.ProductType.New }
            };

            _mockOrderModel.Setup(m => m.GetBorrowedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID))
                .ReturnsAsync(borrowedOrders);
            _mockOrderModel.Setup(m => m.GetNewOrUsedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID))
                .ReturnsAsync(newOrUsedOrders);

            // Act
            var result = await _orderViewModel.GetOrderByIdAsync(orderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(orderId, result.OrderID);
            Assert.AreEqual(OrderViewModel.ProductType.New, result.ProductType);
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID), Times.Once);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_OrderNotFound_ReturnsNull()
        {
            // Arrange
            int orderId = 999; // Non-existent order ID
            int buyerId = 456;
            var borrowedOrders = new List<Order>
            {
                new Order { OrderID = 101, ProductID = 201, BuyerID = buyerId }
            };
            var newOrUsedOrders = new List<Order>
            {
                new Order { OrderID = 102, ProductID = 202, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetBorrowedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID))
                .ReturnsAsync(borrowedOrders);
            _mockOrderModel.Setup(m => m.GetNewOrUsedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID))
                .ReturnsAsync(newOrUsedOrders);

            // Act
            var result = await _orderViewModel.GetOrderByIdAsync(orderId);

            // Assert
            Assert.IsNull(result);
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID), Times.Once);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(ALL_ORDERS_DEFAULT_BUYER_ID), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_AllTimePeriod_ReturnsCombinedOrders()
        {
            // Arrange
            int buyerId = 456;
            var borrowedOrders = new List<Order>
            {
                new Order { OrderID = 101, ProductID = 201, BuyerID = buyerId, ProductType = OrderViewModel.ProductType.Borrowed }
            };
            var newOrUsedOrders = new List<Order>
            {
                new Order { OrderID = 102, ProductID = 202, BuyerID = buyerId, ProductType = OrderViewModel.ProductType.New }
            };

            _mockOrderModel.Setup(m => m.GetBorrowedOrderHistoryAsync(buyerId))
                .ReturnsAsync(borrowedOrders);
            _mockOrderModel.Setup(m => m.GetNewOrUsedOrderHistoryAsync(buyerId))
                .ReturnsAsync(newOrUsedOrders);

            // Act
            var result = await _orderViewModel.GetCombinedOrderHistoryAsync(buyerId, OrderViewModel.TimePeriodFilter.All);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(101, result[0].OrderID);
            Assert.AreEqual(102, result[1].OrderID);
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(buyerId), Times.Once);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_ThreeMonthsFilter_ReturnsFilteredOrders()
        {
            // Arrange
            int buyerId = 456;
            var threeMonthOrders = new List<Order>
            {
                new Order { OrderID = 103, ProductID = 203, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetOrdersFromLastThreeMonths(buyerId))
                .Returns(threeMonthOrders);

            // Act
            var result = await _orderViewModel.GetCombinedOrderHistoryAsync(buyerId, OrderViewModel.TimePeriodFilter.ThreeMonths);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(103, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFromLastThreeMonths(buyerId), Times.Once);
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(buyerId), Times.Never);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(buyerId), Times.Never);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_SixMonthsFilter_ReturnsFilteredOrders()
        {
            // Arrange
            int buyerId = 456;
            var sixMonthOrders = new List<Order>
            {
                new Order { OrderID = 104, ProductID = 204, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetOrdersFromLastSixMonths(buyerId))
                .Returns(sixMonthOrders);

            // Act
            var result = await _orderViewModel.GetCombinedOrderHistoryAsync(buyerId, OrderViewModel.TimePeriodFilter.SixMonths);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(104, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFromLastSixMonths(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_Year2024Filter_ReturnsFilteredOrders()
        {
            // Arrange
            int buyerId = 456;
            var orders2024 = new List<Order>
            {
                new Order { OrderID = 105, ProductID = 205, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetOrdersFrom2024(buyerId))
                .Returns(orders2024);

            // Act
            var result = await _orderViewModel.GetCombinedOrderHistoryAsync(buyerId, OrderViewModel.TimePeriodFilter.Year2024);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(105, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFrom2024(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_Year2025Filter_ReturnsFilteredOrders()
        {
            // Arrange
            int buyerId = 456;
            var orders2025 = new List<Order>
            {
                new Order { OrderID = 106, ProductID = 206, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetOrdersFrom2025(buyerId))
                .Returns(orders2025);

            // Act
            var result = await _orderViewModel.GetCombinedOrderHistoryAsync(buyerId, OrderViewModel.TimePeriodFilter.Year2025);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(106, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFrom2025(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_InvalidFilter_DefaultsToAllOrders()
        {
            // Arrange
            int buyerId = 456;
            string invalidFilter = "invalid_filter";
            var borrowedOrders = new List<Order>
            {
                new Order { OrderID = 101, ProductID = 201, BuyerID = buyerId }
            };
            var newOrUsedOrders = new List<Order>
            {
                new Order { OrderID = 102, ProductID = 202, BuyerID = buyerId }
            };

            _mockOrderModel.Setup(m => m.GetBorrowedOrderHistoryAsync(buyerId))
                .ReturnsAsync(borrowedOrders);
            _mockOrderModel.Setup(m => m.GetNewOrUsedOrderHistoryAsync(buyerId))
                .ReturnsAsync(newOrUsedOrders);

            // Act
            var result = await _orderViewModel.GetCombinedOrderHistoryAsync(buyerId, invalidFilter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(buyerId), Times.Once);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public void Constructor_WithConnectionString_InitializesCorrectly()
        {
            // Act
            var viewModel = new OrderViewModel(ConnectionString);

            // Assert
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        public void Constructor_WithOrderModelAndOrderSummaryService_InitializesCorrectly()
        {
            // Act
            var viewModel = new OrderViewModel(_mockOrderModel.Object, _mockOrderSummaryService.Object);

            // Assert
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullOrderModel_ThrowsArgumentNullException()
        {
            // Act
            var viewModel = new OrderViewModel(null, _mockOrderSummaryService.Object);

            // Assert is handled by the ExpectedException attribute
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullOrderSummaryService_ThrowsArgumentNullException()
        {
            // Act
            var viewModel = new OrderViewModel(_mockOrderModel.Object, null);

            // Assert is handled by the ExpectedException attribute
        }
    }
}
