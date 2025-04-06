using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ArtAttack.Tests
{
    [TestClass]
    public class OrderModelTests
    {
        private Mock<IDatabaseProvider> _mockDatabaseProvider;
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataParameterCollection> _mockParameterCollection;
        private OrderModel _orderModel;
        private const string ConnectionString = "test_connection_string";

        [TestInitialize]
        public void TestInitialize()
        {
            // Setup mock objects
            _mockDatabaseProvider = new Mock<IDatabaseProvider>();
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockParameterCollection = new Mock<IDataParameterCollection>();

            // Setup parameter collection
            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameterCollection.Object);

            // Setup connection and command creation
            _mockDatabaseProvider.Setup(p => p.CreateConnection(It.IsAny<string>())).Returns(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

            // Create the order model with mocked database provider
            _orderModel = new OrderModel(ConnectionString, _mockDatabaseProvider.Object);
        }

        [TestMethod]
        public async Task AddOrderAsync_ValidParameters_ExecutesStoredProcedure()
        {
            // Arrange
            int productId = 123;
            int buyerId = 456;
            int productType = 1; // New product
            string paymentMethod = "Credit Card";
            int orderSummaryId = 789;
            DateTime orderDate = new DateTime(2025, 4, 1);

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Mock ExecuteNonQueryAsync to return 1 (indicating success)
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            await _orderModel.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate);

            // Assert
            _mockDatabaseProvider.Verify(p => p.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(c => c.CreateCommand(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Verify command properties
            _mockCommand.VerifySet(c => c.CommandText = "AddOrder");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);

            // Verify parameters were created correctly
            _mockCommand.Verify(c => c.CreateParameter(), Times.Exactly(6));

            // We cannot directly verify parameter values with this approach
            // But we can verify that parameters were added to the collection
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Exactly(6));
        }

        [TestMethod]
        public async Task AddOrderAsync_NullPaymentMethod_HandlesNullValue()
        {
            // Arrange
            int productId = 123;
            int buyerId = 456;
            int productType = 1;
            string paymentMethod = null;
            int orderSummaryId = 789;
            DateTime orderDate = new DateTime(2025, 4, 1);

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Act
            await _orderModel.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate);

            // Assert
            // We're verifying that the code runs without throwing an exception when paymentMethod is null
            _mockCommand.Verify(c => c.CreateParameter(), Times.Exactly(6));
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Exactly(6));
        }

        [TestMethod]
        public void ConnectionString_PropertyAccess_ReturnsExpectedValue()
        {
            // Act
            string connectionString = _orderModel.ConnectionString;

            // Assert
            Assert.AreEqual(ConnectionString, connectionString);
        }

        [TestMethod]
        public async Task UpdateOrderAsync_ValidParameters_ExecutesStoredProcedure()
        {
            // Arrange
            int orderId = 123;
            int productType = 2; // Used product
            string paymentMethod = "PayPal";
            DateTime orderDate = new DateTime(2025, 4, 1);

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Mock ExecuteNonQueryAsync to return 1 (indicating success)
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            await _orderModel.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate);

            // Assert
            _mockDatabaseProvider.Verify(p => p.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(c => c.CreateCommand(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Verify command properties
            _mockCommand.VerifySet(c => c.CommandText = "UpdateOrder");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);

            // Verify parameters were created correctly
            _mockCommand.Verify(c => c.CreateParameter(), Times.Exactly(4));
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Exactly(4));
        }

        [TestMethod]
        public async Task DeleteOrderAsync_ValidOrderId_ExecutesStoredProcedure()
        {
            // Arrange
            int orderId = 123;

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Mock ExecuteNonQueryAsync to return 1 (indicating success)
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            await _orderModel.DeleteOrderAsync(orderId);

            // Assert
            _mockDatabaseProvider.Verify(p => p.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(c => c.CreateCommand(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Verify command properties
            _mockCommand.VerifySet(c => c.CommandText = "DeleteOrder");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);

            // Verify parameters were created correctly
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public async Task GetBorrowedOrderHistoryAsync_ValidBuyerId_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            SetupMockDataReader();

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Act
            List<Order> result = await _orderModel.GetBorrowedOrderHistoryAsync(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _mockCommand.VerifySet(c => c.CommandText = "get_borrowed_order_history");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public async Task GetNewOrUsedOrderHistoryAsync_ValidBuyerId_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            SetupMockDataReader();

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Act
            List<Order> result = await _orderModel.GetNewOrUsedOrderHistoryAsync(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _mockCommand.VerifySet(c => c.CommandText = "get_new_or_used_order_history");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public void GetOrdersFromLastThreeMonths_ValidBuyerId_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            SetupMockDataReader(false);

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Act
            List<Order> result = _orderModel.GetOrdersFromLastThreeMonths(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _mockCommand.VerifySet(c => c.CommandText = "get_orders_from_last_3_months");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public void GetOrdersFromLastSixMonths_ValidBuyerId_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            SetupMockDataReader(false);

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Act
            List<Order> result = _orderModel.GetOrdersFromLastSixMonths(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _mockCommand.VerifySet(c => c.CommandText = "get_orders_from_last_6_months");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public void GetOrdersFrom2025_ValidBuyerId_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            SetupMockDataReader(false);

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Act
            List<Order> result = _orderModel.GetOrdersFrom2025(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _mockCommand.VerifySet(c => c.CommandText = "get_orders_from_2025");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public void GetOrdersFrom2024_ValidBuyerId_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            SetupMockDataReader(false);

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Act
            List<Order> result = _orderModel.GetOrdersFrom2024(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _mockCommand.VerifySet(c => c.CommandText = "get_orders_from_2024");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public void GetOrdersByName_ValidParameters_ReturnsOrderList()
        {
            // Arrange
            int buyerId = 456;
            string searchText = "Art";
            SetupMockDataReader(false);

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Act
            List<Order> result = _orderModel.GetOrdersByName(buyerId, searchText);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _mockCommand.VerifySet(c => c.CommandText = "get_orders_by_name");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.CreateParameter(), Times.Exactly(2));
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task GetOrdersFromOrderHistoryAsync_ValidOrderHistoryId_ReturnsOrderList()
        {
            // Arrange
            int orderHistoryId = 789;
            SetupMockDataReader(true, true);

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Act
            List<Order> result = await _orderModel.GetOrdersFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _mockCommand.VerifySet(c => c.CommandText = "get_orders_from_order_history");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromOrderHistoryAsync_NullValues_HandlesNullsCorrectly()
        {
            // Arrange
            int orderHistoryId = 789;
            SetupMockDataReader(true, true, true);

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Act
            List<Order> result = await _orderModel.GetOrdersFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(string.Empty, result[0].PaymentMethod);
            Assert.AreEqual(DateTime.MinValue, result[0].OrderDate);
        }

        #region Helper Methods

        private void SetupMockDataReader(bool async = true, bool withNulls = false, bool withDBNulls = false)
        {
            // Create a mock data reader
            Mock<IDataReader> mockDataReader = new Mock<IDataReader>();

            // Setup ReadAsync behavior
            if (async)
            {
                int callCount = 0;
                mockDataReader.Setup(r => r.Read()).Returns(() => callCount++ == 0);
            }
            else
            {
                int callCount = 0;
                mockDataReader.Setup(r => r.Read()).Returns(() => callCount++ == 0);
            }

            // Setup GetOrdinal for field indexes
            mockDataReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            mockDataReader.Setup(r => r.GetOrdinal("ProductID")).Returns(1);
            mockDataReader.Setup(r => r.GetOrdinal("BuyerID")).Returns(2);
            mockDataReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(3);
            mockDataReader.Setup(r => r.GetOrdinal("OrderHistoryID")).Returns(4);
            mockDataReader.Setup(r => r.GetOrdinal("ProductType")).Returns(5);
            mockDataReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(6);
            mockDataReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(7);

            // Setup field value retrieval
            mockDataReader.Setup(r => r.GetInt32(0)).Returns(101);
            mockDataReader.Setup(r => r.GetInt32(1)).Returns(201);
            mockDataReader.Setup(r => r.GetInt32(2)).Returns(301);
            mockDataReader.Setup(r => r.GetInt32(3)).Returns(401);
            mockDataReader.Setup(r => r.GetInt32(4)).Returns(501);
            mockDataReader.Setup(r => r.GetInt32(5)).Returns(1);

            if (withDBNulls)
            {
                // Setup IsDBNull return true for specified fields
                mockDataReader.Setup(r => r.IsDBNull(6)).Returns(true);
                mockDataReader.Setup(r => r.IsDBNull(7)).Returns(true);
            }
            else if (withNulls)
            {
                // Setup regular values for non-null fields
                mockDataReader.Setup(r => r.IsDBNull(6)).Returns(false);
                mockDataReader.Setup(r => r.IsDBNull(7)).Returns(false);
                mockDataReader.Setup(r => r.GetString(6)).Returns("Credit Card");
                mockDataReader.Setup(r => r.GetDateTime(7)).Returns(new DateTime(2025, 4, 1));
            }
            else
            {
                // Setup regular values for non-null fields
                mockDataReader.Setup(r => r.IsDBNull(6)).Returns(false);
                mockDataReader.Setup(r => r.IsDBNull(7)).Returns(false);
                mockDataReader.Setup(r => r.GetString(6)).Returns("Credit Card");
                mockDataReader.Setup(r => r.GetDateTime(7)).Returns(new DateTime(2025, 4, 1));
            }

            // Setup command to return the mock data reader
            if (async)
            {
                _mockCommand.Setup(c => c.ExecuteReader()).Returns(mockDataReader.Object);
            }
            else
            {
                _mockCommand.Setup(c => c.ExecuteReader()).Returns(mockDataReader.Object);
            }
        }

        #endregion
    }
}