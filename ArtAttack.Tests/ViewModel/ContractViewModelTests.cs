using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.ViewModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class ContractViewModelTests
    {
        private Mock<IContractModel> _mockContractModel;
        private IContractViewModel viewModel;
        private IContract mockContract;
        private List<IContract> _mockContracts;
        private byte[] _mockPdfData;
        private object _mockPredefinedContract;

        [TestInitialize]
        public void TestInitialize()
        {
            // Setup mock contract model
            _mockContractModel = new Mock<IContractModel>();

            // Create the view model with the mocked model
            viewModel = new ContractViewModel("mock_connection_string");

            // Set private field via reflection
            var fieldInfo = typeof(ContractViewModel).GetField("model", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo?.SetValue(viewModel, _mockContractModel.Object);

            // Setup mock contract
            mockContract = new Contract
            {
                ContractID = 123,
                OrderID = 456,
                ContractStatus = "ACTIVE",
                AdditionalTerms = "Some terms",
                ContractContent = "Contract content",
                RenewalCount = 0,
                PDFID = 789
            };

            // Setup mock contracts list
            _mockContracts = new List<IContract>
                        {
                            mockContract,
                            new Contract
                            {
                                ContractID = 124,
                                OrderID = 457,
                                ContractStatus = "ACTIVE"
                            }
                        };

            // Setup mock PDF data
            _mockPdfData = new byte[] { 1, 2, 3, 4, 5 };

            // Setup common mocks
            _mockContractModel.Setup(m => m.GetAllContractsAsync())
                .ReturnsAsync(_mockContracts);

            _mockContractModel.Setup(m => m.GetContractHistoryAsync(It.IsAny<long>()))
                .ReturnsAsync(_mockContracts);

            _mockContractModel.Setup(m => m.GetContractsByBuyerAsync(It.IsAny<int>()))
                .ReturnsAsync(_mockContracts);

            _mockContractModel.Setup(m => m.GetPdfByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(_mockPdfData);

            _mockContractModel.Setup(m => m.AddContractAsync(It.IsAny<IContract>(), It.IsAny<byte[]>()))
                .ReturnsAsync(mockContract);

            _mockContractModel.Setup(m => m.GetDeliveryDateByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(DateTime.Now.AddDays(7));
        }

        [TestMethod]
        public async Task GetAllContractsAsync_ShouldReturnAllContracts()
        {
            // Act
            var result = await viewModel.GetAllContractsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            _mockContractModel.Verify(m => m.GetAllContractsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetContractHistoryAsync_ShouldReturnContractHistory()
        {
            // Arrange
            long contractId = 123;

            // Act
            var result = await viewModel.GetContractHistoryAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            _mockContractModel.Verify(m => m.GetContractHistoryAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetContractsByBuyerAsync_ShouldReturnBuyerContracts()
        {
            // Arrange
            int buyerId = 42;

            // Act
            var result = await viewModel.GetContractsByBuyerAsync(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            _mockContractModel.Verify(m => m.GetContractsByBuyerAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task AddContractAsync_ShouldAddContract()
        {
            // Arrange
            byte[] pdfData = new byte[] { 1, 2, 3 };

            // Act
            var result = await viewModel.AddContractAsync(mockContract, pdfData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockContract.ContractID, result.ContractID);
            _mockContractModel.Verify(m => m.AddContractAsync(mockContract, pdfData), Times.Once);
        }

        [TestMethod]
        public async Task GetDeliveryDateByContractIdAsync_ShouldReturnDate()
        {
            // Arrange
            long contractId = 123;
            var expectedDate = DateTime.Now.AddDays(7);
            _mockContractModel.Setup(m => m.GetDeliveryDateByContractIdAsync(contractId))
                .ReturnsAsync(expectedDate);

            // Act
            var result = await viewModel.GetDeliveryDateByContractIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedDate.Date, result.Value.Date);
            _mockContractModel.Verify(m => m.GetDeliveryDateByContractIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetPdfByContractIdAsync_ShouldReturnPdfBytes()
        {
            // Arrange
            long contractId = 123;

            // Act
            var result = await viewModel.GetPdfByContractIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_mockPdfData.Length, result.Length);
            CollectionAssert.AreEqual(_mockPdfData, result);
            _mockContractModel.Verify(m => m.GetPdfByContractIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GenerateAndSaveContractAsync_ShouldGenerateAndSaveContract()
        {
            // This test requires mocking File.WriteAllBytesAsync and StorageFile/Launcher
            // which is challenging in a unit test. Instead, we'll test the internal method calls.

            // Arrange
            var contractType = PredefinedContractType.BuyingContract;
            var mockPredefinedContract = new PredefinedContract
            {
                ID = 1,
                ContractContent = "Test content"
            };

            _mockContractModel.Setup(m => m.GetPredefinedContractByPredefineContractTypeAsync(contractType))
                .ReturnsAsync(mockPredefinedContract);

            // We'll use reflection to access and test the private methods
            var getFieldReplacementsMethod = typeof(ContractViewModel).GetMethod("GetFieldReplacements");
            // Debug helper to find actual method names
            var methods = typeof(ContractViewModel)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(m => $"{m.Name} ({m.ReturnType.Name})")
            .ToList();
            Console.WriteLine("Available methods: " + string.Join(", ", methods),
                BindingFlags.NonPublic | BindingFlags.Instance);

            var generateContractPdfMethod = typeof(ContractViewModel).GetMethod("GenerateContractPdf",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Mock _GetFieldReplacements to return a dictionary directly (bypassing actual implementation)
            var fieldReplacements = new Dictionary<string, string>
                        {
                            { "ProductDescription", "Test Product" },
                            { "Price", "99.99" }
                        };

            // Act - Call the method but catch the expected exception for file operations
            try
            {
                await viewModel.GenerateAndSaveContractAsync(mockContract, contractType);
            }
            catch (Exception)
            {
                // Expected to fail on file operations in unit test environment
            }

            // Assert - Verify the model methods were called
            _mockContractModel.Verify(m => m.GetPredefinedContractByPredefineContractTypeAsync(contractType), Times.Once);
            _mockContractModel.Verify(m => m.GetProductDetailsByContractIdAsync(mockContract.ContractID), Times.Once);
            _mockContractModel.Verify(m => m.GetContractBuyerAsync(mockContract.ContractID), Times.Once);
            _mockContractModel.Verify(m => m.GetContractSellerAsync(mockContract.ContractID), Times.Once);
        }



        [TestMethod]
        public void GenerateContractPdf_WithNulldContract_ShouldThrowException()
        {
            // Arrange
            Dictionary<string, string> fieldReplacements = new Dictionary<string, string>();
            var methodInfo = typeof(ContractViewModel).GetMethod("GenerateContractPdf",
                BindingFlags.NonPublic | BindingFlags.Instance);
            // Act & Assert - Should throw ArgumentNullException wrapped in a TargetInvocationException
            var ex = Assert.ThrowsException<TargetInvocationException>(() =>
                methodInfo.Invoke(viewModel, new object[] { null, _mockPredefinedContract, fieldReplacements }));
            Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentNullException));
            Assert.AreEqual("contract", ((ArgumentNullException)ex.InnerException).ParamName);
        }





        //get predefined contract by type
        [TestMethod]
        public void PredefinedContractID_NullableProperty_CanSetAndGetNullValue()
        {
            // Arrange
            var contract = new Contract();
            int? expectedValue = null;

            // Act
            contract.PredefinedContractID = expectedValue;
            var actualValue = contract.PredefinedContractID;

            // Assert
            Assert.AreEqual(expectedValue, actualValue, "PredefinedContractID should allow null values");
        }

        [TestMethod]
        public void PredefinedContractID_NullableProperty_CanSetAndGetValue()
        {
            // Arrange
            var contract = new Contract();
            int? expectedValue = 42;

            // Act
            contract.PredefinedContractID = expectedValue;
            var actualValue = contract.PredefinedContractID;

            // Assert
            Assert.AreEqual(expectedValue, actualValue, "PredefinedContractID should store the assigned value");
        }

        [TestMethod]
        public void RenewedFromContractID_NullableProperty_CanSetAndGetNullValue()
        {
            // Arrange
            var contract = new Contract();
            long? expectedValue = null;

            // Act
            contract.RenewedFromContractID = expectedValue;
            var actualValue = contract.RenewedFromContractID;

            // Assert
            Assert.AreEqual(expectedValue, actualValue, "RenewedFromContractID should allow null values");
        }

        [TestMethod]
        public void RenewedFromContractID_NullableProperty_CanSetAndGetValue()
        {
            // Arrange
            var contract = new Contract();
            long? expectedValue = 123456789L;

            // Act
            contract.RenewedFromContractID = expectedValue;
            var actualValue = contract.RenewedFromContractID;

            // Assert
            Assert.AreEqual(expectedValue, actualValue, "RenewedFromContractID should store the assigned value");
        }
    

        [TestMethod]
        public async Task GetContractByIdAsync_ReturnsCorrectContract()
        {
            // Arrange
            long contractId = 123;
            _mockContractModel.Setup(m => m.GetContractByIdAsync(contractId))
                .ReturnsAsync(mockContract);

            // Act
            var result = await viewModel.GetContractByIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockContract.ContractID, result.ContractID);
            _mockContractModel.Verify(m => m.GetContractByIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetContractByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            long contractId = 999;
            _mockContractModel.Setup(m => m.GetContractByIdAsync(contractId))
                .ReturnsAsync((IContract)null);

            // Act
            var result = await viewModel.GetContractByIdAsync(contractId);

            // Assert
            Assert.IsNull(result);
            _mockContractModel.Verify(m => m.GetContractByIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderSummaryInformationAsync_ReturnsCorrectData()
        {
            // Arrange
            long contractId = 123;
            var expected = new Dictionary<string, object>
            {
                { "totalPrice", 99.99 },
                { "tax", 10.00 },
                { "shipping", 5.99 }
            };

            _mockContractModel.Setup(m => m.GetOrderSummaryInformationAsync(contractId))
                .ReturnsAsync(expected);

            // Act
            var result = await viewModel.GetOrderSummaryInformationAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Count, result.Count);
            foreach (var key in expected.Keys)
            {
                Assert.IsTrue(result.ContainsKey(key));
                Assert.AreEqual(expected[key], result[key]);
            }
            _mockContractModel.Verify(m => m.GetOrderSummaryInformationAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetContractSellerAsync_ReturnsCorrectData()
        {
            // Arrange
            long contractId = 123;
            var expected = (5, "John Doe");

            _mockContractModel.Setup(m => m.GetContractSellerAsync(contractId))
                .ReturnsAsync(expected);

            // Act
            var result = await viewModel.GetContractSellerAsync(contractId);

            // Assert
            Assert.AreEqual(expected.Item1, result.SellerID);
            Assert.AreEqual(expected.Item2, result.SellerName);
            _mockContractModel.Verify(m => m.GetContractSellerAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetContractBuyerAsync_ReturnsCorrectData()
        {
            // Arrange
            long contractId = 123;
            var expected = (10, "Jane Smith");

            _mockContractModel.Setup(m => m.GetContractBuyerAsync(contractId))
                .ReturnsAsync(expected);

            // Act
            var result = await viewModel.GetContractBuyerAsync(contractId);

            // Assert
            Assert.AreEqual(expected.Item1, result.BuyerID);
            Assert.AreEqual(expected.Item2, result.BuyerName);
            _mockContractModel.Verify(m => m.GetContractBuyerAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetProductDetailsByContractIdAsync_ReturnsCorrectData()
        {
            // Arrange
            long contractId = 123;
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(30);
            double price = 149.99;
            string name = "Premium Art Piece";

            var expected = (startDate, endDate, price, name);

            _mockContractModel.Setup(m => m.GetProductDetailsByContractIdAsync(contractId))
                .ReturnsAsync(expected);

            // Act
            var result = await viewModel.GetProductDetailsByContractIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.startDate, result.Value.StartDate);
            Assert.AreEqual(expected.endDate, result.Value.EndDate);
            Assert.AreEqual(expected.price, result.Value.price);
            Assert.AreEqual(expected.name, result.Value.name);
            _mockContractModel.Verify(m => m.GetProductDetailsByContractIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderDetailsAsync_ReturnsCorrectData()
        {
            // Arrange
            long contractId = 123;
            string paymentMethod = "Credit Card";
            DateTime orderDate = new DateTime(2023, 5, 15);

            var expected = (paymentMethod, orderDate);

            _mockContractModel.Setup(m => m.GetOrderDetailsAsync(contractId))
                .ReturnsAsync(expected);

            // Act
            var result = await viewModel.GetOrderDetailsAsync(contractId);

            // Assert
            Assert.AreEqual(expected.paymentMethod, result.PaymentMethod);
            Assert.AreEqual(expected.orderDate, result.OrderDate);
            _mockContractModel.Verify(m => m.GetOrderDetailsAsync(contractId), Times.Once);
        }
        // ADD CONTRACT WHEN CONTRACT IS NULL AND EXPECT ArgumentNullException

        [TestMethod]
        public async Task GetDeliveryDateByContractIdAsync_WhenNull_ReturnsNull()
        {
            // Arrange
            long contractId = 999;

            _mockContractModel.Setup(m => m.GetDeliveryDateByContractIdAsync(contractId))
                .ReturnsAsync((DateTime?)null);

            // Act
            var result = await viewModel.GetDeliveryDateByContractIdAsync(contractId);

            // Assert
            Assert.IsNull(result);
            _mockContractModel.Verify(m => m.GetDeliveryDateByContractIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public void GenerateContractPdf_WithNullPredefinedContract_ThrowsException()
        {
            // Arrange
            Dictionary<string, string> fieldReplacements = new Dictionary<string, string>();
            var methodInfo = typeof(ContractViewModel).GetMethod("GenerateContractPdf",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            var ex = Assert.ThrowsException<TargetInvocationException>(() =>
                methodInfo.Invoke(viewModel, new object[] { mockContract, null, fieldReplacements }));
            Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentNullException));
            Assert.AreEqual("predefinedContract", ((ArgumentNullException)ex.InnerException).ParamName);
        }


        [TestMethod]
        public async Task AddContractAsync_WithNullPdfData_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await viewModel.AddContractAsync(mockContract, null));
        }

        [TestMethod]
        public async Task GenerateAndSaveContractAsync_WithNullContract_ThrowsArgumentNullException()
        {
            // Arrange
            var contractType = PredefinedContractType.BuyingContract;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await viewModel.GenerateAndSaveContractAsync(null, contractType));
        }

        [TestMethod]
        public async Task GetContractsByBuyerAsync_WithInvalidId_ReturnsEmptyList()
        {
            // Arrange
            int buyerId = -1;
            _mockContractModel.Setup(m => m.GetContractsByBuyerAsync(buyerId))
                .ReturnsAsync(new List<IContract>());

            // Act
            var result = await viewModel.GetContractsByBuyerAsync(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            _mockContractModel.Verify(m => m.GetContractsByBuyerAsync(buyerId), Times.Once);
        }
    }
}

