using ArtAttack.Model;
using ArtAttack.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data;
using System.Threading.Tasks;

namespace ArtAttack.Tests.Model
{
    [TestClass]
    public class DummyWalletModelTests
    {
        private Mock<IDatabaseProvider> _mockDbProvider;
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private Mock<IDbDataParameter> _mockParameter;
        private DummyWalletModel _walletModel;
        private readonly string _testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockDbProvider = new Mock<IDatabaseProvider>();
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
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Setup connection
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);
            _mockDbProvider.Setup(p => p.CreateConnection(_testConnectionString)).Returns(_mockConnection.Object);

            // Create the model with mocked provider
            _walletModel = new DummyWalletModel(_testConnectionString, _mockDbProvider.Object);
        }

        [TestMethod]
        public void Constructor_WithConnectionString_InitializesCorrectly()
        {
            // Arrange & Act
            var model = new DummyWalletModel(_testConnectionString, _mockDbProvider.Object);

            // Assert - using reflection to access private field
            var field = typeof(DummyWalletModel).GetField("connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var value = field.GetValue(model);

            Assert.IsNotNull(value);
            Assert.AreEqual(_testConnectionString, value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Act - should throw ArgumentNullException
            var model = new DummyWalletModel(null, _mockDbProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullDatabaseProvider_ThrowsArgumentNullException()
        {
            // Act - should throw ArgumentNullException
            var model = new DummyWalletModel(_testConnectionString, null);
        }

        [TestMethod]
        public async Task UpdateWalletBalance_ExecutesCorrectProcedure()
        {
            // Arrange
            int walletId = 42;
            float balance = 500.75f;

            // Act
            await _walletModel.UpdateWalletBalance(walletId, balance);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "UpdateWalletBalance");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Verify parameters
            VerifyParameterAdded("@id", walletId);
            VerifyParameterAdded("@balance", balance);
        }

        [TestMethod]
        public async Task GetWalletBalanceAsync_ReturnsCorrectBalance()
        {
            // Arrange
            int walletId = 42;
            double expectedBalance = 1234.56;

            // Setup reader
            _mockReader.Setup(r => r.Read()).Returns(true);
            _mockReader.Setup(r => r.GetOrdinal("balance")).Returns(0);
            _mockReader.Setup(r => r.GetDouble(0)).Returns(expectedBalance);

            // Act
            float result = await _walletModel.GetWalletBalanceAsync(walletId);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "GetWalletBalance");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);

            // Verify parameters
            VerifyParameterAdded("@id", walletId);

            // Verify result
            Assert.AreEqual((float)expectedBalance, result);
        }

        [TestMethod]
        public async Task GetWalletBalanceAsync_ReturnsNegativeOne_WhenWalletNotFound()
        {
            // Arrange
            int walletId = 42;

            // Setup reader to return no results
            _mockReader.Setup(r => r.Read()).Returns(false);

            // Act
            float result = await _walletModel.GetWalletBalanceAsync(walletId);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "GetWalletBalance");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockParameter.VerifySet(p => p.ParameterName = "@id");
            _mockParameter.VerifySet(p => p.Value = walletId);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            Assert.AreEqual(-1f, result);
        }

        private void VerifyParameterAdded(string name, object value)
        {
            _mockParameter.VerifySet(p => p.ParameterName = name, Times.AtLeastOnce());
            _mockParameter.VerifySet(p => p.Value = value, Times.AtLeastOnce());
        }
    }
}
