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
    public class DummyCardModelTests
    {
        private Mock<IDatabaseProvider> _mockDatabaseProvider;
        private Mock<IDbConnection> _mockDatabaseConnection;
        private Mock<IDbCommand> _mockDatabaseCommand;
        private Mock<IDataReader> _mockDataReader;
        private Mock<IDataParameterCollection> _mockParameterCollection;
        private Mock<IDbDataParameter> _mockDatabaseParameter;
        private DummyCardModel _dummyCardModel;
        private string _testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";

        // Constants for database column indices
        private const int ColumnIndex_Balance = 0;

        // Constants for stored procedure names
        private const string StoredProcedure_DeleteCard = "DeleteCard";
        private const string StoredProcedure_UpdateCardBalance = "UpdateCardBalance";
        private const string StoredProcedure_GetBalance = "GetBalance";

        // Constants for parameter names
        private const string ParameterName_CardNumber = "@cardnumber";
        private const string ParameterName_CompactCardNumber = "@cnumber";
        private const string ParameterName_Balance = "@balance";

        // Constants for test values
        private const string TestCardNumber = "1234-5678-9012-3456";
        private const float TestCardBalance = 500.50f;
        private const double TestCardBalanceDouble = 500.50;
        private const float NoCardFoundValue = -1f;

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
            _mockParameterCollection.Setup(Database_parameters => Database_parameters.Add(It.IsAny<object>())).Returns(0);

            // Setup command
            _mockDatabaseCommand.Setup(Database_command => Database_command.CreateParameter()).Returns(_mockDatabaseParameter.Object);
            _mockDatabaseCommand.Setup(Database_command => Database_command.Parameters).Returns(_mockParameterCollection.Object);

            // Setup connection
            _mockDatabaseConnection.Setup(Database_connection => Database_connection.CreateCommand()).Returns(_mockDatabaseCommand.Object);
            _mockDatabaseProvider.Setup(Database_provider => Database_provider.CreateConnection(_testConnectionString)).Returns(_mockDatabaseConnection.Object);

            // Create the model with mocked provider
            _dummyCardModel = new DummyCardModel(_testConnectionString, _mockDatabaseProvider.Object);
        }

        [TestMethod]
        public void Constructor_WithConnectionString_InitializesCorrectly()
        {
            // Arrange & Act
            var cardModel = new DummyCardModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Assert - using reflection to access private field
            var connectionStringField = typeof(DummyCardModel).GetField("connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var connectionStringValue = connectionStringField.GetValue(cardModel);

            Assert.IsNotNull(connectionStringValue);
            Assert.AreEqual(_testConnectionString, connectionStringValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Act - should throw ArgumentNullException
            var cardModel = new DummyCardModel(null, _mockDatabaseProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullDatabaseProvider_ThrowsArgumentNullException()
        {
            // Act - should throw ArgumentNullException
            var cardModel = new DummyCardModel(_testConnectionString, null);
        }

        [TestMethod]
        public async Task DeleteCardAsync_ExecutesCorrectProcedure()
        {
            // Arrange
            string cardNumber = TestCardNumber;

            // Act
            await _dummyCardModel.DeleteCardAsync(cardNumber);

            // Assert
            _mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandText = StoredProcedure_DeleteCard);
            _mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandType = CommandType.StoredProcedure);
            _mockDatabaseParameter.VerifySet(Database_parameter => Database_parameter.ParameterName = ParameterName_CardNumber);
            _mockDatabaseParameter.VerifySet(Database_parameter => Database_parameter.Value = cardNumber);
            _mockDatabaseConnection.Verify(Database_connection => Database_connection.Open(), Times.Once);
            _mockDatabaseCommand.Verify(Database_command => Database_command.ExecuteNonQuery(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateCardBalanceAsync_ExecutesCorrectProcedure()
        {
            // Arrange
            string cardNumber = TestCardNumber;
            float cardBalance = TestCardBalance;
            var capturedParameters = new List<(string Name, object Value)>();

            // Setup parameter capture
            _mockDatabaseParameter.SetupSet(Database_parameter => Database_parameter.ParameterName = It.IsAny<string>())
                .Callback<string>(name => capturedParameters.Add((name, null)));
            _mockDatabaseParameter.SetupSet(Database_parameter => Database_parameter.Value = It.IsAny<object>())
                .Callback<object>(value => capturedParameters[capturedParameters.Count - 1] =
                    (capturedParameters[capturedParameters.Count - 1].Name, value));

            // Act
            await _dummyCardModel.UpdateCardBalanceAsync(cardNumber, cardBalance);

            // Assert
            _mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandText = StoredProcedure_UpdateCardBalance);
            _mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandType = CommandType.StoredProcedure);
            _mockDatabaseConnection.Verify(Database_connection => Database_connection.Open(), Times.Once);
            _mockDatabaseCommand.Verify(Database_command => Database_command.ExecuteNonQuery(), Times.Once);

            // Check parameters
            int expectedParameterCount = 2;
            Assert.AreEqual(expectedParameterCount, capturedParameters.Count);
            Assert.AreEqual(ParameterName_CompactCardNumber, capturedParameters[0].Name);
            Assert.AreEqual(cardNumber, capturedParameters[0].Value);
            Assert.AreEqual(ParameterName_Balance, capturedParameters[1].Name);
            Assert.AreEqual(cardBalance, capturedParameters[1].Value);
        }

        [TestMethod]
        public async Task GetCardBalanceAsync_ReturnsCorrectBalance()
        {
            // Arrange
            string cardNumber = TestCardNumber;
            double expectedBalance = TestCardBalanceDouble;

            // Setup reader
            _mockDataReader.Setup(Database_reader => Database_reader.Read()).Returns(true);
            _mockDataReader.Setup(Database_reader => Database_reader.GetOrdinal("balance")).Returns(ColumnIndex_Balance);
            _mockDataReader.Setup(Database_reader => Database_reader.GetDouble(ColumnIndex_Balance)).Returns(expectedBalance);
            _mockDatabaseCommand.Setup(Database_command => Database_command.ExecuteReader()).Returns(_mockDataReader.Object);

            // Act
            float actualBalance = await _dummyCardModel.GetCardBalanceAsync(cardNumber);

            // Assert
            _mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandText = StoredProcedure_GetBalance);
            _mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandType = CommandType.StoredProcedure);
            _mockDatabaseParameter.VerifySet(Database_parameter => Database_parameter.ParameterName = ParameterName_CompactCardNumber);
            _mockDatabaseParameter.VerifySet(Database_parameter => Database_parameter.Value = cardNumber);
            _mockDatabaseConnection.Verify(Database_connection => Database_connection.Open(), Times.Once);
            _mockDatabaseCommand.Verify(Database_command => Database_command.ExecuteReader(), Times.Once);
            Assert.AreEqual((float)expectedBalance, actualBalance);
        }

        [TestMethod]
        public async Task GetCardBalanceAsync_ReturnsNegativeOne_WhenCardNotFound()
        {
            // Arrange
            string cardNumber = TestCardNumber;

            // Setup reader to return no results
            _mockDataReader.Setup(Database_reader => Database_reader.Read()).Returns(false);
            _mockDatabaseCommand.Setup(Database_command => Database_command.ExecuteReader()).Returns(_mockDataReader.Object);

            // Act
            float actualBalance = await _dummyCardModel.GetCardBalanceAsync(cardNumber);

            // Assert
            _mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandText = StoredProcedure_GetBalance);
            _mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandType = CommandType.StoredProcedure);
            _mockDatabaseParameter.VerifySet(Database_parameter => Database_parameter.ParameterName = ParameterName_CompactCardNumber);
            _mockDatabaseParameter.VerifySet(Database_parameter => Database_parameter.Value = cardNumber);
            _mockDatabaseConnection.Verify(Database_connection => Database_connection.Open(), Times.Once);
            _mockDatabaseCommand.Verify(Database_command => Database_command.ExecuteReader(), Times.Once);
            Assert.AreEqual(NoCardFoundValue, actualBalance);
        }
    }
}