using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Threading;  // For CancellationToken

using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using System.Linq;

namespace ArtAtackTests
{
    [TestClass]
    public class DummyProductModelTests
    {
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private Mock<IDbDataParameter> _mockParameter;
        private string _testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";
        private DummyProductModel _dummyProductModel;
        private MockDatabase _mockDatabase;

        // Helper method to check parameter values
        private void AssertParameterValue(List<IDbDataParameter> parameters, string paramName, object expectedValue)
        {
            var param = parameters.FirstOrDefault(p => p.ParameterName == paramName);
            Assert.IsNotNull(param, $"Parameter {paramName} not found");
            Assert.AreEqual(expectedValue, param.Value, $"Parameter {paramName} has incorrect value");
        }


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
            _mockCommand.Setup(c => c.CreateParameter()).Returns(() =>
            {
                var mockParam = new Mock<IDbDataParameter>();
                mockParam.SetupAllProperties(); // enables .ParameterName and .Value to store values
                return mockParam.Object;
            });

            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object);

            // Setup the connection mock
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

            // Setup the mock database 
            _mockDatabase = new MockDatabase();
            _mockDatabase.SetupMockConnection(_mockConnection.Object);

            // Initialize the model with the mock database
            _dummyProductModel = new DummyProductModel(_testConnectionString, _mockDatabase);
        }

        [TestMethod]
        public void Constructor_SetsConnectionString()
        {
            // Arrange & Act
            var model = new DummyProductModel(_testConnectionString);

            // Assert - using reflection to access private field
            var field = typeof(DummyProductModel).GetField("_connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var value = field.GetValue(model);

            Assert.AreEqual(_testConnectionString, value);
        }

        [TestMethod]
        public async Task AddDummyProductAsync_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            var name = "Test Product";
            var price = 29.99f;
            var sellerId = 1001;
            var productType = "Electronics";
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(30);

            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters.Setup(parameter => parameter.Add(It.IsAny<IDbDataParameter>())).Callback<object>(parameters => capturedParameters.Add((IDbDataParameter)parameters)).Returns(0);

            //// Mock the behavior of the connection and command
            //_mockCommand.Setup(cmd => cmd.CommandType).Returns(CommandType.StoredProcedure);
            //_mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1); 


            // Act
            await _dummyProductModel.AddDummyProductAsync(name, price, sellerId, productType, startDate, endDate);

            // Assert
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "AddDummyProduct", Times.Once);
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(command => command.Open(), Times.Once);

            Assert.AreEqual(6, capturedParameters.Count, "Should have added 6 parameters");
            AssertParameterValue(capturedParameters, "@name", name);
            AssertParameterValue(capturedParameters, "@price", price);
            AssertParameterValue(capturedParameters, "@SellerID", sellerId);
            AssertParameterValue(capturedParameters, "@productType", productType);
            AssertParameterValue(capturedParameters, "@startDate", startDate);
            AssertParameterValue(capturedParameters, "@endDate", endDate);
            _mockConnection.Verify(c => c.Close(), Times.Once);
        }


        [TestMethod]
        public async Task UpdateDummyProductAsync_ExecuteCorrectStoredProcedure()
        {
            // Arrange
            int id = 1;
            string name = "Dummy Product";
            float price = 19.99f;
            int sellerId = 2;
            string productType = "Electronics";
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now.AddMonths(1);

            // Act
            await _dummyProductModel.UpdateDummyProductAsync(id, name, price, sellerId, productType, startDate, endDate);

            // Assert
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure, Times.Once);
            _mockCommand.VerifySet(command => command.CommandText = "UpdateDummyProduct", Times.Once);

            // Verify parameters were added correctly
            _mockCommand.Verify(command => command.Parameters.Add(It.Is<IDbDataParameter>(parameter => parameter.ParameterName == "@ID" && (int)parameter.Value == id)), Times.Once);
            _mockCommand.Verify(command => command.Parameters.Add(It.Is<IDbDataParameter>(parameter => parameter.ParameterName == "@Name" && (string)parameter.Value == name)), Times.Once);
            _mockCommand.Verify(command => command.Parameters.Add(It.Is<IDbDataParameter>(parameter => parameter.ParameterName == "@Price" && (float)parameter.Value == price)), Times.Once);
            _mockCommand.Verify(command => command.Parameters.Add(It.Is<IDbDataParameter>(parameter => parameter.ParameterName == "@SellerID" && (int)parameter.Value == sellerId)), Times.Once);
            _mockCommand.Verify(command => command.Parameters.Add(It.Is<IDbDataParameter>(parameter => parameter.ParameterName == "@ProductType" && (string)parameter.Value == productType)), Times.Once);
            _mockCommand.Verify(command => command.Parameters.Add(It.Is<IDbDataParameter>(parameter => parameter.ParameterName == "@StartDate" && (DateTime)parameter.Value == startDate)), Times.Once);
            _mockCommand.Verify(command => command.Parameters.Add(It.Is<IDbDataParameter>(parameter => parameter.ParameterName == "@EndDate" && (DateTime)parameter.Value == endDate)), Times.Once);

            // Verify that ExecuteNonQueryAsync is called once
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }


        [TestMethod]
        public async Task DeleteDummyProductAsync_ExecutesCorrectStoredProcedure()
        {
            //Arange
            var id = 1;

            //Act
            await _dummyProductModel.DeleteDummyProduct(id);

            //Assert
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure, Times.Once);
            _mockCommand.VerifySet(command => command.CommandText = "DeleteDummyProduct", Times.Once);

            //Verify the parameter was added correctly
            _mockCommand.Verify(command => command.Parameters.Add(It.Is<IDbDataParameter>(parameter => parameter.ParameterName == "@ID")));

            //Verify that ExecuteNonQueryAsync is called once
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
        }
