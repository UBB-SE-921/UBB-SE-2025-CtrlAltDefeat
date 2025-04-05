using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Data;  // Use System.Data interfaces
using System.Threading;

namespace ArtAttack.Tests.Model
{
    [TestClass]
    public class OrderHistoryModelTests
    {
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private Mock<IDbDataParameter> _mockParameter;
        private string _testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";
        private OrderHistoryModel _orderHistoryModel;
        private MockDatabase _mockDatabase;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockReader = new Mock<IDataReader>();
            _mockParameters = new Mock<IDataParameterCollection>();
            _mockParameter = new Mock<IDbDataParameter>();

            // Setup the parameter collection mock
            _mockParameters.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

            // Setup the command mock
            _mockCommand.Setup(c => c.CreateParameter()).Returns(_mockParameter.Object);
            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object);

            // Setup the connection mock
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

            // Setup the mock database 
            _mockDatabase = new MockDatabase();
            _mockDatabase.SetupMockConnection(_mockConnection.Object);

            // Initialize the model with the mock database
            _orderHistoryModel = new OrderHistoryModel(_testConnectionString, _mockDatabase);
        }

        [TestMethod]
        public void Constructor_SetsConnectionString()
        {
            // Arrange & Act
            var model = new OrderHistoryModel(_testConnectionString);

            // Assert - using reflection to access private field
            var field = typeof(OrderHistoryModel).GetField("connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var value = field.GetValue(model);

            Assert.AreEqual(_testConnectionString, value);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int orderHistoryId = 42;

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);

            // Don't use extension methods in setup
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);
            _mockReader.Setup(r => r.Read()).Returns(false);  // Empty result

            // Act
            var result = await _orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "GetDummyProductsFromOrderHistory");
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);  // Verify standard method call
            _mockConnection.Verify(c => c.Open(), Times.Once);  // Verify standard method call
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_AddsCorrectParameter()
        {
            // Arrange
            int orderHistoryId = 42;

            // Don't use extension methods in setup
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);
            _mockReader.Setup(r => r.Read()).Returns(false);  // Empty result

            List<(string Name, object Value)> capturedParams = new List<(string, object)>();
            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(name => capturedParams.Add((name, null)));
            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(val => capturedParams[capturedParams.Count - 1] = (capturedParams[capturedParams.Count - 1].Name, val));

            // Act
            var result = await _orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.AreEqual(1, capturedParams.Count);
            Assert.AreEqual("@OrderHistory", capturedParams[0].Name);
            Assert.AreEqual(orderHistoryId, capturedParams[0].Value);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_ReturnsCorrectData()
        {
            // Arrange
            int orderHistoryId = 42;
            DateTime today = DateTime.Today;

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("productID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("name")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("price")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("productType")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("SellerID")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("startDate")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("endDate")).Returns(6);

            // Setup Read calls (without using ReadAsync)
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)   // First record
                .Returns(true)   // Second record
                .Returns(false); // End of data

            // First product
            _mockReader.SetupSequence(r => r.GetInt32(0))
                .Returns(101)
                .Returns(102);
            _mockReader.SetupSequence(r => r.GetString(1))
                .Returns("Test Product 1")
                .Returns("Test Product 2");
            _mockReader.SetupSequence(r => r.GetDouble(2))
                .Returns(19.99)
                .Returns(29.99);
            _mockReader.SetupSequence(r => r.GetString(3))
                .Returns("purchase")
                .Returns("borrowed");

            // Set up indexer for DBNull checks
            _mockReader.SetupSequence(r => r["SellerID"])
                .Returns(501)
                .Returns(DBNull.Value);
            _mockReader.SetupSequence(r => r["startDate"])
                .Returns(today)
                .Returns(DBNull.Value);
            _mockReader.SetupSequence(r => r["endDate"])
                .Returns(today.AddDays(30))
                .Returns(DBNull.Value);

            // SellerID
            _mockReader.SetupSequence(r => r.GetInt32(4))
                .Returns(501);
            // startDate
            _mockReader.SetupSequence(r => r.GetDateTime(5))
                .Returns(today);
            // endDate
            _mockReader.SetupSequence(r => r.GetDateTime(6))
                .Returns(today.AddDays(30));

            // Setup standard method (not extension)
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = await _orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.AreEqual(2, results.Count);

            // First product
            Assert.AreEqual(101, results[0].ID);
            Assert.AreEqual("Test Product 1", results[0].Name);
            Assert.AreEqual(19.99f, results[0].Price);
            Assert.AreEqual("purchase", results[0].ProductType);
            Assert.AreEqual(501, results[0].SellerID);
            Assert.AreEqual(today, results[0].StartDate);
            Assert.AreEqual(today.AddDays(30), results[0].EndDate);

            // Second product - with DBNull values
            Assert.AreEqual(102, results[1].ID);
            Assert.AreEqual("Test Product 2", results[1].Name);
            Assert.AreEqual(29.99f, results[1].Price);
            Assert.AreEqual("borrowed", results[1].ProductType);
            Assert.AreEqual(0, results[1].SellerID); // Default for DBNull
            Assert.AreEqual(DateTime.MinValue, results[1].StartDate); // Default for DBNull
            Assert.AreEqual(DateTime.MaxValue, results[1].EndDate); // Default for DBNull
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_ReturnsEmptyList_WhenNoData()
        {
            // Arrange
            int orderHistoryId = 42;

            // Use standard methods instead of extension methods
            _mockReader.Setup(r => r.Read()).Returns(false);
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var result = await _orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetDummyProductsFromOrderHistoryAsync_ThrowsException_WhenExecutionFails()
        {
            // Arrange
            int orderHistoryId = 42;

            // Use standard method, not extension
            _mockCommand.Setup(c => c.ExecuteReader()).Throws(new Exception("Database error"));

            // Act & Assert
            await _orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);
        }
    }
}
