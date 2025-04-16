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
        private Mock<IDbConnection> mockConnection;
        private Mock<IDbCommand> mockCommand;
        private Mock<IDataReader> mockReader;
        private Mock<IDataParameterCollection> mockParameters;
        private Mock<IDbDataParameter> mockParameter;
        private string testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";
        private OrderHistoryModel orderHistoryModel;
        private MockDatabase mockDatabase;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            mockConnection = new Mock<IDbConnection>();
            mockCommand = new Mock<IDbCommand>();
            mockReader = new Mock<IDataReader>();
            mockParameters = new Mock<IDataParameterCollection>();
            mockParameter = new Mock<IDbDataParameter>();

            // Setup the parameter collection mock
            mockParameters.Setup(parameter => parameter.Add(It.IsAny<object>())).Returns(0);

            // Setup the command mock
            mockCommand.Setup(command => command.CreateParameter()).Returns(mockParameter.Object);
            mockCommand.Setup(command => command.Parameters).Returns(mockParameters.Object);

            // Setup the connection mock
            mockConnection.Setup(connection => connection.CreateCommand()).Returns(mockCommand.Object);

            // Setup the mock database 
            mockDatabase = new MockDatabase();
            mockDatabase.SetupMockConnection(mockConnection.Object);

            // Initialize the model with the mock database
            orderHistoryModel = new OrderHistoryModel(testConnectionString, mockDatabase);
        }

        [TestMethod]
        public void Constructor_ShouldSetConnectionString()
        {
            // Arrange & Act
            var testOrderHistoryModel = new OrderHistoryModel(testConnectionString);

            // Assert - using reflection to access private field
            var connectionStringField = typeof(OrderHistoryModel).GetField("connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var connectionStringValue = connectionStringField.GetValue(testOrderHistoryModel);

            Assert.AreEqual(testConnectionString, connectionStringValue);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_ShouldExecuteCorrectStoredProcedure()
        {
            // Arrange
            int orderHistoryId = 42;

            mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);

            // Don't use extension methods in setup
            mockCommand.Setup(command => command.ExecuteReader()).Returns(mockReader.Object);
            mockReader.Setup(reader => reader.Read()).Returns(false);  // Empty result

            // Act
            var result = await orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockCommand.VerifySet(command => command.CommandText = "GetDummyProductsFromOrderHistory");
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_ShouldAddCorrectParameter()
        {
            // Arrange
            int orderHistoryId = 42;

            // Don't use extension methods in setup
            mockCommand.Setup(command => command.ExecuteReader()).Returns(mockReader.Object);
            mockReader.Setup(reader => reader.Read()).Returns(false);  // Empty result

            List<(string Name, object Value)> capturedParameters = new List<(string, object)>();
            mockParameter.SetupSet(parameter => parameter.ParameterName = It.IsAny<string>())
                .Callback<string>(parameterName => capturedParameters.Add((parameterName, null)));
            mockParameter.SetupSet(parameter => parameter.Value = It.IsAny<object>())
                .Callback<object>(parameterValue => capturedParameters[capturedParameters.Count - 1] = (capturedParameters[capturedParameters.Count - 1].Name, parameterValue));

            // Act
            var result = await orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.AreEqual(1, capturedParameters.Count);
            Assert.AreEqual("@OrderHistory", capturedParameters[0].Name);
            Assert.AreEqual(orderHistoryId, capturedParameters[0].Value);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_ShouldReturnCorrectData()
        {
            // Arrange
            int orderHistoryId = 42;
            DateTime today = DateTime.Today;

            // Setup column ordinals
            mockReader.Setup(reader => reader.GetOrdinal("productID")).Returns(0);
            mockReader.Setup(reader => reader.GetOrdinal("name")).Returns(1);
            mockReader.Setup(reader => reader.GetOrdinal("price")).Returns(2);
            mockReader.Setup(reader => reader.GetOrdinal("productType")).Returns(3);
            mockReader.Setup(reader => reader.GetOrdinal("SellerID")).Returns(4);
            mockReader.Setup(reader => reader.GetOrdinal("startDate")).Returns(5);
            mockReader.Setup(reader => reader.GetOrdinal("endDate")).Returns(6);

            // Setup Read calls (without using ReadAsync)
            mockReader.SetupSequence(reader => reader.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            // First product
            mockReader.SetupSequence(reader => reader.GetInt32(0))
                .Returns(101);
            mockReader.SetupSequence(reader => reader.GetString(1))
                .Returns("Test Product 1"); ;
            mockReader.SetupSequence(reader => reader.GetDouble(2))
                .Returns(19.99);
            mockReader.SetupSequence(reader => reader.GetString(3))
                .Returns("purchase");
            mockReader.SetupSequence(reader => reader["SellerID"])
                .Returns(501);
            mockReader.SetupSequence(reader => reader["startDate"])
                .Returns(today);
            mockReader.SetupSequence(reader => reader["endDate"])
                .Returns(today.AddDays(30));

            // SellerID
            mockReader.SetupSequence(reader => reader.GetInt32(4))
                .Returns(501);
            // startDate
            mockReader.SetupSequence(reader => reader.GetDateTime(5))
                .Returns(today);
            // endDate
            mockReader.SetupSequence(reader => reader.GetDateTime(6))
                .Returns(today.AddDays(30));

            // Setup standard method (not extension)
            mockCommand.Setup(command => command.ExecuteReader()).Returns(mockReader.Object);

            // Act
            var results = await orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.AreEqual(1, results.Count);

            Assert.AreEqual(101, results[0].ID);
            Assert.AreEqual("Test Product 1", results[0].Name);
            Assert.AreEqual(19.99f, results[0].Price);
            Assert.AreEqual("purchase", results[0].ProductType);
            Assert.AreEqual(501, results[0].SellerID);
            Assert.AreEqual(today, results[0].StartDate);
            Assert.AreEqual(today.AddDays(30), results[0].EndDate);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_WhenNoData_ReturnsEmptyList()
        {
            // Arrange
            int orderHistoryId = 42;

            mockReader.Setup(reader => reader.Read()).Returns(false);
            mockCommand.Setup(command => command.ExecuteReader()).Returns(mockReader.Object);

            // Act
            var result = await orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetDummyProductsFromOrderHistoryAsync_WhenExecutionFails_ShouldThrowException()
        {
            // Arrange
            int orderHistoryId = 42;

            mockCommand.Setup(command => command.ExecuteReader()).Throws(new Exception("Database error"));

            // Act & Assert
            await orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);
        }
    }
}
