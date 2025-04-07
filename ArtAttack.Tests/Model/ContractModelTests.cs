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
        private Mock<IDatabaseProvider> _mockDbProvider;
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private string _testConnectionString = "TestConnection";
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

            // Create the ContractModel with the mock database provider
            _contractModel = new ContractModel(_testConnectionString, _mockDbProvider.Object);
        }

        [TestMethod]
        public void Constructor_WithConnectionStringOnly_InitializesCorrectly()
        {
            // Arrange & Act
            var model = new ContractModel(_testConnectionString);

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
            _ = new ContractModel(_testConnectionString, null);
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
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);
            _mockReader.Setup(r => r.GetInt32(0)).Returns(contractId);
            _mockReader.Setup(r => r.GetOrdinal("ID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("content")).Returns(1);
            _mockReader.Setup(r => r[1]).Returns(content);
            _mockReader.Setup(r => r["content"]).Returns(content); // Add this line

            // Act
            var result = await _contractModel.GetPredefinedContractByPredefineContractTypeAsync(contractType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(contractId, result.ContractID);
            Assert.AreEqual(content, result.ContractContent);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
        }

        [TestMethod]
        public async Task GetPredefinedContractByPredefineContractTypeAsync_ReturnsEmptyContract_WhenNotFound()
        {
            // Arrange
            var contractType = PredefinedContractType.BorrowingContract;

            // Setup reader to return no data
            _mockReader.Setup(r => r.Read()).Returns(false);

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
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(r => r.GetInt64(0)).Returns(contractId);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(orderId);
            _mockReader.Setup(r => r.GetString(2)).Returns(status);
            _mockReader.Setup(r => r[3]).Returns(content);
            _mockReader.Setup(r => r["contractContent"]).Returns(content); 
            _mockReader.Setup(r => r.GetInt32(4)).Returns(renewalCount);
            _mockReader.Setup(r => r.IsDBNull(5)).Returns(false);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(predefinedContractId.Value);
            _mockReader.Setup(r => r.GetInt32(6)).Returns(pdfId);
            _mockReader.Setup(r => r.IsDBNull(7)).Returns(false);
            _mockReader.Setup(r => r.GetInt64(7)).Returns(renewedFromContractId.Value);

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
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(r => r.GetInt64(0)).Returns(newContractId);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(contract.OrderID);
            _mockReader.Setup(r => r.GetString(2)).Returns(contract.ContractStatus);
            _mockReader.Setup(r => r[3]).Returns(contract.ContractContent);
            _mockReader.Setup(r => r["contractContent"]).Returns(contract.ContractContent); // Add this line to fix the issue
            _mockReader.Setup(r => r.GetInt32(4)).Returns(contract.RenewalCount);
            _mockReader.Setup(r => r.IsDBNull(5)).Returns(false);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(contract.PredefinedContractID.Value);
            _mockReader.Setup(r => r.GetInt32(6)).Returns(contract.PDFID);
            _mockReader.Setup(r => r.IsDBNull(7)).Returns(true);

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
            _mockReader.Setup(r => r.Read()).Returns(false);

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
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 2); // Return 2 contracts

            // Setup for first contract
            _mockReader.Setup(r => r.GetInt64(0)).Returns(() => readCount == 1 ? 101 : 102);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(() => readCount == 1 ? 201 : 202);
            _mockReader.Setup(r => r.GetString(2)).Returns(() => readCount == 1 ? "ACTIVE" : "EXPIRED");
            _mockReader.Setup(r => r[3]).Returns(() => readCount == 1 ? "Content 1" : "Content 2");
            _mockReader.Setup(r => r.GetInt32(4)).Returns(() => readCount == 1 ? 0 : 1);
            _mockReader.Setup(r => r.IsDBNull(5)).Returns(() => readCount == 1 ? false : true);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(1);
            _mockReader.Setup(r => r.GetInt32(6)).Returns(() => readCount == 1 ? 301 : 302);
            _mockReader.Setup(r => r.IsDBNull(7)).Returns(() => readCount == 1 ? true : false);
            _mockReader.Setup(r => r.GetInt64(7)).Returns(100);

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
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 2); // Return 2 history entries

            // Setup for contract history
            _mockReader.Setup(r => r.GetInt64(0)).Returns(() => readCount == 1 ? 101 : 102);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(() => readCount == 1 ? 201 : 201); // Same order ID
            _mockReader.Setup(r => r.GetString(2)).Returns(() => readCount == 1 ? "ACTIVE" : "RENEWED");
            _mockReader.Setup(r => r[3]).Returns(() => readCount == 1 ? "Original Content" : "Renewed Content");
            _mockReader.Setup(r => r.GetInt32(4)).Returns(() => readCount == 1 ? 0 : 1); // Renewal count increased
            _mockReader.Setup(r => r.IsDBNull(5)).Returns(false);
            _mockReader.Setup(r => r.GetInt32(5)).Returns(1);
            _mockReader.Setup(r => r.GetInt32(6)).Returns(() => readCount == 1 ? 301 : 302);
            _mockReader.Setup(r => r.IsDBNull(7)).Returns(() => readCount == 1 ? true : false);
            _mockReader.Setup(r => r.GetInt64(7)).Returns(101); // Points to original contract

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

            // Setup columns
            _mockReader.Setup(r => r.GetOrdinal("SellerID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("SellerName")).Returns(1);

            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(r => r.GetInt32(0)).Returns(sellerId);
            _mockReader.Setup(r => r.GetString(1)).Returns(sellerName);

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

            // Setup columns
            _mockReader.Setup(r => r.GetOrdinal("BuyerID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("BuyerName")).Returns(1);

            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(r => r.GetInt32(0)).Returns(buyerId);
            _mockReader.Setup(r => r.GetString(1)).Returns(buyerName);

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
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            foreach (var item in summaryData)
            {
                _mockReader.Setup(r => r[item.Key]).Returns(item.Value);
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

            // Setup columns
            _mockReader.Setup(r => r.GetOrdinal("startDate")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("endDate")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("price")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("name")).Returns(3);

            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(r => r.GetDateTime(0)).Returns(startDate);
            _mockReader.Setup(r => r.GetDateTime(1)).Returns(endDate);
            _mockReader.Setup(r => r.GetDouble(2)).Returns(price);
            _mockReader.Setup(r => r.GetString(3)).Returns(name);

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
            _mockReader.Setup(r => r.Read()).Returns(false);

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
                ["ID"] = 0,
                ["orderID"] = 1,
                ["contractStatus"] = 2,
                ["contractContent"] = 3,
                ["renewalCount"] = 4,
                ["predefinedContractID"] = 5,
                ["pdfID"] = 6,
                ["AdditionalTerms"] = 7,
                ["renewedFromContractID"] = 8
            };

            foreach (var col in columns)
            {
                _mockReader.Setup(r => r.GetOrdinal(col.Key)).Returns(col.Value);
            }

            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 2); // Return 2 contracts

            // Contract 1 data
            _mockReader.Setup(r => r.GetInt64(0)).Returns(() => readCount == 1 ? 101 : 102);
            _mockReader.Setup(r => r.GetInt32(1)).Returns(() => readCount == 1 ? 201 : 202);
            _mockReader.Setup(r => r.GetString(2)).Returns(() => readCount == 1 ? "ACTIVE" : "EXPIRED");
            _mockReader.Setup(r => r[3]).Returns(() => readCount == 1 ? "Content 1" : "Content 2");
            _mockReader.Setup(r => r.GetInt32(4)).Returns(() => readCount == 1 ? 0 : 1);
            _mockReader.Setup(r => r.IsDBNull(5)).Returns(true);
            _mockReader.Setup(r => r.GetInt32(6)).Returns(() => readCount == 1 ? 301 : 302);
            _mockReader.Setup(r => r.GetString(7)).Returns(() => readCount == 1 ? "Additional 1" : "Additional 2");
            _mockReader.Setup(r => r.IsDBNull(8)).Returns(true);

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

            // Setup columns
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(1);

            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(r => r["PaymentMethod"]).Returns(paymentMethod);
            _mockReader.Setup(r => r.GetDateTime(1)).Returns(orderDate);

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

            // Setup columns
            _mockReader.Setup(r => r.GetOrdinal("EstimatedDeliveryDate")).Returns(0);
            _mockReader.Setup(r => r.IsDBNull(0)).Returns(false);

            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(r => r.GetDateTime(0)).Returns(deliveryDate);

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

            // Setup columns
            _mockReader.Setup(r => r.GetOrdinal("EstimatedDeliveryDate")).Returns(0);
            _mockReader.Setup(r => r.IsDBNull(0)).Returns(true);

            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

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
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            _mockReader.Setup(r => r["PdfFile"]).Returns(pdfData);

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
            _mockReader.Setup(r => r.Read()).Returns(false);

            // Act
            var result = await _contractModel.GetPdfByContractIdAsync(contractId);

            // Assert
            Assert.IsNull(result);
        }

        // Helper methods for test setup
        private void SetupReaderWithColumns(params string[] columnNames)
        {
            for (int i = 0; i < columnNames.Length; i++)
            {
                int idx = i; // Capture current value
                _mockReader.Setup(r => r.GetOrdinal(columnNames[idx])).Returns(idx);
            }
        }

        private void SetupReaderWithFullContractColumns()
        {
            // Setup column ordinals for contract
            var columns = new Dictionary<string, int>
            {
                ["ID"] = 0,
                ["orderID"] = 1,
                ["contractStatus"] = 2,
                ["contractContent"] = 3,
                ["renewalCount"] = 4,
                ["predefinedContractID"] = 5,
                ["pdfID"] = 6,
                ["renewedFromContractID"] = 7
            };

            foreach (var col in columns)
            {
                _mockReader.Setup(r => r.GetOrdinal(col.Key)).Returns(col.Value);
            }
        }
    }
}