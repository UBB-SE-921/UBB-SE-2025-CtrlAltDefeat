using Moq;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.ViewModel;
using System.Reflection;
using Contract = ArtAttack.Domain.Contract;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class GenerateContractTest
    {
        private Mock<IContractModel> mockContractModel;
        private IContractViewModel viewModel;
        private IContract mockContract;
        private IPredefinedContract mockPredefinedContract;
        private Dictionary<string, object> mockOrderSummary;

        [TestInitialize]
        public void TestInitialize()
        {
            // Setup mock contract model
            mockContractModel = new Mock<IContractModel>();

            // Create the view model with the mocked model
            viewModel = new ContractViewModel("mock_connection_string");

            // Set private field via reflection
            var fieldInfo = typeof(ContractViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fieldInfo?.SetValue(viewModel, mockContractModel.Object);

            // Setup mock contract
            mockContract = new Contract
            {
                ContractID = 123,
                OrderID = 456,
                ContractStatus = "ACTIVE",
                AdditionalTerms = "Some terms",
                ContractContent = "Contract content",
                RenewalCount = 0
            };

            // Setup mock predefined contract
            mockPredefinedContract = new PredefinedContract
            {
                ID = 1,
                ContractContent = "This is a {ProductDescription} contract between {BuyerName} and {SellerName}."
            };

            // Setup mock order summary
            mockOrderSummary = new Dictionary<string, object>
            {
                { "warrantyTax", 25.50 }
            };

            // Setup mock model responses
            mockContractModel.Setup(m => m.GetContractByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(mockContract);

            mockContractModel.Setup(m => m.GetPredefinedContractByPredefineContractTypeAsync(It.IsAny<PredefinedContractType>()))
                .ReturnsAsync(mockPredefinedContract);

            mockContractModel.Setup(m => m.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync((DateTime.Now, DateTime.Now.AddDays(30), 99.99, "Test Product"));

            mockContractModel.Setup(m => m.GetContractBuyerAsync(It.IsAny<long>()))
                .ReturnsAsync((1, "Test Buyer"));

            mockContractModel.Setup(m => m.GetContractSellerAsync(It.IsAny<long>()))
                .ReturnsAsync((2, "Test Seller"));

            mockContractModel.Setup(m => m.GetOrderDetailsAsync(It.IsAny<long>()))
                .ReturnsAsync(("Credit Card", DateTime.Now));

            mockContractModel.Setup(m => m.GetOrderSummaryInformationAsync(It.IsAny<long>()))
                .ReturnsAsync(mockOrderSummary);
        }

        [TestMethod]
        public async Task GetContractByIdAsync_ShouldReturnContract()
        {
            // Arrange
            long contractId = 123;

            // Act
            var result = await viewModel.GetContractByIdAsync(contractId);      

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockContract.ContractID, result.ContractID);
            mockContractModel.Verify(m => m.GetContractByIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetPredefinedContractByType_ShouldReturnPredefinedContract()
        {
            // Arrange
            var contractType = PredefinedContractType.BuyingContract;

            // Act
            var result = await viewModel.GetPredefinedContractByPredefineContractTypeAsync(contractType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockPredefinedContract.ID, result.ID);
            mockContractModel.Verify(m => m.GetPredefinedContractByPredefineContractTypeAsync(contractType), Times.Once);
        }

       

        [TestMethod]
        public async Task GetFieldReplacements_ShouldPopulateCorrectFields()
        {
            // We need to test the private _GetFieldReplacements method
            // Using reflection to access the private method
            var methodInfo = typeof(ContractViewModel).GetMethod("GetFieldReplacements",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = await (Task<Dictionary<string, string>>)methodInfo.Invoke(viewModel, new object[] { mockContract });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey("ProductDescription"));
            Assert.IsTrue(result.ContainsKey("BuyerName"));
            Assert.IsTrue(result.ContainsKey("SellerName"));
            Assert.IsTrue(result.ContainsKey("Price"));
            Assert.AreEqual("Test Product", result["ProductDescription"]);
            Assert.AreEqual("Test Buyer", result["BuyerName"]);
            Assert.AreEqual("Test Seller", result["SellerName"]);
            Assert.AreEqual("99,99", result["Price"]);

            // Verify all the necessary methods were called
            mockContractModel.Verify(m => m.GetProductDetailsByContractIdAsync(mockContract.ContractID), Times.Once);
            mockContractModel.Verify(m => m.GetContractBuyerAsync(mockContract.ContractID), Times.Once);
            mockContractModel.Verify(m => m.GetContractSellerAsync(mockContract.ContractID), Times.Once);
            mockContractModel.Verify(m => m.GetOrderDetailsAsync(mockContract.ContractID), Times.Once);
            mockContractModel.Verify(m => m.GetOrderSummaryInformationAsync(mockContract.ContractID), Times.Once);
        }

        [TestMethod]
        public async Task GetFieldReplacements_WhenProductDetailsNull_ShouldUseDefaultValues()
        {
            // Arrange - Setup null product details
            mockContractModel.Setup(m => m.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
        .ReturnsAsync(default((DateTime, DateTime, double, string)?));

            // We need to test the private _GetFieldReplacements method
            var methodInfo = typeof(ContractViewModel).GetMethod("GetFieldReplacements",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = await (Task<Dictionary<string, string>>)methodInfo.Invoke(viewModel, new object[] { mockContract });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("N/A", result["ProductDescription"]);
            Assert.AreEqual("N/A", result["BuyerName"]);
            Assert.AreEqual("N/A", result["SellerName"]);
            Assert.AreEqual("N/A", result["Price"]);
        }

    }
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
    }
}
