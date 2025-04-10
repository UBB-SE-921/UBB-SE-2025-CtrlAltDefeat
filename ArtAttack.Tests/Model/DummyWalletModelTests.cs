using ArtAttack.Model;
using ArtAttack.Shared;
using Moq;
using System.Data;

namespace ArtAttack.Tests.Model
{
    [TestClass]
    public class DummyWalletModelTests
    {
        private Mock<IDatabaseProvider> mockDatabaseProvider;
        private Mock<IDbConnection> mockConnection;
        private Mock<IDbCommand> mockCommand;
        private Mock<IDataReader> mockReader;
        private Mock<IDataParameterCollection> mockParameters;
        private Mock<IDbDataParameter> mockParameter;
        private DummyWalletModel dummyWalletModel;
        private readonly string testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            mockDatabaseProvider = new Mock<IDatabaseProvider>();
            mockConnection = new Mock<IDbConnection>();
            mockCommand = new Mock<IDbCommand>();
            mockReader = new Mock<IDataReader>();
            mockParameters = new Mock<IDataParameterCollection>();
            mockParameter = new Mock<IDbDataParameter>();

            // Setup parameter collection
            mockParameters.Setup(mockParametersCollection => mockParametersCollection.Add(It.IsAny<object>())).Returns(0);

            // Setup command
            mockCommand.Setup(command => command.CreateParameter()).Returns(mockParameter.Object);
            mockCommand.Setup(command => command.Parameters).Returns(mockParameters.Object);
            mockCommand.Setup(command => command.ExecuteReader()).Returns(mockReader.Object);

            // Setup connection
            mockConnection.Setup(connection => connection.CreateCommand()).Returns(mockCommand.Object);
            mockDatabaseProvider.Setup(databaseProviderMock => databaseProviderMock.CreateConnection(testConnectionString)).Returns(mockConnection.Object);

            // Create the model with mocked provider
            dummyWalletModel = new DummyWalletModel(testConnectionString, mockDatabaseProvider.Object);
        }

        [TestMethod]
        public void ConstructorWithConnectionString_ShouldInitializCorrectly()
        {
            // Arrange & Act
            var dummyWalletModel = new DummyWalletModel(testConnectionString, mockDatabaseProvider.Object);

            // Assert - using reflection to access private field
            var connectionStringField = typeof(DummyWalletModel).GetField("connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var connectionStringValue = connectionStringField.GetValue(dummyWalletModel);

            Assert.AreEqual(testConnectionString, connectionStringValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNullConnectionString_ShouldThrowArgumentNullException()
        {
            // Act
            var dummyWalletModel = new DummyWalletModel(null, mockDatabaseProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNullDatabaseProvider_ShouldThrowArgumentNullException()
        {
            // Act
            var dummyWalletModel = new DummyWalletModel(testConnectionString, null);
        }

        [TestMethod]
        public async Task UpdateWalletBalance_ShouldExecuteCorrectProcedure()
        {
            // Arrange
            int walletId = 42;
            float balance = 500.75f;

            // Act
            await dummyWalletModel.UpdateWalletBalance(walletId, balance);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "UpdateWalletBalance");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockConnection.Verify(command => command.Open(), Times.Once);
            mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            // Verify parameters
            VerifyParameterAdded("@id", walletId);
            VerifyParameterAdded("@balance", balance);
        }


        [TestMethod]
        public async Task GetWalletBalanceAsync_ShouldReturnCorrectBalance()
        {
            // Arrange
            int walletId = 42;
            double expectedBalance = 1234.56;

            // Setup reader
            mockReader.Setup(reader => reader.Read()).Returns(true);
            mockReader.Setup(reader => reader.GetOrdinal("balance")).Returns(0);
            mockReader.Setup(reader => reader.GetDouble(0)).Returns(expectedBalance);

            // Act
            float result = await dummyWalletModel.GetWalletBalanceAsync(walletId);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "GetWalletBalance");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockConnection.Verify(command => command.Open(), Times.Once);
            mockCommand.Verify(command => command.ExecuteReader(), Times.Once);

            // Verify parameters
            VerifyParameterAdded("@id", walletId);

            // Verify result
            Assert.AreEqual((float)expectedBalance, result);
        }

        [TestMethod]
        public async Task GetWalletBalanceAsync_WhenWalletNotFound_ShouldReturnNegativeOne()
        {
            // Arrange
            int walletId = 42;

            // Setup reader to return no results
            mockReader.Setup(reader => reader.Read()).Returns(false);

            // Act
            float result = await dummyWalletModel.GetWalletBalanceAsync(walletId);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "GetWalletBalance");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockParameter.VerifySet(parameter=> parameter.ParameterName = "@id");
            mockParameter.VerifySet(parameter => parameter.Value = walletId);
            mockConnection.Verify(reader => reader.Open(), Times.Once);
            mockCommand.Verify(reader => reader.ExecuteReader(), Times.Once);

            Assert.AreEqual(-1f, result);
        }


        private void VerifyParameterAdded(string parameterName, object parameterValue)
        {
            mockParameter.VerifySet(parameter => parameter.ParameterName = parameterName, Times.AtLeastOnce());
            mockParameter.VerifySet(parameter => parameter.Value = parameterValue, Times.AtLeastOnce());
        }
    }
}
