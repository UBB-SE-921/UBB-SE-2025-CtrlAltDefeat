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
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private Mock<IDbDataParameter> _mockParameter;
        private OrderSummaryModel _orderSummaryModel;
        private string _testConnectionString = "TestConnectionString";

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockDatabaseProvider = new Mock<IDatabaseProvider>();
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockReader = new Mock<IDataReader>();
            _mockParameters = new Mock<IDataParameterCollection>();
            _mockParameter = new Mock<IDbDataParameter>();

            // Setup parameter collection
            _mockParameters.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

            // Setup command
            _mockCommand.Setup(c => c.CreateParameter()).Returns(_mockParameter.Object);
            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object);

            // Setup connection
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

            // Setup database provider
            _mockDatabaseProvider.Setup(dp => dp.CreateConnection(It.IsAny<string>()))
                .Returns(_mockConnection.Object);

            // Initialize model with mocked dependencies
            _orderSummaryModel = new OrderSummaryModel(_testConnectionString, _mockDatabaseProvider.Object);
        }

        [TestMethod]
        public void Constructor_WithConnectionString_InitializesCorrectly()
        {
            // Act
            var model = new OrderSummaryModel(_testConnectionString);

            // Assert
            Assert.IsNotNull(model);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Act
            new OrderSummaryModel(null, _mockDatabaseProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullDatabaseProvider_ThrowsArgumentNullException()
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

            // Setup parameter name capture
            Dictionary<string, object> capturedParameterValues = new Dictionary<string, object>();

            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(name => _mockParameter.Setup(p => p.ParameterName).Returns(name));

            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(value =>
                {
                    string paramName = _mockParameter.Object.ParameterName;
                    capturedParameterValues[paramName] = value;
                });

            // Act
            await _orderSummaryModel.AddOrderSummaryAsync(
                subtotal, warrantyTax, deliveryFee, finalTotal, fullName, email,
                phoneNumber, address, postalCode, additionalInfo, contractDetails);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "AddOrderSummary");
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Verify parameters - check count and key parameters
            Assert.AreEqual(11, _mockParameter.Invocations.Count(i => i.Method.Name == "set_ParameterName"));

            // Check specific parameters
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
        public async Task AddOrderSummaryAsync_WithNullContractDetails_UsesDBNull()
        {
            // Arrange
            Dictionary<string, object> capturedParameterValues = new Dictionary<string, object>();

            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(name => _mockParameter.Setup(p => p.ParameterName).Returns(name));

            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(value =>
                {
                    string paramName = _mockParameter.Object.ParameterName;
                    capturedParameterValues[paramName] = value;
                });

            // Act
            await _orderSummaryModel.AddOrderSummaryAsync(
                100.0f, 10.0f, 5.0f, 115.0f, "John", "email", "phone",
                "address", "postal", "info", null);

            // Assert
            Assert.IsTrue(capturedParameterValues.ContainsKey("@ContractDetails"));
            Assert.AreEqual(DBNull.Value, capturedParameterValues["@ContractDetails"]);
        }

        [TestMethod]
        public async Task DeleteOrderSummaryAsync_ExecutesCorrectCommand()
        {
            // Arrange
            int id = 1;

            Dictionary<string, object> capturedParameterValues = new Dictionary<string, object>();

            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(name => _mockParameter.Setup(p => p.ParameterName).Returns(name));

            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(value =>
                {
                    string paramName = _mockParameter.Object.ParameterName;
                    capturedParameterValues[paramName] = value;
                });

            // Act
            await _orderSummaryModel.DeleteOrderSummaryAsync(id);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "DeleteOrderSummary");
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Verify parameter
            Assert.AreEqual(1, _mockParameter.Invocations.Count(i => i.Method.Name == "set_ParameterName"));
            Assert.IsTrue(capturedParameterValues.ContainsKey("@ID"));
            Assert.AreEqual(id, capturedParameterValues["@ID"]);
        }


        [TestMethod]
        public async Task UpdateOrderSummaryAsync_ExecutesCorrectCommand()
        {
            // Arrange
            int id = 1;
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

            // Setup parameter name capture
            Dictionary<string, object> capturedParameterValues = new Dictionary<string, object>();

            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(name => _mockParameter.Setup(p => p.ParameterName).Returns(name));

            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(value =>
                {
                    string paramName = _mockParameter.Object.ParameterName;
                    capturedParameterValues[paramName] = value;
                });

            // Act
            await _orderSummaryModel.UpdateOrderSummaryAsync(
                id, subtotal, warrantyTax, deliveryFee, finalTotal, fullName, email,
                phoneNumber, address, postalCode, additionalInfo, contractDetails);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "UpdateOrderSummary");
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Verify parameters - just check quantity and a few key parameters
            Assert.AreEqual(12, _mockParameter.Invocations.Count(i => i.Method.Name == "set_ParameterName"));

            // Check specific parameters
            Assert.IsTrue(capturedParameterValues.ContainsKey("@ID"));
            Assert.AreEqual(id, capturedParameterValues["@ID"]);
            Assert.IsTrue(capturedParameterValues.ContainsKey("@FullName"));
            Assert.AreEqual(fullName, capturedParameterValues["@FullName"]);
            Assert.IsTrue(capturedParameterValues.ContainsKey("@Email"));
            Assert.AreEqual(email, capturedParameterValues["@Email"]);
        }

        [TestMethod]
        public async Task UpdateOrderSummaryAsync_WithNullValues_UsesDBNull()
        {
            // Arrange
            // Setup parameter name and value capture
            Dictionary<string, object> capturedParameterValues = new Dictionary<string, object>();

            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(name => _mockParameter.Setup(p => p.ParameterName).Returns(name));

            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(value =>
                {
                    string paramName = _mockParameter.Object.ParameterName;
                    capturedParameterValues[paramName] = value;
                });

            // Act
            await _orderSummaryModel.UpdateOrderSummaryAsync(
                1, 100.0f, 10.0f, 5.0f, 115.0f, "John", "email", "phone",
                "address", "postal", null, null);

            // Assert
            Assert.IsTrue(capturedParameterValues.ContainsKey("@AdditionalInfo"));
            Assert.IsTrue(capturedParameterValues.ContainsKey("@ContractDetails"));
            Assert.AreEqual(DBNull.Value, capturedParameterValues["@AdditionalInfo"]);
            Assert.AreEqual(DBNull.Value, capturedParameterValues["@ContractDetails"]);
        }

        [TestMethod]
        public async Task GetOrderSummaryByIDAsync_WhenOrderDoesNotExist_ReturnsNull()
        {
            // Arrange
            int nonExistentOrderSummaryId = 999;

            // Setup an empty data reader that returns false on Read (no records found)
            _mockReader.Setup(r => r.Read()).Returns(false);
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Setup parameter creation
            Dictionary<string, object> capturedParameterValues = new Dictionary<string, object>();

            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(name => _mockParameter.Setup(p => p.ParameterName).Returns(name));

            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(value =>
                {
                    string paramName = _mockParameter.Object.ParameterName;
                    capturedParameterValues[paramName] = value;
                });

            // Act
            var result = await _orderSummaryModel.GetOrderSummaryByIDAsync(nonExistentOrderSummaryId);

            // Assert
            Assert.IsNull(result, "Method should return null when no order summary is found");
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            Assert.IsTrue(capturedParameterValues.ContainsKey("@ID"));
            Assert.AreEqual(nonExistentOrderSummaryId, capturedParameterValues["@ID"]);
        }


        [TestMethod]
        public async Task GetOrderSummaryByIDAsync_WhenOrderExists_ReturnsOrderSummary()
        {
            // Arrange
            int orderSummaryId = 1;
            float subtotal = 100.0f;
            float warrantyTax = 10.0f;
            float deliveryFee = 5.0f;
            float finalTotal = 115.0f;
            string fullName = "John Doe";
            string email = "john@example.com";
            string phoneNumber = "1234567890";
            string address = "123 Main St";
            string postalCode = "12345";
            string additionalInfo = "Additional info";
            string contractDetails = "Contract details";

            // Setup reader ordinals
            _mockReader.Setup(r => r.GetOrdinal("ID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("Subtotal")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("WarrantyTax")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("DeliveryFee")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("FinalTotal")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("FullName")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("Email")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("PhoneNumber")).Returns(7);
            _mockReader.Setup(r => r.GetOrdinal("Address")).Returns(8);
            _mockReader.Setup(r => r.GetOrdinal("PostalCode")).Returns(9);
            _mockReader.Setup(r => r.GetOrdinal("AdditionalInfo")).Returns(10);
            _mockReader.Setup(r => r.GetOrdinal("ContractDetails")).Returns(11);

            // Setup reader values
            _mockReader.Setup(r => r.GetInt32(0)).Returns(orderSummaryId);
            _mockReader.Setup(r => r.GetDouble(1)).Returns((double)subtotal);
            _mockReader.Setup(r => r.GetDouble(2)).Returns((double)warrantyTax);
            _mockReader.Setup(r => r.GetDouble(3)).Returns((double)deliveryFee);
            _mockReader.Setup(r => r.GetDouble(4)).Returns((double)finalTotal);
            _mockReader.Setup(r => r.GetString(5)).Returns(fullName);
            _mockReader.Setup(r => r.GetString(6)).Returns(email);
            _mockReader.Setup(r => r.GetString(7)).Returns(phoneNumber);
            _mockReader.Setup(r => r.GetString(8)).Returns(address);
            _mockReader.Setup(r => r.GetString(9)).Returns(postalCode);
            _mockReader.Setup(r => r.IsDBNull(10)).Returns(false);
            _mockReader.Setup(r => r.GetString(10)).Returns(additionalInfo);
            _mockReader.Setup(r => r.IsDBNull(11)).Returns(false);
            _mockReader.Setup(r => r.GetString(11)).Returns(contractDetails);

            // Setup reader to return true once then false
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var result = await _orderSummaryModel.GetOrderSummaryByIDAsync(orderSummaryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(orderSummaryId, result.ID);
            Assert.AreEqual(subtotal, result.Subtotal);
            Assert.AreEqual(warrantyTax, result.WarrantyTax);
            Assert.AreEqual(deliveryFee, result.DeliveryFee);
            Assert.AreEqual(finalTotal, result.FinalTotal);
            Assert.AreEqual(fullName, result.FullName);
            Assert.AreEqual(email, result.Email);
            Assert.AreEqual(phoneNumber, result.PhoneNumber);
            Assert.AreEqual(address, result.Address);
            Assert.AreEqual(postalCode, result.PostalCode);
            Assert.AreEqual(additionalInfo, result.AdditionalInfo);
            Assert.AreEqual(contractDetails, result.ContractDetails);

            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderSummaryByIDAsync_WithNullAdditionalFieldsInDB_ReturnsNullForThoseFields()
        {
            // Arrange
            _mockReader.Setup(r => r.GetOrdinal("ID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("Subtotal")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("WarrantyTax")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("DeliveryFee")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("FinalTotal")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("FullName")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("Email")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("PhoneNumber")).Returns(7);
            _mockReader.Setup(r => r.GetOrdinal("Address")).Returns(8);
            _mockReader.Setup(r => r.GetOrdinal("PostalCode")).Returns(9);
            _mockReader.Setup(r => r.GetOrdinal("AdditionalInfo")).Returns(10);
            _mockReader.Setup(r => r.GetOrdinal("ContractDetails")).Returns(11);

            // Setup basic values
            _mockReader.Setup(r => r.GetInt32(0)).Returns(1);
            _mockReader.Setup(r => r.GetDouble(It.IsInRange(1, 4, Moq.Range.Inclusive))).Returns(100.0);
            _mockReader.Setup(r => r.GetString(It.IsInRange(5, 9, Moq.Range.Inclusive))).Returns("test");

            // Setup null fields
            _mockReader.Setup(r => r.IsDBNull(10)).Returns(true);
            _mockReader.Setup(r => r.IsDBNull(11)).Returns(true);

            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var result = await _orderSummaryModel.GetOrderSummaryByIDAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.AdditionalInfo);
            Assert.IsNull(result.ContractDetails);
        }

        // Helper method to verify parameters
        private void VerifyParameter(List<IDbDataParameter> parameters, string name, object expectedValue)
        {
            var parameter = parameters.Find(p => p.ParameterName == name);
            Assert.IsNotNull(parameter, $"Parameter {name} was not found");
            if (expectedValue == null)
            {
                Assert.AreEqual(DBNull.Value, parameter.Value);
            }
            else
            {
                Assert.AreEqual(expectedValue, parameter.Value);
            }
        }
    }
}
