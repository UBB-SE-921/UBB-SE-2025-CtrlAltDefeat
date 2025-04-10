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
        private Mock<IDatabaseProvider> _mockDbProvider;
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private Mock<IDbDataParameter> _mockParameter;
        private DummyCardModel _cardModel;
        private string _testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";

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

            // Setup connection
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);
            _mockDbProvider.Setup(p => p.CreateConnection(_testConnectionString)).Returns(_mockConnection.Object);

            // Create the model with mocked provider
            _cardModel = new DummyCardModel(_testConnectionString, _mockDbProvider.Object);
        }

        [TestMethod]
        public void Constructor_WithConnectionString_InitializesCorrectly()
        {
            // Arrange & Act
            var model = new DummyCardModel(_testConnectionString, _mockDbProvider.Object);

            // Assert - using reflection to access private field
            var field = typeof(DummyCardModel).GetField("connectionString",
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
            var model = new DummyCardModel(null, _mockDbProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullDatabaseProvider_ThrowsArgumentNullException()
        {
            // Act - should throw ArgumentNullException
            var model = new DummyCardModel(_testConnectionString, null);
        }

        [TestMethod]
        public async Task DeleteCardAsync_ExecutesCorrectProcedure()
        {
            // Arrange
            string cardNumber = "1234-5678-9012-3456";

            // Act
            await _cardModel.DeleteCardAsync(cardNumber);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "DeleteCard");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockParameter.VerifySet(p => p.ParameterName = "@cardnumber");
            _mockParameter.VerifySet(p => p.Value = cardNumber);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }

        [TestMethod]
        public async Task UpdateCardBalanceAsync_ExecutesCorrectProcedure()
        {
            // Arrange
            string cardNumber = "1234-5678-9012-3456";
            float balance = 500.50f;
            var capturedParams = new List<(string Name, object Value)>();

            // Setup parameter capture
            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(name => capturedParams.Add((name, null)));
            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(val => capturedParams[capturedParams.Count - 1] =
                    (capturedParams[capturedParams.Count - 1].Name, val));

            // Act
            await _cardModel.UpdateCardBalanceAsync(cardNumber, balance);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "UpdateCardBalance");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Check parameters
            Assert.AreEqual(2, capturedParams.Count);
            Assert.AreEqual("@cnumber", capturedParams[0].Name);
            Assert.AreEqual(cardNumber, capturedParams[0].Value);
            Assert.AreEqual("@balance", capturedParams[1].Name);
            Assert.AreEqual(balance, capturedParams[1].Value);
        }

        [TestMethod]
        public async Task GetCardBalanceAsync_ReturnsCorrectBalance()
        {
            // Arrange
            string cardNumber = "1234-5678-9012-3456";
            double expectedBalance = 500.50;

            // Setup reader
            _mockReader.Setup(r => r.Read()).Returns(true);
            _mockReader.Setup(r => r.GetOrdinal("balance")).Returns(0);
            _mockReader.Setup(r => r.GetDouble(0)).Returns(expectedBalance);
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            float result = await _cardModel.GetCardBalanceAsync(cardNumber);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "GetBalance");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockParameter.VerifySet(p => p.ParameterName = "@cnumber");
            _mockParameter.VerifySet(p => p.Value = cardNumber);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            Assert.AreEqual((float)expectedBalance, result);
        }

        [TestMethod]
        public async Task GetCardBalanceAsync_ReturnsNegativeOne_WhenCardNotFound()
        {
            // Arrange
            string cardNumber = "1234-5678-9012-3456";

            // Setup reader to return no results
            _mockReader.Setup(r => r.Read()).Returns(false);
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            float result = await _cardModel.GetCardBalanceAsync(cardNumber);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "GetBalance");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockParameter.VerifySet(p => p.ParameterName = "@cnumber");
            _mockParameter.VerifySet(p => p.Value = cardNumber);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            Assert.AreEqual(-1f, result);
        }


    }
}