#nullable enable
        [TestMethod]
        public async Task GetSellerNameAsync_ReturnsCorrectSellerName()
        {
            // Arrange
            int sellerId = 1;
            string expectedName = "John Doe";

            // Mock the behavior of the reader
            _mockReader.SetupSequence(reader => reader.Read())
                .Returns(true)  // First call returns true (indicating data exists)
                .Returns(false); // Second call returns false (end of data)
            _mockReader.Setup(reader => reader["Name"]).Returns(expectedName);

            // Mock the command to return the reader
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            string? result = await _dummyProductModel.GetSellerNameAsync(sellerId);

            // Assert
            if (result == null)
            {
                Assert.IsNull(result, "Expected null when no data is found.");
            }
            else
            {
                Assert.AreEqual(expectedName, result);
            }

            // Verify that the connection was opened and closed
            _mockConnection.Verify(command => command.Open(), Times.Once);
        }
#nullable disable
        [TestMethod]
        public async Task GetDummyProductByIdAsync_ReturnsCorrectDummyProduct()
        {
            // Arrange
            int productId = 42;
            string expectedName = "Mock Product";
            double expectedPrice = 49.99; 
            int expectedSellerId = 101;
            string expectedProductType = "Art";
            DateTime expectedStartDate = new DateTime(2025, 1, 1);
            DateTime expectedEndDate = new DateTime(2025, 12, 31);

            await _dummyProductModel.AddDummyProductAsync(expectedName, (float)expectedPrice, expectedSellerId, expectedProductType, expectedStartDate, expectedEndDate);

            // Setup mock reader
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            _mockReader.Setup(reader => reader.GetInt32(0)).Returns(productId); // ID
            _mockReader.Setup(reader => reader.GetString(1)).Returns(expectedName); // Name
            _mockReader.Setup(reader => reader.GetDouble(2)).Returns(expectedPrice); // Price
            _mockReader.Setup(reader => reader.IsDBNull(3)).Returns(false);
            _mockReader.Setup(reader => reader.GetInt32(3)).Returns(expectedSellerId); // SellerID
            _mockReader.Setup(reader => reader.GetString(4)).Returns(expectedProductType); // ProductType
            _mockReader.Setup(reader => reader.IsDBNull(5)).Returns(false);
            _mockReader.Setup(reader => reader.GetDateTime(5)).Returns(expectedStartDate); // StartDate
            _mockReader.Setup(reader => reader.IsDBNull(6)).Returns(false);
            _mockReader.Setup(reader => reader.GetDateTime(6)).Returns(expectedEndDate); // EndDate

            // Setup command and connection
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);
            _mockConnection.Setup(command => command.Open());
            _mockConnection.Setup(command => command.Close());

            // Act
            var result = await _dummyProductModel.GetDummyProductByIdAsync(productId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(productId, result.ID);
            Assert.AreEqual(expectedName, result.Name);
            Assert.AreEqual((float)expectedPrice, result.Price);
            Assert.AreEqual(expectedSellerId, result.SellerID);
            Assert.AreEqual(expectedProductType, result.ProductType);
            Assert.AreEqual(expectedStartDate, result.StartDate);
            Assert.AreEqual(expectedEndDate, result.EndDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetSellerNameAsync_ThrowsArgumentNullException_WhenSellerIdIsNull()
        {
            // Act
            await _dummyProductModel.GetSellerNameAsync(null);
        }

        [TestMethod]
        public async Task GetSellerNameAsync_ReturnsNull_WhenNoSellerFound()
        {
            // Arrange
            int sellerId = 999;
            _mockReader.Setup(reader => reader.Read()).Returns(false);
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var result = await _dummyProductModel.GetSellerNameAsync(sellerId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetSellerByIdAsync_ReturnsNull_WhenNoSellerFound()
        {
            // Arrange
            int sellerId = 999;
            _mockReader.Setup(reader => reader.Read()).Returns(false);
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var result = await _dummyProductModel.GetSellerNameAsync(sellerId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_WhenConnectionStringIsNull()
        {
            // Act
            var model = new DummyProductModel(null, _mockDatabase);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_WhenDatabaseProviderIsNull()
        {
            // Act
            var model = new DummyProductModel(_testConnectionString, null);
        }

        [TestMethod]
        public async Task GetDummyProductByIdAsync_ReturnsNull_WhenNoRecordFound()
        {
            // Arrange
            int productId = 99;

            // Simulate no rows returned
            _mockReader.Setup(reader => reader.Read()).Returns(false);

            // Set up the command to return the mock reader
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);
            _mockConnection.Setup(command => command.Open());
            _mockConnection.Setup(command => command.Close());

            // Act
            var result = await _dummyProductModel.GetDummyProductByIdAsync(productId);

            // Assert
            Assert.IsNull(result);
            _mockConnection.Verify(command => command.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
        }

    }
}
