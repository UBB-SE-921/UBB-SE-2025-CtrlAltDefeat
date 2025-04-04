using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;


namespace ArtAttack.Tests
{
    [TestClass]
    public class TrackedOrderModelTests
    {
        private Mock<IDatabaseProvider> _mockDatabaseProvider;
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataParameterCollection> _mockParameterCollection;
        private TrackedOrderModel _trackedOrderModel;
        private const string ConnectionString = "test_connection_string";

        [TestInitialize]
        public void TestInitialize()
        {
            // Setup mock objects
            _mockDatabaseProvider = new Mock<IDatabaseProvider>();
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockParameterCollection = new Mock<IDataParameterCollection>();

            // Setup parameter collection
            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameterCollection.Object);

            // Setup connection and command creation
            _mockDatabaseProvider.Setup(p => p.CreateConnection(It.IsAny<string>())).Returns(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

            // Create the tracked order model with mocked database provider
            _trackedOrderModel = new TrackedOrderModel(ConnectionString, _mockDatabaseProvider.Object);
        }

        //[TestMethod]
        //public async Task AddTrackedOrderAsync_ValidParameters_ExecutesStoredProcedure()
        //{
        //    // Arrange
        //    var order = new TrackedOrder
        //    {
        //        OrderID = 123,
        //        EstimatedDeliveryDate = new DateOnly(2025, 4, 15),
        //        DeliveryAddress = "123 Test St, Test City",
        //        CurrentStatus = OrderStatus.PROCESSING
        //    };

        //    // Setup parameter creation
        //    Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
        //    _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

        //    // Setup output parameter
        //    var outputParam = new SqlParameter("@newTrackedOrderID", SqlDbType.Int) { Value = 456 };
        //    _mockCommand.Setup(c => c.Parameters["@newTrackedOrderID"]).Returns(outputParam);

        //    // Mock the behavior of OpenAsync
        //    _mockConnection.Setup(c => c.Open());

        //    // Mock ExecuteNonQueryAsync
        //    _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

        //    // Act
        //    int result = await _trackedOrderModel.AddTrackedOrderAsync(order);

        //    // Assert
        //    Assert.AreEqual(456, result);
        //    _mockDatabaseProvider.Verify(p => p.CreateConnection(ConnectionString), Times.Once);
        //    _mockConnection.Verify(c => c.CreateCommand(), Times.Once);
        //    _mockConnection.Verify(c => c.Open(), Times.Once);
        //    _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

        //    // Verify command properties
        //    _mockCommand.VerifySet(c => c.CommandText = "uspInsertTrackedOrder");
        //    _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);

        //    // Verify parameters were created correctly
        //    _mockCommand.Verify(c => c.CreateParameter(), Times.Exactly(5));
        //    _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Exactly(5));
        //}

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AddTrackedOrderAsync_NegativeReturnValue_ThrowsException()
        {
            // Arrange
            var order = new TrackedOrder
            {
                OrderID = 123,
                EstimatedDeliveryDate = new DateOnly(2025, 4, 15),
                DeliveryAddress = "123 Test St, Test City",
                CurrentStatus = OrderStatus.PROCESSING
            };

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Setup output parameter with negative value
            var outputParam = new SqlParameter("@newTrackedOrderID", SqlDbType.Int) { Value = 0 };
            _mockCommand.Setup(c => c.Parameters["@newTrackedOrderID"]).Returns(outputParam);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Mock ExecuteNonQueryAsync
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            await _trackedOrderModel.AddTrackedOrderAsync(order);

            // Assert - Exception is expected
        }

        //[TestMethod]
        //public async Task AddOrderCheckpointAsync_ValidParameters_ExecutesStoredProcedure()
        //{
        //    // Arrange
        //    var checkpoint = new OrderCheckpoint
        //    {
        //        TrackedOrderID = 123,
        //        Timestamp = new DateTime(2025, 4, 10, 12, 0, 0),
        //        Location = "Distribution Center",
        //        Description = "Order processed",
        //        Status = OrderStatus.PROCESSING
        //    };

