using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.ViewModel;
using Moq;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class ContractViewModelTests
    {
        private Mock<IContractRepository> mockContractModel;
        private IContractViewModel contractViewModel;
        private IContract mockContract;
        private List<IContract> mockContracts;
        private byte[] mockPdfData;
        private object mockPredefinedContract;

        [TestInitialize]
        public void TestInitialize()
        {
            // Setup mock contract model
            mockContractModel = new Mock<IContractRepository>();

            // Create the view model with the mocked model
            contractViewModel = new ContractViewModel("Test_Connection_String");

            // Set private field via reflection
            var fieldInfo = typeof(ContractViewModel).GetField("model", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo?.SetValue(contractViewModel, mockContractModel.Object);

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
            mockContracts = new List<IContract>
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
            mockPdfData = new byte[] { 1, 2, 3, 4, 5 };

            // Setup common mocks
            mockContractModel.Setup(contractModel => contractModel.GetAllContractsAsync())
                .ReturnsAsync(mockContracts);

            mockContractModel.Setup(contractModel => contractModel.GetContractHistoryAsync(It.IsAny<long>()))
                .ReturnsAsync(mockContracts);

            mockContractModel.Setup(contractModel => contractModel.GetContractsByBuyerAsync(It.IsAny<int>()))
                .ReturnsAsync(mockContracts);

            mockContractModel.Setup(contractModel => contractModel.GetPdfByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(mockPdfData);

            mockContractModel.Setup(contractModel => contractModel.AddContractAsync(It.IsAny<IContract>(), It.IsAny<byte[]>()))
                .ReturnsAsync(mockContract);

            mockContractModel.Setup(contractModel => contractModel.GetDeliveryDateByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(DateTime.Now.AddDays(7));
        }

        [TestMethod]
        public async Task GetAllContractsAsync_ShouldReturnAllContracts()
        {
            // Act
            var result = await contractViewModel.GetAllContractsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            mockContractModel.Verify(contractModel => contractModel.GetAllContractsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetContractHistoryAsync_ShouldReturnContractHistory()
        {
            // Arrange
            long contractId = 123;

            // Act
            var result = await contractViewModel.GetContractHistoryAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            mockContractModel.Verify(contractModel => contractModel.GetContractHistoryAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetContractsByBuyerAsync_ShouldReturnBuyerContracts()
        {
            // Arrange
            int buyerId = 42;

            // Act
            var result = await contractViewModel.GetContractsByBuyerAsync(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            mockContractModel.Verify(contractModel => contractModel.GetContractsByBuyerAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task AddContractAsync_ShouldAddContract()
        {
            // Arrange
            byte[] pdfData = new byte[] { 1, 2, 3 };

            // Act
            var result = await contractViewModel.AddContractAsync(mockContract, pdfData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockContract.ContractID, result.ContractID);
            mockContractModel.Verify(contractModel => contractModel.AddContractAsync(mockContract, pdfData), Times.Once);
        }

        [TestMethod]
        public async Task GetDeliveryDateByContractIdAsync_ShouldReturnDate()
        {
            // Arrange
            long contractId = 123;
            var expectedDate = DateTime.Now.AddDays(7);
            mockContractModel.Setup(contractModel => contractModel.GetDeliveryDateByContractIdAsync(contractId))
                .ReturnsAsync(expectedDate);

            // Act
            var result = await contractViewModel.GetDeliveryDateByContractIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedDate.Date, result.Value.Date);
            mockContractModel.Verify(contractModel => contractModel.GetDeliveryDateByContractIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetPdfByContractIdAsync_ShouldReturnPdfBytes()
        {
            // Arrange
            long contractId = 123;

            // Act
            var result = await contractViewModel.GetPdfByContractIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockPdfData.Length, result.Length);
            CollectionAssert.AreEqual(mockPdfData, result);
            mockContractModel.Verify(contractModel => contractModel.GetPdfByContractIdAsync(contractId), Times.Once);
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

            mockContractModel.Setup(contractModel => contractModel.GetPredefinedContractByPredefineContractTypeAsync(contractType))
                .ReturnsAsync(mockPredefinedContract);

            // We'll use reflection to access and test the private methods
            var getFieldReplacementsMethod = typeof(ContractViewModel).GetMethod("GetFieldReplacements");
            // Debug helper to find actual method names
            var methods = typeof(ContractViewModel)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(methodInfo => $"{methodInfo.Name} ({methodInfo.ReturnType.Name})")
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
                await contractViewModel.GenerateAndSaveContractAsync(mockContract, contractType);
            }
            catch (Exception)
            {
                // Expected to fail on file operations in unit test environment
            }

            // Assert - Verify the model methods were called
            mockContractModel.Verify(contractModel => contractModel.GetPredefinedContractByPredefineContractTypeAsync(contractType), Times.Once);
            mockContractModel.Verify(contractModel => contractModel.GetProductDetailsByContractIdAsync(mockContract.ContractID), Times.Once);
            mockContractModel.Verify(contractModel => contractModel.GetContractBuyerAsync(mockContract.ContractID), Times.Once);
            mockContractModel.Verify(contractModel => contractModel.GetContractSellerAsync(mockContract.ContractID), Times.Once);
        }

        [TestMethod]
        public void GenerateContractPdf_WhenNullContract_ShouldThrowException()
        {
            // Arrange
            Dictionary<string, string> fieldReplacements = new Dictionary<string, string>();
            var methodInfo = typeof(ContractViewModel).GetMethod("GenerateContractPdf",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert - Should throw ArgumentNullException wrapped in a TargetInvocationException
            var exception = Assert.ThrowsException<TargetInvocationException>(() =>
                methodInfo.Invoke(contractViewModel, new object[] { null, mockPredefinedContract, fieldReplacements }));

            Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            Assert.AreEqual("contract", ((ArgumentNullException)exception.InnerException).ParamName);
        }

        [TestMethod]
        public void PredefinedContractIDNullableProperty_ShouldSetAndGetNullValue()
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
        public void PredefinedContractIDNullableProperty_ShouldSetAndGetValue()
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
        public void RenewedFromContractIDNullableProperty_ShouldSetAndGetNullValue()
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
        public void RenewedFromContractIDNullableProperty_ShouldSetAndGetValue()
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
        public async Task GetContractByIdAsync_ShouldReturnCorrectContract()
        {
            // Arrange
            long contractId = 123;
            mockContractModel.Setup(contractModel => contractModel.GetContractByIdAsync(contractId))
                .ReturnsAsync(mockContract);

            // Act
            var result = await contractViewModel.GetContractByIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockContract.ContractID, result.ContractID);
            mockContractModel.Verify(contractModel => contractModel.GetContractByIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetContractByIdAsync_WhenInvalidId_ShouldReturnNull()
        {
            // Arrange
            long contractId = 999;
            mockContractModel.Setup(contractModel => contractModel.GetContractByIdAsync(contractId)).ReturnsAsync((IContract)null);

            // Act
            var result = await contractViewModel.GetContractByIdAsync(contractId);

            // Assert
            Assert.IsNull(result);
            mockContractModel.Verify(contractModel => contractModel.GetContractByIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderSummaryInformationAsync_ShouldReturnCorrectData()
        {
            // Arrange
            long contractId = 123;
            var expected = new Dictionary<string, object>
            {
                { "totalPrice", 99.99 },
                { "tax", 10.00 },
                { "shipping", 5.99 }
            };

            mockContractModel.Setup(contractModel => contractModel.GetOrderSummaryInformationAsync(contractId))
                .ReturnsAsync(expected);

            // Act
            var result = await contractViewModel.GetOrderSummaryInformationAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Count, result.Count);
            foreach (var key in expected.Keys)
            {
                Assert.IsTrue(result.ContainsKey(key));
                Assert.AreEqual(expected[key], result[key]);
            }
            mockContractModel.Verify(contractModel => contractModel.GetOrderSummaryInformationAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetContractSellerAsync_ShouldReturnCorrectData()
        {
            // Arrange
            long contractId = 123;
            var expected = (5, "John Doe");

            mockContractModel.Setup(contractModel => contractModel.GetContractSellerAsync(contractId))
                .ReturnsAsync(expected);

            // Act
            var result = await contractViewModel.GetContractSellerAsync(contractId);

            // Assert
            Assert.AreEqual(expected.Item1, result.SellerID);
            Assert.AreEqual(expected.Item2, result.SellerName);
            mockContractModel.Verify(contractModel => contractModel.GetContractSellerAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetContractBuyerAsync_ShouldReturnCorrectData()
        {
            // Arrange
            long contractId = 123;
            var expected = (10, "Jane Smith");

            mockContractModel.Setup(contractModel => contractModel.GetContractBuyerAsync(contractId))
                .ReturnsAsync(expected);

            // Act
            var result = await contractViewModel.GetContractBuyerAsync(contractId);

            // Assert
            Assert.AreEqual(expected.Item1, result.BuyerID);
            Assert.AreEqual(expected.Item2, result.BuyerName);
            mockContractModel.Verify(contractModel => contractModel.GetContractBuyerAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetProductDetailsByContractIdAsync_ShouldReturnCorrectData()
        {
            // Arrange
            long contractId = 123;
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(30);
            double price = 149.99;
            string name = "Premium Art Piece";

            var expected = (startDate, endDate, price, name);

            mockContractModel.Setup(contractModel => contractModel.GetProductDetailsByContractIdAsync(contractId))
                .ReturnsAsync(expected);

            // Act
            var result = await contractViewModel.GetProductDetailsByContractIdAsync(contractId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.startDate, result.Value.StartDate);
            Assert.AreEqual(expected.endDate, result.Value.EndDate);
            Assert.AreEqual(expected.price, result.Value.price);
            Assert.AreEqual(expected.name, result.Value.name);
            mockContractModel.Verify(contractModel => contractModel.GetProductDetailsByContractIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderDetailsAsync_ShouldReturnCorrectData()
        {
            // Arrange
            long contractId = 123;
            string paymentMethod = "Credit Card";
            DateTime orderDate = new DateTime(2023, 5, 15);

            var expected = (paymentMethod, orderDate);

            mockContractModel.Setup(contractModel => contractModel.GetOrderDetailsAsync(contractId))
                .ReturnsAsync(expected);

            // Act
            var result = await contractViewModel.GetOrderDetailsAsync(contractId);

            // Assert
            Assert.AreEqual(expected.paymentMethod, result.PaymentMethod);
            Assert.AreEqual(expected.orderDate, result.OrderDate);
            mockContractModel.Verify(contractModel => contractModel.GetOrderDetailsAsync(contractId), Times.Once);
        }

        [TestMethod]
        public async Task GetDeliveryDateByContractIdAsync_WhenNull_ShouldReturnNull()
        {
            // Arrange
            long contractId = 999;

            mockContractModel.Setup(contractModel => contractModel.GetDeliveryDateByContractIdAsync(contractId))
                .ReturnsAsync((DateTime?)null);

            // Act
            var result = await contractViewModel.GetDeliveryDateByContractIdAsync(contractId);

            // Assert
            Assert.IsNull(result);
            mockContractModel.Verify(contractModel => contractModel.GetDeliveryDateByContractIdAsync(contractId), Times.Once);
        }

        [TestMethod]
        public void GenerateContractPdf_WhenNullPredefinedContract_ShouldThrowException()
        {
            // Arrange
            Dictionary<string, string> fieldReplacements = new Dictionary<string, string>();
            var methodInfo = typeof(ContractViewModel).GetMethod("GenerateContractPdf",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            var exception = Assert.ThrowsException<TargetInvocationException>(() =>
                methodInfo.Invoke(contractViewModel, new object[] { mockContract, null, fieldReplacements }));
            Assert.IsInstanceOfType(exception.InnerException, typeof(ArgumentNullException));
            Assert.AreEqual("predefinedContract", ((ArgumentNullException)exception.InnerException).ParamName);
        }

        [TestMethod]
        public async Task AddContractAsync_WhenNullPdfData_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await contractViewModel.AddContractAsync(mockContract, null));
        }

        [TestMethod]
        public async Task GenerateAndSaveContractAsync_WhenNullContract_ShouldThrowArgumentNullException()
        {
            // Arrange
            var contractType = PredefinedContractType.BuyingContract;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await contractViewModel.GenerateAndSaveContractAsync(null, contractType));
        }

        [TestMethod]
        public async Task GetContractsByBuyerAsync_WhenInvalidId_ShouldReturnEmptyList()
        {
            // Arrange
            int buyerId = -1;
            mockContractModel.Setup(contractModel => contractModel.GetContractsByBuyerAsync(buyerId))
                .ReturnsAsync(new List<IContract>());

            // Act
            var result = await contractViewModel.GetContractsByBuyerAsync(buyerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            mockContractModel.Verify(contractModel => contractModel.GetContractsByBuyerAsync(buyerId), Times.Once);
        }
    }
}

