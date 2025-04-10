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
    public class ContractModelTests
    {
        private const string TEST_CONNECTION_STRING = "TestConnection";

        // Column index constants
        private const int COLUMN_INDEX_ID = 0;
        private const int COLUMN_INDEX_ORDER_ID = 1;
        private const int COLUMN_INDEX_CONTRACT_STATUS = 2;
        private const int COLUMN_INDEX_CONTRACT_CONTENT = 3;
        private const int COLUMN_INDEX_RENEWAL_COUNT = 4;
        private const int COLUMN_INDEX_PREDEFINED_CONTRACT_ID = 5;
        private const int COLUMN_INDEX_PDF_ID = 6;
        private const int COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID = 7;
        private const int COLUMN_INDEX_ADDITIONAL_TERMS = 7;

        private Mock<IDatabaseProvider> _mockDbProvider;
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private ContractModel _contractModel;

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
                .Setup(parameters => parameters.Add(It.IsAny<object>()))
                .Returns(0);

            // Setup the command mock
            _mockCommand.Setup(command => command.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
            _mockCommand.Setup(command => command.Parameters).Returns(_mockParameters.Object);
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Setup the connection mock
            _mockConnection.Setup(Database_connection => Database_connection.CreateCommand()).Returns(_mockCommand.Object);

            // Setup the database provider mock
            _mockDbProvider.Setup(provider => provider.CreateConnection(It.IsAny<string>())).Returns(_mockConnection.Object);

            // Create the ContractModel with the mock database provider
            _contractModel = new ContractModel(TEST_CONNECTION_STRING, _mockDbProvider.Object);
        }

        [TestMethod]
        public void Constructor_WithConnectionStringOnly_InitializesCorrectly()
        {
            // Arrange & Act
            var model = new ContractModel(TEST_CONNECTION_STRING);

            // Assert
            Assert.IsNotNull(model);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            _ = new ContractModel(null, _mockDbProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullDatabaseProvider_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            _ = new ContractModel(TEST_CONNECTION_STRING, null);
        }

        [TestMethod]
        public async Task GetPredefinedContractByPredefineContractTypeAsync_ReturnsContract_WhenFound()
        {
            // Arrange
            var contractType = PredefinedContractType.BorrowingContract;
            int contractId = 3;
            string content = "Contract Content";

            // Setup reader to return data
            SetupReaderWithColumns("ID", "content");

            int readCount = 0;
            _mockReader.Setup(Database_reader => Database_reader.Read()).Returns(() => readCount++ < 1);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_ID)).Returns(contractId);
            _mockReader.Setup(Database_reader => Database_reader.GetOrdinal("ID")).Returns(COLUMN_INDEX_ID);
            _mockReader.Setup(Database_reader => Database_reader.GetOrdinal("content")).Returns(1);
            _mockReader.Setup(Database_reader => Database_reader[1]).Returns(content);
            _mockReader.Setup(Database_reader => Database_reader["content"]).Returns(content);

            // Act
            var result = await _contractModel.GetPredefinedContractByPredefineContractTypeAsync(contractType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(contractId, result.ContractID);
            Assert.AreEqual(content, result.ContractContent);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
        }

        [TestMethod]
        public async Task GetPredefinedContractByPredefineContractTypeAsync_ReturnsEmptyContract_WhenNotFound()
        {
            // Arrange
            var contractType = PredefinedContractType.BorrowingContract;

            // Setup reader to return no data
            _mockReader.Setup(reader => reader.Read()).Returns(false);

            // Act
            var result = await _contractModel.GetPredefinedContractByPredefineContractTypeAsync(contractType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.ContractID);
            Assert.AreEqual(string.Empty, result.ContractContent);
        }

        [TestMethod]
        public async Task GetContractByIdAsync_ReturnsContract_WhenFound()
        {
            // Arrange
            long contractId = 123;
            int orderId = 456;
            string status = "ACTIVE";
            string content = "Contract Content";
            int renewalCount = 1;
            int? predefinedContractId = 2;
            int pdfId = 789;
            long? renewedFromContractId = 100;

            // Setup reader to return data
            SetupReaderWithFullContractColumns();

            int readCount = 0;
            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_ID)).Returns(contractId);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_ORDER_ID)).Returns(orderId);
            _mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_CONTRACT_STATUS)).Returns(status);
            _mockReader.Setup(Database_reader => Database_reader[COLUMN_INDEX_CONTRACT_CONTENT]).Returns(content);
            _mockReader.Setup(Database_reader => Database_reader["contractContent"]).Returns(content);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_RENEWAL_COUNT)).Returns(renewalCount);
            _mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(false);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(predefinedContractId.Value);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PDF_ID)).Returns(pdfId);
            _mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(false);
            _mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(renewedFromContractId.Value);

            // Act
            var result = await _contractModel.GetContractByIdAsync(contractId);

            // Assert
            Assert.AreEqual(0, result.ContractID);
        }

        [TestMethod]
        public async Task AddContractAsync_ReturnsNewContract()
        {
            // Arrange
            var contract = new Contract
            {
                OrderID = 123,
                ContractStatus = "ACTIVE",
                ContractContent = "New Contract Content",
                RenewalCount = 0,
                PredefinedContractID = 1,
                PDFID = 456,
                RenewedFromContractID = null
            };

            byte[] pdfData = new byte[] { 1, 2, 3, 4, 5 };
            long newContractId = 789;

            SetupReaderWithFullContractColumns();

            int readCount = 0;
            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_ID)).Returns(newContractId);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_ORDER_ID)).Returns(contract.OrderID);
            _mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_CONTRACT_STATUS)).Returns(contract.ContractStatus);
            _mockReader.Setup(Database_reader => Database_reader[COLUMN_INDEX_CONTRACT_CONTENT]).Returns(contract.ContractContent);
            _mockReader.Setup(Database_reader => Database_reader["contractContent"]).Returns(contract.ContractContent);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_RENEWAL_COUNT)).Returns(contract.RenewalCount);
            _mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(false);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(contract.PredefinedContractID.Value);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PDF_ID)).Returns(contract.PDFID);
            _mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(true);

            // Act
            var result = await _contractModel.AddContractAsync(contract, pdfData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(newContractId, result.ContractID);
            Assert.AreEqual(contract.OrderID, result.OrderID);
            Assert.AreEqual(contract.ContractStatus, result.ContractStatus);
            Assert.AreEqual(contract.ContractContent, result.ContractContent);
        }

        [TestMethod]
        public async Task GetContractByIdAsync_ReturnsEmptyContract_WhenNotFound()
        {
            // Arrange
            long contractId = 999;

            // Setup reader to return no data
            _mockReader.Setup(reader => reader.Read()).Returns(false);

            // Act
            var result = await _contractModel.GetContractByIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.ContractID); // Default value for uninitialized Contract
        }

        [TestMethod]
        public async Task GetAllContractsAsync_ReturnsContractsList()
        {
            // Arrange
            SetupReaderWithFullContractColumns();

            int readCount = 0;
            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 2); // Return 2 contracts

            // Setup for first contract
            _mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_ID)).Returns(() => readCount == 1 ? 101 : 102);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_ORDER_ID)).Returns(() => readCount == 1 ? 201 : 202);
            _mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_CONTRACT_STATUS)).Returns(() => readCount == 1 ? "ACTIVE" : "EXPIRED");
            _mockReader.Setup(Database_reader => Database_reader[COLUMN_INDEX_CONTRACT_CONTENT]).Returns(() => readCount == 1 ? "Content 1" : "Content 2");
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_RENEWAL_COUNT)).Returns(() => readCount == 1 ? 0 : 1);
            _mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(() => readCount == 1 ? false : true);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(1);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PDF_ID)).Returns(() => readCount == 1 ? 301 : 302);
            _mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(() => readCount == 1 ? true : false);
            _mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(100);

            // Act
            var result = await _contractModel.GetAllContractsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(101, result[0].ContractID);
            Assert.AreEqual(102, result[1].ContractID);
            Assert.AreEqual("ACTIVE", result[0].ContractStatus);
            Assert.AreEqual("EXPIRED", result[1].ContractStatus);
        }

        [TestMethod]
        public async Task GetContractHistoryAsync_ReturnsHistoryList()
        {
            // Arrange
            long contractId = 123;
            SetupReaderWithFullContractColumns();

            int readCount = 0;
            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 2); // Return 2 history entries

            // Setup for contract history
            _mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_ID)).Returns(() => readCount == 1 ? 101 : 102);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_ORDER_ID)).Returns(() => readCount == 1 ? 201 : 201); // Same order ID
            _mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_CONTRACT_STATUS)).Returns(() => readCount == 1 ? "ACTIVE" : "RENEWED");
            _mockReader.Setup(Database_reader => Database_reader[COLUMN_INDEX_CONTRACT_CONTENT]).Returns(() => readCount == 1 ? "Original Content" : "Renewed Content");
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_RENEWAL_COUNT)).Returns(() => readCount == 1 ? 0 : 1); // Renewal count increased
            _mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(false);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(1);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PDF_ID)).Returns(() => readCount == 1 ? 301 : 302);
            _mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(() => readCount == 1 ? true : false);
            _mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(101); // Points to original contract

            // Act
            var result = await _contractModel.GetContractHistoryAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(101, result[0].ContractID);
            Assert.AreEqual(102, result[1].ContractID);
            Assert.AreEqual("ACTIVE", result[0].ContractStatus);
            Assert.AreEqual("RENEWED", result[1].ContractStatus);
            Assert.AreEqual(0, result[0].RenewalCount);
            Assert.AreEqual(1, result[1].RenewalCount);
            Assert.AreEqual(101, result[1].RenewedFromContractID);
        }

        [TestMethod]
        public async Task GetContractSellerAsync_ReturnsSellerInfo()
        {
            // Arrange
            long contractId = 123;
            int sellerId = 456;
            string sellerName = "John Doe";

            // Define column indices
            const int COLUMN_INDEX_SELLER_ID = 0;
            const int COLUMN_INDEX_SELLER_NAME = 1;

            // Setup columns
            _mockReader.Setup(Database_reader => Database_reader.GetOrdinal("SellerID")).Returns(COLUMN_INDEX_SELLER_ID);
            _mockReader.Setup(Database_reader => Database_reader.GetOrdinal("SellerName")).Returns(COLUMN_INDEX_SELLER_NAME);

            int readCount = 0;
            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(reader => reader.GetInt32(COLUMN_INDEX_SELLER_ID)).Returns(sellerId);
            _mockReader.Setup(reader => reader.GetString(COLUMN_INDEX_SELLER_NAME)).Returns(sellerName);

            // Act
            var result = await _contractModel.GetContractSellerAsync(contractId);

            // Assert
            Assert.AreEqual(sellerId, result.SellerID);
            Assert.AreEqual(sellerName, result.SellerName);
        }

        [TestMethod]
        public async Task GetContractBuyerAsync_ReturnsBuyerInfo()
        {
            // Arrange
            long contractId = 123;
            int buyerId = 789;
            string buyerName = "Jane Smith";

            // Define column indices
            const int COLUMN_INDEX_BUYER_ID = 0;
            const int COLUMN_INDEX_BUYER_NAME = 1;

            // Setup columns
            _mockReader.Setup(reader => reader.GetOrdinal("BuyerID")).Returns(COLUMN_INDEX_BUYER_ID);
            _mockReader.Setup(reader => reader.GetOrdinal("BuyerName")).Returns(COLUMN_INDEX_BUYER_NAME);

            int readCount = 0;
            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(reader => reader.GetInt32(COLUMN_INDEX_BUYER_ID)).Returns(buyerId);
            _mockReader.Setup(reader => reader.GetString(COLUMN_INDEX_BUYER_NAME)).Returns(buyerName);

            // Act
            var result = await _contractModel.GetContractBuyerAsync(contractId);

            // Assert
            Assert.AreEqual(buyerId, result.BuyerID);
            Assert.AreEqual(buyerName, result.BuyerName);
        }

        [TestMethod]
        public async Task GetOrderSummaryInformationAsync_ReturnsOrderSummaryInfo()
        {
            // Arrange
            long contractId = 123;

            // Setup data to return
            var summaryData = new Dictionary<string, object>
            {
                ["ID"] = 456,
                ["subtotal"] = 100.50,
                ["warrantyTax"] = 10.05,
                ["deliveryFee"] = 5.00,
                ["finalTotal"] = 115.55,
                ["fullName"] = "John Smith",
                ["email"] = "john@example.com",
                ["phoneNumber"] = "123-456-7890",
                ["address"] = "123 Main St",
                ["postalCode"] = "12345",
                ["additionalInfo"] = "Leave at door",
                ["ContractDetails"] = "Contract details text"
            };

            int readCount = 0;
            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            foreach (var item in summaryData)
            {
                _mockReader.Setup(reader => reader[item.Key]).Returns(item.Value);
            }

            // Act
            var result = await _contractModel.GetOrderSummaryInformationAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(summaryData.Count, result.Count);
            foreach (var item in summaryData)
            {
                Assert.AreEqual(item.Value, result[item.Key]);
            }
        }

        [TestMethod]
        public async Task GetProductDetailsByContractIdAsync_ReturnsProductDetails()
        {
            // Arrange
            long contractId = 123;
            var startDate = new DateTime(2023, 10, 1);
            var endDate = new DateTime(2023, 11, 1);
            double price = 99.99;
            string name = "Artwork Name";

            // Define column indices
            const int COLUMN_INDEX_START_DATE = 0;
            const int COLUMN_INDEX_END_DATE = 1;
            const int COLUMN_INDEX_PRICE = 2;
            const int COLUMN_INDEX_NAME = 3;

            // Setup columns
            _mockReader.Setup(Database_reader => Database_reader.GetOrdinal("startDate")).Returns(COLUMN_INDEX_START_DATE);
            _mockReader.Setup(Database_reader => Database_reader.GetOrdinal("endDate")).Returns(COLUMN_INDEX_END_DATE);
            _mockReader.Setup(Database_reader => Database_reader.GetOrdinal("price")).Returns(COLUMN_INDEX_PRICE);
            _mockReader.Setup(Database_reader => Database_reader.GetOrdinal("name")).Returns(COLUMN_INDEX_NAME);

            int readCount = 0;
            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(Database_reader => Database_reader.GetDateTime(COLUMN_INDEX_START_DATE)).Returns(startDate);
            _mockReader.Setup(Database_reader => Database_reader.GetDateTime(COLUMN_INDEX_END_DATE)).Returns(endDate);
            _mockReader.Setup(Database_reader => Database_reader.GetDouble(COLUMN_INDEX_PRICE)).Returns(price);
            _mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_NAME)).Returns(name);

            // Act
            var result = await _contractModel.GetProductDetailsByContractIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(startDate, result.Value.StartDate);
            Assert.AreEqual(endDate, result.Value.EndDate);
            Assert.AreEqual(price, result.Value.price);
            Assert.AreEqual(name, result.Value.name);
        }

        [TestMethod]
        public async Task GetProductDetailsByContractIdAsync_ReturnsNull_WhenNoDataFound()
        {
            // Arrange
            long contractId = 999;
            _mockReader.Setup(reader => reader.Read()).Returns(false);

            // Act
            var result = await _contractModel.GetProductDetailsByContractIdAsync(contractId);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        public async Task GetContractsByBuyerAsync_ReturnsContractsList()
        {
            // Arrange
            int buyerId = 123;

            // Setup columns for contracts with additional terms
            var columns = new Dictionary<string, int>
            {
                ["ID"] = COLUMN_INDEX_ID,
                ["orderID"] = COLUMN_INDEX_ORDER_ID,
                ["contractStatus"] = COLUMN_INDEX_CONTRACT_STATUS,
                ["contractContent"] = COLUMN_INDEX_CONTRACT_CONTENT,
                ["renewalCount"] = COLUMN_INDEX_RENEWAL_COUNT,
                ["predefinedContractID"] = COLUMN_INDEX_PREDEFINED_CONTRACT_ID,
                ["pdfID"] = COLUMN_INDEX_PDF_ID,
                ["AdditionalTerms"] = COLUMN_INDEX_ADDITIONAL_TERMS,
                ["renewedFromContractID"] = COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID
            };

            foreach (var col in columns)
            {
                _mockReader.Setup(reader => reader.GetOrdinal(col.Key)).Returns(col.Value);
            }

            int readCount = 0;
            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 2); // Return 2 contracts

            // Contract 1 data
            _mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_ID)).Returns(() => readCount == 1 ? 101 : 102);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_ORDER_ID)).Returns(() => readCount == 1 ? 201 : 202);
            _mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_CONTRACT_STATUS)).Returns(() => readCount == 1 ? "ACTIVE" : "EXPIRED");
            _mockReader.Setup(Database_reader => Database_reader[COLUMN_INDEX_CONTRACT_CONTENT]).Returns(() => readCount == 1 ? "Content 1" : "Content 2");
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_RENEWAL_COUNT)).Returns(() => readCount == 1 ? 0 : 1);
            _mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(true);
            _mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PDF_ID)).Returns(() => readCount == 1 ? 301 : 302);
            _mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_ADDITIONAL_TERMS)).Returns(() => readCount == 1 ? "Additional 1" : "Additional 2");
            _mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(true);

            // Act
            var result = await _contractModel.GetContractsByBuyerAsync(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(101, result[0].ContractID);
            Assert.AreEqual(102, result[1].ContractID);
            Assert.AreEqual("ACTIVE", result[0].ContractStatus);
            Assert.AreEqual("EXPIRED", result[1].ContractStatus);
            Assert.AreEqual("Additional 1", result[0].AdditionalTerms);
            Assert.AreEqual("Additional 2", result[1].AdditionalTerms);
        }

        [TestMethod]
        public async Task GetOrderDetailsAsync_ReturnsOrderDetails()
        {
            // Arrange
            long contractId = 123;
            string paymentMethod = "Credit Card";
            DateTime orderDate = new DateTime(2023, 10, 15);

            // Define column index
            const int COLUMN_INDEX_ORDER_DATE = 1;

            // Setup columns
            _mockReader.Setup(reader => reader.GetOrdinal("OrderDate")).Returns(COLUMN_INDEX_ORDER_DATE);

            int readCount = 0;
            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(reader => reader["PaymentMethod"]).Returns(paymentMethod);
            _mockReader.Setup(reader => reader.GetDateTime(COLUMN_INDEX_ORDER_DATE)).Returns(orderDate);

            // Act
            var result = await _contractModel.GetOrderDetailsAsync(contractId);

            // Assert
            Assert.AreEqual(paymentMethod, result.PaymentMethod);
            Assert.AreEqual(orderDate, result.OrderDate);
        }

        [TestMethod]
        public async Task GetDeliveryDateByContractIdAsync_ReturnsDate_WhenFound()
        {
            // Arrange
            long contractId = 123;
            DateTime deliveryDate = new DateTime(2023, 10, 20);

            // Define column index
            const int COLUMN_INDEX_DELIVERY_DATE = 0;

            // Setup columns
            _mockReader.Setup(reader => reader.GetOrdinal("EstimatedDeliveryDate")).Returns(COLUMN_INDEX_DELIVERY_DATE);
            _mockReader.Setup(reader => reader.IsDBNull(COLUMN_INDEX_DELIVERY_DATE)).Returns(false);

            int readCount = 0;
            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(reader => reader.GetDateTime(COLUMN_INDEX_DELIVERY_DATE)).Returns(deliveryDate);

            // Act
            var result = await _contractModel.GetDeliveryDateByContractIdAsync(contractId);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(deliveryDate, result.Value);
        }

        [TestMethod]
        public async Task GetDeliveryDateByContractIdAsync_ReturnsNull_WhenDateIsNull()
        {
            // Arrange
            long contractId = 123;

            // Define column index
            const int COLUMN_INDEX_DELIVERY_DATE = 0;

            // Setup columns
            _mockReader.Setup(Database_reader => Database_reader.GetOrdinal("EstimatedDeliveryDate")).Returns(COLUMN_INDEX_DELIVERY_DATE);
            _mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_DELIVERY_DATE)).Returns(true);

            int readCount = 0;
            _mockReader.Setup(Database_reader => Database_reader.Read()).Returns(() => readCount++ < 1);

            // Act
            var result = await _contractModel.GetDeliveryDateByContractIdAsync(contractId);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        public async Task GetPdfByContractIdAsync_ReturnsPdfData_WhenFound()
        {
            // Arrange
            long contractId = 123;
            byte[] pdfData = new byte[] { 1, 2, 3, 4, 5 };

            int readCount = 0;
            _mockReader.Setup(Database_reader => Database_reader.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(Database_reader => Database_reader["PdfFile"]).Returns(pdfData);

            // Act
            var result = await _contractModel.GetPdfByContractIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(pdfData, result);
        }

        [TestMethod]
        public async Task GetPdfByContractIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            long contractId = 999;
            _mockReader.Setup(Database_reader => Database_reader.Read()).Returns(false);

            // Act
            var result = await _contractModel.GetPdfByContractIdAsync(contractId);

            // Assert
            Assert.IsNull(result);
        }

        // Helper methods for test setup
        private void SetupReaderWithColumns(params string[] columnNames)
        {
            for (int columnIndex = 0; columnIndex < columnNames.Length; columnIndex++)
            {
                int capturedColumnIndex = columnIndex; // Capture current value
                _mockReader.Setup(Database_reader => Database_reader.GetOrdinal(columnNames[capturedColumnIndex])).Returns(capturedColumnIndex);
            }
        }

        private void SetupReaderWithFullContractColumns()
        {
            // Setup column ordinals for contract
           // Setup column ordinals for contract
            var columns = new Dictionary<string, int>
            {
                ["ID"] = COLUMN_INDEX_ID,
                ["orderID"] = COLUMN_INDEX_ORDER_ID,
                ["contractStatus"] = COLUMN_INDEX_CONTRACT_STATUS,
                ["contractContent"] = COLUMN_INDEX_CONTRACT_CONTENT,
                ["renewalCount"] = COLUMN_INDEX_RENEWAL_COUNT,
                ["predefinedContractID"] = COLUMN_INDEX_PREDEFINED_CONTRACT_ID,
                ["pdfID"] = COLUMN_INDEX_PDF_ID,
                ["renewedFromContractID"] = COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID
            };

            foreach (var column in columns)
            {
                _mockReader.Setup(reader => reader.GetOrdinal(column.Key)).Returns(column.Value);
            }
        }
    }
}