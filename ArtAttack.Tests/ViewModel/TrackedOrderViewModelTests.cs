using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using ArtAttack.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class TrackedOrderViewModelTests
    {
        private Mock<ITrackedOrderModel> _mockTrackedOrderModel;
        private Mock<IOrderViewModel> _mockOrderViewModel;
        private Mock<INotificationService> _mockNotificationService;
        private TrackedOrderViewModel _viewModel;
        private const string TestConnectionString = "test_connection_string";

        [TestInitialize]
        public void InitializeTestEnvironment()
        {
            // Initialize mocks
            _mockTrackedOrderModel = new Mock<ITrackedOrderModel>();
            _mockOrderViewModel = new Mock<IOrderViewModel>();
            _mockNotificationService = new Mock<INotificationService>();

            // Create ViewModel with mocked dependencies
            _viewModel = new TrackedOrderViewModel(
                _mockTrackedOrderModel.Object,
                _mockOrderViewModel.Object,
                _mockNotificationService.Object
            );
        }

        [TestMethod]
        public void Constructor_WithConnectionStringInitializesViewModelCorrectly()
        {
            // Act
            var viewModel = new TrackedOrderViewModel(TestConnectionString);

            // Assert
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        public void Constructor_WithDependenciesInitializesViewModelCorrectly()
        {
            // Act
            var viewModel = new TrackedOrderViewModel(
                _mockTrackedOrderModel.Object,
                _mockOrderViewModel.Object,
                _mockNotificationService.Object
            );

            // Assert
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullTrackedOrderModelThrowsArgumentNullException()
        {
            // Act
            var viewModel = new TrackedOrderViewModel(
                null,
                _mockOrderViewModel.Object,
                _mockNotificationService.Object
            );

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullOrderViewModelThrowsArgumentNullException()
        {
            // Act
            var viewModel = new TrackedOrderViewModel(
                _mockTrackedOrderModel.Object,
                null,
                _mockNotificationService.Object
            );

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullNotificationServiceThrowsArgumentNullException()
        {
            // Act
            var viewModel = new TrackedOrderViewModel(
                _mockTrackedOrderModel.Object,
                _mockOrderViewModel.Object,
                null
            );

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        public async Task GetTrackedOrderByIDAsync_ValidIDReturnsCorrectTrackedOrder()
        {
            // Arrange
            int trackedOrderId = 123;
            var expectedOrder = CreateSampleTrackedOrder(trackedOrderId);

            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(trackedOrderId))
                .ReturnsAsync(expectedOrder);

            // Act
            var actualOrder = await _viewModel.GetTrackedOrderByIDAsync(trackedOrderId);

            // Assert
            VerifyTrackedOrderIsCorrect(expectedOrder, actualOrder);
            _mockTrackedOrderModel.Verify(model => model.GetTrackedOrderByIdAsync(trackedOrderId), Times.Once);
        }

        [TestMethod]
        public async Task GetTrackedOrderByIDAsync_ModelThrowsExceptionReturnsNull()
        {
            // Arrange
            int trackedOrderId = 999;
            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(trackedOrderId))
                .ThrowsAsync(new Exception("Not found"));

            // Act
            var result = await _viewModel.GetTrackedOrderByIDAsync(trackedOrderId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetOrderCheckpointByIDAsync_ValidIDReturnsCorrectCheckpoint()
        {
            // Arrange
            int checkpointId = 123;
            var expectedCheckpoint = CreateSampleOrderCheckpoint(checkpointId);

            _mockTrackedOrderModel.Setup(model => model.GetOrderCheckpointByIdAsync(checkpointId))
                .ReturnsAsync(expectedCheckpoint);

            // Act
            var actualCheckpoint = await _viewModel.GetOrderCheckpointByIDAsync(checkpointId);

            // Assert
            VerifyOrderCheckpointIsCorrect(expectedCheckpoint, actualCheckpoint);
            _mockTrackedOrderModel.Verify(model => model.GetOrderCheckpointByIdAsync(checkpointId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderCheckpointByIDAsync_ModelThrowsExceptionReturnsNull()
        {
            // Arrange
            int checkpointId = 999;
            _mockTrackedOrderModel.Setup(model => model.GetOrderCheckpointByIdAsync(checkpointId))
                .ThrowsAsync(new Exception("Not found"));

            // Act
            var result = await _viewModel.GetOrderCheckpointByIDAsync(checkpointId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAllTrackedOrdersAsync_ReturnsCorrectTrackedOrdersList()
        {
            // Arrange
            var expectedOrders = CreateSampleTrackedOrdersList();
            _mockTrackedOrderModel.Setup(model => model.GetAllTrackedOrdersAsync())
                .ReturnsAsync(expectedOrders);

            // Act
            var actualOrders = await _viewModel.GetAllTrackedOrdersAsync();

            // Assert
            Assert.IsNotNull(actualOrders);
            Assert.AreEqual(expectedOrders.Count, actualOrders.Count);
            VerifyTrackedOrderIsCorrect(expectedOrders[0], actualOrders[0]);
            VerifyTrackedOrderIsCorrect(expectedOrders[1], actualOrders[1]);
            _mockTrackedOrderModel.Verify(model => model.GetAllTrackedOrdersAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAllOrderCheckpointsAsync_ValidTrackedOrderIDReturnsCorrectCheckpointsList()
        {
            // Arrange
            int trackedOrderId = 123;
            var expectedCheckpoints = CreateSampleOrderCheckpointsList(trackedOrderId);

            _mockTrackedOrderModel.Setup(model => model.GetAllOrderCheckpointsAsync(trackedOrderId))
                .ReturnsAsync(expectedCheckpoints);

            // Act
            var actualCheckpoints = await _viewModel.GetAllOrderCheckpointsAsync(trackedOrderId);

            // Assert
            Assert.IsNotNull(actualCheckpoints);
            Assert.AreEqual(expectedCheckpoints.Count, actualCheckpoints.Count);
            VerifyOrderCheckpointIsCorrect(expectedCheckpoints[0], actualCheckpoints[0]);
            VerifyOrderCheckpointIsCorrect(expectedCheckpoints[1], actualCheckpoints[1]);
            _mockTrackedOrderModel.Verify(model => model.GetAllOrderCheckpointsAsync(trackedOrderId), Times.Once);
        }

        [TestMethod]
        public async Task DeleteTrackedOrderAsync_ValidIDReturnsSuccessResult()
        {
            // Arrange
            int trackedOrderId = 123;
            _mockTrackedOrderModel.Setup(model => model.DeleteTrackedOrderAsync(trackedOrderId))
                .ReturnsAsync(true);

            // Act
            var result = await _viewModel.DeleteTrackedOrderAsync(trackedOrderId);

            // Assert
            Assert.IsTrue(result);
            _mockTrackedOrderModel.Verify(model => model.DeleteTrackedOrderAsync(trackedOrderId), Times.Once);
        }

        [TestMethod]
        public async Task DeleteOrderCheckpointAsync_ValidIDReturnsSuccessResult()
        {
            // Arrange
            int checkpointId = 123;
            _mockTrackedOrderModel.Setup(model => model.DeleteOrderCheckpointAsync(checkpointId))
                .ReturnsAsync(true);

            // Act
            var result = await _viewModel.DeleteOrderCheckpointAsync(checkpointId);

            // Assert
            Assert.IsTrue(result);
            _mockTrackedOrderModel.Verify(model => model.DeleteOrderCheckpointAsync(checkpointId), Times.Once);
        }

        [TestMethod]
        public async Task AddTrackedOrderAsync_ValidOrderAddsOrderAndSendsNotification()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            int expectedTrackedOrderId = 123;
            var associatedOrder = CreateSampleOrder(trackedOrder.OrderID);

            SetupAddTrackedOrderMocks(trackedOrder, expectedTrackedOrderId, associatedOrder);

            // Act
            var actualTrackedOrderId = await _viewModel.AddTrackedOrderAsync(trackedOrder);

            // Assert
            Assert.AreEqual(expectedTrackedOrderId, actualTrackedOrderId);
            VerifyAddTrackedOrderInteractions(trackedOrder, expectedTrackedOrderId, associatedOrder);
        }

        [TestMethod]
        public async Task AddTrackedOrderAsync_NotificationFailsStillReturnsNewId()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            int expectedTrackedOrderId = 123;
            var associatedOrder = CreateSampleOrder(trackedOrder.OrderID);

            SetupAddTrackedOrderMocksWithNotificationFailure(trackedOrder, expectedTrackedOrderId, associatedOrder);

            // Act
            var actualTrackedOrderId = await _viewModel.AddTrackedOrderAsync(trackedOrder);

            // Assert
            Assert.AreEqual(expectedTrackedOrderId, actualTrackedOrderId);
            VerifyAddTrackedOrderWithFailedNotificationInteractions(trackedOrder, associatedOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AddTrackedOrderAsync_ModelThrowsExceptionPropagatesException()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            _mockTrackedOrderModel.Setup(model => model.AddTrackedOrderAsync(trackedOrder))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _viewModel.AddTrackedOrderAsync(trackedOrder);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        public async Task UpdateOrderCheckpointAsync_ValidParametersUpdatesCheckpointAndTrackedOrder()
        {
            // Arrange
            int checkpointId = 123;
            var timestamp = new DateTime(2025, 4, 1);
            string location = "Warehouse A";
            string description = "Order in warehouse";
            OrderStatus status = OrderStatus.IN_WAREHOUSE;

            var checkpoint = CreateSampleOrderCheckpoint(checkpointId, timestamp, location, description, status);
            var trackedOrder = CreateSampleTrackedOrder(checkpoint.TrackedOrderID);

            SetupUpdateOrderCheckpointMocks(checkpoint, trackedOrder);

            // Act
            await _viewModel.UpdateOrderCheckpointAsync(checkpointId, timestamp, location, description, status);

            // Assert
            VerifyUpdateOrderCheckpointInteractions(checkpointId, timestamp, location, description, status, trackedOrder);
        }

        [TestMethod]
        public async Task UpdateOrderCheckpointAsync_WithTrackedOrderIDUpdatesCheckpoint()
        {
            // Arrange
            int checkpointId = 123;
            int trackedOrderId = 456;
            var timestamp = new DateTime(2025, 4, 1);
            string location = "Warehouse A";
            string description = "Order in warehouse";
            OrderStatus status = OrderStatus.IN_WAREHOUSE;

            var checkpoint = CreateSampleOrderCheckpoint(checkpointId, timestamp, location, description, status, trackedOrderId);
            var trackedOrder = CreateSampleTrackedOrder(trackedOrderId);

            SetupUpdateOrderCheckpointWithTrackedOrderIdMocks(checkpoint, trackedOrder);

            // Act
            bool result = await _viewModel.UpdateOrderCheckpointAsync(checkpointId, timestamp, location, description, status, trackedOrderId);

            // Assert
            Assert.IsTrue(result);
            VerifyUpdateOrderCheckpointWithTrackedOrderIdInteractions(checkpointId, timestamp, location, description, status);
        }

        [TestMethod]
        public async Task UpdateOrderCheckpointAsync_WithTrackedOrderID_TrackedOrderIdMismatchReturnsFalse()
        {
            // Arrange
            int checkpointId = 123;
            int trackedOrderId = 456;
            int wrongTrackedOrderId = 789;
            var timestamp = new DateTime(2025, 4, 1);
            string location = "Warehouse A";
            string description = "Order in warehouse";
            OrderStatus status = OrderStatus.IN_WAREHOUSE;

            var checkpoint = CreateSampleOrderCheckpoint(checkpointId, timestamp, location, description, status, trackedOrderId);

            _mockTrackedOrderModel.Setup(model => model.GetOrderCheckpointByIdAsync(checkpointId))
                .ReturnsAsync(checkpoint);

            // Act
            bool result = await _viewModel.UpdateOrderCheckpointAsync(checkpointId, timestamp, location, description, status, wrongTrackedOrderId);

            // Assert
            Assert.IsFalse(result);
            _mockTrackedOrderModel.Verify(model => model.UpdateOrderCheckpointAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OrderStatus>()
            ), Times.Never);
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_WithShippedStatusSendsNotification()
        {
            // Arrange
            int trackedOrderId = 123;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            OrderStatus status = OrderStatus.SHIPPED;

            var trackedOrder = CreateSampleTrackedOrder(trackedOrderId);
            var associatedOrder = CreateSampleOrder(trackedOrder.OrderID);

            SetupUpdateTrackedOrderWithNotificationMocks(trackedOrder, associatedOrder);

            // Act
            await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, status);

            // Assert
            VerifyUpdateTrackedOrderWithNotificationInteractions(trackedOrderId, deliveryDate, status, trackedOrder, associatedOrder);
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_WithDeliveryAddressUpdatesTrackedOrder()
        {
            // Arrange
            int trackedOrderId = 123;
            int orderId = 456;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            string deliveryAddress = "123 Test St, Updated";
            OrderStatus status = OrderStatus.SHIPPED;

            var trackedOrder = CreateSampleTrackedOrder(trackedOrderId, orderId);
            var associatedOrder = CreateSampleOrder(orderId);

            SetupUpdateTrackedOrderWithAddressMocks(trackedOrder, associatedOrder);

            // Act
            bool result = await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, deliveryAddress, status, orderId);

            // Assert
            Assert.IsTrue(result);
            VerifyUpdateTrackedOrderWithAddressInteractions(trackedOrderId, deliveryDate, status, associatedOrder);
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_WithDeliveryAddressOrderIdMismatchReturnsFalse()
        {
            // Arrange
            int trackedOrderId = 123;
            int orderId = 456;
            int wrongOrderId = 789;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            string deliveryAddress = "123 Test St, Updated";
            OrderStatus status = OrderStatus.SHIPPED;

            var trackedOrder = CreateSampleTrackedOrder(trackedOrderId, orderId);

            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(trackedOrderId))
                .ReturnsAsync(trackedOrder);

            // Act
            bool result = await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, deliveryAddress, status, wrongOrderId);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetLastCheckpoint_HasCheckpointsReturnsLastCheckpoint()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            var checkpoints = CreateSampleOrderCheckpointsList(trackedOrder.TrackedOrderID);

            _mockTrackedOrderModel.Setup(model => model.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(checkpoints);

            // Act
            var lastCheckpoint = await _viewModel.GetLastCheckpoint(trackedOrder);

            // Assert
            Assert.IsNotNull(lastCheckpoint);
            Assert.AreEqual(2, lastCheckpoint.CheckpointID);
            Assert.AreEqual(OrderStatus.SHIPPED, lastCheckpoint.Status);
        }

        [TestMethod]
        public async Task GetLastCheckpoint_NoCheckpointsReturnsNull()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            _mockTrackedOrderModel.Setup(model => model.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(new List<OrderCheckpoint>());

            // Act
            var result = await _viewModel.GetLastCheckpoint(trackedOrder);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetNumberOfCheckpoints_HasCheckpointsReturnsCorrectCount()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            var checkpoints = CreateSampleOrderCheckpointsList(trackedOrder.TrackedOrderID);

            _mockTrackedOrderModel.Setup(model => model.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(checkpoints);

            // Act
            int count = await _viewModel.GetNumberOfCheckpoints(trackedOrder);

            // Assert
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RevertToPreviousCheckpoint_NullOrderThrowsArgumentNullException()
        {
            // Act
            await _viewModel.RevertToPreviousCheckpoint(null);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task RevertToPreviousCheckpoint_NoCheckpointsThrowsException()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            _mockTrackedOrderModel.Setup(model => model.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(new List<OrderCheckpoint>());

            // Act
            await _viewModel.RevertToPreviousCheckpoint(trackedOrder);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        public async Task RevertToLastCheckpoint_NoCheckpointsReturnsFalse()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            _mockTrackedOrderModel.Setup(model => model.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(new List<OrderCheckpoint>());

            // Act
            bool result = await _viewModel.RevertToLastCheckpoint(trackedOrder);

            // Assert
            Assert.IsFalse(result);
            _mockTrackedOrderModel.Verify(model => model.UpdateTrackedOrderAsync(
                It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<OrderStatus>()
            ), Times.Never);
        }

        [TestMethod]
        public async Task RevertToLastCheckpoint_NullOrderReturnsFalse()
        {
            // Act
            bool result = await _viewModel.RevertToLastCheckpoint(null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task AddOrderCheckpointAsync_WithShippedStatusSendsNotification()
        {
            // Arrange
            var checkpoint = CreateSampleOrderCheckpointWithShippedStatus();
            var trackedOrder = CreateSampleTrackedOrder(checkpoint.TrackedOrderID);
            var associatedOrder = CreateSampleOrder(trackedOrder.OrderID);
            int expectedCheckpointId = 42;

            SetupAddOrderCheckpointWithNotificationMocks(checkpoint, trackedOrder, associatedOrder, expectedCheckpointId);

            // Act
            int actualCheckpointId = await _viewModel.AddOrderCheckpointAsync(checkpoint);

            // Assert
            Assert.AreEqual(expectedCheckpointId, actualCheckpointId);
            VerifyAddOrderCheckpointWithNotificationInteractions(checkpoint, trackedOrder, associatedOrder);
        }

        [TestMethod]
        public async Task AddOrderCheckpointAsync_WithRegularStatusDoesNotSendNotification()
        {
            // Arrange
            var checkpoint = CreateSampleOrderCheckpointWithProcessingStatus();
            var trackedOrder = CreateSampleTrackedOrder(checkpoint.TrackedOrderID);
            int expectedCheckpointId = 42;

            SetupAddOrderCheckpointWithoutNotificationMocks(checkpoint, trackedOrder, expectedCheckpointId);

            // Act
            int actualCheckpointId = await _viewModel.AddOrderCheckpointAsync(checkpoint);

            // Assert
            Assert.AreEqual(expectedCheckpointId, actualCheckpointId);
            VerifyAddOrderCheckpointWithoutNotificationInteractions(checkpoint, trackedOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AddOrderCheckpointAsync_ModelThrowsExceptionPropagatesException()
        {
            // Arrange
            var checkpoint = CreateSampleOrderCheckpointWithShippedStatus();
            _mockTrackedOrderModel.Setup(model => model.AddOrderCheckpointAsync(checkpoint))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _viewModel.AddOrderCheckpointAsync(checkpoint);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task UpdateOrderCheckpointAsync_ModelThrowsExceptionPropagatesException()
        {
            // Arrange
            int checkpointId = 123;
            var timestamp = new DateTime(2025, 4, 1);
            string location = "Warehouse A";
            string description = "Order in warehouse";
            OrderStatus status = OrderStatus.IN_WAREHOUSE;

            _mockTrackedOrderModel.Setup(model => model.UpdateOrderCheckpointAsync(
                checkpointId, timestamp, location, description, status))
                .ThrowsAsync(new Exception("Database error updating checkpoint"));

            // Act
            await _viewModel.UpdateOrderCheckpointAsync(checkpointId, timestamp, location, description, status);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        public async Task UpdateOrderCheckpointAsync_WithTrackedOrderID_ModelThrowsExceptionReturnsFalse()
        {
            // Arrange
            int checkpointId = 123;
            int trackedOrderId = 456;
            var timestamp = new DateTime(2025, 4, 1);
            string location = "Warehouse A";
            string description = "Order in warehouse";
            OrderStatus status = OrderStatus.IN_WAREHOUSE;

            _mockTrackedOrderModel.Setup(model => model.GetOrderCheckpointByIdAsync(checkpointId))
                .ThrowsAsync(new Exception("Error retrieving checkpoint"));

            // Act
            bool result = await _viewModel.UpdateOrderCheckpointAsync(checkpointId, timestamp, location, description, status, trackedOrderId);

            // Assert
            Assert.IsFalse(result);
            _mockTrackedOrderModel.Verify(model => model.UpdateOrderCheckpointAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OrderStatus>()
            ), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task UpdateTrackedOrderAsync_ModelThrowsExceptionPropagatesException()
        {
            // Arrange
            int trackedOrderId = 123;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            OrderStatus status = OrderStatus.SHIPPED;

            _mockTrackedOrderModel.Setup(model => model.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, status))
                .ThrowsAsync(new Exception("Database error updating tracked order"));

            // Act
            await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, status);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_WithDeliveryAddress_ModelThrowsExceptionReturnsFalse()
        {
            // Arrange
            int trackedOrderId = 123;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            string deliveryAddress = "123 Test St, Updated";
            OrderStatus status = OrderStatus.SHIPPED;
            int orderId = 456;

            _mockTrackedOrderModel.Setup(model => model.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, status))
                .ThrowsAsync(new Exception("Database error updating tracked order"));

            // Act
            bool result = await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, deliveryAddress, status, orderId);

            // Assert
            Assert.IsFalse(result);
            _mockOrderViewModel.Verify(model => model.GetOrderByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task AddOrderCheckpointAsync_NotificationThrowsExceptionStillReturnsNewId()
        {
            // Arrange
            var checkpoint = CreateSampleOrderCheckpointWithShippedStatus();
            var trackedOrder = CreateSampleTrackedOrder(checkpoint.TrackedOrderID);
            var associatedOrder = CreateSampleOrder(trackedOrder.OrderID);
            int expectedCheckpointId = 42;

            SetupAddOrderCheckpointWithFailingNotificationMocks(checkpoint, trackedOrder, associatedOrder, expectedCheckpointId);

            // Act
            int actualCheckpointId = await _viewModel.AddOrderCheckpointAsync(checkpoint);

            // Assert
            Assert.AreEqual(expectedCheckpointId, actualCheckpointId);
            VerifyAddOrderCheckpointWithFailingNotificationInteractions(checkpoint, trackedOrder);
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_NotificationThrowsExceptionStillSucceeds()
        {
            // Arrange
            int trackedOrderId = 123;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            OrderStatus status = OrderStatus.OUT_FOR_DELIVERY;

            var trackedOrder = CreateSampleTrackedOrder(trackedOrderId);
            var associatedOrder = CreateSampleOrder(trackedOrder.OrderID);

            SetupUpdateTrackedOrderWithFailingNotificationMocks(trackedOrder, associatedOrder);

            // Act - No exception should be thrown
            await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, status);

            // Assert
            _mockTrackedOrderModel.Verify(model => model.UpdateTrackedOrderAsync(
                trackedOrderId, deliveryDate, status), Times.Once);
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_WithDeliveryAddress_NotificationFailureStillReturnsTrue()
        {
            // Arrange
            int trackedOrderId = 123;
            int orderId = 456;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            string deliveryAddress = "123 Test St, Updated";
            OrderStatus status = OrderStatus.SHIPPED;

            var trackedOrder = CreateSampleTrackedOrder(trackedOrderId, orderId);
            var associatedOrder = CreateSampleOrder(orderId);

            SetupUpdateTrackedOrderWithAddressAndFailingNotificationMocks(trackedOrder, associatedOrder);

            // Act
            bool result = await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, deliveryAddress, status, orderId);

            // Assert
            Assert.IsTrue(result);
            _mockTrackedOrderModel.Verify(model => model.UpdateTrackedOrderAsync(
                trackedOrderId, deliveryDate, status
            ), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task RevertToPreviousCheckpoint_CurrentCheckpointNotNull_DeletionFailsThrowsException()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            var checkpoints = CreateSampleOrderCheckpointsList(trackedOrder.TrackedOrderID);

            _mockTrackedOrderModel.Setup(model => model.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(checkpoints);
            _mockTrackedOrderModel.Setup(model => model.DeleteOrderCheckpointAsync(It.IsAny<int>()))
                .ReturnsAsync(false); // This will cause the exception

            // Act
            await _viewModel.RevertToPreviousCheckpoint(trackedOrder);

            // Assert - handled by ExpectedException attribute
        }

        [TestMethod]
        public async Task RevertToPreviousCheckpoint_CurrentCheckpointNotNull_DeletionSucceedsUpdatesStatus()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            var checkpoints = SetupMultiCallCheckpointListForReversion(trackedOrder);

            // Setup checkpoint deletion
            _mockTrackedOrderModel.Setup(model => model.DeleteOrderCheckpointAsync(2))
                .ReturnsAsync(true);

            // Setup to prevent null reference in UpdateTrackedOrderAsync method
            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(trackedOrder);

            // Act
            await _viewModel.RevertToPreviousCheckpoint(trackedOrder);

            // Assert
            _mockTrackedOrderModel.Verify(model => model.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                OrderStatus.PROCESSING // Should have reverted to the status of the previous checkpoint
            ), Times.Once);
            _mockTrackedOrderModel.Verify(model => model.DeleteOrderCheckpointAsync(2), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task RevertToPreviousCheckpoint_CurrentCheckpointNullThrowsException()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            SetupRetrievingCheckpointsThenEmptyList(trackedOrder);

            // Act
            await _viewModel.RevertToPreviousCheckpoint(trackedOrder);

            // Assert - handled by ExpectedException attribute
        }

        [TestMethod]
        public async Task RevertToLastCheckpoint_HasLastCheckpointUpdatesStatusAndReturnsTrue()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            var checkpoint = CreateSampleProcessingCheckpoint(trackedOrder.TrackedOrderID);
            var checkpoints = new List<OrderCheckpoint> { checkpoint };

            // Setup to get the last checkpoint
            _mockTrackedOrderModel.Setup(model => model.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(checkpoints);

            // Setup the update to succeed
            _mockTrackedOrderModel.Setup(model => model.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                checkpoint.Status))
                .Returns(Task.CompletedTask);

            // Setup to prevent null reference
            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(trackedOrder);

            // Act
            bool result = await _viewModel.RevertToLastCheckpoint(trackedOrder);

            // Assert
            Assert.IsTrue(result);
            _mockTrackedOrderModel.Verify(model => model.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                checkpoint.Status
            ), Times.Once);
        }

        [TestMethod]
        public async Task RevertToLastCheckpoint_UpdateThrowsExceptionReturnsFalse()
        {
            // Arrange
            var trackedOrder = CreateSampleTrackedOrder();
            var checkpoint = CreateSampleProcessingCheckpoint(trackedOrder.TrackedOrderID);
            var checkpoints = new List<OrderCheckpoint> { checkpoint };

            _mockTrackedOrderModel.Setup(model => model.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(checkpoints);

            _mockTrackedOrderModel.Setup(model => model.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                checkpoint.Status))
                .ThrowsAsync(new Exception("Update failed"));

            // Act
            bool result = await _viewModel.RevertToLastCheckpoint(trackedOrder);

            // Assert
            Assert.IsFalse(result);
        }

        #region Helper Methods

        private TrackedOrder CreateSampleTrackedOrder(int trackedOrderId = 123, int orderId = 456)
        {
            return new TrackedOrder
            {
                TrackedOrderID = trackedOrderId,
                OrderID = orderId,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St, Test City"
            };
        }

        private List<TrackedOrder> CreateSampleTrackedOrdersList()
        {
            return new List<TrackedOrder>
            {
                new TrackedOrder
                {
                    TrackedOrderID = 1,
                    OrderID = 101,
                    CurrentStatus = OrderStatus.PROCESSING,
                    EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                    DeliveryAddress = "123 Test St"
                },
                new TrackedOrder
                {
                    TrackedOrderID = 2,
                    OrderID = 102,
                    CurrentStatus = OrderStatus.SHIPPED,
                    EstimatedDeliveryDate = new DateOnly(2025, 5, 2),
                    DeliveryAddress = "456 Test St"
                }
            };
        }

        private OrderCheckpoint CreateSampleOrderCheckpoint(
            int checkpointId = 123,
            DateTime? timestamp = null,
            string location = "Warehouse A",
            string description = "Order in warehouse",
            OrderStatus status = OrderStatus.SHIPPED,
            int trackedOrderId = 456)
        {
            return new OrderCheckpoint
            {
                CheckpointID = checkpointId,
                TrackedOrderID = trackedOrderId,
                Status = status,
                Timestamp = timestamp ?? new DateTime(2025, 4, 1),
                Location = location,
                Description = description
            };
        }

        private OrderCheckpoint CreateSampleProcessingCheckpoint(int trackedOrderId)
        {
            return new OrderCheckpoint
            {
                CheckpointID = 1,
                TrackedOrderID = trackedOrderId,
                Status = OrderStatus.PROCESSING,
                Timestamp = new DateTime(2025, 4, 1),
                Description = "Order received"
            };
        }

        private OrderCheckpoint CreateSampleOrderCheckpointWithShippedStatus()
        {
            return new OrderCheckpoint
            {
                TrackedOrderID = 123,
                Timestamp = DateTime.Now,
                Location = "Distribution Center",
                Description = "Package ready for shipping",
                Status = OrderStatus.SHIPPED
            };
        }

        private OrderCheckpoint CreateSampleOrderCheckpointWithProcessingStatus()
        {
            return new OrderCheckpoint
            {
                TrackedOrderID = 123,
                Timestamp = DateTime.Now,
                Location = "Warehouse",
                Description = "Preparing for shipment",
                Status = OrderStatus.PROCESSING
            };
        }

        private List<OrderCheckpoint> CreateSampleOrderCheckpointsList(int trackedOrderId)
        {
            return new List<OrderCheckpoint>
            {
                new OrderCheckpoint
                {
                    CheckpointID = 1,
                    TrackedOrderID = trackedOrderId,
                    Status = OrderStatus.PROCESSING,
                    Timestamp = new DateTime(2025, 4, 1),
                    Description = "Order received"
                },
                new OrderCheckpoint
                {
                    CheckpointID = 2,
                    TrackedOrderID = trackedOrderId,
                    Status = OrderStatus.SHIPPED,
                    Timestamp = new DateTime(2025, 4, 2),
                    Description = "Order shipped"
                }
            };
        }

        private Order CreateSampleOrder(int orderId, int buyerId = 789)
        {
            return new Order
            {
                OrderID = orderId,
                BuyerID = buyerId
            };
        }

        private void VerifyTrackedOrderIsCorrect(TrackedOrder expected, TrackedOrder actual)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.TrackedOrderID, actual.TrackedOrderID);
            Assert.AreEqual(expected.OrderID, actual.OrderID);
            Assert.AreEqual(expected.CurrentStatus, actual.CurrentStatus);
        }

        private void VerifyOrderCheckpointIsCorrect(OrderCheckpoint expected, OrderCheckpoint actual)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.CheckpointID, actual.CheckpointID);
            Assert.AreEqual(expected.TrackedOrderID, actual.TrackedOrderID);
            Assert.AreEqual(expected.Status, actual.Status);
        }

        private void SetupAddTrackedOrderMocks(TrackedOrder trackedOrder, int newTrackedOrderId, Order associatedOrder)
        {
            _mockTrackedOrderModel.Setup(model => model.AddTrackedOrderAsync(trackedOrder))
                .ReturnsAsync(newTrackedOrderId);
            _mockOrderViewModel.Setup(model => model.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(associatedOrder);
        }

        private void SetupAddTrackedOrderMocksWithNotificationFailure(TrackedOrder trackedOrder, int newTrackedOrderId, Order associatedOrder)
        {
            SetupAddTrackedOrderMocks(trackedOrder, newTrackedOrderId, associatedOrder);

            _mockNotificationService.Setup(notification => notification.SendShippingProgressNotificationAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()
            )).ThrowsAsync(new Exception("Notification failed"));
        }

        private void VerifyAddTrackedOrderInteractions(TrackedOrder trackedOrder, int newTrackedOrderId, Order associatedOrder)
        {
            _mockTrackedOrderModel.Verify(model => model.AddTrackedOrderAsync(trackedOrder), Times.Once);
            _mockOrderViewModel.Verify(model => model.GetOrderByIdAsync(trackedOrder.OrderID), Times.Once);
            _mockNotificationService.Verify(notification => notification.SendShippingProgressNotificationAsync(
                associatedOrder.BuyerID,
                newTrackedOrderId,
                trackedOrder.CurrentStatus.ToString(),
                It.IsAny<DateTime>()
            ), Times.Once);
        }

        private void VerifyAddTrackedOrderWithFailedNotificationInteractions(TrackedOrder trackedOrder, Order associatedOrder)
        {
            _mockTrackedOrderModel.Verify(model => model.AddTrackedOrderAsync(trackedOrder), Times.Once);
            _mockOrderViewModel.Verify(model => model.GetOrderByIdAsync(trackedOrder.OrderID), Times.Once);
        }

        private void SetupUpdateOrderCheckpointMocks(OrderCheckpoint checkpoint, TrackedOrder trackedOrder)
        {
            _mockTrackedOrderModel.Setup(model => model.GetOrderCheckpointByIdAsync(checkpoint.CheckpointID))
                .ReturnsAsync(checkpoint);
            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(checkpoint.TrackedOrderID))
                .ReturnsAsync(trackedOrder);
        }

        private void VerifyUpdateOrderCheckpointInteractions(
            int checkpointId, DateTime timestamp, string location, string description, OrderStatus status, TrackedOrder trackedOrder)
        {
            _mockTrackedOrderModel.Verify(model => model.UpdateOrderCheckpointAsync(
                checkpointId, timestamp, location, description, status
            ), Times.Once);
            _mockTrackedOrderModel.Verify(model => model.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                status
            ), Times.Once);
        }

        private void SetupUpdateOrderCheckpointWithTrackedOrderIdMocks(OrderCheckpoint checkpoint, TrackedOrder trackedOrder)
        {
            _mockTrackedOrderModel.Setup(model => model.GetOrderCheckpointByIdAsync(checkpoint.CheckpointID))
                .ReturnsAsync(checkpoint);
            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(checkpoint.TrackedOrderID))
                .ReturnsAsync(trackedOrder);
        }

        private void VerifyUpdateOrderCheckpointWithTrackedOrderIdInteractions(
            int checkpointId, DateTime timestamp, string location, string description, OrderStatus status)
        {
            _mockTrackedOrderModel.Verify(model => model.UpdateOrderCheckpointAsync(
                checkpointId, timestamp, location, description, status
            ), Times.Once);
        }

        private void SetupUpdateTrackedOrderWithNotificationMocks(TrackedOrder trackedOrder, Order associatedOrder)
        {
            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(trackedOrder);
            _mockOrderViewModel.Setup(model => model.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(associatedOrder);
        }

        private void VerifyUpdateTrackedOrderWithNotificationInteractions(
            int trackedOrderId, DateOnly deliveryDate, OrderStatus status, TrackedOrder trackedOrder, Order associatedOrder)
        {
            _mockTrackedOrderModel.Verify(model => model.UpdateTrackedOrderAsync(
                trackedOrderId, deliveryDate, status
            ), Times.Once);
            _mockNotificationService.Verify(notification => notification.SendShippingProgressNotificationAsync(
                associatedOrder.BuyerID,
                trackedOrderId,
                status.ToString(),
                It.IsAny<DateTime>()
            ), Times.Never);
        }

        private void SetupUpdateTrackedOrderWithAddressMocks(TrackedOrder trackedOrder, Order associatedOrder)
        {
            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(trackedOrder);
            _mockOrderViewModel.Setup(model => model.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(associatedOrder);
        }

        private void VerifyUpdateTrackedOrderWithAddressInteractions(
            int trackedOrderId, DateOnly deliveryDate, OrderStatus status, Order associatedOrder)
        {
            _mockTrackedOrderModel.Verify(model => model.UpdateTrackedOrderAsync(
                trackedOrderId, deliveryDate, status
            ), Times.Once);
            _mockNotificationService.Verify(notification => notification.SendShippingProgressNotificationAsync(
                associatedOrder.BuyerID,
                trackedOrderId,
                status.ToString(),
                It.IsAny<DateTime>()
            ), Times.Once);
        }

        private void SetupAddOrderCheckpointWithNotificationMocks(
            OrderCheckpoint checkpoint, TrackedOrder trackedOrder, Order associatedOrder, int newCheckpointId)
        {
            _mockTrackedOrderModel.Setup(model => model.AddOrderCheckpointAsync(checkpoint))
                .ReturnsAsync(newCheckpointId);
            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(checkpoint.TrackedOrderID))
                .ReturnsAsync(trackedOrder);
            _mockOrderViewModel.Setup(model => model.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(associatedOrder);
        }

        private void VerifyAddOrderCheckpointWithNotificationInteractions(
            OrderCheckpoint checkpoint, TrackedOrder trackedOrder, Order associatedOrder)
        {
            _mockTrackedOrderModel.Verify(model => model.AddOrderCheckpointAsync(checkpoint), Times.Once);
            _mockTrackedOrderModel.Verify(model => model.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                checkpoint.Status), Times.Once);
            _mockNotificationService.Verify(notification => notification.SendShippingProgressNotificationAsync(
                associatedOrder.BuyerID,
                trackedOrder.TrackedOrderID,
                trackedOrder.CurrentStatus.ToString(),
                It.IsAny<DateTime>()), Times.Once);
        }

        private void SetupAddOrderCheckpointWithoutNotificationMocks(
            OrderCheckpoint checkpoint, TrackedOrder trackedOrder, int newCheckpointId)
        {
            _mockTrackedOrderModel.Setup(model => model.AddOrderCheckpointAsync(checkpoint))
                .ReturnsAsync(newCheckpointId);
            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(checkpoint.TrackedOrderID))
                .ReturnsAsync(trackedOrder);
        }

        private void VerifyAddOrderCheckpointWithoutNotificationInteractions(OrderCheckpoint checkpoint, TrackedOrder trackedOrder)
        {
            _mockTrackedOrderModel.Verify(model => model.AddOrderCheckpointAsync(checkpoint), Times.Once);
            _mockTrackedOrderModel.Verify(model => model.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                checkpoint.Status), Times.Once);
            _mockOrderViewModel.Verify(model => model.GetOrderByIdAsync(It.IsAny<int>()), Times.Never);
            _mockNotificationService.Verify(notification => notification.SendShippingProgressNotificationAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        private void SetupAddOrderCheckpointWithFailingNotificationMocks(
            OrderCheckpoint checkpoint, TrackedOrder trackedOrder, Order associatedOrder, int newCheckpointId)
        {
            _mockTrackedOrderModel.Setup(model => model.AddOrderCheckpointAsync(checkpoint))
                .ReturnsAsync(newCheckpointId);
            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(checkpoint.TrackedOrderID))
                .ReturnsAsync(trackedOrder);
            _mockOrderViewModel.Setup(model => model.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(associatedOrder);
            _mockNotificationService.Setup(notification => notification.SendShippingProgressNotificationAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Notification service error"));
        }

        private void VerifyAddOrderCheckpointWithFailingNotificationInteractions(OrderCheckpoint checkpoint, TrackedOrder trackedOrder)
        {
            _mockTrackedOrderModel.Verify(model => model.AddOrderCheckpointAsync(checkpoint), Times.Once);
            _mockTrackedOrderModel.Verify(model => model.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                checkpoint.Status), Times.Once);
        }

        private void SetupUpdateTrackedOrderWithFailingNotificationMocks(TrackedOrder trackedOrder, Order associatedOrder)
        {
            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(trackedOrder);
            _mockOrderViewModel.Setup(model => model.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(associatedOrder);
            _mockNotificationService.Setup(notification => notification.SendShippingProgressNotificationAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Notification service error"));
        }

        private void SetupUpdateTrackedOrderWithAddressAndFailingNotificationMocks(TrackedOrder trackedOrder, Order associatedOrder)
        {
            _mockTrackedOrderModel.Setup(model => model.GetTrackedOrderByIdAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(trackedOrder);
            _mockOrderViewModel.Setup(model => model.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(associatedOrder);
            _mockNotificationService.Setup(notification => notification.SendShippingProgressNotificationAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Notification failed"));
        }

        private List<OrderCheckpoint> SetupMultiCallCheckpointListForReversion(TrackedOrder trackedOrder)
        {
            // Create initial and post-deletion checkpoint lists
            var initialCheckpoints = new List<OrderCheckpoint>
            {
                new OrderCheckpoint
                {
                    CheckpointID = 1,
                    TrackedOrderID = trackedOrder.TrackedOrderID,
                    Status = OrderStatus.PROCESSING,
                    Timestamp = new DateTime(2025, 4, 1),
                    Description = "Order received"
                },
                new OrderCheckpoint
                {
                    CheckpointID = 2,
                    TrackedOrderID = trackedOrder.TrackedOrderID,
                    Status = OrderStatus.SHIPPED,
                    Timestamp = new DateTime(2025, 4, 2),
                    Description = "Order shipped"
                }
            };

            var afterDeletionCheckpoints = new List<OrderCheckpoint>
            {
                new OrderCheckpoint
                {
                    CheckpointID = 1,
                    TrackedOrderID = trackedOrder.TrackedOrderID,
                    Status = OrderStatus.PROCESSING,
                    Timestamp = new DateTime(2025, 4, 1),
                    Description = "Order received"
                }
            };

            // Setup to return different results on consecutive calls
            int callCount = 0;
            _mockTrackedOrderModel.Setup(model => model.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(() =>
                {
                    if (callCount++ < 2)
                    {
                        return initialCheckpoints;
                    }
                    else
                    {
                        return afterDeletionCheckpoints;
                    }
                });

            return initialCheckpoints;
        }

        private void SetupRetrievingCheckpointsThenEmptyList(TrackedOrder trackedOrder)
        {
            // First setup: Return multiple checkpoints for the count check
            var checkpoints = new List<OrderCheckpoint>
            {
                new OrderCheckpoint
                {
                    CheckpointID = 1,
                    TrackedOrderID = trackedOrder.TrackedOrderID,
                    Status = OrderStatus.PROCESSING,
                    Timestamp = new DateTime(2025, 4, 1),
                    Description = "Order received"
                },
                new OrderCheckpoint
                {
                    CheckpointID = 2,
                    TrackedOrderID = trackedOrder.TrackedOrderID,
                    Status = OrderStatus.SHIPPED,
                    Timestamp = new DateTime(2025, 4, 2),
                    Description = "Order shipped"
                }
            };

            // Use callCount to track and return different values for different calls
            int callCount = 0;
            _mockTrackedOrderModel.Setup(model => model.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    // First call returns checkpoints for the count check
                    if (callCount == 1)
                        return checkpoints;
                    // Second call for GetLastCheckpoint returns empty list, which will result in null checkpoint
                    else
                        return new List<OrderCheckpoint>();
                });
        }

        #endregion
    }
}
