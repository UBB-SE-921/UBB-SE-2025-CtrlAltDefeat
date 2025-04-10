using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ArtAttack.Tests.Model
{
    [TestClass]
    public class ContractRenewalModelTests
    {
        private Mock<IDatabaseProvider> _mockDbProvider;
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private string _testConnectionString = "TestConnection";
        private ContractRenewalModel _renewalModel;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockDbProvider = new Mock<IDatabaseProvider>();
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockReader = new Mock<IDataReader>();
            _mockParameters = new Mock<IDataParameterCollection>();

            // Setup the parameter collection mock
            _mockParameters
                .Setup(p => p.Add(It.IsAny<object>()))
                .Returns(0);

            // Setup the command mock
            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object);
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Setup the connection mock
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

            // Setup the database provider mock
            _mockDbProvider.Setup(p => p.CreateConnection(It.IsAny<string>())).Returns(_mockConnection.Object);

            // Create the ContractRenewalModel with the mock database provider
            _renewalModel = new ContractRenewalModel(_testConnectionString, _mockDbProvider.Object);
        }

        [TestMethod]
        public void Constructor_WithConnectionStringOnly_InitializesCorrectly()
        {
            // Arrange & Act
            var model = new ContractRenewalModel(_testConnectionString);

            // Assert
            Assert.IsNotNull(model);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            _ = new ContractRenewalModel(null, _mockDbProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullDatabaseProvider_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            _ = new ContractRenewalModel(_testConnectionString, null);
        }

        [TestMethod]
        public async Task AddRenewedContractAsync_WithValidContract_ExecutesStoredProcedure()
        {
            // Arrange
            var contract = new Contract
            {
                OrderID = 123,
                ContractStatus = "RENEWED",
                ContractContent = "Contract Content",
                RenewalCount = 1,
                PredefinedContractID = 2,
                PDFID = 456,
                RenewedFromContractID = 789
            };
            byte[] pdfData = new byte[] { 1, 2, 3, 4, 5 };

            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            await _renewalModel.AddRenewedContractAsync(contract, pdfData);

            // Assert
            _mockDbProvider.Verify(p => p.CreateConnection(_testConnectionString), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }

        [TestMethod]
        public async Task HasContractBeenRenewedAsync_WhenContractHasBeenRenewed_ReturnsTrue()
        {
            // Arrange
            long contractId = 123;
            _mockCommand.Setup(c => c.ExecuteScalar()).Returns(1);

            // Act
            bool result = await _renewalModel.HasContractBeenRenewedAsync(contractId);

            // Assert
            Assert.IsTrue(result);
            _mockDbProvider.Verify(p => p.CreateConnection(_testConnectionString), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteScalar(), Times.Once);
            //_mockParameters.Verify(p => p.Add(It.Is<object>(o =>
            //    o is IDataParameter param &&
            //    param.ParameterName == "@ContractID" &&
            //    (long)param.Value == contractId)), Times.Once);
        }

        [TestMethod]
        public async Task HasContractBeenRenewedAsync_WhenContractHasNotBeenRenewed_ReturnsFalse()
        {
            // Arrange
            long contractId = 456;
            _mockCommand.Setup(c => c.ExecuteScalar()).Returns(0);

            // Act
            bool result = await _renewalModel.HasContractBeenRenewedAsync(contractId);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasContractBeenRenewedAsync_WhenScalarReturnsNull_ReturnsFalse()
        {
            // Arrange
            long contractId = 456;
            _mockCommand.Setup(c => c.ExecuteScalar()).Returns(null);

            // Act
            bool result = await _renewalModel.HasContractBeenRenewedAsync(contractId);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetRenewedContractsAsync_ReturnsListOfContracts()
        {
            // Arrange
            SetupReaderWithContractColumns();

            // Setup to track which read call we're on
            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 2); // Return 2 contracts

            // Store expected values in arrays for consistent access
            long[] contractIds = new long[] { 101, 102 };
            int[] orderIds = new int[] { 201, 202 };
            string[] contents = new string[] { "Content 1", "Content 2" };
            int[] renewalCounts = new int[] { 1, 2 };
            int[] predefinedContractIds = new int[] { 301, 302 };
            int[] pdfIds = new int[] { 401, 402 };
            long[] renewedFromIds = new long[] { 501, 502 };

            // Setup data for contracts - use proper function syntax
            _mockReader.Setup(r => r.GetInt64(0)).Returns(() => contractIds[Math.Min(readCount - 1, 1)]);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(() => orderIds[Math.Min(readCount - 1, 1)]);
            _mockReader.Setup(r => r.GetString(2)).Returns("RENEWED");
            _mockReader.Setup(r => r["contractContent"]).Returns(() => contents[Math.Min(readCount - 1, 1)]);
            _mockReader.Setup(r => r.GetInt32(4)).Returns(() => renewalCounts[Math.Min(readCount - 1, 1)]);
            _mockReader.Setup(r => r.IsDBNull(5)).Returns(false);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(() => predefinedContractIds[Math.Min(readCount - 1, 1)]);
            _mockReader.Setup(r => r.GetInt32(6)).Returns(() => pdfIds[Math.Min(readCount - 1, 1)]);
            _mockReader.Setup(r => r.IsDBNull(7)).Returns(false);
            _mockReader.Setup(r => r.GetInt64(7)).Returns(() => renewedFromIds[Math.Min(readCount - 1, 1)]);

            // Act
            var result = await _renewalModel.GetRenewedContractsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(contractIds[0], result[0].ContractID);
            Assert.AreEqual(contractIds[1], result[1].ContractID);
            Assert.AreEqual(orderIds[0], result[0].OrderID);
            Assert.AreEqual(orderIds[1], result[1].OrderID);
            Assert.AreEqual("RENEWED", result[0].ContractStatus);
            Assert.AreEqual("RENEWED", result[1].ContractStatus);
            Assert.AreEqual(contents[0], result[0].ContractContent);
            Assert.AreEqual(contents[1], result[1].ContractContent);

            _mockDbProvider.Verify(p => p.CreateConnection(_testConnectionString), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
        }

        [TestMethod]
        public async Task GetRenewedContractsAsync_WhenNoContractsFound_ReturnsEmptyList()
        {
            // Arrange
            SetupReaderWithContractColumns();
            _mockReader.Setup(r => r.Read()).Returns(false); // Return no contracts

            // Act
            var result = await _renewalModel.GetRenewedContractsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetRenewedContractsAsync_HandlesNullValues()
        {
            // Arrange
            SetupReaderWithContractColumns();

            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            // Setup data with nulls
            _mockReader.Setup(r => r.GetInt64(0)).Returns(101L);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(201);
            _mockReader.Setup(r => r.GetString(2)).Returns("RENEWED");
            _mockReader.Setup(r => r["contractContent"]).Returns(null);
            _mockReader.Setup(r => r.GetInt32(4)).Returns(1);
            _mockReader.Setup(r => r.IsDBNull(5)).Returns(true);
            _mockReader.Setup(r => r.GetInt32(6)).Returns(401);
            _mockReader.Setup(r => r.IsDBNull(7)).Returns(true);

            // Act
            var result = await _renewalModel.GetRenewedContractsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(101L, result[0].ContractID);
            Assert.IsNull(result[0].ContractContent);
            Assert.IsNull(result[0].PredefinedContractID);
            Assert.IsNull(result[0].RenewedFromContractID);
        }

        // Helper method to setup the reader columns for contract data
        private void SetupReaderWithContractColumns()
        {
            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("ID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("orderID")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("contractStatus")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("contractContent")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("renewalCount")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("predefinedContractID")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("pdfID")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("renewedFromContractID")).Returns(7);
        }
    }
}