        //    // Setup parameter creation
        //    Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
        //    _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

        //    // Setup output parameter
        //    var outputParam = new SqlParameter("@newCheckpointID", SqlDbType.Int) { Value = 789 };
        //    _mockCommand.Setup(c => c.Parameters["@newCheckpointID"]).Returns(outputParam);

        //    // Mock the behavior of OpenAsync
        //    _mockConnection.Setup(c => c.Open());

        //    // Mock ExecuteNonQueryAsync
        //    _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

        //    // Act
        //    int result = await _trackedOrderModel.AddOrderCheckpointAsync(checkpoint);

        //    // Assert
        //    Assert.AreEqual(789, result);
        //    _mockDatabaseProvider.Verify(p => p.CreateConnection(ConnectionString), Times.Once);
        //    _mockConnection.Verify(c => c.CreateCommand(), Times.Once);
        //    _mockConnection.Verify(c => c.Open(), Times.Once);
        //    _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

        //    // Verify command properties
        //    _mockCommand.VerifySet(c => c.CommandText = "uspInsertOrderCheckpoint");
        //    _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);

        //    // Verify parameters were created correctly
        //    _mockCommand.Verify(c => c.CreateParameter(), Times.Exactly(6));
        //    _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Exactly(6));
        //}

        //[TestMethod]
        //public async Task AddOrderCheckpointAsync_NullLocation_HandlesNullValue()
        //{
        //    // Arrange
        //    var checkpoint = new OrderCheckpoint
        //    {
        //        TrackedOrderID = 123,
        //        Timestamp = new DateTime(2025, 4, 10, 12, 0, 0),
        //        Location = null,
        //        Description = "Order processed",
        //        Status = OrderStatus.PROCESSING
        //    };

        //    // Setup parameter creation
        //    Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
        //    _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

        //    // Setup output parameter
        //    var outputParam = new SqlParameter("@newCheckpointID", SqlDbType.Int) { Value = 789 };
        //    _mockCommand.Setup(c => c.Parameters["@newCheckpointID"]).Returns(outputParam);

        //    // Mock the behavior of OpenAsync
        //    _mockConnection.Setup(c => c.Open());

        //    // Mock ExecuteNonQuery
        //    _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

        //    // Option 1: Change the test to expect an exception
        //    // [ExpectedException(typeof(Exception))]

        //    // Option 2: Set a default non-null value instead of null
        //    checkpoint.Location = string.Empty; // or "Unknown"

        //    // Act
        //    int result = await _trackedOrderModel.AddOrderCheckpointAsync(checkpoint);

        //    // Assert
        //    Assert.AreEqual(789, result);
        //    _mockDatabaseProvider.Verify(p => p.CreateConnection(ConnectionString), Times.Once);
        //    _mockConnection.Verify(c => c.Open(), Times.Once);
        //    _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
        //}

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AddOrderCheckpointAsync_NegativeReturnValue_ThrowsException()
        {
            // Arrange
            var checkpoint = new OrderCheckpoint
            {
                TrackedOrderID = 123,
                Timestamp = new DateTime(2025, 4, 10, 12, 0, 0),
                Location = "Distribution Center",
                Description = "Order processed",
                Status = OrderStatus.PROCESSING
            };

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Setup output parameter with negative value
            var outputParam = new SqlParameter("@newCheckpointID", SqlDbType.Int) { Value = 0 };
            _mockCommand.Setup(c => c.Parameters["@newCheckpointID"]).Returns(outputParam);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Mock ExecuteNonQueryAsync
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            await _trackedOrderModel.AddOrderCheckpointAsync(checkpoint);

            // Assert - Exception is expected
        }

