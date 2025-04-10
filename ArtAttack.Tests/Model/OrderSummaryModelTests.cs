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
    public class OrderSummaryModelTests
    {
        private Mock<IDatabaseProvider> _mockDatabaseProvider;
        private Mock<IDbConnection> _mockDatabaseConnection;
        private Mock<IDbCommand> _mockDatabaseCommand;
        private Mock<IDataReader> _mockDataReader;
        private Mock<IDataParameterCollection> _mockParameterCollection;
        private Mock<IDbDataParameter> _mockDatabaseParameter;
        private OrderSummaryModel _orderSummaryModel;
        private string _testConnectionString = "TestConnectionString";

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockDatabaseProvider = new Mock<IDatabaseProvider>();
            _mockDatabaseConnection = new Mock<IDbConnection>();
            _mockDatabaseCommand = new Mock<IDbCommand>();
            _mockDataReader = new Mock<IDataReader>();
            _mockParameterCollection = new Mock<IDataParameterCollection>();
            _mockDatabaseParameter = new Mock<IDbDataParameter>();

            // Setup parameter collection
            _mockParameterCollection.Setup(parameters => parameters.Add(It.IsAny<object>())).Returns(0);

            // Setup command to create parameters and return the parameter collection
            _mockDatabaseCommand.Setup(command => command.CreateParameter()).Returns(_mockDatabaseParameter.Object);
            _mockDatabaseCommand.Setup(command => command.Parameters).Returns(_mockParameterCollection.Object);

            // Setup connection to create commands
            _mockDatabaseConnection.Setup(connection => connection.CreateCommand()).Returns(_mockDatabaseCommand.Object);

            // Setup database provider to return the connection
            _mockDatabaseProvider.Setup(provider => provider.CreateConnection(It.IsAny<string>()))
                .Returns(_mockDatabaseConnection.Object);

            // Initialize the model with the mocked provider and connection string
            _orderSummaryModel = new OrderSummaryModel(_testConnectionString, _mockDatabaseProvider.Object);
        }

        /// <summary>
        /// Helper method to set up the capture of parameter names and values.
        /// Whenever a parameter’s ParameterName and Value are set,
        /// the provided dictionary will record the value by its name.
        /// </summary>
        /// <returns>A dictionary of captured parameter names and values.</returns>
        private Dictionary<string, object> CaptureParameterValues()
        {
            var capturedParameterValues = new Dictionary<string, object>();

            _mockDatabaseParameter.SetupSet(parameter => parameter.ParameterName = It.IsAny<string>())
                .Callback<string>(parameterName =>
                {
                    _mockDatabaseParameter.Setup(parameter => parameter.ParameterName).Returns(parameterName);
                });
            _mockDatabaseParameter.SetupSet(parameter => parameter.Value = It.IsAny<object>())
                .Callback<object>(parameterValue =>
                {
                    string currentParameterName = _mockDatabaseParameter.Object.ParameterName;
                    capturedParameterValues[currentParameterName] = parameterValue;
                });

            return capturedParameterValues;
        }

        [TestMethod]
        public void Constructor_WhenWithConnectionString_InitializesCorrectly()
        {
            // Act
            var model = new OrderSummaryModel(_testConnectionString);

            // Assert
            Assert.IsNotNull(model);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WhenWithNullConnectionString_ThrowsArgumentNullException()
        {
            // Act
            new OrderSummaryModel(null, _mockDatabaseProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WhenWithNullDatabaseProvider_ThrowsArgumentNullException()
        {
            // Act
            new OrderSummaryModel(_testConnectionString, null);
        }

        [TestMethod]
        public async Task AddOrderSummaryAsync_ExecutesCorrectCommand()
        {
            // Arrange
            float subtotal = 100.0f;
            float warrantyTax = 10.0f;
            float deliveryFee = 5.0f;
            float finalTotal = 115.0f;
            string fullName = "John Doe";
            string email = "john@example.com";
            string phoneNumber = "1234567890";
            string address = "123 Main St";
            string postalCode = "12345";
            string additionalInfo = "Some additional info";
            string contractDetails = "Contract details";

            Dictionary<string, object> capturedParameterValues = CaptureParameterValues();

            // Act
            await _orderSummaryModel.AddOrderSummaryAsync(
                subtotal, warrantyTax, deliveryFee, finalTotal, fullName, email,
                phoneNumber, address, postalCode, additionalInfo, contractDetails);

            // Assert
            _mockDatabaseCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockDatabaseCommand.VerifySet(command => command.CommandText = "AddOrderSummary");
            _mockDatabaseConnection.Verify(connection => connection.Open(), Times.Once);
            _mockDatabaseCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            // We expect 11 parameters to be set in total
            Assert.AreEqual(11, _mockDatabaseParameter.Invocations.Count(invocation => invocation.Method.Name == "set_ParameterName"));

            // Verify specific key parameters
            Assert.IsTrue(capturedParameterValues.ContainsKey("@Subtotal"));
            Assert.AreEqual(subtotal, capturedParameterValues["@Subtotal"]);

            Assert.IsTrue(capturedParameterValues.ContainsKey("@FullName"));
            Assert.AreEqual(fullName, capturedParameterValues["@FullName"]);

            Assert.IsTrue(capturedParameterValues.ContainsKey("@Email"));
            Assert.AreEqual(email, capturedParameterValues["@Email"]);

            Assert.IsTrue(capturedParameterValues.ContainsKey("@ContractDetails"));
            Assert.AreEqual(contractDetails, capturedParameterValues["@ContractDetails"]);
        }

        [TestMethod]
        public async Task AddOrderSummaryAsync_WhenWithNullContractDetails_UsesDBNull()
        {
            // Arrange
            Dictionary<string, object> capturedParameterValues = CaptureParameterValues();

            // Act
            await _orderSummaryModel.AddOrderSummaryAsync(
                100.0f, 10.0f, 5.0f, 115.0f, "John", "email", "phone",
                "address", "postal", "info", null);

            // Assert that @ContractDetails was set to DBNull.Value when null
            Assert.IsTrue(capturedParameterValues.ContainsKey("@ContractDetails"));
            Assert.AreEqual(DBNull.Value, capturedParameterValues["@ContractDetails"]);
        }

        [TestMethod]
        public async Task DeleteOrderSummaryAsync_ExecutesCorrectCommand()
        {
            // Arrange
            int id = 1;
            Dictionary<string, object> capturedParameterValues = CaptureParameterValues();

            // Act
            await _orderSummaryModel.DeleteOrderSummaryAsync(id);

            // Assert
            _mockDatabaseCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockDatabaseCommand.VerifySet(command => command.CommandText = "DeleteOrderSummary");
            _mockDatabaseConnection.Verify(connection => connection.Open(), Times.Once);
            _mockDatabaseCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            // We expect 1 parameter to be set
            Assert.AreEqual(1, _mockDatabaseParameter.Invocations.Count(invocation => invocation.Method.Name == "set_ParameterName"));
            Assert.IsTrue(capturedParameterValues.ContainsKey("@ID"));
            Assert.AreEqual(id, capturedParameterValues["@ID"]);
        }

    }
}