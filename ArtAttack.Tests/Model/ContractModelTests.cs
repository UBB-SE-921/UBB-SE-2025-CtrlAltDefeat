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

        private Mock<IDatabaseProvider> mockDbProvider;
        private Mock<IDbConnection> mockConnection;
        private Mock<IDbCommand> mockCommand;
        private Mock<IDataReader> mockReader;
        private Mock<IDataParameterCollection> mockParameters;
        private ContractRepository contractModel;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            mockDbProvider = new Mock<IDatabaseProvider>();
            mockConnection = new Mock<IDbConnection>();
            mockCommand = new Mock<IDbCommand>();
            mockReader = new Mock<IDataReader>();
            mockParameters = new Mock<IDataParameterCollection>();

            // Setup the parameter collection mock
            mockParameters
                .Setup(parameters => parameters.Add(It.IsAny<object>()))
                .Returns(0);

            // Setup the command mock
            mockCommand.Setup(command => command.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
            mockCommand.Setup(command => command.Parameters).Returns(mockParameters.Object);
            mockCommand.Setup(command => command.ExecuteReader()).Returns(mockReader.Object);

            // Setup the connection mock
            mockConnection.Setup(Database_connection => Database_connection.CreateCommand()).Returns(mockCommand.Object);

            // Setup the database provider mock
            mockDbProvider.Setup(provider => provider.CreateConnection(It.IsAny<string>())).Returns(mockConnection.Object);

            // Create the ContractModel with the mock database provider
            contractModel = new ContractRepository(TEST_CONNECTION_STRING, mockDbProvider.Object);
        }

        [TestMethod]
        public void Constructor_WithConnectionStringOnly_InitializesCorrectly()
        {
            // Arrange & Act
            var model = new ContractRepository(TEST_CONNECTION_STRING);

            // Assert
            Assert.IsNotNull(model);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            _ = new ContractRepository(null, mockDbProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullDatabaseProvider_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            _ = new ContractRepository(TEST_CONNECTION_STRING, null);
        }

        [TestMethod]
        public async Task GetPredefinedContractByPredefineContractTypeAsync_WhenFound_ReturnsContract()
        {
            // Arrange
            var contractType = PredefinedContractType.BorrowingContract;
            int contractId = 3;
            string content = "Contract Content";

            mockReader.Setup(Database_reader => Database_reader.GetOrdinal("ID")).Returns(COLUMN_INDEX_ID);
            mockReader.Setup(Database_reader => Database_reader.GetOrdinal("content")).Returns(1);

            int readCount = 0;
            mockReader.Setup(Database_reader => Database_reader.Read()).Returns(() => readCount++ < 1);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_ID)).Returns(contractId);
            mockReader.Setup(Database_reader => Database_reader.GetOrdinal("ID")).Returns(COLUMN_INDEX_ID);
            mockReader.Setup(Database_reader => Database_reader.GetOrdinal("content")).Returns(1);
            mockReader.Setup(Database_reader => Database_reader[1]).Returns(content);
            mockReader.Setup(Database_reader => Database_reader["content"]).Returns(content);

            // Act
            var result = await contractModel.GetPredefinedContractByPredefineContractTypeAsync(contractType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(contractId, result.ContractID);
            Assert.AreEqual(content, result.ContractContent);
            mockConnection.Verify(connection => connection.Open(), Times.Once);
            mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
        }

        [TestMethod]
        public async Task GetPredefinedContractByPredefineContractTypeAsync_WhenNotFound_ReturnsEmptyContract()
        {
            // Arrange
            var contractType = PredefinedContractType.BorrowingContract;

            // Setup reader to return no data
            mockReader.Setup(reader => reader.Read()).Returns(false);

            // Act
            var result = await contractModel.GetPredefinedContractByPredefineContractTypeAsync(contractType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.ContractID);
            Assert.AreEqual(string.Empty, result.ContractContent);
        }

        [TestMethod]
        public async Task GetContractByIdAsync_WhenFound_ReturnsContract()
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
            mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_ID)).Returns(contractId);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_ORDER_ID)).Returns(orderId);
            mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_CONTRACT_STATUS)).Returns(status);
            mockReader.Setup(Database_reader => Database_reader[COLUMN_INDEX_CONTRACT_CONTENT]).Returns(content);
            mockReader.Setup(Database_reader => Database_reader["contractContent"]).Returns(content);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_RENEWAL_COUNT)).Returns(renewalCount);
            mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(false);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(predefinedContractId.Value);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PDF_ID)).Returns(pdfId);
            mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(false);
            mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(renewedFromContractId.Value);

            // Act
            var result = await contractModel.GetContractByIdAsync(contractId);

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
            mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_ID)).Returns(newContractId);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_ORDER_ID)).Returns(contract.OrderID);
            mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_CONTRACT_STATUS)).Returns(contract.ContractStatus);
            mockReader.Setup(Database_reader => Database_reader[COLUMN_INDEX_CONTRACT_CONTENT]).Returns(contract.ContractContent);
            mockReader.Setup(Database_reader => Database_reader["contractContent"]).Returns(contract.ContractContent);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_RENEWAL_COUNT)).Returns(contract.RenewalCount);
            mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(false);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(contract.PredefinedContractID.Value);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PDF_ID)).Returns(contract.PDFID);
            mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(true);

            // Act
            var result = await contractModel.AddContractAsync(contract, pdfData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(newContractId, result.ContractID);
            Assert.AreEqual(contract.OrderID, result.OrderID);
            Assert.AreEqual(contract.ContractStatus, result.ContractStatus);
            Assert.AreEqual(contract.ContractContent, result.ContractContent);
        }

        [TestMethod]
        public async Task GetContractByIdAsync_WhenNotFound_ReturnsEmptyContract()
        {
            // Arrange
            long contractId = 999;

            // Setup reader to return no data
            mockReader.Setup(reader => reader.Read()).Returns(false);

            // Act
            var result = await contractModel.GetContractByIdAsync(contractId);

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
            mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 2); // Return 2 contracts

            // Setup for first contract
            mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_ID)).Returns(() => readCount == 1 ? 101 : 102);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_ORDER_ID)).Returns(() => readCount == 1 ? 201 : 202);
            mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_CONTRACT_STATUS)).Returns(() => readCount == 1 ? "ACTIVE" : "EXPIRED");
            mockReader.Setup(Database_reader => Database_reader[COLUMN_INDEX_CONTRACT_CONTENT]).Returns(() => readCount == 1 ? "Content 1" : "Content 2");
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_RENEWAL_COUNT)).Returns(() => readCount == 1 ? 0 : 1);
            mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(() => readCount == 1 ? false : true);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(1);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PDF_ID)).Returns(() => readCount == 1 ? 301 : 302);
            mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(() => readCount == 1 ? true : false);
            mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(100);

            // Act
            var result = await contractModel.GetAllContractsAsync();

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
            mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 2); // Return 2 history entries

            // Setup for contract history
            mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_ID)).Returns(() => readCount == 1 ? 101 : 102);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_ORDER_ID)).Returns(() => readCount == 1 ? 201 : 201); // Same order ID
            mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_CONTRACT_STATUS)).Returns(() => readCount == 1 ? "ACTIVE" : "RENEWED");
            mockReader.Setup(Database_reader => Database_reader[COLUMN_INDEX_CONTRACT_CONTENT]).Returns(() => readCount == 1 ? "Original Content" : "Renewed Content");
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_RENEWAL_COUNT)).Returns(() => readCount == 1 ? 0 : 1); // Renewal count increased
            mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(false);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(1);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PDF_ID)).Returns(() => readCount == 1 ? 301 : 302);
            mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(() => readCount == 1 ? true : false);
            mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(101); // Points to original contract

            // Act
            var result = await contractModel.GetContractHistoryAsync(contractId);

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
            mockReader.Setup(Database_reader => Database_reader.GetOrdinal("SellerID")).Returns(COLUMN_INDEX_SELLER_ID);
            mockReader.Setup(Database_reader => Database_reader.GetOrdinal("SellerName")).Returns(COLUMN_INDEX_SELLER_NAME);

            int readCount = 0;
            mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            mockReader.Setup(reader => reader.GetInt32(COLUMN_INDEX_SELLER_ID)).Returns(sellerId);
            mockReader.Setup(reader => reader.GetString(COLUMN_INDEX_SELLER_NAME)).Returns(sellerName);

            // Act
            var result = await contractModel.GetContractSellerAsync(contractId);

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
            mockReader.Setup(reader => reader.GetOrdinal("BuyerID")).Returns(COLUMN_INDEX_BUYER_ID);
            mockReader.Setup(reader => reader.GetOrdinal("BuyerName")).Returns(COLUMN_INDEX_BUYER_NAME);

            int readCount = 0;
            mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            mockReader.Setup(reader => reader.GetInt32(COLUMN_INDEX_BUYER_ID)).Returns(buyerId);
            mockReader.Setup(reader => reader.GetString(COLUMN_INDEX_BUYER_NAME)).Returns(buyerName);

            // Act
            var result = await contractModel.GetContractBuyerAsync(contractId);

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
            mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            // Setup each key-value pair explicitly
            mockReader.Setup(reader => reader["ID"]).Returns(summaryData["ID"]);
            mockReader.Setup(reader => reader["subtotal"]).Returns(summaryData["subtotal"]);
            mockReader.Setup(reader => reader["warrantyTax"]).Returns(summaryData["warrantyTax"]);
            mockReader.Setup(reader => reader["deliveryFee"]).Returns(summaryData["deliveryFee"]);
            mockReader.Setup(reader => reader["finalTotal"]).Returns(summaryData["finalTotal"]);
            mockReader.Setup(reader => reader["fullName"]).Returns(summaryData["fullName"]);
            mockReader.Setup(reader => reader["email"]).Returns(summaryData["email"]);
            mockReader.Setup(reader => reader["phoneNumber"]).Returns(summaryData["phoneNumber"]);
            mockReader.Setup(reader => reader["address"]).Returns(summaryData["address"]);
            mockReader.Setup(reader => reader["postalCode"]).Returns(summaryData["postalCode"]);
            mockReader.Setup(reader => reader["additionalInfo"]).Returns(summaryData["additionalInfo"]);
            mockReader.Setup(reader => reader["ContractDetails"]).Returns(summaryData["ContractDetails"]);

            // Act
            var result = await contractModel.GetOrderSummaryInformationAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(summaryData.Count, result.Count);

            // Assert each key-value pair explicitly
            Assert.AreEqual(summaryData["ID"], result["ID"]);
            Assert.AreEqual(summaryData["subtotal"], result["subtotal"]);
            Assert.AreEqual(summaryData["warrantyTax"], result["warrantyTax"]);
            Assert.AreEqual(summaryData["deliveryFee"], result["deliveryFee"]);
            Assert.AreEqual(summaryData["finalTotal"], result["finalTotal"]);
            Assert.AreEqual(summaryData["fullName"], result["fullName"]);
            Assert.AreEqual(summaryData["email"], result["email"]);
            Assert.AreEqual(summaryData["phoneNumber"], result["phoneNumber"]);
            Assert.AreEqual(summaryData["address"], result["address"]);
            Assert.AreEqual(summaryData["postalCode"], result["postalCode"]);
            Assert.AreEqual(summaryData["additionalInfo"], result["additionalInfo"]);
            Assert.AreEqual(summaryData["ContractDetails"], result["ContractDetails"]);
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
            mockReader.Setup(Database_reader => Database_reader.GetOrdinal("startDate")).Returns(COLUMN_INDEX_START_DATE);
            mockReader.Setup(Database_reader => Database_reader.GetOrdinal("endDate")).Returns(COLUMN_INDEX_END_DATE);
            mockReader.Setup(Database_reader => Database_reader.GetOrdinal("price")).Returns(COLUMN_INDEX_PRICE);
            mockReader.Setup(Database_reader => Database_reader.GetOrdinal("name")).Returns(COLUMN_INDEX_NAME);

            int readCount = 0;
            mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            mockReader.Setup(Database_reader => Database_reader.GetDateTime(COLUMN_INDEX_START_DATE)).Returns(startDate);
            mockReader.Setup(Database_reader => Database_reader.GetDateTime(COLUMN_INDEX_END_DATE)).Returns(endDate);
            mockReader.Setup(Database_reader => Database_reader.GetDouble(COLUMN_INDEX_PRICE)).Returns(price);
            mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_NAME)).Returns(name);

            // Act
            var result = await contractModel.GetProductDetailsByContractIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(startDate, result.Value.StartDate);
            Assert.AreEqual(endDate, result.Value.EndDate);
            Assert.AreEqual(price, result.Value.price);
            Assert.AreEqual(name, result.Value.name);
        }

        [TestMethod]
        public async Task GetProductDetailsByContractIdAsync_WhenNoDataFound_ReturnsNull()
        {
            // Arrange
            long contractId = 999;
            mockReader.Setup(reader => reader.Read()).Returns(false);

            // Act
            var result = await contractModel.GetProductDetailsByContractIdAsync(contractId);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        public async Task GetContractsByBuyerAsync_ReturnsContractsList()
        {
            // Arrange
            int buyerId = 123;

            // Setup columns for contracts with additional terms
            mockReader.Setup(reader => reader.GetOrdinal("ID")).Returns(COLUMN_INDEX_ID);
            mockReader.Setup(reader => reader.GetOrdinal("orderID")).Returns(COLUMN_INDEX_ORDER_ID);
            mockReader.Setup(reader => reader.GetOrdinal("contractStatus")).Returns(COLUMN_INDEX_CONTRACT_STATUS);
            mockReader.Setup(reader => reader.GetOrdinal("contractContent")).Returns(COLUMN_INDEX_CONTRACT_CONTENT);
            mockReader.Setup(reader => reader.GetOrdinal("renewalCount")).Returns(COLUMN_INDEX_RENEWAL_COUNT);
            mockReader.Setup(reader => reader.GetOrdinal("predefinedContractID")).Returns(COLUMN_INDEX_PREDEFINED_CONTRACT_ID);
            mockReader.Setup(reader => reader.GetOrdinal("pdfID")).Returns(COLUMN_INDEX_PDF_ID);
            mockReader.Setup(reader => reader.GetOrdinal("AdditionalTerms")).Returns(COLUMN_INDEX_ADDITIONAL_TERMS);
            mockReader.Setup(reader => reader.GetOrdinal("renewedFromContractID")).Returns(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID);

            int readCount = 0;
            mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 2); // Return 2 contracts

            // Contract 1 data
            mockReader.Setup(Database_reader => Database_reader.GetInt64(COLUMN_INDEX_ID)).Returns(() => readCount == 1 ? 101 : 102);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_ORDER_ID)).Returns(() => readCount == 1 ? 201 : 202);
            mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_CONTRACT_STATUS)).Returns(() => readCount == 1 ? "ACTIVE" : "EXPIRED");
            mockReader.Setup(Database_reader => Database_reader[COLUMN_INDEX_CONTRACT_CONTENT]).Returns(() => readCount == 1 ? "Content 1" : "Content 2");
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_RENEWAL_COUNT)).Returns(() => readCount == 1 ? 0 : 1);
            mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_PREDEFINED_CONTRACT_ID)).Returns(true);
            mockReader.Setup(Database_reader => Database_reader.GetInt32(COLUMN_INDEX_PDF_ID)).Returns(() => readCount == 1 ? 301 : 302);
            mockReader.Setup(Database_reader => Database_reader.GetString(COLUMN_INDEX_ADDITIONAL_TERMS)).Returns(() => readCount == 1 ? "Additional 1" : "Additional 2");
            mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID)).Returns(true);

            // Act
            var result = await contractModel.GetContractsByBuyerAsync(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            // Assert contract 1
            Assert.AreEqual(101, result[0].ContractID);
            Assert.AreEqual("ACTIVE", result[0].ContractStatus);
            Assert.AreEqual("Additional 1", result[0].AdditionalTerms);

            // Assert contract 2
            Assert.AreEqual(102, result[1].ContractID);
            Assert.AreEqual("EXPIRED", result[1].ContractStatus);
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
            mockReader.Setup(reader => reader.GetOrdinal("OrderDate")).Returns(COLUMN_INDEX_ORDER_DATE);

            int readCount = 0;
            mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            mockReader.Setup(reader => reader["PaymentMethod"]).Returns(paymentMethod);
            mockReader.Setup(reader => reader.GetDateTime(COLUMN_INDEX_ORDER_DATE)).Returns(orderDate);

            // Act
            var result = await contractModel.GetOrderDetailsAsync(contractId);

            // Assert
            Assert.AreEqual(paymentMethod, result.PaymentMethod);
            Assert.AreEqual(orderDate, result.OrderDate);
        }

        [TestMethod]
        public async Task GetDeliveryDateByContractIdAsync_WhenFound_ReturnsDate()
        {
            // Arrange
            long contractId = 123;
            DateTime deliveryDate = new DateTime(2023, 10, 20);

            // Define column index
            const int COLUMN_INDEX_DELIVERY_DATE = 0;

            // Setup columns
            mockReader.Setup(reader => reader.GetOrdinal("EstimatedDeliveryDate")).Returns(COLUMN_INDEX_DELIVERY_DATE);
            mockReader.Setup(reader => reader.IsDBNull(COLUMN_INDEX_DELIVERY_DATE)).Returns(false);

            int readCount = 0;
            mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            mockReader.Setup(reader => reader.GetDateTime(COLUMN_INDEX_DELIVERY_DATE)).Returns(deliveryDate);

            // Act
            var result = await contractModel.GetDeliveryDateByContractIdAsync(contractId);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(deliveryDate, result.Value);
        }

        [TestMethod]
        public async Task GetDeliveryDateByContractIdAsync_WhenDateIsNull_ReturnsNull()
        {
            // Arrange
            long contractId = 123;

            // Define column index
            const int COLUMN_INDEX_DELIVERY_DATE = 0;

            // Setup columns
            mockReader.Setup(Database_reader => Database_reader.GetOrdinal("EstimatedDeliveryDate")).Returns(COLUMN_INDEX_DELIVERY_DATE);
            mockReader.Setup(Database_reader => Database_reader.IsDBNull(COLUMN_INDEX_DELIVERY_DATE)).Returns(true);

            int readCount = 0;
            mockReader.Setup(Database_reader => Database_reader.Read()).Returns(() => readCount++ < 1);

            // Act
            var result = await contractModel.GetDeliveryDateByContractIdAsync(contractId);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        public async Task GetPdfByContractIdAsync_WhenFound_ReturnsPdfData()
        {
            // Arrange
            long contractId = 123;
            byte[] pdfData = new byte[] { 1, 2, 3, 4, 5 };

            int readCount = 0;
            mockReader.Setup(Database_reader => Database_reader.Read()).Returns(() => readCount++ < 1);

            mockReader.Setup(Database_reader => Database_reader["PdfFile"]).Returns(pdfData);

            // Act
            var result = await contractModel.GetPdfByContractIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(pdfData, result);
        }

        [TestMethod]
        public async Task GetPdfByContractIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            long contractId = 999;
            mockReader.Setup(Database_reader => Database_reader.Read()).Returns(false);

            // Act
            var result = await contractModel.GetPdfByContractIdAsync(contractId);

            // Assert
            Assert.IsNull(result);
        }


        private void SetupReaderWithFullContractColumns()
        {
            // Setup column ordinals for contract
            
            mockReader.Setup(reader => reader.GetOrdinal("ID")).Returns(COLUMN_INDEX_ID);
            mockReader.Setup(reader => reader.GetOrdinal("orderID")).Returns(COLUMN_INDEX_ORDER_ID);
            mockReader.Setup(reader => reader.GetOrdinal("contractStatus")).Returns(COLUMN_INDEX_CONTRACT_STATUS);
            mockReader.Setup(reader => reader.GetOrdinal("contractContent")).Returns(COLUMN_INDEX_CONTRACT_CONTENT);
            mockReader.Setup(reader => reader.GetOrdinal("renewalCount")).Returns(COLUMN_INDEX_RENEWAL_COUNT);
            mockReader.Setup(reader => reader.GetOrdinal("predefinedContractID")).Returns(COLUMN_INDEX_PREDEFINED_CONTRACT_ID);
            mockReader.Setup(reader => reader.GetOrdinal("pdfID")).Returns(COLUMN_INDEX_PDF_ID);
            mockReader.Setup(reader => reader.GetOrdinal("renewedFromContractID")).Returns(COLUMN_INDEX_RENEWED_FROM_CONTRACT_ID);
        }
    }
}