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
        private const string ConnectionString = "test_connection_string";

        [TestInitialize]
        public void TestInitialize()
        {
            // Create mock dependencies
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
        public void Constructor_WithConnectionString_InitializesViewModel()
        {
            // Act
            var viewModel = new TrackedOrderViewModel(ConnectionString);

            // Assert
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        public void Constructor_WithDependencies_InitializesViewModel()
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
        public void Constructor_WithNullTrackedOrderModel_ThrowsArgumentNullException()
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
        public void Constructor_WithNullOrderViewModel_ThrowsArgumentNullException()
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
        public void Constructor_WithNullNotificationService_ThrowsArgumentNullException()
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
        public async Task GetTrackedOrderByIDAsync_ValidID_ReturnsTrackedOrder()
        {
            // Arrange
            int trackedOrderId = 123;
            var expectedOrder = new TrackedOrder
            {
                TrackedOrderID = trackedOrderId,
                OrderID = 456,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St, Test City"
            };

            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(trackedOrderId))
                .ReturnsAsync(expectedOrder);

            // Act
            var result = await _viewModel.GetTrackedOrderByIDAsync(trackedOrderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(trackedOrderId, result.TrackedOrderID);
            Assert.AreEqual(expectedOrder.OrderID, result.OrderID);
            Assert.AreEqual(expectedOrder.CurrentStatus, result.CurrentStatus);
            _mockTrackedOrderModel.Verify(m => m.GetTrackedOrderByIdAsync(trackedOrderId), Times.Once);
        }

        [TestMethod]
        public async Task GetTrackedOrderByIDAsync_ModelThrowsException_ReturnsNull()
        {
            // Arrange
            int trackedOrderId = 999;
            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(trackedOrderId))
                .ThrowsAsync(new Exception("Not found"));

            // Act
            var result = await _viewModel.GetTrackedOrderByIDAsync(trackedOrderId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetOrderCheckpointByIDAsync_ValidID_ReturnsOrderCheckpoint()
        {
            // Arrange
            int checkpointId = 123;
            var expectedCheckpoint = new OrderCheckpoint
            {
                CheckpointID = checkpointId,
                TrackedOrderID = 456,
                Status = OrderStatus.SHIPPED,
                Timestamp = new DateTime(2025, 4, 1),
                Description = "Order shipped"
            };

            _mockTrackedOrderModel.Setup(m => m.GetOrderCheckpointByIdAsync(checkpointId))
                .ReturnsAsync(expectedCheckpoint);

            // Act
            var result = await _viewModel.GetOrderCheckpointByIDAsync(checkpointId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(checkpointId, result.CheckpointID);
            Assert.AreEqual(expectedCheckpoint.TrackedOrderID, result.TrackedOrderID);
            Assert.AreEqual(expectedCheckpoint.Status, result.Status);
            _mockTrackedOrderModel.Verify(m => m.GetOrderCheckpointByIdAsync(checkpointId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderCheckpointByIDAsync_ModelThrowsException_ReturnsNull()
        {
            // Arrange
            int checkpointId = 999;
            _mockTrackedOrderModel.Setup(m => m.GetOrderCheckpointByIdAsync(checkpointId))
                .ThrowsAsync(new Exception("Not found"));

            // Act
            var result = await _viewModel.GetOrderCheckpointByIDAsync(checkpointId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAllTrackedOrdersAsync_ReturnsTrackedOrdersList()
        {
            // Arrange
            var expectedOrders = new List<TrackedOrder>
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

            _mockTrackedOrderModel.Setup(m => m.GetAllTrackedOrdersAsync())
                .ReturnsAsync(expectedOrders);

            // Act
            var result = await _viewModel.GetAllTrackedOrdersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].TrackedOrderID);
            Assert.AreEqual(2, result[1].TrackedOrderID);
            _mockTrackedOrderModel.Verify(m => m.GetAllTrackedOrdersAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAllOrderCheckpointsAsync_ValidTrackedOrderID_ReturnsCheckpointsList()
        {
            // Arrange
            int trackedOrderId = 123;
            var expectedCheckpoints = new List<OrderCheckpoint>
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

            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrderId))
                .ReturnsAsync(expectedCheckpoints);

            // Act
            var result = await _viewModel.GetAllOrderCheckpointsAsync(trackedOrderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].CheckpointID);
            Assert.AreEqual(2, result[1].CheckpointID);
            _mockTrackedOrderModel.Verify(m => m.GetAllOrderCheckpointsAsync(trackedOrderId), Times.Once);
        }

        [TestMethod]
        public async Task DeleteTrackedOrderAsync_ValidID_ReturnsSuccessResult()
        {
            // Arrange
            int trackedOrderId = 123;
            _mockTrackedOrderModel.Setup(m => m.DeleteTrackedOrderAsync(trackedOrderId))
                .ReturnsAsync(true);

            // Act
            var result = await _viewModel.DeleteTrackedOrderAsync(trackedOrderId);

            // Assert
            Assert.IsTrue(result);
            _mockTrackedOrderModel.Verify(m => m.DeleteTrackedOrderAsync(trackedOrderId), Times.Once);
        }

        [TestMethod]
        public async Task DeleteOrderCheckpointAsync_ValidID_ReturnsSuccessResult()
        {
            // Arrange
            int checkpointId = 123;
            _mockTrackedOrderModel.Setup(m => m.DeleteOrderCheckpointAsync(checkpointId))
                .ReturnsAsync(true);

            // Act
            var result = await _viewModel.DeleteOrderCheckpointAsync(checkpointId);

            // Assert
            Assert.IsTrue(result);
            _mockTrackedOrderModel.Verify(m => m.DeleteOrderCheckpointAsync(checkpointId), Times.Once);
        }

        [TestMethod]
        public async Task AddTrackedOrderAsync_ValidOrder_AddsOrderAndSendsNotification()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                OrderID = 456,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            int newTrackedOrderId = 123;
            var order = new Order
            {
                OrderID = 456,
                BuyerID = 789
            };

            _mockTrackedOrderModel.Setup(m => m.AddTrackedOrderAsync(trackedOrder))
                .ReturnsAsync(newTrackedOrderId);
            _mockOrderViewModel.Setup(m => m.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(order);

            // Act
            var result = await _viewModel.AddTrackedOrderAsync(trackedOrder);

            // Assert
            Assert.AreEqual(newTrackedOrderId, result);
            _mockTrackedOrderModel.Verify(m => m.AddTrackedOrderAsync(trackedOrder), Times.Once);
            _mockOrderViewModel.Verify(m => m.GetOrderByIdAsync(trackedOrder.OrderID), Times.Once);
            _mockNotificationService.Verify(n => n.SendShippingProgressNotificationAsync(
                order.BuyerID,
                newTrackedOrderId,
                trackedOrder.CurrentStatus.ToString(),
                It.IsAny<DateTime>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task AddTrackedOrderAsync_NotificationFails_StillReturnsNewId()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                OrderID = 456,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            int newTrackedOrderId = 123;
            var order = new Order
            {
                OrderID = 456,
                BuyerID = 789
            };

            _mockTrackedOrderModel.Setup(m => m.AddTrackedOrderAsync(trackedOrder))
                .ReturnsAsync(newTrackedOrderId);
            _mockOrderViewModel.Setup(m => m.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(order);
            _mockNotificationService.Setup(n => n.SendShippingProgressNotificationAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()
            )).ThrowsAsync(new Exception("Notification failed"));

            // Act
            var result = await _viewModel.AddTrackedOrderAsync(trackedOrder);

            // Assert
            Assert.AreEqual(newTrackedOrderId, result);
            _mockTrackedOrderModel.Verify(m => m.AddTrackedOrderAsync(trackedOrder), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AddTrackedOrderAsync_ModelThrowsException_PropagatesException()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                OrderID = 456,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            _mockTrackedOrderModel.Setup(m => m.AddTrackedOrderAsync(trackedOrder))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _viewModel.AddTrackedOrderAsync(trackedOrder);

            // Assert - handled by ExpectedException
        }



        [TestMethod]
        public async Task UpdateOrderCheckpointAsync_ValidParameters_UpdatesCheckpointAndTrackedOrder()
        {
            // Arrange
            int checkpointId = 123;
            DateTime timestamp = new DateTime(2025, 4, 1);
            string location = "Warehouse A";
            string description = "Order in warehouse";
            OrderStatus status = OrderStatus.IN_WAREHOUSE;

            var checkpoint = new OrderCheckpoint
            {
                CheckpointID = checkpointId,
                TrackedOrderID = 456,
                Status = status,
                Timestamp = timestamp,
                Location = location,
                Description = description
            };

            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 456,
                OrderID = 789,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            _mockTrackedOrderModel.Setup(m => m.GetOrderCheckpointByIdAsync(checkpointId))
                .ReturnsAsync(checkpoint);
            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(checkpoint.TrackedOrderID))
                .ReturnsAsync(trackedOrder);

            // Act
            await _viewModel.UpdateOrderCheckpointAsync(checkpointId, timestamp, location, description, status);

            // Assert
            _mockTrackedOrderModel.Verify(m => m.UpdateOrderCheckpointAsync(
                checkpointId, timestamp, location, description, status
            ), Times.Once);
            _mockTrackedOrderModel.Verify(m => m.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                status
            ), Times.Once);
        }

        [TestMethod]
        public async Task UpdateOrderCheckpointAsync_WithTrackedOrderID_UpdatesCheckpoint()
        {
            // Arrange
            int checkpointId = 123;
            int trackedOrderId = 456;
            DateTime timestamp = new DateTime(2025, 4, 1);
            string location = "Warehouse A";
            string description = "Order in warehouse";
            OrderStatus status = OrderStatus.IN_WAREHOUSE;

            var checkpoint = new OrderCheckpoint
            {
                CheckpointID = checkpointId,
                TrackedOrderID = trackedOrderId,
                Status = status,
                Timestamp = timestamp,
                Location = location,
                Description = description
            };

            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = trackedOrderId,
                OrderID = 789,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            _mockTrackedOrderModel.Setup(m => m.GetOrderCheckpointByIdAsync(checkpointId))
                .ReturnsAsync(checkpoint);
            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(trackedOrderId))
                .ReturnsAsync(trackedOrder);

            // Act
            bool result = await _viewModel.UpdateOrderCheckpointAsync(checkpointId, timestamp, location, description, status, trackedOrderId);

            // Assert
            Assert.IsTrue(result);
            _mockTrackedOrderModel.Verify(m => m.UpdateOrderCheckpointAsync(
                checkpointId, timestamp, location, description, status
            ), Times.Once);
        }

        [TestMethod]
        public async Task UpdateOrderCheckpointAsync_WithTrackedOrderID_TrackedOrderIdMismatch_ReturnsFalse()
        {
            // Arrange
            int checkpointId = 123;
            int trackedOrderId = 456;
            int wrongTrackedOrderId = 789;
            DateTime timestamp = new DateTime(2025, 4, 1);
            string location = "Warehouse A";
            string description = "Order in warehouse";
            OrderStatus status = OrderStatus.IN_WAREHOUSE;

            var checkpoint = new OrderCheckpoint
            {
                CheckpointID = checkpointId,
                TrackedOrderID = trackedOrderId, // This is different than the one passed to the method
                Status = status,
                Timestamp = timestamp,
                Location = location,
                Description = description
            };

            _mockTrackedOrderModel.Setup(m => m.GetOrderCheckpointByIdAsync(checkpointId))
                .ReturnsAsync(checkpoint);

            // Act
            bool result = await _viewModel.UpdateOrderCheckpointAsync(checkpointId, timestamp, location, description, status, wrongTrackedOrderId);

            // Assert
            Assert.IsFalse(result);
            _mockTrackedOrderModel.Verify(m => m.UpdateOrderCheckpointAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OrderStatus>()
            ), Times.Never);
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_ShippedStatus_SendsNotification()
        {
            // Arrange
            int trackedOrderId = 123;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            OrderStatus status = OrderStatus.SHIPPED;

            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = trackedOrderId,
                OrderID = 456,
                CurrentStatus = status,
                EstimatedDeliveryDate = deliveryDate,
                DeliveryAddress = "123 Test St"
            };

            var order = new Order
            {
                OrderID = 456,
                BuyerID = 789
            };

            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(trackedOrderId))
                .ReturnsAsync(trackedOrder);
            _mockOrderViewModel.Setup(m => m.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(order);

            // Act
            await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, status);

            // Assert
            _mockTrackedOrderModel.Verify(m => m.UpdateTrackedOrderAsync(
                trackedOrderId, deliveryDate, status
            ), Times.Once);
            _mockNotificationService.Verify(n => n.SendShippingProgressNotificationAsync(
                order.BuyerID,
                trackedOrderId,
                status.ToString(),
                It.IsAny<DateTime>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_WithDeliveryAddress_UpdatesTrackedOrder()
        {
            // Arrange
            int trackedOrderId = 123;
            int orderId = 456;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            string deliveryAddress = "123 Test St, Updated";
            OrderStatus status = OrderStatus.SHIPPED;

            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = trackedOrderId,
                OrderID = orderId,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 4, 15),
                DeliveryAddress = "123 Test St"
            };

            var order = new Order
            {
                OrderID = orderId,
                BuyerID = 789
            };

            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(trackedOrderId))
                .ReturnsAsync(trackedOrder);
            _mockOrderViewModel.Setup(m => m.GetOrderByIdAsync(orderId))
                .ReturnsAsync(order);

            // Act
            bool result = await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, deliveryAddress, status, orderId);

            // Assert
            Assert.IsTrue(result);
            _mockTrackedOrderModel.Verify(m => m.UpdateTrackedOrderAsync(
                trackedOrderId, deliveryDate, status
            ), Times.Once);
            _mockNotificationService.Verify(n => n.SendShippingProgressNotificationAsync(
                order.BuyerID,
                trackedOrderId,
                status.ToString(),
                It.IsAny<DateTime>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_WithDeliveryAddress_OrderIdMismatch_ReturnsFalse()
        {
            // Arrange
            int trackedOrderId = 123;
            int orderId = 456;
            int wrongOrderId = 789;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            string deliveryAddress = "123 Test St, Updated";
            OrderStatus status = OrderStatus.SHIPPED;

            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = trackedOrderId,
                OrderID = orderId, // This is different than the one passed to the method
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 4, 15),
                DeliveryAddress = "123 Test St"
            };

            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(trackedOrderId))
                .ReturnsAsync(trackedOrder);

            // Act
            bool result = await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, deliveryAddress, status, wrongOrderId);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetLastCheckpoint_HasCheckpoints_ReturnsLastCheckpoint()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.SHIPPED,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            var checkpoints = new List<OrderCheckpoint>
            {
                new OrderCheckpoint
                {
                    CheckpointID = 1,
                    TrackedOrderID = 123,
                    Status = OrderStatus.PROCESSING,
                    Timestamp = new DateTime(2025, 4, 1),
                    Description = "Order received"
                },
                new OrderCheckpoint
                {
                    CheckpointID = 2,
                    TrackedOrderID = 123,
                    Status = OrderStatus.SHIPPED,
                    Timestamp = new DateTime(2025, 4, 2),
                    Description = "Order shipped"
                }
            };

            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(checkpoints);

            // Act
            var result = await _viewModel.GetLastCheckpoint(trackedOrder);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.CheckpointID);
            Assert.AreEqual(OrderStatus.SHIPPED, result.Status);
        }

        [TestMethod]
        public async Task GetLastCheckpoint_NoCheckpoints_ReturnsNull()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(new List<OrderCheckpoint>());

            // Act
            var result = await _viewModel.GetLastCheckpoint(trackedOrder);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetNumberOfCheckpoints_HasCheckpoints_ReturnsCount()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.SHIPPED,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            var checkpoints = new List<OrderCheckpoint>
            {
                new OrderCheckpoint
                {
                    CheckpointID = 1,
                    TrackedOrderID = 123,
                    Status = OrderStatus.PROCESSING,
                    Timestamp = new DateTime(2025, 4, 1),
                    Description = "Order received"
                },
                new OrderCheckpoint
                {
                    CheckpointID = 2,
                    TrackedOrderID = 123,
                    Status = OrderStatus.SHIPPED,
                    Timestamp = new DateTime(2025, 4, 2),
                    Description = "Order shipped"
                }
            };

            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(checkpoints);

            // Act
            var result = await _viewModel.GetNumberOfCheckpoints(trackedOrder);

            // Assert
            Assert.AreEqual(2, result);
        }



        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RevertToPreviousCheckpoint_NullOrder_ThrowsArgumentNullException()
        {
            // Act
            await _viewModel.RevertToPreviousCheckpoint(null);

            // Assert - handled by ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task RevertToPreviousCheckpoint_NoCheckpoints_ThrowsException()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(new List<OrderCheckpoint>());

            // Act
            await _viewModel.RevertToPreviousCheckpoint(trackedOrder);

            // Assert - handled by ExpectedException
        }



        [TestMethod]
        public async Task RevertToLastCheckpoint_NoCheckpoints_ReturnsFalse()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(new List<OrderCheckpoint>());

            // Act
            bool result = await _viewModel.RevertToLastCheckpoint(trackedOrder);

            // Assert
            Assert.IsFalse(result);
            _mockTrackedOrderModel.Verify(m => m.UpdateTrackedOrderAsync(
                It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<OrderStatus>()
            ), Times.Never);
        }

        [TestMethod]
        public async Task RevertToLastCheckpoint_NullOrder_ReturnsFalse()
        {
            // Act
            bool result = await _viewModel.RevertToLastCheckpoint(null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task AddOrderCheckpointAsync_WithShippedStatus_SendsNotification()
        {
            // Arrange
            var checkpoint = new OrderCheckpoint
            {
                TrackedOrderID = 123,
                Timestamp = DateTime.Now,
                Location = "Distribution Center",
                Description = "Package ready for shipping",
                Status = OrderStatus.SHIPPED
            };

            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            var order = new Order
            {
                OrderID = 456,
                BuyerID = 789
            };

            int newCheckpointId = 42;

            _mockTrackedOrderModel.Setup(m => m.AddOrderCheckpointAsync(checkpoint))
                .ReturnsAsync(newCheckpointId);
            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(checkpoint.TrackedOrderID))
                .ReturnsAsync(trackedOrder);
            _mockOrderViewModel.Setup(m => m.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(order);

            // Act
            var result = await _viewModel.AddOrderCheckpointAsync(checkpoint);

            // Assert
            Assert.AreEqual(newCheckpointId, result);
            _mockTrackedOrderModel.Verify(m => m.AddOrderCheckpointAsync(checkpoint), Times.Once);
            _mockTrackedOrderModel.Verify(m => m.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                checkpoint.Status), Times.Once);
            _mockNotificationService.Verify(n => n.SendShippingProgressNotificationAsync(
                order.BuyerID,
                trackedOrder.TrackedOrderID,
                trackedOrder.CurrentStatus.ToString(),
                It.IsAny<DateTime>()), Times.Once);
        }

        [TestMethod]
        public async Task AddOrderCheckpointAsync_WithRegularStatus_DoesNotSendNotification()
        {
            // Arrange
            var checkpoint = new OrderCheckpoint
            {
                TrackedOrderID = 123,
                Timestamp = DateTime.Now,
                Location = "Warehouse",
                Description = "Preparing for shipment",
                Status = OrderStatus.PROCESSING
            };

            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            int newCheckpointId = 42;

            _mockTrackedOrderModel.Setup(m => m.AddOrderCheckpointAsync(checkpoint))
                .ReturnsAsync(newCheckpointId);
            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(checkpoint.TrackedOrderID))
                .ReturnsAsync(trackedOrder);

            // Act
            var result = await _viewModel.AddOrderCheckpointAsync(checkpoint);

            // Assert
            Assert.AreEqual(newCheckpointId, result);
            _mockTrackedOrderModel.Verify(m => m.AddOrderCheckpointAsync(checkpoint), Times.Once);
            _mockOrderViewModel.Verify(m => m.GetOrderByIdAsync(It.IsAny<int>()), Times.Never);
            _mockNotificationService.Verify(n => n.SendShippingProgressNotificationAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AddOrderCheckpointAsync_ModelThrowsException_PropagatesException()
        {
            // Arrange
            var checkpoint = new OrderCheckpoint
            {
                TrackedOrderID = 123,
                Timestamp = DateTime.Now,
                Location = "Distribution Center",
                Description = "Package ready for shipping",
                Status = OrderStatus.SHIPPED
            };

            _mockTrackedOrderModel.Setup(m => m.AddOrderCheckpointAsync(checkpoint))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _viewModel.AddOrderCheckpointAsync(checkpoint);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task UpdateOrderCheckpointAsync_ModelThrowsException_PropagatesException()
        {
            // Arrange
            int checkpointId = 123;
            DateTime timestamp = new DateTime(2025, 4, 1);
            string location = "Warehouse A";
            string description = "Order in warehouse";
            OrderStatus status = OrderStatus.IN_WAREHOUSE;

            _mockTrackedOrderModel.Setup(m => m.UpdateOrderCheckpointAsync(checkpointId, timestamp, location, description, status))
                .ThrowsAsync(new Exception("Database error updating checkpoint"));

            // Act
            await _viewModel.UpdateOrderCheckpointAsync(checkpointId, timestamp, location, description, status);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public async Task UpdateOrderCheckpointAsync_WithTrackedOrderID_ModelThrowsException_ReturnsFalse()
        {
            // Arrange
            int checkpointId = 123;
            int trackedOrderId = 456;
            DateTime timestamp = new DateTime(2025, 4, 1);
            string location = "Warehouse A";
            string description = "Order in warehouse";
            OrderStatus status = OrderStatus.IN_WAREHOUSE;

            _mockTrackedOrderModel.Setup(m => m.GetOrderCheckpointByIdAsync(checkpointId))
                .ThrowsAsync(new Exception("Error retrieving checkpoint"));

            // Act
            bool result = await _viewModel.UpdateOrderCheckpointAsync(checkpointId, timestamp, location, description, status, trackedOrderId);

            // Assert
            Assert.IsFalse(result);
            _mockTrackedOrderModel.Verify(m => m.UpdateOrderCheckpointAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OrderStatus>()
            ), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task UpdateTrackedOrderAsync_ModelThrowsException_PropagatesException()
        {
            // Arrange
            int trackedOrderId = 123;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            OrderStatus status = OrderStatus.SHIPPED;

            _mockTrackedOrderModel.Setup(m => m.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, status))
                .ThrowsAsync(new Exception("Database error updating tracked order"));

            // Act
            await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, status);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_WithDeliveryAddress_ModelThrowsException_ReturnsFalse()
        {
            // Arrange
            int trackedOrderId = 123;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            string deliveryAddress = "123 Test St, Updated";
            OrderStatus status = OrderStatus.SHIPPED;
            int orderId = 456;

            _mockTrackedOrderModel.Setup(m => m.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, status))
                .ThrowsAsync(new Exception("Database error updating tracked order"));

            // Act
            bool result = await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, deliveryAddress, status, orderId);

            // Assert
            Assert.IsFalse(result);
            _mockOrderViewModel.Verify(m => m.GetOrderByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public async Task AddOrderCheckpointAsync_NotificationThrowsException_StillReturnsNewId()
        {
            // Arrange
            var checkpoint = new OrderCheckpoint
            {
                TrackedOrderID = 123,
                Timestamp = DateTime.Now,
                Location = "Distribution Center",
                Description = "Package ready for shipping",
                Status = OrderStatus.SHIPPED
            };

            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.SHIPPED,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            var order = new Order
            {
                OrderID = 456,
                BuyerID = 789
            };

            int newCheckpointId = 42;

            _mockTrackedOrderModel.Setup(m => m.AddOrderCheckpointAsync(checkpoint))
                .ReturnsAsync(newCheckpointId);
            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(checkpoint.TrackedOrderID))
                .ReturnsAsync(trackedOrder);
            _mockOrderViewModel.Setup(m => m.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(order);
            _mockNotificationService.Setup(n => n.SendShippingProgressNotificationAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Notification service error"));

            // Act
            var result = await _viewModel.AddOrderCheckpointAsync(checkpoint);

            // Assert
            Assert.AreEqual(newCheckpointId, result);
            _mockTrackedOrderModel.Verify(m => m.AddOrderCheckpointAsync(checkpoint), Times.Once);
            _mockTrackedOrderModel.Verify(m => m.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                checkpoint.Status), Times.Once);
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_NotificationThrowsException_StillSucceeds()
        {
            // Arrange
            int trackedOrderId = 123;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            OrderStatus status = OrderStatus.OUT_FOR_DELIVERY;

            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = trackedOrderId,
                OrderID = 456,
                CurrentStatus = status,
                EstimatedDeliveryDate = deliveryDate,
                DeliveryAddress = "123 Test St"
            };

            var order = new Order
            {
                OrderID = 456,
                BuyerID = 789
            };

            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(trackedOrderId))
                .ReturnsAsync(trackedOrder);
            _mockOrderViewModel.Setup(m => m.GetOrderByIdAsync(trackedOrder.OrderID))
                .ReturnsAsync(order);
            _mockNotificationService.Setup(n => n.SendShippingProgressNotificationAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Notification service error"));

            // Act & Assert - no exception should be thrown
            await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, status);

            _mockTrackedOrderModel.Verify(m => m.UpdateTrackedOrderAsync(
                trackedOrderId, deliveryDate, status), Times.Once);
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_WithDeliveryAddress_NotificationFailure_StillReturnsTrue()
        {
            // Arrange
            int trackedOrderId = 123;
            int orderId = 456;
            DateOnly deliveryDate = new DateOnly(2025, 5, 1);
            string deliveryAddress = "123 Test St, Updated";
            OrderStatus status = OrderStatus.SHIPPED;

            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = trackedOrderId,
                OrderID = orderId,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 4, 15),
                DeliveryAddress = "123 Test St"
            };

            var order = new Order
            {
                OrderID = orderId,
                BuyerID = 789
            };

            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(trackedOrderId))
                .ReturnsAsync(trackedOrder);
            _mockOrderViewModel.Setup(m => m.GetOrderByIdAsync(orderId))
                .ReturnsAsync(order);
            _mockNotificationService.Setup(n => n.SendShippingProgressNotificationAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Notification failed"));

            // Act
            bool result = await _viewModel.UpdateTrackedOrderAsync(trackedOrderId, deliveryDate, deliveryAddress, status, orderId);

            // Assert
            Assert.IsTrue(result);
            _mockTrackedOrderModel.Verify(m => m.UpdateTrackedOrderAsync(
                trackedOrderId, deliveryDate, status
            ), Times.Once);
            _mockNotificationService.Verify(n => n.SendShippingProgressNotificationAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()
            ), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task RevertToPreviousCheckpoint_CurrentCheckpointNotNull_DeletionFails_ThrowsException()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.SHIPPED,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            var checkpoints = new List<OrderCheckpoint>
    {
        new OrderCheckpoint
        {
            CheckpointID = 1,
            TrackedOrderID = 123,
            Status = OrderStatus.PROCESSING,
            Timestamp = new DateTime(2025, 4, 1),
            Description = "Order received"
        },
        new OrderCheckpoint
        {
            CheckpointID = 2,
            TrackedOrderID = 123,
            Status = OrderStatus.SHIPPED,
            Timestamp = new DateTime(2025, 4, 2),
            Description = "Order shipped"
        }
    };

            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(checkpoints);
            _mockTrackedOrderModel.Setup(m => m.DeleteOrderCheckpointAsync(It.IsAny<int>()))
                .ReturnsAsync(false); // This will cause the exception

            // Act
            await _viewModel.RevertToPreviousCheckpoint(trackedOrder);

            // Assert - handled by ExpectedException
        }

        [TestMethod]
        public async Task RevertToPreviousCheckpoint_CurrentCheckpointNotNull_DeletionSucceeds_UpdatesStatus()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.SHIPPED,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            // Create a list with two checkpoints
            var initialCheckpoints = new List<OrderCheckpoint>
    {
        new OrderCheckpoint
        {
            CheckpointID = 1,
            TrackedOrderID = 123,
            Status = OrderStatus.PROCESSING,
            Timestamp = new DateTime(2025, 4, 1),
            Description = "Order received"
        },
        new OrderCheckpoint
        {
            CheckpointID = 2,
            TrackedOrderID = 123,
            Status = OrderStatus.SHIPPED,
            Timestamp = new DateTime(2025, 4, 2),
            Description = "Order shipped"
        }
    };

            // Create a list with just the first checkpoint (after deletion)
            var afterDeletionCheckpoints = new List<OrderCheckpoint>
    {
        new OrderCheckpoint
        {
            CheckpointID = 1,
            TrackedOrderID = 123,
            Status = OrderStatus.PROCESSING,
            Timestamp = new DateTime(2025, 4, 1),
            Description = "Order received"
        }
    };

            // Setup to return different results on consecutive calls
            var checkpointListCallCounter = 0;
            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(() =>
                {
                    if (checkpointListCallCounter == 0)
                    {
                        checkpointListCallCounter++;
                        return initialCheckpoints; // First call for count
                    }
                    else if (checkpointListCallCounter == 1)
                    {
                        checkpointListCallCounter++;
                        return initialCheckpoints; // Second call to get current checkpoint
                    }
                    else
                    {
                        return afterDeletionCheckpoints; // Third call to get previous checkpoint
                    }
                });

            // THIS IS THE MISSING SETUP:
            _mockTrackedOrderModel.Setup(m => m.DeleteOrderCheckpointAsync(2))
                .ReturnsAsync(true);

            // THIS IS ALSO NEEDED TO PREVENT NULL REFERENCE IN UpdateTrackedOrderAsync METHOD:
            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(trackedOrder);

            // Act
            await _viewModel.RevertToPreviousCheckpoint(trackedOrder);

            // Assert
            _mockTrackedOrderModel.Verify(m => m.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                OrderStatus.PROCESSING // Should have reverted to the status of the previous checkpoint
            ), Times.Once);
            _mockTrackedOrderModel.Verify(m => m.DeleteOrderCheckpointAsync(2), Times.Once);
        }



        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task RevertToPreviousCheckpoint_CurrentCheckpointNull_ThrowsException()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.PROCESSING,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            // Return enough checkpoints to pass the count check, but then return null for the current checkpoint
            var checkpoints = new List<OrderCheckpoint>
    {
        new OrderCheckpoint
        {
            CheckpointID = 1,
            TrackedOrderID = 123,
            Status = OrderStatus.PROCESSING,
            Timestamp = new DateTime(2025, 4, 1),
            Description = "Order received"
        },
        new OrderCheckpoint
        {
            CheckpointID = 2,
            TrackedOrderID = 123,
            Status = OrderStatus.SHIPPED,
            Timestamp = new DateTime(2025, 4, 2),
            Description = "Order shipped"
        }
    };

            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(checkpoints);

            // This setup ensures that GetLastCheckpoint returns null, simulating no checkpoints found
            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(new List<OrderCheckpoint>());

            // Act
            await _viewModel.RevertToPreviousCheckpoint(trackedOrder);

            // Assert - handled by ExpectedException
        }

        [TestMethod]
        public async Task RevertToLastCheckpoint_HasLastCheckpoint_UpdatesStatusAndReturnsTrue()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.SHIPPED,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            var checkpoint = new OrderCheckpoint
            {
                CheckpointID = 1,
                TrackedOrderID = 123,
                Status = OrderStatus.PROCESSING,
                Timestamp = new DateTime(2025, 4, 1),
                Description = "Order received"
            };

            var checkpoints = new List<OrderCheckpoint> { checkpoint };

            // Setup to get the last checkpoint
            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(checkpoints);

            // Setup the update to succeed
            _mockTrackedOrderModel.Setup(m => m.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                checkpoint.Status))
                .Returns(Task.CompletedTask);

            // THIS IS NEEDED TO PREVENT NULL REFERENCE IN UpdateTrackedOrderAsync METHOD:
            _mockTrackedOrderModel.Setup(m => m.GetTrackedOrderByIdAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(trackedOrder);

            // Act
            bool result = await _viewModel.RevertToLastCheckpoint(trackedOrder);

            // Assert
            Assert.IsTrue(result);
            _mockTrackedOrderModel.Verify(m => m.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                checkpoint.Status
            ), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = false)]
        public async Task RevertToPreviousCheckpoint_HasEnoughCheckpoints_ButNullCurrentCheckpoint_ThrowsException()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.SHIPPED,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            // First setup: Return multiple checkpoints for the count check
            var checkpoints = new List<OrderCheckpoint>
    {
        new OrderCheckpoint
        {
            CheckpointID = 1,
            TrackedOrderID = 123,
            Status = OrderStatus.PROCESSING,
            Timestamp = new DateTime(2025, 4, 1),
            Description = "Order received"
        },
        new OrderCheckpoint
        {
            CheckpointID = 2,
            TrackedOrderID = 123,
            Status = OrderStatus.SHIPPED,
            Timestamp = new DateTime(2025, 4, 2),
            Description = "Order shipped"
        }
    };

            // Use callCount to track and return different values for different calls
            int callCount = 0;
            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
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

            // Act - this should throw "No checkpoints found to revert"
            await _viewModel.RevertToPreviousCheckpoint(trackedOrder);

            // Assert is handled by ExpectedException
        }


        [TestMethod]
        public async Task RevertToLastCheckpoint_UpdateThrowsException_ReturnsFalse()
        {
            // Arrange
            var trackedOrder = new TrackedOrder
            {
                TrackedOrderID = 123,
                OrderID = 456,
                CurrentStatus = OrderStatus.SHIPPED,
                EstimatedDeliveryDate = new DateOnly(2025, 5, 1),
                DeliveryAddress = "123 Test St"
            };

            var checkpoint = new OrderCheckpoint
            {
                CheckpointID = 1,
                TrackedOrderID = 123,
                Status = OrderStatus.PROCESSING,
                Timestamp = new DateTime(2025, 4, 1),
                Description = "Order received"
            };

            var checkpoints = new List<OrderCheckpoint> { checkpoint };

            _mockTrackedOrderModel.Setup(m => m.GetAllOrderCheckpointsAsync(trackedOrder.TrackedOrderID))
                .ReturnsAsync(checkpoints);

            _mockTrackedOrderModel.Setup(m => m.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                checkpoint.Status))
                .ThrowsAsync(new Exception("Update failed"));

            // Act
            bool result = await _viewModel.RevertToLastCheckpoint(trackedOrder);

            // Assert
            Assert.IsFalse(result);
            _mockTrackedOrderModel.Verify(m => m.UpdateTrackedOrderAsync(
                trackedOrder.TrackedOrderID,
                trackedOrder.EstimatedDeliveryDate,
                checkpoint.Status
            ), Times.Once);
        }
    }
}