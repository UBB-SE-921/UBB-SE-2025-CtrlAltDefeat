//using ArtAttack.Domain;
//using ArtAttack.Model;
//using ArtAttack.ViewModel;
//using ArtAttack.Shared;
//using Microsoft.Data.SqlClient;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ArtAttack.Tests.ViewModel
//{
//    [TestClass]
//    public class ContractRenewViewModelTests
//    {
//        private Mock<IContractModel> _mockContractModel = null!;
//        private Mock<IContractRenewalModel> _mockRenewalModel = null!;
//        private Mock<NotificationDataAdapter> _mockNotificationAdapter = null!;
//        private ContractRenewViewModel _viewModel = null!;
//        private const string ConnectionString = "test_connection_string";

//        [TestInitialize]
//        public void Setup()
//        {
//            _mockContractModel = new Mock<IContractModel>();
//            _mockRenewalModel = new Mock<IContractRenewalModel>();
//            _mockNotificationAdapter = new Mock<NotificationDataAdapter>(ConnectionString);

//            // Use reflection to set private fields
//            _viewModel = new ContractRenewViewModel(ConnectionString);
//            var type = typeof(ContractRenewViewModel);
//            var contractModelField = type.GetField("contractModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//            var renewalModelField = type.GetField("renewalModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//            var notificationAdapterField = type.GetField("notificationAdapter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

//            contractModelField?.SetValue(_viewModel, _mockContractModel.Object);
//            renewalModelField?.SetValue(_viewModel, _mockRenewalModel.Object);
//            notificationAdapterField?.SetValue(_viewModel, _mockNotificationAdapter.Object);
//        }
//        [TestMethod]
//        public async Task SubmitRenewalRequestAsync_ValidRequest_ReturnsSuccess()
//        {
//            // Arrange
//            var contractId = 123L;
//            var mockContract = new Mock<IContract>();
//            mockContract.Setup(c => c.ContractID).Returns(contractId);
//            mockContract.Setup(c => c.RenewalCount).Returns(0);
//            mockContract.Setup(c => c.ContractStatus).Returns("ACTIVE");
//            var newEndDate = DateTime.Now.AddDays(60);
//            var buyerId = 1;
//            var productId = 456;
//            var sellerId = 2;

//            // Set the selected contract
//            var type = typeof(ContractRenewViewModel);
//            var selectedContractProperty = type.GetProperty("SelectedContract");
//            selectedContractProperty?.SetValue(_viewModel, mockContract.Object);

//            // Setup dates to be within renewal period
//            var today = DateTime.Now.Date;
//            var dates = (StartDate: today.AddDays(-25), EndDate: today.AddDays(5), price: 100.0, name: "Test");

//            _mockContractModel.Setup(m => m.GetProductDetailsByContractIdAsync(contractId)).ReturnsAsync(dates);
//            _mockRenewalModel.Setup(m => m.HasContractBeenRenewedAsync(contractId)).ReturnsAsync(false);
//            _mockRenewalModel.Setup(m => m.AddRenewedContractAsync(It.IsAny<IContract>(), It.IsAny<byte[]>())).Returns(Task.CompletedTask);

//            // Act
//            var result = await _viewModel.SubmitRenewalRequestAsync(newEndDate, buyerId, productId, sellerId);

//            // Assert
//            Assert.IsTrue(result.Success);
//            Assert.AreEqual("Contract renewed successfully!", result.Message);
//            _mockRenewalModel.Verify(m => m.AddRenewedContractAsync(It.IsAny<IContract>(), It.IsAny<byte[]>()), Times.Once);
//            _mockNotificationAdapter.Verify(n => n.AddNotification(It.IsAny<ContractRenewalRequestNotification>()), Times.Once);
//            _mockNotificationAdapter.Verify(n => n.AddNotification(It.IsAny<ContractRenewalAnswerNotification>()), Times.Once);
//            _mockNotificationAdapter.Verify(n => n.AddNotification(It.IsAny<ContractRenewalWaitlistNotification>()), Times.Once);
//        }

//    }
//}
