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
        private Mock<IDatabaseProvider> mockDatabaseProvider;
        private Mock<IDbConnection> mockDatabaseConnection;
        private Mock<IDbCommand> mockDatabaseCommand;
        private Mock<IDataReader> mockDataReader;
        private Mock<IDataParameterCollection> mockParameterCollection;
        private Mock<IDbDataParameter> mockDatabaseParameter;
        private DummyCardModel dummyCardModel;
        private string testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";

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
            mockDatabaseProvider = new Mock<IDatabaseProvider>();
            mockDatabaseConnection = new Mock<IDbConnection>();
            mockDatabaseCommand = new Mock<IDbCommand>();
            mockDataReader = new Mock<IDataReader>();
            mockParameterCollection = new Mock<IDataParameterCollection>();
            mockDatabaseParameter = new Mock<IDbDataParameter>();

            // Setup parameter collection
            mockParameterCollection.Setup(Database_parameters => Database_parameters.Add(It.IsAny<object>())).Returns(0);

            // Setup command
            mockDatabaseCommand.Setup(Database_command => Database_command.CreateParameter()).Returns(mockDatabaseParameter.Object);
            mockDatabaseCommand.Setup(Database_command => Database_command.Parameters).Returns(mockParameterCollection.Object);

            // Setup connection
            mockDatabaseConnection.Setup(Database_connection => Database_connection.CreateCommand()).Returns(mockDatabaseCommand.Object);
            mockDatabaseProvider.Setup(Database_provider => Database_provider.CreateConnection(testConnectionString)).Returns(mockDatabaseConnection.Object);

            // Create the model with mocked provider
            dummyCardModel = new DummyCardModel(testConnectionString, mockDatabaseProvider.Object);
        }

        [TestMethod]
        public void Constructor_WithConnectionString_InitializesCorrectly()
        {
            // Arrange & Act
            var cardModel = new DummyCardModel(testConnectionString, mockDatabaseProvider.Object);

            // Assert - using reflection to access private field
            var connectionStringField = typeof(DummyCardModel).GetField("connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var connectionStringValue = connectionStringField.GetValue(cardModel);

            Assert.IsNotNull(connectionStringValue);
            Assert.AreEqual(testConnectionString, connectionStringValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Act - should throw ArgumentNullException
            var cardModel = new DummyCardModel(null, mockDatabaseProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullDatabaseProvider_ThrowsArgumentNullException()
        {
            // Act - should throw ArgumentNullException
            var cardModel = new DummyCardModel(testConnectionString, null);
        }

        [TestMethod]
        public async Task DeleteCardAsync_ExecutesCorrectProcedure()
        {
            // Arrange
            string cardNumber = TestCardNumber;

            // Act
            await dummyCardModel.DeleteCardAsync(cardNumber);

            // Assert
            mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandText = StoredProcedure_DeleteCard);
            mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandType = CommandType.StoredProcedure);
            mockDatabaseParameter.VerifySet(Database_parameter => Database_parameter.ParameterName = ParameterName_CardNumber);
            mockDatabaseParameter.VerifySet(Database_parameter => Database_parameter.Value = cardNumber);
            mockDatabaseConnection.Verify(Database_connection => Database_connection.Open(), Times.Once);
            mockDatabaseCommand.Verify(Database_command => Database_command.ExecuteNonQuery(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateCardBalanceAsync_ExecutesCorrectProcedure()
        {
            // Arrange
            string cardNumber = TestCardNumber;
            float cardBalance = TestCardBalance;
            var capturedParameters = new List<(string Name, object Value)>();

            // Setup parameter capture
            mockDatabaseParameter.SetupSet(Database_parameter => Database_parameter.ParameterName = It.IsAny<string>())
                .Callback<string>(name => capturedParameters.Add((name, null)));
            mockDatabaseParameter.SetupSet(Database_parameter => Database_parameter.Value = It.IsAny<object>())
                .Callback<object>(value => capturedParameters[capturedParameters.Count - 1] =
                    (capturedParameters[capturedParameters.Count - 1].Name, value));

            // Act
            await dummyCardModel.UpdateCardBalanceAsync(cardNumber, cardBalance);

            // Assert
            mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandText = StoredProcedure_UpdateCardBalance);
            mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandType = CommandType.StoredProcedure);
            mockDatabaseConnection.Verify(Database_connection => Database_connection.Open(), Times.Once);
            mockDatabaseCommand.Verify(Database_command => Database_command.ExecuteNonQuery(), Times.Once);

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
            mockDataReader.Setup(Database_reader => Database_reader.Read()).Returns(true);
            mockDataReader.Setup(Database_reader => Database_reader.GetOrdinal("balance")).Returns(ColumnIndex_Balance);
            mockDataReader.Setup(Database_reader => Database_reader.GetDouble(ColumnIndex_Balance)).Returns(expectedBalance);
            mockDatabaseCommand.Setup(Database_command => Database_command.ExecuteReader()).Returns(mockDataReader.Object);

            // Act
            float actualBalance = await dummyCardModel.GetCardBalanceAsync(cardNumber);

            // Assert
            mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandText = StoredProcedure_GetBalance);
            mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandType = CommandType.StoredProcedure);
            mockDatabaseParameter.VerifySet(Database_parameter => Database_parameter.ParameterName = ParameterName_CompactCardNumber);
            mockDatabaseParameter.VerifySet(Database_parameter => Database_parameter.Value = cardNumber);
            mockDatabaseConnection.Verify(Database_connection => Database_connection.Open(), Times.Once);
            mockDatabaseCommand.Verify(Database_command => Database_command.ExecuteReader(), Times.Once);
            Assert.AreEqual((float)expectedBalance, actualBalance);
        }

        [TestMethod]
        public async Task GetCardBalanceAsync_WhenCardNotFound_ReturnsNegativeOne()
        {
            // Arrange
            string cardNumber = TestCardNumber;

            // Setup reader to return no results
            mockDataReader.Setup(Database_reader => Database_reader.Read()).Returns(false);
            mockDatabaseCommand.Setup(Database_command => Database_command.ExecuteReader()).Returns(mockDataReader.Object);

            // Act
            float actualBalance = await dummyCardModel.GetCardBalanceAsync(cardNumber);

            // Assert
            mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandText = StoredProcedure_GetBalance);
            mockDatabaseCommand.VerifySet(Database_command => Database_command.CommandType = CommandType.StoredProcedure);
            mockDatabaseParameter.VerifySet(Database_parameter => Database_parameter.ParameterName = ParameterName_CompactCardNumber);
            mockDatabaseParameter.VerifySet(Database_parameter => Database_parameter.Value = cardNumber);
            mockDatabaseConnection.Verify(Database_connection => Database_connection.Open(), Times.Once);
            mockDatabaseCommand.Verify(Database_command => Database_command.ExecuteReader(), Times.Once);
            Assert.AreEqual(NoCardFoundValue, actualBalance);
        }
    }
}