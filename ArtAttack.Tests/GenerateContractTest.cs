using Moq;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.ViewModel;
using System.Reflection;
using Contract = ArtAttack.Domain.Contract;

namespace ArtAttack.Tests
{
    [TestClass]
    public class GenerateContractTest
    {
        private Mock<IContractModel> _mockContractModel;
        private ContractViewModel _viewModel;
        private IContract _mockContract;
        private IPredefinedContract _mockPredefinedContract;
        private Dictionary<string, object> _mockOrderSummary;

        [TestInitialize]
        public void TestInitialize()
        {
            // Setup mock contract model
            _mockContractModel = new Mock<IContractModel>();

            // Create the view model with the mocked model
            _viewModel = new ContractViewModel("mock_connection_string");

            // Set private field via reflection
            var fieldInfo = typeof(ContractViewModel).GetField("_model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fieldInfo?.SetValue(_viewModel, _mockContractModel.Object);

            // Setup mock contract
            _mockContract = new Contract
            {
                ContractID = 123,
                OrderID = 456,
                ContractStatus = "ACTIVE",
                AdditionalTerms = "Some terms",
                ContractContent = "Contract content",
                RenewalCount = 0
            };

            // Setup mock predefined contract
            _mockPredefinedContract = new PredefinedContract
            {
                ID = 1,
                Content = "This is a {ProductDescription} contract between {BuyerName} and {SellerName}."
            };

            // Setup mock order summary
            _mockOrderSummary = new Dictionary<string, object>
            {
                { "warrantyTax", 25.50 }
            };

            // Setup mock model responses
            _mockContractModel.Setup(m => m.GetContractByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(_mockContract);

            _mockContractModel.Setup(m => m.GetPredefinedContractByPredefineContractTypeAsync(It.IsAny<PredefinedContractType>()))
                .ReturnsAsync(_mockPredefinedContract);

            _mockContractModel.Setup(m => m.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync((DateTime.Now, DateTime.Now.AddDays(30), 99.99, "Test Product"));

            _mockContractModel.Setup(m => m.GetContractBuyerAsync(It.IsAny<long>()))
                .ReturnsAsync((1, "Test Buyer"));

            _mockContractModel.Setup(m => m.GetContractSellerAsync(It.IsAny<long>()))
                .ReturnsAsync((2, "Test Seller"));

            _mockContractModel.Setup(m => m.GetOrderDetailsAsync(It.IsAny<long>()))
                .ReturnsAsync(("Credit Card", DateTime.Now));

            _mockContractModel.Setup(m => m.GetOrderSummaryInformationAsync(It.IsAny<long>()))
                .ReturnsAsync(_mockOrderSummary);
        }

        [TestMethod]
        public async Task GetContractByIdAsync_ShouldReturnContract()
        {
            // Arrange
            long contractId = 123;

            // Act
            var result = await _viewModel.GetContractByIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_mockContract.ContractID, result.ContractID);
            _mockContractModel.Verify(m => m.GetContractByIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetPredefinedContractByType_ShouldReturnPredefinedContract()
        {
            // Arrange
            var contractType = PredefinedContractType.Buying;

            // Act
            var result = await _viewModel.GetPredefinedContractByPredefineContractTypeAsync(contractType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_mockPredefinedContract.ID, result.ID);
            _mockContractModel.Verify(m => m.GetPredefinedContractByPredefineContractTypeAsync(contractType), Times.Once);
        }

        [TestMethod]
        public async Task GeneratePDFAndAddContract_ShouldAddContractWithPDF()
        {
            // Arrange
            byte[] pdfBytes = new byte[10];
            _mockContractModel.Setup(m => m.GetPdfByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync((byte[])null);

            _mockContractModel.Setup(m => m.AddContractAsync(It.IsAny<IContract>(), It.IsAny<byte[]>()))
                .ReturnsAsync(_mockContract);

            // Act
            await _viewModel.GeneratePDFAndAddContract(_mockContract, PredefinedContractType.Buying);

            // Assert
            _mockContractModel.Verify(m => m.GetPdfByContractIdAsync(_mockContract.ContractID), Times.Once);
            _mockContractModel.Verify(m => m.GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType.Buying), Times.Once);
            _mockContractModel.Verify(m => m.AddContractAsync(It.IsAny<IContract>(), It.IsAny<byte[]>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GeneratePDFAndAddContract_WhenPDFExists_ShouldThrowException()
        {
            // Arrange
            byte[] existingPdfBytes = new byte[10];
            _mockContractModel.Setup(m => m.GetPdfByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(existingPdfBytes);

            // Act & Assert - Should throw exception
            await _viewModel.GeneratePDFAndAddContract(_mockContract, PredefinedContractType.Buying);
        }

        [TestMethod]
        public async Task GetFieldReplacements_ShouldPopulateCorrectFields()
        {
            // We need to test the private _GetFieldReplacements method
            // Using reflection to access the private method
            var methodInfo = typeof(ContractViewModel).GetMethod("_GetFieldReplacements",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = await (Task<Dictionary<string, string>>)methodInfo.Invoke(_viewModel, new object[] { _mockContract });

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
            _mockContractModel.Verify(m => m.GetProductDetailsByContractIdAsync(_mockContract.ContractID), Times.Once);
            _mockContractModel.Verify(m => m.GetContractBuyerAsync(_mockContract.ContractID), Times.Once);
            _mockContractModel.Verify(m => m.GetContractSellerAsync(_mockContract.ContractID), Times.Once);
            _mockContractModel.Verify(m => m.GetOrderDetailsAsync(_mockContract.ContractID), Times.Once);
            _mockContractModel.Verify(m => m.GetOrderSummaryInformationAsync(_mockContract.ContractID), Times.Once);
        }

        [TestMethod]
        public async Task GetFieldReplacements_WhenProductDetailsNull_ShouldUseDefaultValues()
        {
            // Arrange - Setup null product details
            _mockContractModel.Setup(m => m.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
        .ReturnsAsync(default((DateTime, DateTime, double, string)?));

            // We need to test the private _GetFieldReplacements method
            var methodInfo = typeof(ContractViewModel).GetMethod("_GetFieldReplacements",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = await (Task<Dictionary<string, string>>)methodInfo.Invoke(_viewModel, new object[] { _mockContract });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("N/A", result["ProductDescription"]);
            Assert.AreEqual("N/A", result["BuyerName"]);
            Assert.AreEqual("N/A", result["SellerName"]);
            Assert.AreEqual("N/A", result["Price"]);
        }

        [TestMethod]
        public void GenerateContractPdf_ShouldCreatePdfBytes()
        {
            // Setup
            var fieldReplacements = new Dictionary<string, string>
            {
                { "ProductDescription", "Test Product" },
                { "BuyerName", "Test Buyer" },
                { "SellerName", "Test Seller" }
            };

            // We need to test the private _GenerateContractPdf method
            var methodInfo = typeof(ContractViewModel).GetMethod("_GenerateContractPdf",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = (byte[])methodInfo.Invoke(_viewModel, new object[] { _mockContract, _mockPredefinedContract, fieldReplacements });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void GenerateContractPdf_WithNullContract_ShouldThrowException()
        {
            // Setup
            var fieldReplacements = new Dictionary<string, string>();

            // We need to test the private _GenerateContractPdf method
            var methodInfo = typeof(ContractViewModel).GetMethod("_GenerateContractPdf",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act & Assert - Should throw ArgumentNullException wrapped in a TargetInvocationException
            methodInfo.Invoke(_viewModel, new object[] { null, _mockPredefinedContract, fieldReplacements });
        }
    }
    [TestClass]
    public class ContractViewModelTests
    {
        private Mock<IContractModel> _mockContractModel;
        private ContractViewModel _viewModel;
        private IContract _mockContract;
        private List<IContract> _mockContracts;
        private byte[] _mockPdfData;
        private object _mockPredefinedContract;

        [TestInitialize]
        public void TestInitialize()
        {
            // Setup mock contract model
            _mockContractModel = new Mock<IContractModel>();

            // Create the view model with the mocked model
            _viewModel = new ContractViewModel("mock_connection_string");

            // Set private field via reflection
            var fieldInfo = typeof(ContractViewModel).GetField("_model", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo?.SetValue(_viewModel, _mockContractModel.Object);

            // Setup mock contract
            _mockContract = new Contract
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
                            _mockContract,
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
                .ReturnsAsync(_mockContract);

            _mockContractModel.Setup(m => m.GetDeliveryDateByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(DateTime.Now.AddDays(7));
        }

        [TestMethod]
        public async Task GetAllContractsAsync_ShouldReturnAllContracts()
        {
            // Act
            var result = await _viewModel.GetAllContractsAsync();

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
            var result = await _viewModel.GetContractHistoryAsync(contractId);

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
            var result = await _viewModel.GetContractsByBuyerAsync(buyerId);

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
            var result = await _viewModel.AddContractAsync(_mockContract, pdfData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_mockContract.ContractID, result.ContractID);
            _mockContractModel.Verify(m => m.AddContractAsync(_mockContract, pdfData), Times.Once);
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
            var result = await _viewModel.GetDeliveryDateByContractIdAsync(contractId);

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
            var result = await _viewModel.GetPdfByContractIdAsync(contractId);

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
            var contractType = PredefinedContractType.Buying;
            var mockPredefinedContract = new PredefinedContract
            {
                ID = 1,
                Content = "Test content"
            };

            _mockContractModel.Setup(m => m.GetPredefinedContractByPredefineContractTypeAsync(contractType))
                .ReturnsAsync(mockPredefinedContract);

            // We'll use reflection to access and test the private methods
            var getFieldReplacementsMethod = typeof(ContractViewModel).GetMethod("_GetFieldReplacements",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var generateContractPdfMethod = typeof(ContractViewModel).GetMethod("_GenerateContractPdf",
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
                await _viewModel.GenerateAndSaveContractAsync(_mockContract, contractType);
            }
            catch (Exception)
            {
                // Expected to fail on file operations in unit test environment
            }

            // Assert - Verify the model methods were called
            _mockContractModel.Verify(m => m.GetPredefinedContractByPredefineContractTypeAsync(contractType), Times.Once);
            _mockContractModel.Verify(m => m.GetProductDetailsByContractIdAsync(_mockContract.ContractID), Times.Once);
            _mockContractModel.Verify(m => m.GetContractBuyerAsync(_mockContract.ContractID), Times.Once);
            _mockContractModel.Verify(m => m.GetContractSellerAsync(_mockContract.ContractID), Times.Once);
        }

        [TestMethod]
        public async Task GeneratePDFAndAddContract_WithNullPdf_ShouldGenerateAndAddContract()
        {
            // Arrange
            var contractType = PredefinedContractType.Buying;
            var mockPredefinedContract = new PredefinedContract
            {
                ID = 1,
                Content = "Test content with {ProductDescription}"
            };

            // Setup mock responses for method calls
            _mockContractModel.Setup(m => m.GetPdfByContractIdAsync(_mockContract.ContractID))
                .ReturnsAsync((byte[])null);

            _mockContractModel.Setup(m => m.GetPredefinedContractByPredefineContractTypeAsync(contractType))
                .ReturnsAsync(mockPredefinedContract);

            _mockContractModel.Setup(m => m.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync((DateTime.Now, DateTime.Now.AddDays(30), 99.99, "Test Product"));

            _mockContractModel.Setup(m => m.GetContractBuyerAsync(It.IsAny<long>()))
                .ReturnsAsync((1, "Test Buyer"));

            _mockContractModel.Setup(m => m.GetContractSellerAsync(It.IsAny<long>()))
                .ReturnsAsync((2, "Test Seller"));

            _mockContractModel.Setup(m => m.GetOrderDetailsAsync(It.IsAny<long>()))
                .ReturnsAsync(("Credit Card", DateTime.Now));

            _mockContractModel.Setup(m => m.GetOrderSummaryInformationAsync(It.IsAny<long>()))
                .ReturnsAsync(new Dictionary<string, object> { { "warrantyTax", 25.50 } });

            // Act
            await _viewModel.GeneratePDFAndAddContract(_mockContract, contractType);

            // Assert
            _mockContractModel.Verify(m => m.GetPdfByContractIdAsync(_mockContract.ContractID), Times.Once);
            _mockContractModel.Verify(m => m.GetPredefinedContractByPredefineContractTypeAsync(contractType), Times.Once);
            _mockContractModel.Verify(m => m.AddContractAsync(_mockContract, It.IsAny<byte[]>()), Times.Once);
        }

        [TestMethod]
        public void GenerateContractPdf_WithNullPredefinedContract_ShouldThrowException()
        {
            // Arrange
            Dictionary<string, string> fieldReplacements = new Dictionary<string, string>();
            var methodInfo = typeof(ContractViewModel).GetMethod("_GenerateContractPdf",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert - Should throw ArgumentNullException wrapped in a TargetInvocationException
            var ex = Assert.ThrowsException<TargetInvocationException>(() =>
                methodInfo.Invoke(_viewModel, new object[] { _mockContract, null, fieldReplacements }));

            Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentNullException));
            Assert.AreEqual("predefinedContract", ((ArgumentNullException)ex.InnerException).ParamName);
        }

        [TestMethod]
        public void GenerateContractPdf_WithNulldContract_ShouldThrowException()
        {
            // Arrange
            Dictionary<string, string> fieldReplacements = new Dictionary<string, string>();
            var methodInfo = typeof(ContractViewModel).GetMethod("_GenerateContractPdf",
                BindingFlags.NonPublic | BindingFlags.Instance);
            // Act & Assert - Should throw ArgumentNullException wrapped in a TargetInvocationException
            var ex = Assert.ThrowsException<TargetInvocationException>(() =>
                methodInfo.Invoke(_viewModel, new object[] { null, _mockPredefinedContract, fieldReplacements }));
            Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentNullException));
            Assert.AreEqual("contract", ((ArgumentNullException)ex.InnerException).ParamName);
        }

        [TestMethod]
        public void GenerateContractPdf_WithNullFieldReplacements_ShouldUseEmptyDictionary()
        {
            // Arrange
            var mockPredefinedContract = new PredefinedContract
            {
                ID = 1,
                Content = "Test content"
            };

            var methodInfo = typeof(ContractViewModel).GetMethod("_GenerateContractPdf",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act - Pass null as fieldReplacements
            var result = (byte[])methodInfo.Invoke(_viewModel, new object[] { _mockContract, mockPredefinedContract, null });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }
        //public async Task<IContract> GetContractByIdAsync(long contractId)
        //    {
        //        return await _model.GetContractByIdAsync(contractId);
        //}

        [TestMethod]
        public async Task GetContractByIdAsync_ShouldReturnContract()
        {
            // Arrange
            long contractId = 123;
            // Act
            var result = await _viewModel.GetContractByIdAsync(contractId);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_mockContract.ContractID, result.ContractID);
            _mockContractModel.Verify(m => m.GetContractByIdAsync(contractId), Times.Once);
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