        [TestMethod]
        public async Task DeleteTrackedOrderAsync_ValidID_ReturnsTrue()
        {
            // Arrange
            int trackOrderID = 123;

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Mock ExecuteNonQueryAsync to return 1 (indicating success)
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            bool result = await _trackedOrderModel.DeleteTrackedOrderAsync(trackOrderID);

            // Assert
            Assert.IsTrue(result);
            _mockDatabaseProvider.Verify(p => p.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(c => c.CreateCommand(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Verify command properties
            _mockCommand.VerifySet(c => c.CommandText = "uspDeleteTrackedOrder");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);

            // Verify parameters were created correctly
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteTrackedOrderAsync_NoRowsAffected_ReturnsFalse()
        {
            // Arrange
            int trackOrderID = 123;

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Mock ExecuteNonQueryAsync to return 0 (indicating no rows affected)
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(0);

            // Act
            bool result = await _trackedOrderModel.DeleteTrackedOrderAsync(trackOrderID);

            // Assert
            Assert.IsFalse(result);
            _mockDatabaseProvider.Verify(p => p.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(c => c.CreateCommand(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }

        [TestMethod]
        public async Task DeleteOrderCheckpointAsync_ValidID_ReturnsTrue()
        {
            // Arrange
            int checkpointID = 456;

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Mock ExecuteNonQueryAsync to return 1 (indicating success)
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            bool result = await _trackedOrderModel.DeleteOrderCheckpointAsync(checkpointID);

            // Assert
            Assert.IsTrue(result);
            _mockDatabaseProvider.Verify(p => p.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(c => c.CreateCommand(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Verify command properties
            _mockCommand.VerifySet(c => c.CommandText = "uspDeleteOrderCheckpoint");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);

            // Verify parameters were created correctly
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public async Task GetAllTrackedOrdersAsync_ReturnsOrderList()
        {
            // Arrange
            SetupMockDataReaderForTrackedOrders();

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Act
            List<TrackedOrder> result = await _trackedOrderModel.GetAllTrackedOrdersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(101, result[0].TrackedOrderID);
            Assert.AreEqual(201, result[0].OrderID);
            Assert.AreEqual(OrderStatus.PROCESSING, result[0].CurrentStatus);

            _mockCommand.VerifySet(c => c.CommandText = "SELECT * FROM TrackedOrders");
        }

        [TestMethod]
        public async Task GetAllOrderCheckpointsAsync_ValidTrackedOrderID_ReturnsCheckpointList()
        {
            // Arrange
            int trackedOrderID = 101;
            SetupMockDataReaderForOrderCheckpoints();

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Act
            List<OrderCheckpoint> result = await _trackedOrderModel.GetAllOrderCheckpointsAsync(trackedOrderID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(201, result[0].CheckpointID);
            Assert.AreEqual("Distribution Center", result[0].Location);
            Assert.AreEqual("Order processed", result[0].Description);
            Assert.AreEqual(OrderStatus.PROCESSING, result[0].Status);

            _mockCommand.VerifySet(c => c.CommandText = "SELECT * FROM OrderCheckpoints WHERE TrackedOrderID = @trackedOrderID");
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public async Task GetTrackedOrderByIdAsync_ValidID_ReturnsTrackedOrder()
        {
            // Arrange
            int trackedOrderID = 101;
            SetupMockDataReaderForTrackedOrders();

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Act
            TrackedOrder result = await _trackedOrderModel.GetTrackedOrderByIdAsync(trackedOrderID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(101, result.TrackedOrderID);
            Assert.AreEqual(201, result.OrderID);
            Assert.AreEqual(OrderStatus.PROCESSING, result.CurrentStatus);

            _mockCommand.VerifySet(c => c.CommandText = "SELECT * FROM TrackedOrders WHERE TrackedOrderID = @trackedOrderID");
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetTrackedOrderByIdAsync_NonExistentID_ThrowsException()
        {
            // Arrange
            int trackedOrderID = 999;

            // Setup an empty data reader (no rows)
            Mock<IDataReader> mockDataReader = new Mock<IDataReader>();
            mockDataReader.Setup(r => r.Read()).Returns(false);

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Setup command to return the mock data reader
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(mockDataReader.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Act
            await _trackedOrderModel.GetTrackedOrderByIdAsync(trackedOrderID);

            // Assert - Exception is expected
        }

        [TestMethod]
        public async Task GetOrderCheckpointByIdAsync_ValidID_ReturnsOrderCheckpoint()
        {
            // Arrange
            int checkpointID = 201;
            SetupMockDataReaderForOrderCheckpoints();

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Act
            OrderCheckpoint result = await _trackedOrderModel.GetOrderCheckpointByIdAsync(checkpointID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.CheckpointID);
            Assert.AreEqual("Distribution Center", result.Location);
            Assert.AreEqual("Order processed", result.Description);
            Assert.AreEqual(OrderStatus.PROCESSING, result.Status);

            _mockCommand.VerifySet(c => c.CommandText = "SELECT * FROM OrderCheckpoints WHERE CheckpointID = @checkpointID");
            _mockCommand.Verify(c => c.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetOrderCheckpointByIdAsync_NonExistentID_ThrowsException()
        {
            // Arrange
            int checkpointID = 999;

            // Setup an empty data reader (no rows)
            Mock<IDataReader> mockDataReader = new Mock<IDataReader>();
            mockDataReader.Setup(r => r.Read()).Returns(false);

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Setup command to return the mock data reader
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(mockDataReader.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Act
            await _trackedOrderModel.GetOrderCheckpointByIdAsync(checkpointID);

            // Assert - Exception is expected
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_ValidParameters_ExecutesStoredProcedure()
        {
            // Arrange
            int trackedOrderID = 101;
            DateOnly estimatedDeliveryDate = new DateOnly(2025, 4, 20);
            OrderStatus currentStatus = OrderStatus.SHIPPED;

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Mock ExecuteNonQueryAsync
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            await _trackedOrderModel.UpdateTrackedOrderAsync(trackedOrderID, estimatedDeliveryDate, currentStatus);

            // Assert
            _mockDatabaseProvider.Verify(p => p.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(c => c.CreateCommand(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Verify command properties
            _mockCommand.VerifySet(c => c.CommandText = "uspUpdateTrackedOrder");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);

            // Verify parameters were created correctly
            _mockCommand.Verify(c => c.CreateParameter(), Times.Exactly(3));
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Exactly(3));
        }

        [TestMethod]
        public async Task UpdateOrderCheckpointAsync_ValidParameters_ExecutesStoredProcedure()
        {
            // Arrange
            int checkpointID = 201;
            DateTime timestamp = new DateTime(2025, 4, 15, 14, 30, 0);
            string location = "Regional Hub";
            string description = "Order in transit";
            OrderStatus status = OrderStatus.SHIPPED;

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Mock ExecuteNonQueryAsync
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            await _trackedOrderModel.UpdateOrderCheckpointAsync(checkpointID, timestamp, location, description, status);

            // Assert
            _mockDatabaseProvider.Verify(p => p.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(c => c.CreateCommand(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Verify command properties
            _mockCommand.VerifySet(c => c.CommandText = "uspUpdateOrderCheckpoint");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);

            // Verify parameters were created correctly
            _mockCommand.Verify(c => c.CreateParameter(), Times.Exactly(5));
            _mockParameterCollection.Verify(p => p.Add(It.IsAny<object>()), Times.Exactly(5));
        }

        [TestMethod]
        public async Task UpdateOrderCheckpointAsync_NullLocation_HandlesNullValue()
        {
            // Arrange
            int checkpointID = 201;
            DateTime timestamp = new DateTime(2025, 4, 15, 14, 30, 0);
            string location = null;
            string description = "Order in transit";
            OrderStatus status = OrderStatus.SHIPPED;

            // Setup parameter creation
            Mock<IDbDataParameter> mockParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(c => c.CreateParameter()).Returns(mockParameter.Object);

            // Mock the behavior of OpenAsync
            _mockConnection.Setup(c => c.Open());

            // Mock ExecuteNonQueryAsync
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            await _trackedOrderModel.UpdateOrderCheckpointAsync(checkpointID, timestamp, location, description, status);

            // Assert
            _mockDatabaseProvider.Verify(p => p.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(c => c.CreateCommand(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }

        #region Helper Methods

        private void SetupMockDataReaderForTrackedOrders()
        {
            // Create a mock data reader
            Mock<IDataReader> mockDataReader = new Mock<IDataReader>();

            // Setup Read behavior to return true once then false
            int callCount = 0;
            mockDataReader.Setup(r => r.Read()).Returns(() => callCount++ == 0);

            // Setup GetOrdinal for field indexes
            mockDataReader.Setup(r => r.GetOrdinal("TrackedOrderID")).Returns(0);
            mockDataReader.Setup(r => r.GetOrdinal("OrderID")).Returns(1);
            mockDataReader.Setup(r => r.GetOrdinal("OrderStatus")).Returns(2);
            mockDataReader.Setup(r => r.GetOrdinal("EstimatedDeliveryDate")).Returns(3);
            mockDataReader.Setup(r => r.GetOrdinal("DeliveryAddress")).Returns(4);

            // Setup field value retrieval
            mockDataReader.Setup(r => r.GetInt32(0)).Returns(101);
            mockDataReader.Setup(r => r.GetInt32(1)).Returns(201);
            mockDataReader.Setup(r => r.GetString(2)).Returns(OrderStatus.PROCESSING.ToString());
            mockDataReader.Setup(r => r.GetDateTime(3)).Returns(new DateTime(2025, 4, 15));
            mockDataReader.Setup(r => r.GetString(4)).Returns("123 Test St, Test City");

            // Setup for DateOnly conversion via GetFieldValue
            mockDataReader.Setup(r => r.GetDateTime(3)).Returns(new DateTime(2025, 4, 15));

            // Setup command to return the mock data reader
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(mockDataReader.Object);
        }

        private void SetupMockDataReaderForOrderCheckpoints()
        {
            // Create a mock data reader
            Mock<IDataReader> mockDataReader = new Mock<IDataReader>();

            // Setup Read behavior to return true once then false
            int callCount = 0;
            mockDataReader.Setup(r => r.Read()).Returns(() => callCount++ == 0);

            // Setup GetOrdinal for field indexes
            mockDataReader.Setup(r => r.GetOrdinal("CheckpointID")).Returns(0);
            mockDataReader.Setup(r => r.GetOrdinal("Timestamp")).Returns(1);
            mockDataReader.Setup(r => r.GetOrdinal("Location")).Returns(2);
            mockDataReader.Setup(r => r.GetOrdinal("Description")).Returns(3);
            mockDataReader.Setup(r => r.GetOrdinal("CheckpointStatus")).Returns(4);
            mockDataReader.Setup(r => r.GetOrdinal("TrackedOrderID")).Returns(5);

            // Setup IsDBNull for nullable fields
            mockDataReader.Setup(r => r.IsDBNull(2)).Returns(false);

            // Setup field value retrieval
            mockDataReader.Setup(r => r.GetInt32(0)).Returns(201);
            mockDataReader.Setup(r => r.GetDateTime(1)).Returns(new DateTime(2025, 4, 10, 12, 0, 0));
            mockDataReader.Setup(r => r.GetString(2)).Returns("Distribution Center");
            mockDataReader.Setup(r => r.GetString(3)).Returns("Order processed");
            mockDataReader.Setup(r => r.GetString(4)).Returns(OrderStatus.PROCESSING.ToString());
            mockDataReader.Setup(r => r.GetInt32(5)).Returns(101);

            // Setup command to return the mock data reader
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(mockDataReader.Object);
        }

        #endregion
    }
}