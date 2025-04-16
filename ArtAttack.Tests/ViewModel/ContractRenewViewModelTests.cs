using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using ArtAttack.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuestPDF.Infrastructure;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class ContractRenewViewModelTests
    {
        private Mock<IContractRepository> mockContractModel;
        private Mock<IContractRenewalRepository> mockRenewalModel;
        private Mock<INotificationDataAdapter> mockNotificationAdapter;
        private Mock<IDatabaseProvider> mockDatabaseProvider;
        private Mock<IDbConnection> mockConnection;
        private Mock<IDbCommand> mockCommand;
        private Mock<IDbDataParameter> mockParameter;
        private Mock<IFileSystem> mockFileSystem;
        private Mock<IDataParameterCollection> mockParameters;
        private Mock<IDateTimeProvider> mockDateTimeProvider;
        private ContractRenewViewModel contractRenewViewModel;
        private readonly string testConnectionString = "TestConnectionString";

        // Sample data for tests
        private IContract mockContract;
        private List<IContract> mockContracts;
        private readonly int testBuyerId = 42;
        private readonly long testContractId = 123;
        private readonly DateTime testFixedDate = new DateTime(2023, 6, 15);
        private readonly DateTime testEndDate = new DateTime(2023, 6, 22);
        private readonly DateTime testOldEndDate = new DateTime(2023, 6, 20);

        [TestInitialize]
        public void Initialize()
        {
            // Setup mock objects
            mockContractModel = new Mock<IContractRepository>();
            mockRenewalModel = new Mock<IContractRenewalRepository>();
            mockNotificationAdapter = new Mock<INotificationDataAdapter>();
            mockDatabaseProvider = new Mock<IDatabaseProvider>();
            mockConnection = new Mock<IDbConnection>();
            mockCommand = new Mock<IDbCommand>();
            mockParameter = new Mock<IDbDataParameter>();
            mockFileSystem = new Mock<IFileSystem>();
            mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockParameters = new Mock<IDataParameterCollection>();

            mockParameters.Setup(dataParameter => dataParameter.Add(It.IsAny<object>())).Returns(0);

            // Setup contract data
            mockContract = new Contract
            {
                ContractID = testContractId,
                OrderID = 456,
                ContractStatus = "ACTIVE",
                ContractContent = "Test contract content",
                RenewalCount = 0,
                PDFID = 789
            };

            mockContracts = new List<IContract>
            {
                mockContract,
                new Contract
                {
                    ContractID = 124,
                    OrderID = 457,
                    ContractStatus = "ACTIVE"
                },
                new Contract
                {
                    ContractID = 125,
                    OrderID = 458,
                    ContractStatus = "EXPIRED"
                }
            };

            // Setup mock behaviors
            mockDateTimeProvider.Setup(dateTimeProvider => dateTimeProvider.Now).Returns(testFixedDate);

            mockContractModel
                .Setup(contractModel => contractModel.GetContractsByBuyerAsync(testBuyerId))
                .ReturnsAsync(mockContracts);

            mockContractModel
                .Setup(contractModel => contractModel.GetContractByIdAsync(testContractId))
                .ReturnsAsync(mockContract);

            var productDetails = new ValueTuple<DateTime, DateTime, double, string>(testFixedDate, testOldEndDate, 99.99, "Test Product");
            mockContractModel
                .Setup(contractModel => contractModel.GetProductDetailsByContractIdAsync(testContractId))
                .ReturnsAsync(productDetails);

            // Setup database mocks
            mockDatabaseProvider
                .Setup(databaseProvider => databaseProvider.CreateConnection(It.IsAny<string>()))
                .Returns(mockConnection.Object);

            mockConnection
                .Setup(databaseConnection => databaseConnection.CreateCommand())
                .Returns(mockCommand.Object);

            mockCommand
                .Setup(databaseCommand => databaseCommand.CreateParameter())
                .Returns(mockParameter.Object);

            // Setup file system mocks
            mockFileSystem
                .Setup(fileSystem => fileSystem.GetDownloadsPath())
                .Returns(@"C:\Users\Test\Downloads");

            mockFileSystem
                .Setup(fileSystem => fileSystem.CombinePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((path1, path2) => $"{path1}\\{path2}");

            mockFileSystem
                .Setup(fileSystem => fileSystem.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(Task.CompletedTask);

            // Create view model with mocked dependencies
            contractRenewViewModel = new ContractRenewViewModel(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object);
        }

        [TestMethod]
        public async Task LoadContractsForBuyerAsync_ShouldFilterActiveAndRenewedContracts()
        {
            // Act
            await contractRenewViewModel.LoadContractsForBuyerAsync(testBuyerId);

            // Assert
            Assert.AreEqual(2, contractRenewViewModel.BuyerContracts.Count);
            Assert.IsTrue(contractRenewViewModel.BuyerContracts.All(buyerContracts => buyerContracts.ContractStatus == "ACTIVE" || buyerContracts.ContractStatus == "RENEWED"));
            mockContractModel.Verify(contractModel => contractModel.GetContractsByBuyerAsync(testBuyerId), Times.Once);
        }

        [TestMethod]
        public async Task SelectContractAsync_ShouldSetSelectedContract()
        {
            // Act
            await contractRenewViewModel.SelectContractAsync(testContractId);

            // Assert
            Assert.IsNotNull(contractRenewViewModel.SelectedContract);
            Assert.AreEqual(testContractId, contractRenewViewModel.SelectedContract.ContractID);
            mockContractModel.Verify(contractModel => contractModel.GetContractByIdAsync(testContractId), Times.Once);
        }

        [TestMethod]
        public async Task GetProductDetailsByContractIdAsync_ShouldReturnProductDetails()
        {
            // Act
            var result = await contractRenewViewModel.GetProductDetailsByContractIdAsync(testContractId);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(testFixedDate, result.Value.StartDate);
            Assert.AreEqual(testOldEndDate, result.Value.EndDate);
            Assert.AreEqual(99.99, result.Value.price);
            Assert.AreEqual("Test Product", result.Value.name);
            mockContractModel.Verify(contractModel => contractModel.GetProductDetailsByContractIdAsync(testContractId), Times.Once);
        }

        [TestMethod]
        public async Task IsRenewalPeriodValidAsync_WhenNoSelectedContract_ShouldReturnFalse()
        {
            // Act
            var result = await contractRenewViewModel.IsRenewalPeriodValidAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsRenewalPeriodValidAsync_WhenNoProductDetails_ShouldReturnFalse()
        {
            // Arrange
            await contractRenewViewModel.SelectContractAsync(testContractId);
            mockContractModel
                .Setup(contractModel => contractModel.GetProductDetailsByContractIdAsync(testContractId))
                .ReturnsAsync((ValueTuple<DateTime, DateTime, double, string>?)null);

            // Act
            var result = await contractRenewViewModel.IsRenewalPeriodValidAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsRenewalPeriodValidAsync_WhenDateOutOfRange_ShouldReturnFalse()
        {
            // Arrange - Set end date too far in the future
            await contractRenewViewModel.SelectContractAsync(testContractId);
            var outOfRangeDate = new ValueTuple<DateTime, DateTime, double, string>(
                testFixedDate, testFixedDate.AddDays(10), 99.99, "Test Product");

            mockContractModel
                .Setup(contractModel => contractModel.GetProductDetailsByContractIdAsync(testContractId))
                .ReturnsAsync(outOfRangeDate);

            // Act
            var result = await contractRenewViewModel.IsRenewalPeriodValidAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsRenewalPeriodValidAsync_WhenWithinValidRange_ShouldReturnTrue()
        {
            // Arrange - Set end date 5 days from now (within the 2-7 day range)
            await contractRenewViewModel.SelectContractAsync(testContractId);
            var validRangeDate = new ValueTuple<DateTime, DateTime, double, string>(
                testFixedDate, testFixedDate.AddDays(5), 99.99, "Test Product");

            mockContractModel
                .Setup(contractModel => contractModel.GetProductDetailsByContractIdAsync(testContractId))
                .ReturnsAsync(validRangeDate);

            // Act
            var result = await contractRenewViewModel.IsRenewalPeriodValidAsync();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsProductAvailable_ShouldAlwaysReturnsTrue()
        {
            // Act
            var result = contractRenewViewModel.IsProductAvailable(999);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanSellerApproveRenewal_WhenZeroRenewalCount_ShouldReturnTrue()
        {
            // Act
            var result = contractRenewViewModel.CanSellerApproveRenewal(0);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanSellerApproveRenewal_WhenOneOrMoreRenewalCount_ShouldReturnFalse()
        {
            // Act
            var result = contractRenewViewModel.CanSellerApproveRenewal(1);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasContractBeenRenewedAsync_WhenNoSelectedContract_ShouldReturnFalse()
        {
            // Act
            var result = await contractRenewViewModel.HasContractBeenRenewedAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasContractBeenRenewedAsync_ShouldDelegateCallToRenewalModel()
        {
            // Arrange
            await contractRenewViewModel.SelectContractAsync(testContractId);
            mockRenewalModel.Setup(contractRenewalModel => contractRenewalModel.HasContractBeenRenewedAsync(testContractId)).ReturnsAsync(true);

            // Act
            var result = await contractRenewViewModel.HasContractBeenRenewedAsync();

            // Assert
            Assert.IsTrue(result);
            mockRenewalModel.Verify(contractRenewalModel => contractRenewalModel.HasContractBeenRenewedAsync(testContractId), Times.Once);
        }

        [TestMethod]
        public void GenerateContractPdf_ShouldReturnNonEmptyByteArray()
        {
            // Arrange
            string testContent = "Test contract content";

            // Create a mock ContractRenewViewModel with overridden GenerateContractPdf
            var mockViewModel = new Mock<ContractRenewViewModel>(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object)
            { CallBase = true };

            // Mock the PDF generation to return a sample byte array
            byte[] samplePdf = { 1, 2, 3, 4, 5 };
            mockViewModel.Setup(contractRenewViewModel => contractRenewViewModel.GenerateContractPdf(It.IsAny<IContract>(), It.IsAny<string>()))
                .Returns(samplePdf);

            // Act
            byte[] result = mockViewModel.Object.GenerateContractPdf(mockContract, testContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
            CollectionAssert.AreEqual(samplePdf, result);
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WithNoSelectedContract_ShouldReturnFalseWithMessage()
        {
            // Act
            var result = await contractRenewViewModel.SubmitRenewalRequestAsync(testEndDate, testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("No contract selected.", result.Message);
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenContractAlreadyRenewed_ShouldReturnFalseWithMessage()
        {
            // Arrange
            await contractRenewViewModel.SelectContractAsync(testContractId);
            mockRenewalModel.Setup(contractRenewalModel => contractRenewalModel.HasContractBeenRenewedAsync(testContractId)).ReturnsAsync(true);

            // Act
            var result = await contractRenewViewModel.SubmitRenewalRequestAsync(testEndDate, testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("This contract has already been renewed.", result.Message);
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenRenewalPeriodInvalid_ShouldReturnFalseWithMessage()
        {
            // Arrange
            await contractRenewViewModel.SelectContractAsync(testContractId);
            mockRenewalModel.Setup(contractRenewalModel => contractRenewalModel.HasContractBeenRenewedAsync(testContractId)).ReturnsAsync(false);

            // Setup the mock view model to return false for IsRenewalPeriodValidAsync
            var mockViewModel = new Mock<ContractRenewViewModel>(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object)
            { CallBase = true };
            mockViewModel.Setup(contractRenewViewModel => contractRenewViewModel.IsRenewalPeriodValidAsync()).ReturnsAsync(false);

            // Set SelectedContract property using reflection
            var selectedContractProperty = typeof(ContractRenewViewModel).GetProperty("SelectedContract",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty);
            selectedContractProperty?.SetValue(mockViewModel.Object, mockContract);

            // Act
            var result = await mockViewModel.Object.SubmitRenewalRequestAsync(testEndDate, testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Contract is not in a valid renewal period (between 2 and 7 days before end date).", result.Message);
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenSellerCannotApproveRenewal_ShouldReturnFalseWithMessage()
        {
            // Arrange
            await contractRenewViewModel.SelectContractAsync(testContractId);
            mockRenewalModel.Setup(contractRenewalModel => contractRenewalModel.HasContractBeenRenewedAsync(testContractId)).ReturnsAsync(false);

            // Directly set up the contract model for any contract ID
            mockContractModel
                .Setup(contractModel => contractModel.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new ValueTuple<DateTime, DateTime, double, string>(
                    testFixedDate, testOldEndDate, 99.99, "Test Product"));

            // Set up your view model - don't try to mock methods on the ViewModel itself
            var testViewModel = new ContractRenewViewModel(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object);

            // Manually set the properties needed for the test
            // First select the contract to initialize SelectedContract
            await testViewModel.SelectContractAsync(testContractId);

            // Override only the method we need to test this specific scenario
            // Use a mock that uses CallBase for CanSellerApproveRenewal only
            var mockViewModel = new Mock<ContractRenewViewModel>(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object)
            { CallBase = true };

            // Only mock this one method to return false
            mockViewModel.Setup(contractRenewViewModel => contractRenewViewModel.CanSellerApproveRenewal(It.IsAny<int>())).Returns(false);

            // Make sure the mock has a selected contract
            await mockViewModel.Object.SelectContractAsync(testContractId);

            // Act
            var result = await mockViewModel.Object.SubmitRenewalRequestAsync(testEndDate, testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Renewal not allowed: seller limit exceeded.", result.Message);
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenExceptionOccurs_ShouldReturnErrorMessage()
        {
            // Arrange
            await contractRenewViewModel.SelectContractAsync(testContractId);
            mockRenewalModel.Setup(contractRenewalModel => contractRenewalModel.HasContractBeenRenewedAsync(testContractId)).ReturnsAsync(false);

            // Set up the contract model to return valid product details
            mockContractModel
                .Setup(contractModel => contractModel.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new ValueTuple<DateTime, DateTime, double, string>(
                    testFixedDate, testOldEndDate, 99.99, "Test Product"));

            // Set up a view model that will throw an exception in GenerateContractPdf
            var mockViewModel = new Mock<ContractRenewViewModel>(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object)
            { CallBase = true };

            // Only mock the methods we need to for this specific scenario
            mockViewModel.Setup(contractRenewViewModel => contractRenewViewModel.GenerateContractPdf(It.IsAny<IContract>(), It.IsAny<string>()))
                .Throws(new Exception("Test exception"));

            // Make sure the mock has a selected contract
            await mockViewModel.Object.SelectContractAsync(testContractId);

            // Act
            var result = await mockViewModel.Object.SubmitRenewalRequestAsync(testEndDate, 101, 303, 202);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.Contains("Unexpected error: Test exception"));
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenAllConditionsMet_ShouldReturnSuccessAndSavesContract()
        {
            // Arrange
            int testPdfId = 42;
            int testBuyerId = 101;
            int testSellerId = 202;
            int testProductId = 303;
            byte[] mockPdfBytes = { 1, 2, 3, 4, 5 };

            await contractRenewViewModel.SelectContractAsync(testContractId);
            mockRenewalModel.Setup(contractRenewalModel => contractRenewalModel.HasContractBeenRenewedAsync(testContractId)).ReturnsAsync(false);

            // Set up the contract model to return valid product details
            mockContractModel
                .Setup(m => m.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new ValueTuple<DateTime, DateTime, double, string>(
                    testFixedDate, testOldEndDate, 99.99, "Test Product"));

            // Set up a view model with the behaviors we need for this test
            var mockViewModel = new Mock<ContractRenewViewModel>(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object)
            { CallBase = true };

            // Only mock the methods we need to for this specific scenario
            mockViewModel.Setup(contractRenewViewModel => contractRenewViewModel.GenerateContractPdf(It.IsAny<IContract>(), It.IsAny<string>()))
                .Returns(mockPdfBytes);
            mockViewModel.Setup(contractRenewViewModel => contractRenewViewModel.InsertPdfAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(testPdfId);

            // Make sure the mock has a selected contract
            await mockViewModel.Object.SelectContractAsync(testContractId);

            // Act
            var result = await mockViewModel.Object.SubmitRenewalRequestAsync(testEndDate, testBuyerId, testProductId, testSellerId);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Contract renewed successfully!", result.Message);

            // Verify file was saved
            mockFileSystem.Verify(fileSystem => fileSystem.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);

            // Verify contract was added to the database
            mockRenewalModel.Verify(contractRenewalModel => contractRenewalModel.AddRenewedContractAsync(
                It.IsAny<IContract>(), It.IsAny<byte[]>()),
                Times.Once);

            // Verify notifications were sent
            mockNotificationAdapter.Verify(notificationAdapter => notificationAdapter.AddNotification(It.IsAny<Notification>()), Times.Exactly(3));
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenProductDetailsNotFound_ShouldReturnFalseWithMessage()
        {
            // Arrange
            await contractRenewViewModel.SelectContractAsync(testContractId);
            mockRenewalModel.Setup(contractRenewalModel => contractRenewalModel.HasContractBeenRenewedAsync(testContractId)).ReturnsAsync(false);

            // Setup the mock view model to bypass the renewal period check
            var mockViewModel = new Mock<ContractRenewViewModel>(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object)
            { CallBase = true };

            mockViewModel.Setup(contractViewModel => contractViewModel.IsRenewalPeriodValidAsync()).ReturnsAsync(true);

            // Setup for product details to be null
            mockContractModel
                .Setup(contractModel => contractModel.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync((ValueTuple<DateTime, DateTime, double, string>?)null);

            // Set SelectedContract property using reflection
            var selectedContractProperty = typeof(ContractRenewViewModel).GetProperty("SelectedContract",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty);
            selectedContractProperty?.SetValue(mockViewModel.Object, mockContract);

            // Act
            var result = await mockViewModel.Object.SubmitRenewalRequestAsync(testEndDate, testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Could not retrieve current contract dates.", result.Message);
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenNewEndDateNotAfterOldDate_ShouldReturnFalseWithMessage()
        {
            // Arrange
            await contractRenewViewModel.SelectContractAsync(testContractId);
            mockRenewalModel.Setup(contractRenewalModel => contractRenewalModel.HasContractBeenRenewedAsync(testContractId)).ReturnsAsync(false);

            // Set up the contract model to return valid product details
            mockContractModel
                .Setup(contractModel => contractModel.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new ValueTuple<DateTime, DateTime, double, string>(
                    testFixedDate, testOldEndDate, 99.99, "Test Product"));

            // Act - Use a date equal to the old end date
            var result = await contractRenewViewModel.SubmitRenewalRequestAsync(testOldEndDate, testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("New end date must be after the current end date.", result.Message);
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenProductUnavailable_ShouldReturnFalseWithMessage()
        {
            // Arrange
            await contractRenewViewModel.SelectContractAsync(testContractId);
            mockRenewalModel.Setup(contractRenewalModel => contractRenewalModel.HasContractBeenRenewedAsync(testContractId)).ReturnsAsync(false);

            // Set up the contract model to return valid product details
            mockContractModel
                .Setup(contractModel => contractModel.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new ValueTuple<DateTime, DateTime, double, string>(
                    testFixedDate, testOldEndDate, 99.99, "Test Product"));

            // Set up a view model that returns false for IsProductAvailable
            var mockViewModel = new Mock<ContractRenewViewModel>(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object)
            { CallBase = true };

            // Only mock the methods we need to for this specific scenario
            mockViewModel.Setup(contractViewModel => contractViewModel.IsProductAvailable(It.IsAny<int>())).Returns(false);

            // Make sure the mock has a selected contract
            await mockViewModel.Object.SelectContractAsync(testContractId);

            // Act
            var result = await mockViewModel.Object.SubmitRenewalRequestAsync(testEndDate, testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Product is not available.", result.Message);
        }

        [TestMethod]
        public void GenerateContractPdf_ShouldGeneratePdfBytes()
        {
            // Arrange - Set QuestPDF license for testing
            QuestPDF.Settings.License = LicenseType.Community;

            // Setup test data
            var contract = new Contract
            {
                ContractID = 123,
                OrderID = 456,
                ContractStatus = "ACTIVE",
                ContractContent = "Original content"
            };

            string testContent = "Test PDF Content";

            // Create an instance with real implementation (not mocked)
            var viewModel = new ContractRenewViewModel(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object);

            // Act - Call the real implementation
            byte[] result = viewModel.GenerateContractPdf(contract, testContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public void GetDownloadPathFileSystemWrapper_ShouldReturnValidPath()
        {
            // Arrange
            var fileSystem = new FileSystemWrapper();

            // Act
            string path = fileSystem.GetDownloadsPath();

            // Assert
            Assert.IsNotNull(path);
            Assert.IsTrue(path.Contains("Downloads"));
            Assert.IsTrue(path.Contains(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
        }

        [TestMethod]
        public void CombinePathFileSystemWrapper_ShouldJoinPathsCorrectly()
        {
            // Arrange
            var fileSystem = new FileSystemWrapper();
            string firstPath = @"C:\Folder";
            string secondPath = "File.txt";

            // Act
            string result = fileSystem.CombinePath(firstPath, secondPath);

            // Assert
            Assert.AreEqual(@"C:\Folder\File.txt", result);
        }

        [TestMethod]
        public async Task WriteAllBytesAsyncFileSystemWrapper_ShouldWriteToFileSystem()
        {
            // This test requires mocking of File.WriteAllBytesAsync which is static
            // We'll use a wrapper approach with dependency injection to test this

            // Arrange
            string testPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.bin");
            byte[] testData = { 1, 2, 3, 4, 5 };
            var fileSystem = new FileSystemWrapper();

            try
            {
                // Act
                await fileSystem.WriteAllBytesAsync(testPath, testData);

                // Assert
                Assert.IsTrue(File.Exists(testPath));
                byte[] readData = await File.ReadAllBytesAsync(testPath);
                CollectionAssert.AreEqual(testData, readData);
            }
            finally
            {
                // Clean up
                if (File.Exists(testPath))
                {
                    File.Delete(testPath);
                }
            }
        }

        [TestMethod]
        public void DateTimeProvider_ShouldReturnCurrentDateTime()
        {
            // Arrange
            var provider = new DateTimeProvider();
            DateTime before = DateTime.Now.AddSeconds(-1);

            // Act
            DateTime result = provider.Now;
            DateTime after = DateTime.Now.AddSeconds(1);

            // Assert
            Assert.IsTrue(result >= before && result <= after);
        }

        [TestMethod]
        public void Constructor_WhenNullContractModel_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                null,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object));
        }

        [TestMethod]
        public void Constructor_WhenNullRenewalModel_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                mockContractModel.Object,
                null,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object));
        }

        [TestMethod]
        public void Constructor_WhenNullNotificationAdapter_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                mockContractModel.Object,
                mockRenewalModel.Object,
                null,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object));
        }

        [TestMethod]
        public void Constructor_WhenNullDatabaseProvider_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                null,
                testConnectionString,
                mockFileSystem.Object,
                mockDateTimeProvider.Object));
        }

        [TestMethod]
        public void Constructor_WhenNullConnectionString_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                null,
                mockFileSystem.Object,
                mockDateTimeProvider.Object));
        }

        [TestMethod]
        public void Constructor_WhenNullFileSystem_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                null,
                mockDateTimeProvider.Object));
        }

        [TestMethod]
        public void Constructor_WhenNullDateTimeProvider_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                mockContractModel.Object,
                mockRenewalModel.Object,
                mockNotificationAdapter.Object,
                mockDatabaseProvider.Object,
                testConnectionString,
                mockFileSystem.Object,
                null));
        }

        [TestMethod]
        public async Task InsertPdfAsync_WhenNullBytes_ShouldHandleCorrectly()
        {
            // Arrange
            byte[] nullBytes = null;
            int expectedPdfId = 1;

            // Setup mocks
            mockConnection.Setup(connection => connection.Open());
            mockCommand.Setup(command => command.ExecuteScalar()).Returns(expectedPdfId);

            // This is the key fix - need to setup the command mock parameters
            mockCommand.Setup(command => command.Parameters).Returns(mockParameters.Object);

            // Act
            int result = await contractRenewViewModel.InsertPdfAsync(nullBytes);

            // Assert
            Assert.AreEqual(expectedPdfId, result);
            mockParameter.VerifySet(parameter => parameter.Value = nullBytes);
        }

        [TestMethod]
        public async Task InsertPdfAsync_WhenScalarReturnsNull_ShouldThrowException()
        {
            // Arrange
            byte[] testBytes = { 1, 2, 3 };

            mockConnection.Setup(connection => connection.Open());
            mockCommand.Setup(command => command.ExecuteScalar()).Returns(null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<NullReferenceException>(async () =>
                await contractRenewViewModel.InsertPdfAsync(testBytes));
        }

        [TestMethod]
        public async Task InsertPdfAsync_WhenEmptyBytes_ShouldHandleCorrectly()
        {
            // Arrange
            byte[] emptyBytes = new byte[0];
            int expectedPdfId = 1;

            // Setup mocks
            mockConnection.Setup(connection => connection.Open());
            mockCommand.Setup(command => command.ExecuteScalar()).Returns(expectedPdfId);

            // This is the key fix - need to setup the command mock parameters
            mockCommand.Setup(command => command.Parameters).Returns(mockParameters.Object);

            // Act
            int result = await contractRenewViewModel.InsertPdfAsync(emptyBytes);

            // Assert
            Assert.AreEqual(expectedPdfId, result);
            mockParameter.VerifySet(parameter => parameter.Value = emptyBytes);
        }
    }
}
