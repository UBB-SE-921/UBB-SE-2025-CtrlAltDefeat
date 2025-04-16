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
        private Mock<IDatabaseProvider> mockDatabaseProvider;
        private Mock<IDbConnection> mockDatabaseConnection;
        private Mock<IDbCommand> mockDatabaseCommand;
        private Mock<IDataReader> mockDataReader;
        private Mock<IDataParameterCollection> mockParameterCollection;
        private string testConnectionString = "TestConnection";
        private ContractRenewalRepository contractRenewalModel;

        // Column indexes for database reader
        private const int ColumnIndex_ID = 0;
        private const int ColumnIndex_OrderID = 1;
        private const int ColumnIndex_ContractStatus = 2;
        private const int ColumnIndex_ContractContent = 3;
        private const int ColumnIndex_RenewalCount = 4;
        private const int ColumnIndex_PredefinedContractID = 5;
        private const int ColumnIndex_PDFID = 6;
        private const int ColumnIndex_RenewedFromContractID = 7;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            mockDatabaseProvider = new Mock<IDatabaseProvider>();
            mockDatabaseConnection = new Mock<IDbConnection>();
            mockDatabaseCommand = new Mock<IDbCommand>();
            mockDataReader = new Mock<IDataReader>();
            mockParameterCollection = new Mock<IDataParameterCollection>();

            // Setup the parameter collection mock
            mockParameterCollection
                .Setup(parameters => parameters.Add(It.IsAny<object>()))
                .Returns(0);

            // Setup the command mock
            mockDatabaseCommand.Setup(command => command.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
            mockDatabaseCommand.Setup(command => command.Parameters).Returns(mockParameterCollection.Object);
            mockDatabaseCommand.Setup(command => command.ExecuteReader()).Returns(mockDataReader.Object);

            // Setup the connection mock
            mockDatabaseConnection.Setup(Database_connection => Database_connection.CreateCommand()).Returns(mockDatabaseCommand.Object);

            // Setup the database provider mock
            mockDatabaseProvider.Setup(Database_provider => Database_provider.CreateConnection(It.IsAny<string>())).Returns(mockDatabaseConnection.Object);

            // Create the ContractRenewalModel with the mock database provider
            contractRenewalModel = new ContractRenewalRepository(testConnectionString, mockDatabaseProvider.Object);
        }

        [TestMethod]
        public void Constructor_WithConnectionStringOnly_InitializesCorrectly()
        {
            // Arrange & Act
            var contractRenewalModel = new ContractRenewalRepository(testConnectionString);

            // Assert
            Assert.IsNotNull(contractRenewalModel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            _ = new ContractRenewalRepository(null, mockDatabaseProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullDatabaseProvider_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            _ = new ContractRenewalRepository(testConnectionString, null);
        }

        [TestMethod]
        public async Task AddRenewedContractAsync_WithValidContract_ExecutesStoredProcedure()
        {
            // Arrange
            var testContract = new Contract
            {
                OrderID = 123,
                ContractStatus = "RENEWED",
                ContractContent = "Contract Content",
                RenewalCount = 1,
                PredefinedContractID = 2,
                PDFID = 456,
                RenewedFromContractID = 789
            };
            byte[] samplePdfData = new byte[] { 1, 2, 3, 4, 5 };

            mockDatabaseCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);

            // Act
            await contractRenewalModel.AddRenewedContractAsync(testContract, samplePdfData);

            // Assert
            mockDatabaseProvider.Verify(provider => provider.CreateConnection(testConnectionString), Times.Once);
            mockDatabaseConnection.Verify(Database_connection => Database_connection.Open(), Times.Once);
            mockDatabaseCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
        }

        [TestMethod]
        public async Task HasContractBeenRenewedAsync_WhenContractHasBeenRenewed_ReturnsTrue()
        {
            // Arrange
            long testContractId = 123;
            mockDatabaseCommand.Setup(command => command.ExecuteScalar()).Returns(1);

            // Act
            bool hasBeenRenewed = await contractRenewalModel.HasContractBeenRenewedAsync(testContractId);

            // Assert
            Assert.IsTrue(hasBeenRenewed);
            mockDatabaseProvider.Verify(provider => provider.CreateConnection(testConnectionString), Times.Once);
            mockDatabaseConnection.Verify(connection => connection.Open(), Times.Once);
            mockDatabaseCommand.Verify(command => command.ExecuteScalar(), Times.Once);
            //_mockParameterCollection.Verify(parameters => parameters.Add(It.Is<object>(o =>
            //    o is IDataParameter param &&
            //    param.ParameterName == "@ContractID" &&
            //    (long)param.Value == testContractId)), Times.Once);
        }

        [TestMethod]
        public async Task HasContractBeenRenewedAsync_WhenContractHasNotBeenRenewed_ReturnsFalse()
        {
            // Arrange
            long testContractId = 456;
            mockDatabaseCommand.Setup(command => command.ExecuteScalar()).Returns(0);

            // Act
            bool hasBeenRenewed = await contractRenewalModel.HasContractBeenRenewedAsync(testContractId);

            // Assert
            Assert.IsFalse(hasBeenRenewed);
        }

        [TestMethod]
        public async Task HasContractBeenRenewedAsync_WhenScalarReturnsNull_ReturnsFalse()
        {
            // Arrange
            long testContractId = 456;
            mockDatabaseCommand.Setup(command => command.ExecuteScalar()).Returns(null);

            // Act
            bool hasBeenRenewed = await contractRenewalModel.HasContractBeenRenewedAsync(testContractId);

            // Assert
            Assert.IsFalse(hasBeenRenewed);
        }

        [TestMethod]
        public async Task GetRenewedContractsAsync_ReturnsListOfContracts()
        {
            // Arrange
            SetupReaderWithContractColumns();

            // Setup to track which read call we're on
            int contractReadCount = 0;
            mockDataReader.Setup(reader => reader.Read()).Returns(() => contractReadCount++ < 2); // Return 2 contracts

            // Store expected values in arrays for consistent access
            long[] expectedContractIds = new long[] { 101, 102 };
            int[] expectedOrderIds = new int[] { 201, 202 };
            string[] expectedContractContents = new string[] { "Content 1", "Content 2" };
            int[] expectedRenewalCounts = new int[] { 1, 2 };
            int[] expectedPredefinedContractIds = new int[] { 301, 302 };
            int[] expectedPdfIds = new int[] { 401, 402 };
            long[] expectedRenewedFromIds = new long[] { 501, 502 };

            // Setup data for contracts - use proper function syntax
            mockDataReader.Setup(Database_reader => Database_reader.GetInt64(ColumnIndex_ID)).Returns(() => expectedContractIds[Math.Min(contractReadCount - 1, 1)]);
            mockDataReader.Setup(Database_reader => Database_reader.GetInt32(ColumnIndex_OrderID)).Returns(() => expectedOrderIds[Math.Min(contractReadCount - 1, 1)]);
            mockDataReader.Setup(Database_reader => Database_reader.GetString(ColumnIndex_ContractStatus)).Returns("RENEWED");
            mockDataReader.Setup(Database_reader => Database_reader["contractContent"]).Returns(() => expectedContractContents[Math.Min(contractReadCount - 1, 1)]);
            mockDataReader.Setup(Database_reader => Database_reader.GetInt32(ColumnIndex_RenewalCount)).Returns(() => expectedRenewalCounts[Math.Min(contractReadCount - 1, 1)]);
            mockDataReader.Setup(Database_reader => Database_reader.IsDBNull(ColumnIndex_PredefinedContractID)).Returns(false);
            mockDataReader.Setup(Database_reader => Database_reader.GetInt32(ColumnIndex_PredefinedContractID)).Returns(() => expectedPredefinedContractIds[Math.Min(contractReadCount - 1, 1)]);
            mockDataReader.Setup(Database_reader => Database_reader.GetInt32(ColumnIndex_PDFID)).Returns(() => expectedPdfIds[Math.Min(contractReadCount - 1, 1)]);
            mockDataReader.Setup(Database_reader => Database_reader.IsDBNull(ColumnIndex_RenewedFromContractID)).Returns(false);
            mockDataReader.Setup(reader => reader.GetInt64(ColumnIndex_RenewedFromContractID)).Returns(() => expectedRenewedFromIds[Math.Min(contractReadCount - 1, 1)]);

            // Act
            var contracts = await contractRenewalModel.GetRenewedContractsAsync();

            // Assert
            Assert.IsNotNull(contracts);
            Assert.AreEqual(2, contracts.Count);
            Assert.AreEqual(expectedContractIds[0], contracts[0].ContractID);
            Assert.AreEqual(expectedContractIds[1], contracts[1].ContractID);
            Assert.AreEqual(expectedOrderIds[0], contracts[0].OrderID);
            Assert.AreEqual(expectedOrderIds[1], contracts[1].OrderID);
            Assert.AreEqual("RENEWED", contracts[0].ContractStatus);
            Assert.AreEqual("RENEWED", contracts[1].ContractStatus);
            Assert.AreEqual(expectedContractContents[0], contracts[0].ContractContent);
            Assert.AreEqual(expectedContractContents[1], contracts[1].ContractContent);

            mockDatabaseProvider.Verify(provider => provider.CreateConnection(testConnectionString), Times.Once);
            mockDatabaseConnection.Verify(connection => connection.Open(), Times.Once);
            mockDatabaseCommand.Verify(command => command.ExecuteReader(), Times.Once);
        }

        [TestMethod]
        public async Task GetRenewedContractsAsync_WhenNoContractsFound_ReturnsEmptyList()
        {
            // Arrange
            SetupReaderWithContractColumns();
            mockDataReader.Setup(reader => reader.Read()).Returns(false); // Return no contracts

            // Act
            var contracts = await contractRenewalModel.GetRenewedContractsAsync();

            // Assert
            Assert.IsNotNull(contracts);
            Assert.AreEqual(0, contracts.Count);
        }

        [TestMethod]
        public async Task GetRenewedContractsAsync_HandlesNullValues()
        {
            // Arrange
            SetupReaderWithContractColumns();

            int contractReadCount = 0;
            mockDataReader.Setup(reader => reader.Read()).Returns(() => contractReadCount++ < 1);

            // Setup data with nulls
            mockDataReader.Setup(Database_reader => Database_reader.GetInt64(ColumnIndex_ID)).Returns(101L);
            mockDataReader.Setup(Database_reader => Database_reader.GetInt32(ColumnIndex_OrderID)).Returns(201);
            mockDataReader.Setup(Database_reader => Database_reader.GetString(ColumnIndex_ContractStatus)).Returns("RENEWED");
            mockDataReader.Setup(Database_reader => Database_reader["contractContent"]).Returns(null);
            mockDataReader.Setup(Database_reader => Database_reader.GetInt32(ColumnIndex_RenewalCount)).Returns(1);
            mockDataReader.Setup(Database_reader => Database_reader.IsDBNull(ColumnIndex_PredefinedContractID)).Returns(true);
            mockDataReader.Setup(Database_reader => Database_reader.GetInt32(ColumnIndex_PDFID)).Returns(401);
            mockDataReader.Setup(Database_reader => Database_reader.IsDBNull(ColumnIndex_RenewedFromContractID)).Returns(true);

            // Act
            var contracts = await contractRenewalModel.GetRenewedContractsAsync();

            // Assert
            Assert.IsNotNull(contracts);
            Assert.AreEqual(1, contracts.Count);
            Assert.AreEqual(101L, contracts[0].ContractID);
            Assert.IsNull(contracts[0].ContractContent);
            Assert.IsNull(contracts[0].PredefinedContractID);
            Assert.IsNull(contracts[0].RenewedFromContractID);
        }

        // Helper method to setup the reader columns for contract data
        private void SetupReaderWithContractColumns()
        {
            // Setup column ordinals
            mockDataReader.Setup(Database_reader => Database_reader.GetOrdinal("ID")).Returns(ColumnIndex_ID);
            mockDataReader.Setup(Database_reader => Database_reader.GetOrdinal("orderID")).Returns(ColumnIndex_OrderID);
            mockDataReader.Setup(Database_reader => Database_reader.GetOrdinal("contractStatus")).Returns(ColumnIndex_ContractStatus);
            mockDataReader.Setup(Database_reader => Database_reader.GetOrdinal("contractContent")).Returns(ColumnIndex_ContractContent);
            mockDataReader.Setup(Database_reader => Database_reader.GetOrdinal("renewalCount")).Returns(ColumnIndex_RenewalCount);
            mockDataReader.Setup(Database_reader => Database_reader.GetOrdinal("predefinedContractID")).Returns(ColumnIndex_PredefinedContractID);
            mockDataReader.Setup(Database_reader => Database_reader.GetOrdinal("pdfID")).Returns(ColumnIndex_PDFID);
            mockDataReader.Setup(Database_reader => Database_reader.GetOrdinal("renewedFromContractID")).Returns(ColumnIndex_RenewedFromContractID);
        }
    }
}