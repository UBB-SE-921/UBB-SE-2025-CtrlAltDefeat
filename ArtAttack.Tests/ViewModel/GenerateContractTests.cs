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

}
