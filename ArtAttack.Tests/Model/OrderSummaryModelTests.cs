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

            // Setup command to create parameters and return the parameter collection
            _mockCommand.Setup(c => c.CreateParameter()).Returns(_mockParameter.Object);
            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object);

            // Setup connection to create commands
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

            // Setup database provider to return the connection
            _mockDatabaseProvider.Setup(dp => dp.CreateConnection(It.IsAny<string>()))
                .Returns(_mockConnection.Object);

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
            var capturedValues = new Dictionary<string, object>();

            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(parameterName =>
                {
                    _mockParameter.Setup(p => p.ParameterName).Returns(parameterName);
                });
            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(paramValue =>
                {
                    string currentParameterName = _mockParameter.Object.ParameterName;
                    capturedValues[currentParameterName] = paramValue;
                });

            return capturedValues;
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
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "AddOrderSummary");
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            // We expect 11 parameters to be set in total
            Assert.AreEqual(11, _mockParameter.Invocations.Count(inv => inv.Method.Name == "set_ParameterName"));

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
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "DeleteOrderSummary");
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            // We expect 1 parameter to be set
            Assert.AreEqual(1, _mockParameter.Invocations.Count(inv => inv.Method.Name == "set_ParameterName"));

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

            Dictionary<string, object> capturedParameterValues = CaptureParameterValues();

            // Act
            await _orderSummaryModel.UpdateOrderSummaryAsync(
                id, subtotal, warrantyTax, deliveryFee, finalTotal, fullName, email,
                phoneNumber, address, postalCode, additionalInfo, contractDetails);

            // Assert
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "UpdateOrderSummary");
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            // Expect 12 parameters to be set
            Assert.AreEqual(12, _mockParameter.Invocations.Count(inv => inv.Method.Name == "set_ParameterName"));

            // Verify a few key parameters
            Assert.IsTrue(capturedParameterValues.ContainsKey("@ID"));
            Assert.AreEqual(id, capturedParameterValues["@ID"]);

            Assert.IsTrue(capturedParameterValues.ContainsKey("@FullName"));
            Assert.AreEqual(fullName, capturedParameterValues["@FullName"]);


            Assert.IsTrue(capturedParameterValues.ContainsKey("@Email"));
            Assert.AreEqual(email, capturedParameterValues["@Email"]);
        }

        [TestMethod]
        public async Task UpdateOrderSummaryAsync_WhenWithNullValues_UsesDBNull()
        {
            // Arrange
            Dictionary<string, object> capturedParameterValues = CaptureParameterValues();


            // Act
            await _orderSummaryModel.UpdateOrderSummaryAsync(
                1, 100.0f, 10.0f, 5.0f, 115.0f, "John", "email", "phone",
                "address", "postal", null, null);

            // Assert that null fields become DBNull.Value
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
            Dictionary<string, object> capturedParameterValues = CaptureParameterValues();

            // Setup the reader to return no records
            _mockReader.Setup(reader => reader.Read()).Returns(false);
            _mockCommand.Setup(cmd => cmd.ExecuteReader()).Returns(_mockReader.Object);

            // (Optional) Re-setup parameter capture for this call
            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(paramName => _mockParameter.Setup(p => p.ParameterName).Returns(paramName));
            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(paramValue =>
                {
                    string currentParameterName = _mockParameter.Object.ParameterName;
                    capturedParameterValues[currentParameterName] = paramValue;
                });

            // Act 

            var result = await _orderSummaryModel.GetOrderSummaryByIDAsync(nonExistentOrderSummaryId);

            // Assert
            Assert.IsNull(result, "Method should return null when no order summary is found");
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(cmd => cmd.ExecuteReader(), Times.Once);

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

            // Setup reader column ordinals
            _mockReader.Setup(reader => reader.GetOrdinal("ID")).Returns(0);
            _mockReader.Setup(reader => reader.GetOrdinal("Subtotal")).Returns(1);
            _mockReader.Setup(reader => reader.GetOrdinal("WarrantyTax")).Returns(2);
            _mockReader.Setup(reader => reader.GetOrdinal("DeliveryFee")).Returns(3);
            _mockReader.Setup(reader => reader.GetOrdinal("FinalTotal")).Returns(4);
            _mockReader.Setup(reader => reader.GetOrdinal("FullName")).Returns(5);
            _mockReader.Setup(reader => reader.GetOrdinal("Email")).Returns(6);
            _mockReader.Setup(reader => reader.GetOrdinal("PhoneNumber")).Returns(7);
            _mockReader.Setup(reader => reader.GetOrdinal("Address")).Returns(8);
            _mockReader.Setup(reader => reader.GetOrdinal("PostalCode")).Returns(9);
            _mockReader.Setup(reader => reader.GetOrdinal("AdditionalInfo")).Returns(10);
            _mockReader.Setup(reader => reader.GetOrdinal("ContractDetails")).Returns(11);

            // Setup reader to return specific values
            _mockReader.Setup(reader => reader.GetInt32(0)).Returns(orderSummaryId);
            _mockReader.Setup(reader => reader.GetDouble(1)).Returns((double)subtotal);
            _mockReader.Setup(reader => reader.GetDouble(2)).Returns((double)warrantyTax);
            _mockReader.Setup(reader => reader.GetDouble(3)).Returns((double)deliveryFee);
            _mockReader.Setup(reader => reader.GetDouble(4)).Returns((double)finalTotal);
            _mockReader.Setup(reader => reader.GetString(5)).Returns(fullName);
            _mockReader.Setup(reader => reader.GetString(6)).Returns(email);
            _mockReader.Setup(reader => reader.GetString(7)).Returns(phoneNumber);
            _mockReader.Setup(reader => reader.GetString(8)).Returns(address);
            _mockReader.Setup(reader => reader.GetString(9)).Returns(postalCode);
            _mockReader.Setup(reader => reader.IsDBNull(10)).Returns(false);
            _mockReader.Setup(reader => reader.GetString(10)).Returns(additionalInfo);
            _mockReader.Setup(reader => reader.IsDBNull(11)).Returns(false);
            _mockReader.Setup(reader => reader.GetString(11)).Returns(contractDetails);

            _mockReader.SetupSequence(reader => reader.Read())
                .Returns(true)
                .Returns(false);
            _mockCommand.Setup(cmd => cmd.ExecuteReader()).Returns(_mockReader.Object);

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

            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderSummaryByIDAsync_WhenWithNullAdditionalFieldsInDB_ReturnsNullForThoseFields()
        {
            // Arrange
            _mockReader.Setup(reader => reader.GetOrdinal("ID")).Returns(0);
            _mockReader.Setup(reader => reader.GetOrdinal("Subtotal")).Returns(1);
            _mockReader.Setup(reader => reader.GetOrdinal("WarrantyTax")).Returns(2);
            _mockReader.Setup(reader => reader.GetOrdinal("DeliveryFee")).Returns(3);
            _mockReader.Setup(reader => reader.GetOrdinal("FinalTotal")).Returns(4);
            _mockReader.Setup(reader => reader.GetOrdinal("FullName")).Returns(5);
            _mockReader.Setup(reader => reader.GetOrdinal("Email")).Returns(6);
            _mockReader.Setup(reader => reader.GetOrdinal("PhoneNumber")).Returns(7);
            _mockReader.Setup(reader => reader.GetOrdinal("Address")).Returns(8);
            _mockReader.Setup(reader => reader.GetOrdinal("PostalCode")).Returns(9);
            _mockReader.Setup(reader => reader.GetOrdinal("AdditionalInfo")).Returns(10);
            _mockReader.Setup(reader => reader.GetOrdinal("ContractDetails")).Returns(11);

            _mockReader.Setup(reader => reader.GetInt32(0)).Returns(1);
            _mockReader.Setup(reader => reader.GetDouble(It.IsInRange(1, 4, Moq.Range.Inclusive))).Returns(100.0);
            _mockReader.Setup(reader => reader.GetString(It.IsInRange(5, 9, Moq.Range.Inclusive))).Returns("test");

            // Setup the null fields
            _mockReader.Setup(reader => reader.IsDBNull(10)).Returns(true);
            _mockReader.Setup(reader => reader.IsDBNull(11)).Returns(true);

            _mockReader.SetupSequence(reader => reader.Read())
                .Returns(true)
                .Returns(false);
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);


            // Act
            var result = await _orderSummaryModel.GetOrderSummaryByIDAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.AdditionalInfo);
            Assert.IsNull(result.ContractDetails);
        }


        // (Optional) Helper method to verify parameters from a list if needed.
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
