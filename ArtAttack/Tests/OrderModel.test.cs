/*
using System.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using Moq;
using Moq.Dapper;
using Xunit;
using Dapper;

namespace ArtAttack.Tests
{
    public class OrderModelTests
    {
        private const string TestConnectionString = "Server=localhost;Database=TestDb;Integrated Security=true";
        private readonly OrderModel orderModel;
        private readonly Mock<IDbConnection> mockConnection;

        public OrderModelTests()
        {
            orderModel = new OrderModel(TestConnectionString);
            mockConnection = new Mock<IDbConnection>();
        }

        [Fact]
        public async Task AddOrderAsync_ShouldExecuteStoredProcedure()
        {
            // Arrange
            int productId = 1;
            int buyerId = 2;
            int productType = 3;
            string paymentMethod = "Credit Card";
            int orderSummaryId = 4;
            DateTime orderDate = new DateTime(2025, 2, 15);

            var mockConnection = MockDbConnection();
            mockConnection.SetupDapper(x => x.ExecuteAsync(
                It.Is<string>(s => s == "AddOrder"),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()
            )).ReturnsAsync(1);

            // Act
            await orderModel.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate);

            // Assert
            mockConnection.Verify(c => c.ExecuteAsync(
                It.Is<string>(s => s == "AddOrder"),
                It.Is<object>(p => VerifyAddOrderParameters(p, productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate)),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.Is<CommandType?>(ct => ct == CommandType.StoredProcedure)
            ), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldExecuteStoredProcedure()
        {
            // Arrange
            int orderId = 1;
            int productType = 2;
            string paymentMethod = "PayPal";
            DateTime orderDate = new DateTime(2025, 3, 20);

            var mockConnection = MockDbConnection();
            mockConnection.SetupDapper(x => x.ExecuteAsync(
                It.Is<string>(s => s == "UpdateOrder"),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()
            )).ReturnsAsync(1);

            // Act
            await orderModel.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate);

            // Assert
            mockConnection.Verify(c => c.ExecuteAsync(
                It.Is<string>(s => s == "UpdateOrder"),
                It.Is<object>(p => VerifyUpdateOrderParameters(p, orderId, productType, paymentMethod, orderDate)),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.Is<CommandType?>(ct => ct == CommandType.StoredProcedure)
            ), Times.Once);
        }

        [Fact]
        public async Task DeleteOrderAsync_ShouldExecuteStoredProcedure()
        {
            // Arrange
            int orderId = 1;

            var mockConnection = MockDbConnection();
            mockConnection.SetupDapper(x => x.ExecuteAsync(
                It.Is<string>(s => s == "DeleteOrder"),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()
            )).ReturnsAsync(1);

            // Act
            await orderModel.DeleteOrderAsync(orderId);

            // Assert
            mockConnection.Verify(c => c.ExecuteAsync(
                It.Is<string>(s => s == "DeleteOrder"),
                It.Is<object>(p => VerifyDeleteOrderParameters(p, orderId)),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.Is<CommandType?>(ct => ct == CommandType.StoredProcedure)
            ), Times.Once);
        }

        [Fact]
        public async Task GetBorrowedOrderHistoryAsync_ShouldReturnCorrectOrders()
        {
            // Arrange
            int buyerId = 1;
            var expectedOrders = CreateTestOrders(3);

            var mockConnection = MockDbConnection();
            mockConnection.SetupDapper(x => x.QueryAsync<Order>(
                It.Is<string>(s => s == "get_borrowed_order_history"),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()
            )).ReturnsAsync(expectedOrders);

            // Act
            var result = await orderModel.GetBorrowedOrderHistoryAsync(buyerId);

            // Assert
            Assert.Equal(expectedOrders.Count, result.Count);
            for (int i = 0; i < expectedOrders.Count; i++)
            {
                Assert.Equal(expectedOrders[i].OrderID, result[i].OrderID);
                Assert.Equal(expectedOrders[i].ProductID, result[i].ProductID);
                Assert.Equal(expectedOrders[i].BuyerID, result[i].BuyerID);
            }
        }

        [Fact]
        public async Task GetNewOrUsedOrderHistoryAsync_ShouldReturnCorrectOrders()
        {
            // Arrange
            int buyerId = 1;
            var expectedOrders = CreateTestOrders(2);

            var mockConnection = MockDbConnection();
            mockConnection.SetupDapper(x => x.QueryAsync<Order>(
                It.Is<string>(s => s == "get_new_or_used_order_history"),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()
            )).ReturnsAsync(expectedOrders);

            // Act
            var result = await orderModel.GetNewOrUsedOrderHistoryAsync(buyerId);

            // Assert
            Assert.Equal(expectedOrders.Count, result.Count);
            for (int i = 0; i < expectedOrders.Count; i++)
            {
                Assert.Equal(expectedOrders[i].OrderID, result[i].OrderID);
                Assert.Equal(expectedOrders[i].ProductID, result[i].ProductID);
                Assert.Equal(expectedOrders[i].BuyerID, result[i].BuyerID);
            }
        }

        [Fact]
        public void GetOrdersFromLastThreeMonths_ShouldReturnCorrectOrders()
        {
            // Arrange
            int buyerId = 1;
            var expectedOrders = CreateTestOrders(4);

            var mockConnection = MockDbConnection();
            mockConnection.SetupDapper(x => x.Query<Order>(
                It.Is<string>(s => s == "get_orders_from_last_3_months"),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()
            )).Returns(expectedOrders);

            // Act
            var result = orderModel.GetOrdersFromLastThreeMonths(buyerId);

            // Assert
            Assert.Equal(expectedOrders.Count, result.Count);
            for (int i = 0; i < expectedOrders.Count; i++)
            {
                Assert.Equal(expectedOrders[i].OrderID, result[i].OrderID);
                Assert.Equal(expectedOrders[i].ProductID, result[i].ProductID);
                Assert.Equal(expectedOrders[i].BuyerID, result[i].BuyerID);
            }
        }

        [Fact]
        public void GetOrdersFromLastSixMonths_ShouldReturnCorrectOrders()
        {
            // Arrange
            int buyerId = 1;
            var expectedOrders = CreateTestOrders(5);

            var mockConnection = MockDbConnection();
            mockConnection.SetupDapper(x => x.Query<Order>(
                It.Is<string>(s => s == "get_orders_from_last_6_months"),
                It.Is<object>(p => VerifyGetOrdersFromLastSixMonthsParameters(p, buyerId)),
                It.IsAny<IDbTransaction>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.Is<CommandType?>(ct => ct == CommandType.StoredProcedure)
            )).Returns(expectedOrders);

            // Act
            var result = orderModel.GetOrdersFromLastSixMonths(buyerId);

            // Assert
            Assert.Equal(expectedOrders.Count, result.Count);
            for (int i = 0; i < expectedOrders.Count; i++)
            {
                Assert.Equal(expectedOrders[i].OrderID, result[i].OrderID);
                Assert.Equal(expectedOrders[i].ProductID, result[i].ProductID);
                Assert.Equal(expectedOrders[i].BuyerID, result[i].BuyerID);
            }
        }

        [Fact]
        public void GetOrdersFrom2025_ShouldReturnCorrectOrders()
        {
            // Arrange
            int buyerId = 1;
            var expectedOrders = CreateTestOrders(2);

            var mockConnection = MockDbConnection();
            mockConnection.SetupDapper(x => x.Query<Order>(
                It.Is<string>(s => s == "get_orders_from_2025"),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()
            )).Returns(expectedOrders);

            // Act
            var result = orderModel.GetOrdersFrom2025(buyerId);

            // Assert
            Assert.Equal(expectedOrders.Count, result.Count);
            for (int i = 0; i < expectedOrders.Count; i++)
            {
                Assert.Equal(expectedOrders[i].OrderID, result[i].OrderID);
                Assert.Equal(expectedOrders[i].ProductID, result[i].ProductID);
                Assert.Equal(expectedOrders[i].BuyerID, result[i].BuyerID);
            }
        }

        [Fact]
        public void GetOrdersFrom2024_ShouldReturnCorrectOrders()
        {
            // Arrange
            int buyerId = 1;
            var expectedOrders = CreateTestOrders(3);

            var mockConnection = MockDbConnection();
            mockConnection.SetupDapper(x => x.Query<Order>(
                It.Is<string>(s => s == "get_orders_from_2024"),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()
            )).Returns(expectedOrders);

            // Act
            var result = orderModel.GetOrdersFrom2024(buyerId);

            // Assert
            Assert.Equal(expectedOrders.Count, result.Count);
            for (int i = 0; i < expectedOrders.Count; i++)
            {
                Assert.Equal(expectedOrders[i].OrderID, result[i].OrderID);
                Assert.Equal(expectedOrders[i].ProductID, result[i].ProductID);
                Assert.Equal(expectedOrders[i].BuyerID, result[i].BuyerID);
            }
        }

        [Fact]
        public void GetOrdersByName_ShouldReturnCorrectOrders()
        {
            // Arrange
            int buyerId = 1;
            string searchText = "Product Name";
            var expectedOrders = CreateTestOrders(2);

            var mockConnection = MockDbConnection();
            mockConnection.SetupDapper(x => x.Query<Order>(
                It.Is<string>(s => s == "get_orders_by_name"),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()
            )).Returns(expectedOrders);

            // Act
            var result = orderModel.GetOrdersByName(buyerId, searchText);

            // Assert
            Assert.Equal(expectedOrders.Count, result.Count);
            for (int i = 0; i < expectedOrders.Count; i++)
            {
                Assert.Equal(expectedOrders[i].OrderID, result[i].OrderID);
                Assert.Equal(expectedOrders[i].ProductID, result[i].ProductID);
                Assert.Equal(expectedOrders[i].BuyerID, result[i].BuyerID);
            }
        }

        [Fact]
        public async Task GetOrdersFromOrderHistoryAsync_ShouldReturnCorrectOrders()
        {
            // Arrange
            int orderHistoryId = 1;
            var expectedOrders = CreateTestOrders(3);

            var mockConnection = MockDbConnection();
            mockConnection.SetupDapper(x => x.QueryAsync<Order>(
                It.Is<string>(s => s == "get_orders_from_order_history"),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()
            )).ReturnsAsync(expectedOrders);

            // Act
            var result = await orderModel.GetOrdersFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.Equal(expectedOrders.Count, result.Count);
            for (int i = 0; i < expectedOrders.Count; i++)
            {
                Assert.Equal(expectedOrders[i].OrderID, result[i].OrderID);
                Assert.Equal(expectedOrders[i].ProductID, result[i].ProductID);
                Assert.Equal(expectedOrders[i].BuyerID, result[i].BuyerID);
            }
        }

        // Helper methods for parameter verification
        private bool VerifyAddOrderParameters(object parameters, int productId, int buyerId, int productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            dynamic param = parameters;
            return param.ProductID == productId &&
                   param.BuyerID == buyerId &&
                   param.ProductType == productType &&
                   param.PaymentMethod == paymentMethod &&
                   param.OrderSummaryID == orderSummaryId &&
                   param.OrderDate == orderDate;
        }

        private bool VerifyUpdateOrderParameters(object parameters, int orderId, int productType, string paymentMethod, DateTime orderDate)
        {
            dynamic param = parameters;
            return param.OrderID == orderId &&
                   param.ProductType == productType &&
                   param.PaymentMethod == paymentMethod &&
                   param.OrderDate == orderDate;
        }

        private bool VerifyDeleteOrderParameters(object parameters, int orderId)
        {
            dynamic param = parameters;
            return param.OrderID == orderId;
        }

        private bool VerifyGetOrdersFromLastSixMonthsParameters(object parameters, int buyerId)
        {
            dynamic param = parameters;
            return param.BuyerID == buyerId;
        }

        private List<Order> CreateTestOrders(int count)
        {
            var orders = new List<Order>();
            for (int i = 1; i <= count; i++)
            {
                orders.Add(new Order
                {
                    OrderID = i,
                    ProductID = i * 10,
                    BuyerID = 1,
                    OrderSummaryID = i + 100,
                    OrderHistoryID = i + 200,
                    ProductType = i % 3 + 1,
                    PaymentMethod = i % 2 == 0 ? "Credit Card" : "PayPal",
                    OrderDate = DateTime.Now.AddDays(-i * 10)
                });
            }
            return orders;
        }

        private Mock<IDbConnection> MockDbConnection()
        {
            return new Mock<IDbConnection>();
        }
    }
}
*/