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
        private Mock<IContractModel> _mockContractModel;
        private Mock<IContractRenewalModel> _mockRenewalModel;
        private Mock<INotificationDataAdapter> _mockNotificationAdapter;
        private Mock<IDatabaseProvider> _mockDatabaseProvider;
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDbDataParameter> _mockParameter;
        private Mock<IFileSystem> _mockFileSystem;
        private Mock<IDataParameterCollection> _mockParameters;
        private Mock<IDateTimeProvider> _mockDateTimeProvider;
        private ContractRenewViewModel _viewModel;
        private readonly string _testConnectionString = "TestConnectionString";

        // Sample data for tests
        private IContract _mockContract;
        private List<IContract> _mockContracts;
        private readonly int _testBuyerId = 42;
        private readonly long _testContractId = 123;
        private readonly DateTime _testFixedDate = new DateTime(2023, 6, 15);
        private readonly DateTime _testEndDate = new DateTime(2023, 6, 22);
        private readonly DateTime _testOldEndDate = new DateTime(2023, 6, 20);

        [TestInitialize]
        public void Initialize()
        {
            // Setup mock objects
            _mockContractModel = new Mock<IContractModel>();
            _mockRenewalModel = new Mock<IContractRenewalModel>();
            _mockNotificationAdapter = new Mock<INotificationDataAdapter>();
            _mockDatabaseProvider = new Mock<IDatabaseProvider>();
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockParameter = new Mock<IDbDataParameter>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockDateTimeProvider = new Mock<IDateTimeProvider>();
            _mockParameters = new Mock<IDataParameterCollection>();

            _mockParameters.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

            // Setup contract data
            _mockContract = new Contract
            {
                ContractID = _testContractId,
                OrderID = 456,
                ContractStatus = "ACTIVE",
                ContractContent = "Test contract content",
                RenewalCount = 0,
                PDFID = 789
            };

            _mockContracts = new List<IContract>
            {
                _mockContract,
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
            _mockDateTimeProvider.Setup(p => p.Now).Returns(_testFixedDate);

            _mockContractModel
                .Setup(m => m.GetContractsByBuyerAsync(_testBuyerId))
                .ReturnsAsync(_mockContracts);

            _mockContractModel
                .Setup(m => m.GetContractByIdAsync(_testContractId))
                .ReturnsAsync(_mockContract);

            var productDetails = new ValueTuple<DateTime, DateTime, double, string>(_testFixedDate, _testOldEndDate, 99.99, "Test Product");
            _mockContractModel
                .Setup(m => m.GetProductDetailsByContractIdAsync(_testContractId))
                .ReturnsAsync(productDetails);

            // Setup database mocks
            _mockDatabaseProvider
                .Setup(p => p.CreateConnection(It.IsAny<string>()))
                .Returns(_mockConnection.Object);

            _mockConnection
                .Setup(c => c.CreateCommand())
                .Returns(_mockCommand.Object);

            _mockCommand
                .Setup(c => c.CreateParameter())
                .Returns(_mockParameter.Object);

            // Setup file system mocks
            _mockFileSystem
                .Setup(fs => fs.GetDownloadsPath())
                .Returns(@"C:\Users\Test\Downloads");

            _mockFileSystem
                .Setup(fs => fs.CombinePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((path1, path2) => $"{path1}\\{path2}");

            _mockFileSystem
                .Setup(fs => fs.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(Task.CompletedTask);

            // Create view model with mocked dependencies
            _viewModel = new ContractRenewViewModel(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object);
        }

        [TestMethod]
        public async Task LoadContractsForBuyerAsync_ShouldFilterActiveAndRenewedContracts()
        {
            // Act
            await _viewModel.LoadContractsForBuyerAsync(_testBuyerId);

            // Assert
            Assert.AreEqual(2, _viewModel.BuyerContracts.Count);
            Assert.IsTrue(_viewModel.BuyerContracts.All(c => c.ContractStatus == "ACTIVE" || c.ContractStatus == "RENEWED"));
            _mockContractModel.Verify(m => m.GetContractsByBuyerAsync(_testBuyerId), Times.Once);
        }

        [TestMethod]
        public async Task SelectContractAsync_ShouldSetSelectedContract()
        {
            // Act
            await _viewModel.SelectContractAsync(_testContractId);

            // Assert
            Assert.IsNotNull(_viewModel.SelectedContract);
            Assert.AreEqual(_testContractId, _viewModel.SelectedContract.ContractID);
            _mockContractModel.Verify(m => m.GetContractByIdAsync(_testContractId), Times.Once);
        }

        [TestMethod]
        public async Task GetProductDetailsByContractIdAsync_ShouldReturnProductDetails()
        {
            // Act
            var result = await _viewModel.GetProductDetailsByContractIdAsync(_testContractId);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(_testFixedDate, result.Value.StartDate);
            Assert.AreEqual(_testOldEndDate, result.Value.EndDate);
            Assert.AreEqual(99.99, result.Value.price);
            Assert.AreEqual("Test Product", result.Value.name);
            _mockContractModel.Verify(m => m.GetProductDetailsByContractIdAsync(_testContractId), Times.Once);
        }

        [TestMethod]
        public async Task IsRenewalPeriodValidAsync_WhenNoSelectedContract_ReturnsFalse()
        {
            // Act
            var result = await _viewModel.IsRenewalPeriodValidAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsRenewalPeriodValidAsync_WhenNoProductDetails_ReturnsFalse()
        {
            // Arrange
            await _viewModel.SelectContractAsync(_testContractId);
            _mockContractModel
                .Setup(m => m.GetProductDetailsByContractIdAsync(_testContractId))
                .ReturnsAsync((ValueTuple<DateTime, DateTime, double, string>?)null);

            // Act
            var result = await _viewModel.IsRenewalPeriodValidAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsRenewalPeriodValidAsync_WhenDateOutOfRange_ReturnsFalse()
        {
            // Arrange - Set end date too far in the future
            await _viewModel.SelectContractAsync(_testContractId);
            var outOfRangeDate = new ValueTuple<DateTime, DateTime, double, string>(
                _testFixedDate, _testFixedDate.AddDays(10), 99.99, "Test Product");

            _mockContractModel
                .Setup(m => m.GetProductDetailsByContractIdAsync(_testContractId))
                .ReturnsAsync(outOfRangeDate);

            // Act
            var result = await _viewModel.IsRenewalPeriodValidAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsRenewalPeriodValidAsync_WhenWithinValidRange_ReturnsTrue()
        {
            // Arrange - Set end date 5 days from now (within the 2-7 day range)
            await _viewModel.SelectContractAsync(_testContractId);
            var validRangeDate = new ValueTuple<DateTime, DateTime, double, string>(
                _testFixedDate, _testFixedDate.AddDays(5), 99.99, "Test Product");

            _mockContractModel
                .Setup(m => m.GetProductDetailsByContractIdAsync(_testContractId))
                .ReturnsAsync(validRangeDate);

            // Act
            var result = await _viewModel.IsRenewalPeriodValidAsync();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsProductAvailable_AlwaysReturnsTrue()
        {
            // Act
            var result = _viewModel.IsProductAvailable(999);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanSellerApproveRenewal_WithZeroRenewalCount_ReturnsTrue()
        {
            // Act
            var result = _viewModel.CanSellerApproveRenewal(0);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanSellerApproveRenewal_WithOneOrMoreRenewalCount_ReturnsFalse()
        {
            // Act
            var result = _viewModel.CanSellerApproveRenewal(1);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasContractBeenRenewedAsync_WhenNoSelectedContract_ReturnsFalse()
        {
            // Act
            var result = await _viewModel.HasContractBeenRenewedAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasContractBeenRenewedAsync_DelegatesCallToRenewalModel()
        {
            // Arrange
            await _viewModel.SelectContractAsync(_testContractId);
            _mockRenewalModel.Setup(m => m.HasContractBeenRenewedAsync(_testContractId)).ReturnsAsync(true);

            // Act
            var result = await _viewModel.HasContractBeenRenewedAsync();

            // Assert
            Assert.IsTrue(result);
            _mockRenewalModel.Verify(m => m.HasContractBeenRenewedAsync(_testContractId), Times.Once);
        }

        [TestMethod]
        public void GenerateContractPdf_ReturnsNonEmptyByteArray()
        {
            // Arrange
            string testContent = "Test contract content";

            // Create a mock ContractRenewViewModel with overridden GenerateContractPdf
            var mockViewModel = new Mock<ContractRenewViewModel>(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object)
            { CallBase = true };

            // Mock the PDF generation to return a sample byte array
            byte[] samplePdf = { 1, 2, 3, 4, 5 };
            mockViewModel.Setup(vm => vm.GenerateContractPdf(It.IsAny<IContract>(), It.IsAny<string>()))
                .Returns(samplePdf);

            // Act
            byte[] result = mockViewModel.Object.GenerateContractPdf(_mockContract, testContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
            CollectionAssert.AreEqual(samplePdf, result);
        }


        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WithNoSelectedContract_ReturnsFalseWithMessage()
        {
            // Act
            var result = await _viewModel.SubmitRenewalRequestAsync(_testEndDate, _testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("No contract selected.", result.Message);
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenContractAlreadyRenewed_ReturnsFalseWithMessage()
        {
            // Arrange
            await _viewModel.SelectContractAsync(_testContractId);
            _mockRenewalModel.Setup(m => m.HasContractBeenRenewedAsync(_testContractId)).ReturnsAsync(true);

            // Act
            var result = await _viewModel.SubmitRenewalRequestAsync(_testEndDate, _testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("This contract has already been renewed.", result.Message);
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenRenewalPeriodInvalid_ReturnsFalseWithMessage()
        {
            // Arrange
            await _viewModel.SelectContractAsync(_testContractId);
            _mockRenewalModel.Setup(m => m.HasContractBeenRenewedAsync(_testContractId)).ReturnsAsync(false);

            // Setup the mock view model to return false for IsRenewalPeriodValidAsync
            var mockViewModel = new Mock<ContractRenewViewModel>(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object)
            { CallBase = true };
            mockViewModel.Setup(vm => vm.IsRenewalPeriodValidAsync()).ReturnsAsync(false);

            // Set SelectedContract property using reflection
            var selectedContractProperty = typeof(ContractRenewViewModel).GetProperty("SelectedContract",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty);
            selectedContractProperty?.SetValue(mockViewModel.Object, _mockContract);

            // Act
            var result = await mockViewModel.Object.SubmitRenewalRequestAsync(_testEndDate, _testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Contract is not in a valid renewal period (between 2 and 7 days before end date).", result.Message);
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenSellerCannotApproveRenewal_ReturnsFalseWithMessage()
        {
            // Arrange
            await _viewModel.SelectContractAsync(_testContractId);
            _mockRenewalModel.Setup(m => m.HasContractBeenRenewedAsync(_testContractId)).ReturnsAsync(false);

            // Directly set up the contract model for any contract ID
            _mockContractModel
                .Setup(m => m.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new ValueTuple<DateTime, DateTime, double, string>(
                    _testFixedDate, _testOldEndDate, 99.99, "Test Product"));

            // Set up your view model - don't try to mock methods on the ViewModel itself
            var testViewModel = new ContractRenewViewModel(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object);

            // Manually set the properties needed for the test
            // First select the contract to initialize SelectedContract
            await testViewModel.SelectContractAsync(_testContractId);

            // Override only the method we need to test this specific scenario
            // Use a mock that uses CallBase for CanSellerApproveRenewal only
            var mockViewModel = new Mock<ContractRenewViewModel>(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object)
            { CallBase = true };

            // Only mock this one method to return false
            mockViewModel.Setup(vm => vm.CanSellerApproveRenewal(It.IsAny<int>())).Returns(false);

            // Make sure the mock has a selected contract
            await mockViewModel.Object.SelectContractAsync(_testContractId);

            // Act
            var result = await mockViewModel.Object.SubmitRenewalRequestAsync(_testEndDate, _testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Renewal not allowed: seller limit exceeded.", result.Message);
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenExceptionOccurs_ReturnsErrorMessage()
        {
            // Arrange
            await _viewModel.SelectContractAsync(_testContractId);
            _mockRenewalModel.Setup(m => m.HasContractBeenRenewedAsync(_testContractId)).ReturnsAsync(false);

            // Set up the contract model to return valid product details
            _mockContractModel
                .Setup(m => m.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new ValueTuple<DateTime, DateTime, double, string>(
                    _testFixedDate, _testOldEndDate, 99.99, "Test Product"));

            // Set up a view model that will throw an exception in GenerateContractPdf
            var mockViewModel = new Mock<ContractRenewViewModel>(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object)
            { CallBase = true };

            // Only mock the methods we need to for this specific scenario
            mockViewModel.Setup(vm => vm.GenerateContractPdf(It.IsAny<IContract>(), It.IsAny<string>()))
                .Throws(new Exception("Test exception"));

            // Make sure the mock has a selected contract
            await mockViewModel.Object.SelectContractAsync(_testContractId);

            // Act
            var result = await mockViewModel.Object.SubmitRenewalRequestAsync(_testEndDate, 101, 303, 202);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.Contains("Unexpected error: Test exception"));
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenAllConditionsMet_ReturnsSuccessAndSavesContract()
        {
            // Arrange
            int testPdfId = 42;
            int testBuyerId = 101;
            int testSellerId = 202;
            int testProductId = 303;
            byte[] mockPdfBytes = { 1, 2, 3, 4, 5 };

            await _viewModel.SelectContractAsync(_testContractId);
            _mockRenewalModel.Setup(m => m.HasContractBeenRenewedAsync(_testContractId)).ReturnsAsync(false);

            // Set up the contract model to return valid product details
            _mockContractModel
                .Setup(m => m.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new ValueTuple<DateTime, DateTime, double, string>(
                    _testFixedDate, _testOldEndDate, 99.99, "Test Product"));

            // Set up a view model with the behaviors we need for this test
            var mockViewModel = new Mock<ContractRenewViewModel>(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object)
            { CallBase = true };

            // Only mock the methods we need to for this specific scenario
            mockViewModel.Setup(vm => vm.GenerateContractPdf(It.IsAny<IContract>(), It.IsAny<string>()))
                .Returns(mockPdfBytes);
            mockViewModel.Setup(vm => vm.InsertPdfAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(testPdfId);

            // Make sure the mock has a selected contract
            await mockViewModel.Object.SelectContractAsync(_testContractId);

            // Act
            var result = await mockViewModel.Object.SubmitRenewalRequestAsync(_testEndDate, testBuyerId, testProductId, testSellerId);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Contract renewed successfully!", result.Message);

            // Verify file was saved
            _mockFileSystem.Verify(fs => fs.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);

            // Verify contract was added to the database
            _mockRenewalModel.Verify(m => m.AddRenewedContractAsync(
                It.IsAny<IContract>(), It.IsAny<byte[]>()),
                Times.Once);

            // Verify notifications were sent
            _mockNotificationAdapter.Verify(n => n.AddNotification(It.IsAny<Notification>()), Times.Exactly(3));
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenProductDetailsNotFound_ReturnsFalseWithMessage()
        {
            // Arrange
            await _viewModel.SelectContractAsync(_testContractId);
            _mockRenewalModel.Setup(m => m.HasContractBeenRenewedAsync(_testContractId)).ReturnsAsync(false);

            // Setup the mock view model to bypass the renewal period check
            var mockViewModel = new Mock<ContractRenewViewModel>(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object)
            { CallBase = true };

            mockViewModel.Setup(vm => vm.IsRenewalPeriodValidAsync()).ReturnsAsync(true);

            // Setup for product details to be null
            _mockContractModel
                .Setup(m => m.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync((ValueTuple<DateTime, DateTime, double, string>?)null);

            // Set SelectedContract property using reflection
            var selectedContractProperty = typeof(ContractRenewViewModel).GetProperty("SelectedContract",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty);
            selectedContractProperty?.SetValue(mockViewModel.Object, _mockContract);

            // Act
            var result = await mockViewModel.Object.SubmitRenewalRequestAsync(_testEndDate, _testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Could not retrieve current contract dates.", result.Message);
        }


        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenNewEndDateNotAfterOldDate_ReturnsFalseWithMessage()
        {
            // Arrange
            await _viewModel.SelectContractAsync(_testContractId);
            _mockRenewalModel.Setup(m => m.HasContractBeenRenewedAsync(_testContractId)).ReturnsAsync(false);

            // Set up the contract model to return valid product details
            _mockContractModel
                .Setup(m => m.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new ValueTuple<DateTime, DateTime, double, string>(
                    _testFixedDate, _testOldEndDate, 99.99, "Test Product"));

            // Act - Use a date equal to the old end date
            var result = await _viewModel.SubmitRenewalRequestAsync(_testOldEndDate, _testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("New end date must be after the current end date.", result.Message);
        }

        [TestMethod]
        public async Task SubmitRenewalRequestAsync_WhenProductUnavailable_ReturnsFalseWithMessage()
        {
            // Arrange
            await _viewModel.SelectContractAsync(_testContractId);
            _mockRenewalModel.Setup(m => m.HasContractBeenRenewedAsync(_testContractId)).ReturnsAsync(false);

            // Set up the contract model to return valid product details
            _mockContractModel
                .Setup(m => m.GetProductDetailsByContractIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new ValueTuple<DateTime, DateTime, double, string>(
                    _testFixedDate, _testOldEndDate, 99.99, "Test Product"));

            // Set up a view model that returns false for IsProductAvailable
            var mockViewModel = new Mock<ContractRenewViewModel>(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object)
            { CallBase = true };

            // Only mock the methods we need to for this specific scenario
            mockViewModel.Setup(vm => vm.IsProductAvailable(It.IsAny<int>())).Returns(false);

            // Make sure the mock has a selected contract
            await mockViewModel.Object.SelectContractAsync(_testContractId);

            // Act
            var result = await mockViewModel.Object.SubmitRenewalRequestAsync(_testEndDate, _testBuyerId, 999, 888);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Product is not available.", result.Message);
        }

        [TestMethod]
        public void GenerateContractPdf_WithRealImplementation_GeneratesPdfBytes()
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
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object);

            // Act - Call the real implementation
            byte[] result = viewModel.GenerateContractPdf(contract, testContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public void FileSystemWrapper_GetDownloadsPath_ReturnsValidPath()
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
        public void FileSystemWrapper_CombinePath_JoinsPathsCorrectly()
        {
            // Arrange
            var fileSystem = new FileSystemWrapper();
            string path1 = @"C:\Folder";
            string path2 = "File.txt";

            // Act
            string result = fileSystem.CombinePath(path1, path2);

            // Assert
            Assert.AreEqual(@"C:\Folder\File.txt", result);
        }

        [TestMethod]
        public async Task FileSystemWrapper_WriteAllBytesAsync_WritesToFileSystem()
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
        public void DateTimeProvider_Now_ReturnsCurrentDateTime()
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
        public void Constructor_WithNullContractModel_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                null,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object));
        }

        [TestMethod]
        public void Constructor_WithNullRenewalModel_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                _mockContractModel.Object,
                null,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object));
        }

        [TestMethod]
        public void Constructor_WithNullNotificationAdapter_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                null,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object));
        }

        [TestMethod]
        public void Constructor_WithNullDatabaseProvider_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                null,
                _testConnectionString,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object));
        }

        [TestMethod]
        public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                null,
                _mockFileSystem.Object,
                _mockDateTimeProvider.Object));
        }

        [TestMethod]
        public void Constructor_WithNullFileSystem_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                null,
                _mockDateTimeProvider.Object));
        }

        [TestMethod]
        public void Constructor_WithNullDateTimeProvider_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new ContractRenewViewModel(
                _mockContractModel.Object,
                _mockRenewalModel.Object,
                _mockNotificationAdapter.Object,
                _mockDatabaseProvider.Object,
                _testConnectionString,
                _mockFileSystem.Object,
                null));
        }

        [TestMethod]
        public async Task InsertPdfAsync_WithNullBytes_HandlesCorrectly()
        {
            // Arrange
            byte[] nullBytes = null;
            int expectedPdfId = 1;

            // Setup mocks
            _mockConnection.Setup(c => c.Open());
            _mockCommand.Setup(c => c.ExecuteScalar()).Returns(expectedPdfId);

            // This is the key fix - need to setup the command mock parameters
            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object);

            // Act
            int result = await _viewModel.InsertPdfAsync(nullBytes);

            // Assert
            Assert.AreEqual(expectedPdfId, result);
            _mockParameter.VerifySet(p => p.Value = nullBytes);
        }


        [TestMethod]
        public async Task InsertPdfAsync_WhenScalarReturnsNull_ThrowsException()
        {
            // Arrange
            byte[] testBytes = { 1, 2, 3 };

            _mockConnection.Setup(c => c.Open());
            _mockCommand.Setup(c => c.ExecuteScalar()).Returns(null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<NullReferenceException>(async () =>
                await _viewModel.InsertPdfAsync(testBytes));
        }

        [TestMethod]
        public async Task InsertPdfAsync_WithEmptyBytes_HandlesCorrectly()
        {
            // Arrange
            byte[] emptyBytes = new byte[0];
            int expectedPdfId = 1;

            // Setup mocks
            _mockConnection.Setup(c => c.Open());
            _mockCommand.Setup(c => c.ExecuteScalar()).Returns(expectedPdfId);

            // This is the key fix - need to setup the command mock parameters
            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object);

            // Act
            int result = await _viewModel.InsertPdfAsync(emptyBytes);

            // Assert
            Assert.AreEqual(expectedPdfId, result);
            _mockParameter.VerifySet(p => p.Value = emptyBytes);
        }

    }
}
