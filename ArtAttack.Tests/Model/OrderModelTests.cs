using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ArtAttack.Tests.Model
{
    [TestClass]
    public class OrderModelTests
    {
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private Mock<IDbDataParameter> _mockParameter;
        private Mock<IDatabaseProvider> _mockDatabaseProvider;
        private string _testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";
        private OrderModel _orderModel;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockReader = new Mock<IDataReader>();
            _mockParameters = new Mock<IDataParameterCollection>();
            _mockParameter = new Mock<IDbDataParameter>();
            _mockDatabaseProvider = new Mock<IDatabaseProvider>();

            // Setup the parameter collection mock
            _mockParameters.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

            // Setup the command mock
            _mockCommand.Setup(c => c.CreateParameter()).Returns(_mockParameter.Object);
            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object);

            // Setup the connection mock
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

            // Setup the database provider mock
            _mockDatabaseProvider.Setup(p => p.CreateConnection(It.IsAny<string>())).Returns(_mockConnection.Object);

            // Initialize the model with the mock database provider
            _orderModel = new OrderModel(_testConnectionString, _mockDatabaseProvider.Object);
        }

        [TestMethod]
        public async Task AddOrderAsync_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int productId = 1;
            int buyerId = 2;
            int productType = 3;
            string paymentMethod = "Credit Card";
            int orderSummaryId = 4;
            DateTime orderDate = DateTime.Now;

            // Setup parameter capture
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Act
            await _orderModel.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "AddOrder");

            // Instead of verifying ExecuteNonQueryAsync directly, verify ExecuteNonQuery which is what the extension method calls
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Instead of verifying OpenAsync directly, verify Open which is what the extension method calls
            _mockConnection.Verify(c => c.Open(), Times.Once);

            // Verify parameters
            Assert.AreEqual(6, capturedParameters.Count, "Should have added 6 parameters");
            AssertParameterValue(capturedParameters, "@ProductID", productId);
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);
            AssertParameterValue(capturedParameters, "@ProductType", productType);
            AssertParameterValue(capturedParameters, "@PaymentMethod", paymentMethod);
            AssertParameterValue(capturedParameters, "@OrderSummaryID", orderSummaryId);
            AssertParameterValue(capturedParameters, "@OrderDate", orderDate);
        }


        // Helper method to check parameter values
        private void AssertParameterValue(List<IDbDataParameter> parameters, string paramName, object expectedValue)
        {
            var param = parameters.FirstOrDefault(p => p.ParameterName == paramName);
            Assert.IsNotNull(param, $"Parameter {paramName} not found");
            Assert.AreEqual(expectedValue, param.Value, $"Parameter {paramName} has incorrect value");
        }

        [TestMethod]
        public async Task UpdateOrderAsync_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int orderId = 1;
            int productType = 2;
            string paymentMethod = "Credit Card";
            DateTime orderDate = DateTime.Now;

            // Setup parameter capture
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Act
            await _orderModel.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "UpdateOrder");

            // Instead of verifying extension methods directly
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);

            // Verify parameters
            Assert.AreEqual(4, capturedParameters.Count, "Should have added 4 parameters");
            AssertParameterValue(capturedParameters, "@OrderID", orderId);
            AssertParameterValue(capturedParameters, "@ProductType", productType);
            AssertParameterValue(capturedParameters, "@PaymentMethod", paymentMethod);
            AssertParameterValue(capturedParameters, "@OrderDate", orderDate);
        }

        [TestMethod]
        public async Task DeleteOrderAsync_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int orderId = 1;

            // Setup parameter capture
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Act
            await _orderModel.DeleteOrderAsync(orderId);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "DeleteOrder");

            // Instead of verifying extension methods directly
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);

            // Verify parameters
            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@OrderID", orderId);
        }

        [TestMethod]
        public async Task GetBorrowedOrderHistoryAsync_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            DateTime orderDate = DateTime.Now;

            // Setup parameter capture
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductID")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("BuyerID")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderHistoryID")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(7);

            // Setup Read calls
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)   // First record
                .Returns(true)   // Second record
                .Returns(false); // End of data

            // First order
            _mockReader.SetupSequence(r => r.GetInt32(0))
                .Returns(101)
                .Returns(102);
            _mockReader.SetupSequence(r => r.GetInt32(1))
                .Returns(201)
                .Returns(202);
            _mockReader.SetupSequence(r => r.GetInt32(2))
                .Returns(buyerId)
                .Returns(buyerId);
            _mockReader.SetupSequence(r => r.GetInt32(3))
                .Returns(301)
                .Returns(302);
            _mockReader.SetupSequence(r => r.GetInt32(4))
                .Returns(401)
                .Returns(402);
            _mockReader.SetupSequence(r => r.GetInt32(5))
                .Returns(1)
                .Returns(2);
            _mockReader.SetupSequence(r => r.GetString(6))
                .Returns("Credit Card")
                .Returns("PayPal");
            _mockReader.SetupSequence(r => r.GetDateTime(7))
                .Returns(orderDate)
                .Returns(orderDate.AddDays(-1));

            // Setup standard method
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = await _orderModel.GetBorrowedOrderHistoryAsync(buyerId);

            // Assert
            Assert.AreEqual(2, results.Count);

            // First order
            Assert.AreEqual(101, results[0].OrderID);
            Assert.AreEqual(201, results[0].ProductID);
            Assert.AreEqual(buyerId, results[0].BuyerID);
            Assert.AreEqual(301, results[0].OrderSummaryID);
            Assert.AreEqual(401, results[0].OrderHistoryID);
            Assert.AreEqual(1, results[0].ProductType);
            Assert.AreEqual("Credit Card", results[0].PaymentMethod);
            Assert.AreEqual(orderDate, results[0].OrderDate);

            // Second order
            Assert.AreEqual(102, results[1].OrderID);
            Assert.AreEqual(202, results[1].ProductID);
            Assert.AreEqual(buyerId, results[1].BuyerID);
            Assert.AreEqual(302, results[1].OrderSummaryID);
            Assert.AreEqual(402, results[1].OrderHistoryID);
            Assert.AreEqual(2, results[1].ProductType);
            Assert.AreEqual("PayPal", results[1].PaymentMethod);
            Assert.AreEqual(orderDate.AddDays(-1), results[1].OrderDate);

            // Verify stored procedure call
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "get_borrowed_order_history");

            // Verify parameters
            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);

            // Verify non-extension methods
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
        }

        [TestMethod]
        public async Task GetNewOrUsedOrderHistoryAsync_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            DateTime orderDate = DateTime.Now;

            // Setup parameter capture
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductID")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("BuyerID")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderHistoryID")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(7);

            // Setup Read calls
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            // First order
            _mockReader.Setup(r => r.GetInt32(0)).Returns(101);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(201);
            _mockReader.Setup(r => r.GetInt32(2)).Returns(buyerId);
            _mockReader.Setup(r => r.GetInt32(3)).Returns(301);
            _mockReader.Setup(r => r.GetInt32(4)).Returns(401);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(3); // New or used
            _mockReader.Setup(r => r.GetString(6)).Returns("Credit Card");
            _mockReader.Setup(r => r.GetDateTime(7)).Returns(orderDate);

            // Setup standard method
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = await _orderModel.GetNewOrUsedOrderHistoryAsync(buyerId);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(101, results[0].OrderID);
            Assert.AreEqual(201, results[0].ProductID);
            Assert.AreEqual(buyerId, results[0].BuyerID);
            Assert.AreEqual(301, results[0].OrderSummaryID);
            Assert.AreEqual(401, results[0].OrderHistoryID);
            Assert.AreEqual(3, results[0].ProductType);
            Assert.AreEqual("Credit Card", results[0].PaymentMethod);
            Assert.AreEqual(orderDate, results[0].OrderDate);

            // Verify stored procedure call
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "get_new_or_used_order_history");

            // Verify parameters 
            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);

            // Verify non-extension methods
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
        }

        [TestMethod]
        public void GetOrdersFromLastThreeMonths_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            DateTime orderDate = DateTime.Now;

            // Setup parameter capture
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductID")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("BuyerID")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderHistoryID")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(7);

            // Setup Read calls
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            // First order
            _mockReader.Setup(r => r.GetInt32(0)).Returns(101);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(201);
            _mockReader.Setup(r => r.GetInt32(2)).Returns(buyerId);
            _mockReader.Setup(r => r.GetInt32(3)).Returns(301);
            _mockReader.Setup(r => r.GetInt32(4)).Returns(401);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(1);
            _mockReader.Setup(r => r.GetString(6)).Returns("Credit Card");
            _mockReader.Setup(r => r.GetDateTime(7)).Returns(orderDate.AddMonths(-2));

            // Setup standard method
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = _orderModel.GetOrdersFromLastThreeMonths(buyerId);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(101, results[0].OrderID);
            Assert.AreEqual(orderDate.AddMonths(-2), results[0].OrderDate);

            // Verify stored procedure call
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "get_orders_from_last_3_months");

            // Verify parameters
            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);

            // Verify non-extension methods
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
        }

        [TestMethod]
        public void GetOrdersFromLastSixMonths_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            DateTime orderDate = DateTime.Now;

            // Setup parameter capture
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductID")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("BuyerID")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderHistoryID")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(7);

            // Setup Read calls
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            // First order
            _mockReader.Setup(r => r.GetInt32(0)).Returns(101);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(201);
            _mockReader.Setup(r => r.GetInt32(2)).Returns(buyerId);
            _mockReader.Setup(r => r.GetInt32(3)).Returns(301);
            _mockReader.Setup(r => r.GetInt32(4)).Returns(401);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(1);
            _mockReader.Setup(r => r.GetString(6)).Returns("Credit Card");
            _mockReader.Setup(r => r.GetDateTime(7)).Returns(orderDate.AddMonths(-5));

            // Setup standard method
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = _orderModel.GetOrdersFromLastSixMonths(buyerId);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(101, results[0].OrderID);
            Assert.AreEqual(orderDate.AddMonths(-5), results[0].OrderDate);

            // Verify stored procedure call
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "get_orders_from_last_6_months");

            // Verify parameters
            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);

            // Verify non-extension methods
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
        }

        [TestMethod]
        public void GetOrdersByName_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            string searchText = "test";
            DateTime orderDate = DateTime.Now;

            // Setup parameter capture
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductID")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("BuyerID")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderHistoryID")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(7);

            // Setup Read calls
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            // First order
            _mockReader.Setup(r => r.GetInt32(0)).Returns(101);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(201);
            _mockReader.Setup(r => r.GetInt32(2)).Returns(buyerId);
            _mockReader.Setup(r => r.GetInt32(3)).Returns(301);
            _mockReader.Setup(r => r.GetInt32(4)).Returns(401);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(1);
            _mockReader.Setup(r => r.GetString(6)).Returns("Credit Card");
            _mockReader.Setup(r => r.GetDateTime(7)).Returns(orderDate);

            // Setup standard method
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = _orderModel.GetOrdersByName(buyerId, searchText);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(101, results[0].OrderID);

            // Verify stored procedure call
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "get_orders_by_name");

            // Verify parameters
            Assert.AreEqual(2, capturedParameters.Count, "Should have added 2 parameters");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);
            AssertParameterValue(capturedParameters, "@text", searchText);

            // Verify non-extension methods
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
        }

        [TestMethod]
        public void GetOrdersFrom2024_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            DateTime orderDate = new DateTime(2024, 3, 10);

            // Setup parameter capture
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductID")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("BuyerID")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderHistoryID")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(7);

            // Setup Read calls
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            // First order
            _mockReader.Setup(r => r.GetInt32(0)).Returns(101);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(201);
            _mockReader.Setup(r => r.GetInt32(2)).Returns(buyerId);
            _mockReader.Setup(r => r.GetInt32(3)).Returns(301);
            _mockReader.Setup(r => r.GetInt32(4)).Returns(401);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(1);
            _mockReader.Setup(r => r.GetString(6)).Returns("Credit Card");
            _mockReader.Setup(r => r.GetDateTime(7)).Returns(orderDate);

            // Setup standard method
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = _orderModel.GetOrdersFrom2024(buyerId);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(101, results[0].OrderID);
            Assert.AreEqual(orderDate, results[0].OrderDate);

            // Verify stored procedure call
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "get_orders_from_2024");

            // Verify parameters
            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);

            // Verify non-extension methods
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
        }

        [TestMethod]
        public void GetOrdersFrom2025_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            DateTime orderDate = new DateTime(2025, 6, 15);

            // Setup parameter capture
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductID")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("BuyerID")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderHistoryID")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(7);

            // Setup Read calls
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            // First order
            _mockReader.Setup(r => r.GetInt32(0)).Returns(101);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(201);
            _mockReader.Setup(r => r.GetInt32(2)).Returns(buyerId);
            _mockReader.Setup(r => r.GetInt32(3)).Returns(301);
            _mockReader.Setup(r => r.GetInt32(4)).Returns(401);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(1);
            _mockReader.Setup(r => r.GetString(6)).Returns("Credit Card");
            _mockReader.Setup(r => r.GetDateTime(7)).Returns(orderDate);

            // Setup standard method
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = _orderModel.GetOrdersFrom2025(buyerId);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(101, results[0].OrderID);
            Assert.AreEqual(orderDate, results[0].OrderDate);

            // Verify stored procedure call
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "get_orders_from_2025");

            // Verify parameters
            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);

            // Verify non-extension methods
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
        }


        [TestMethod]
        public async Task GetOrdersFromOrderHistoryAsync_ReturnsCorrectData()
        {
            // Arrange
            int orderHistoryId = 42;
            DateTime orderDate = DateTime.Now;

            // Setup parameter capture
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductID")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("BuyerID")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderHistoryID")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(7);

            // Setup Read calls
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            // Setup IsDBNull for PaymentMethod and OrderDate
            _mockReader.Setup(r => r.IsDBNull(6)).Returns(false);
            _mockReader.Setup(r => r.IsDBNull(7)).Returns(false);

            // First order
            _mockReader.Setup(r => r.GetInt32(0)).Returns(101);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(201);
            _mockReader.Setup(r => r.GetInt32(2)).Returns(301);
            _mockReader.Setup(r => r.GetInt32(3)).Returns(401);
            _mockReader.Setup(r => r.GetInt32(4)).Returns(orderHistoryId);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(1);
            _mockReader.Setup(r => r.GetString(6)).Returns("Credit Card");
            _mockReader.Setup(r => r.GetDateTime(7)).Returns(orderDate);

            // Setup standard method
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = await _orderModel.GetOrdersFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(101, results[0].OrderID);
            Assert.AreEqual(201, results[0].ProductID);
            Assert.AreEqual(301, results[0].BuyerID);
            Assert.AreEqual(401, results[0].OrderSummaryID);
            Assert.AreEqual(orderHistoryId, results[0].OrderHistoryID);
            Assert.AreEqual(1, results[0].ProductType);
            Assert.AreEqual("Credit Card", results[0].PaymentMethod);
            Assert.AreEqual(orderDate, results[0].OrderDate);

            // Verify stored procedure call
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "get_orders_from_order_history");

            // Verify parameters
            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@OrderHistoryID", orderHistoryId);

            // Verify non-extension methods
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromOrderHistoryAsync_HandlesNullValues()
        {
            // Arrange
            int orderHistoryId = 42;

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductID")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("BuyerID")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderHistoryID")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(7);

            // Setup Read calls
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            // Setup IsDBNull for PaymentMethod and OrderDate
            _mockReader.Setup(r => r.IsDBNull(6)).Returns(true);
            _mockReader.Setup(r => r.IsDBNull(7)).Returns(true);

            // First order
            _mockReader.Setup(r => r.GetInt32(0)).Returns(101);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(201);
            _mockReader.Setup(r => r.GetInt32(2)).Returns(301);
            _mockReader.Setup(r => r.GetInt32(3)).Returns(401);
            _mockReader.Setup(r => r.GetInt32(4)).Returns(orderHistoryId);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(1);

            // Setup standard method
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = await _orderModel.GetOrdersFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("", results[0].PaymentMethod);
            Assert.AreEqual(DateTime.MinValue, results[0].OrderDate);
        }

        [TestMethod]
        public void Constructor_SetsConnectionString()
        {
            // Arrange & Act
            var model = new OrderModel(_testConnectionString);

            // Assert - using reflection to access private field
            var field = typeof(OrderModel).GetField("connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var value = field.GetValue(model);

            Assert.AreEqual(_testConnectionString, value);
        }

        [TestMethod]
        public void ConnectionString_Property_ReturnsCorrectValue()
        {
            // Arrange
            string expectedConnectionString = _testConnectionString;

            // Act
            string actualConnectionString = _orderModel.ConnectionString;

            // Assert
            Assert.AreEqual(expectedConnectionString, actualConnectionString,
                "The ConnectionString property should return the value of the private _connectionString field");
        }


        private void SetupMockDataReader(List<Dictionary<string, object>> data)
        {
            _mockReader.Setup(r => r.Read())
                .Returns(() => data.Count > 0)
                .Callback(() => {
                    if (data.Count > 0) data.RemoveAt(0);
                });

            foreach (var record in data)
            {
                foreach (var column in record.Keys)
                {
                    _mockReader.Setup(r => r.GetOrdinal(column)).Returns(0);

                    if (record[column] is int intValue)
                        _mockReader.Setup(r => r.GetInt32(0)).Returns(intValue);

                    if (record[column] is DateTime dateValue)
                        _mockReader.Setup(r => r.GetDateTime(0)).Returns(dateValue);

                    if (record[column] is string stringValue)
                        _mockReader.Setup(r => r.GetString(0)).Returns(stringValue);

                    _mockReader.Setup(r => r[column]).Returns(record[column]);
                }
            }

            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);
        }

        private void SetupOutputParameter(string paramName, object value)
        {
            _mockParameter.Setup(p => p.Direction).Returns(ParameterDirection.Output);
            _mockParameter.SetupProperty(p => p.Value);
            _mockParameter.Setup(p => p.ParameterName).Returns(paramName);

            _mockCommand.Setup(c => c.ExecuteNonQuery())
                .Callback(() => {
                    _mockParameter.Object.Value = value;
                })
                .Returns(1);
        }
    }
}
