using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Threading.Tasks;


namespace ArtAttack.Tests.Model
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
            // Initialize mock objects
            _mockDatabaseProvider = new Mock<IDatabaseProvider>();
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockParameterCollection = new Mock<IDataParameterCollection>();

            // Setup the command’s parameter collection
            _mockCommand.Setup(command => command.Parameters).Returns(_mockParameterCollection.Object);

            // Setup connection and command creation
            _mockDatabaseProvider
                .Setup(provider => provider.CreateConnection(It.IsAny<string>()))
                .Returns(_mockConnection.Object);
            _mockConnection
                .Setup(connection => connection.CreateCommand())
                .Returns(_mockCommand.Object);

            // Create the TrackedOrderModel with the mocked database provider and connection string

            _trackedOrderModel = new TrackedOrderModel(ConnectionString, _mockDatabaseProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WhenNullConnectionString_ThrowsArgumentNullException_()
        {
            // Act
            var model = new TrackedOrderModel(null, _mockDatabaseProvider.Object);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WhenNullDatabaseProvider_ThrowsArgumentNullException_()
        {
            // Act
            var model = new TrackedOrderModel(ConnectionString, null);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AddTrackedOrderAsync_WhenNegativeReturnValue_ThrowsException_()
        {
            // Arrange
            var order = new TrackedOrder
            {
                OrderID = 123,
                EstimatedDeliveryDate = new DateOnly(2025, 4, 15),
                DeliveryAddress = "123 Test St, Test City",
                CurrentStatus = OrderStatus.PROCESSING
            };

            // Use descriptive variable name "dbParameter" for the created parameter
            Mock<IDbDataParameter> dbParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(command => command.CreateParameter()).Returns(dbParameter.Object);

            // Setup output parameter with an invalid value (0)
            var outputParameter = new SqlParameter("@newTrackedOrderID", SqlDbType.Int) { Value = 0 };
            _mockCommand.Setup(command => command.Parameters["@newTrackedOrderID"]).Returns(outputParameter);

            // Mock connection open and command execution
            _mockConnection.Setup(connection => connection.Open());
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);

            // Act
            await _trackedOrderModel.AddTrackedOrderAsync(order);

            // Assert - Exception is expected
        }

        [TestMethod]
        public async Task AddOrderCheckpointAsync_WhenSuccessfulExecution_ReturnsNewId_()

        {
            // Arrange
            var checkpoint = new OrderCheckpoint
            {
                TrackedOrderID = 123,
                Timestamp = new DateTime(2025, 4, 15, 12, 0, 0),
                Location = "Distribution Center",
                Description = "Order processed",
                Status = OrderStatus.PROCESSING
            };

            int expectedCheckpointId = 42;
            // Use a descriptive name for the output parameter
            Mock<IDbDataParameter> outputDbParameter = new Mock<IDbDataParameter>();
            outputDbParameter.Setup(parameter => parameter.ParameterName).Returns("@newCheckpointID");
            outputDbParameter.Setup(parameter => parameter.Value).Returns(expectedCheckpointId);

            // Setup sequence of parameter creation so that the last one is our output parameter.
            _mockCommand.SetupSequence(command => command.CreateParameter())

                .Returns(new Mock<IDbDataParameter>().Object)
                .Returns(new Mock<IDbDataParameter>().Object)
                .Returns(new Mock<IDbDataParameter>().Object)
                .Returns(new Mock<IDbDataParameter>().Object)
                .Returns(new Mock<IDbDataParameter>().Object)
                .Returns(outputDbParameter.Object);

            _mockCommand
                .Setup(command => command.Parameters["@newCheckpointID"])
                .Returns(outputDbParameter.Object);

            _mockConnection.Setup(connection => connection.Open());
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);


            // Act
            int result = await _trackedOrderModel.AddOrderCheckpointAsync(checkpoint);

            // Assert
            Assert.AreEqual(expectedCheckpointId, result);
            _mockDatabaseProvider.Verify(provider => provider.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
        }

        [TestMethod]
        public async Task AddTrackedOrderAsync_WhenSuccessfulExecution_ReturnsNewId_()

        {
            // Arrange
            var order = new TrackedOrder
            {
                OrderID = 123,
                EstimatedDeliveryDate = new DateOnly(2025, 4, 15),
                DeliveryAddress = "123 Test St, Test City",
                CurrentStatus = OrderStatus.PROCESSING
            };

            int expectedTrackedOrderId = 789;
            Mock<IDbDataParameter> outputDbParameter = new Mock<IDbDataParameter>();
            outputDbParameter.Setup(parameter => parameter.ParameterName).Returns("@newTrackedOrderID");
            outputDbParameter.Setup(parameter => parameter.Value).Returns(expectedTrackedOrderId);

            _mockCommand.SetupSequence(command => command.CreateParameter())

                .Returns(new Mock<IDbDataParameter>().Object)
                .Returns(new Mock<IDbDataParameter>().Object)
                .Returns(new Mock<IDbDataParameter>().Object)
                .Returns(new Mock<IDbDataParameter>().Object)
                .Returns(outputDbParameter.Object);

            _mockCommand.Setup(command => command.Parameters["@newTrackedOrderID"])
                .Returns(outputDbParameter.Object);

            _mockConnection.Setup(connection => connection.Open());
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);


            // Act
            int result = await _trackedOrderModel.AddTrackedOrderAsync(order);

            // Assert
            Assert.AreEqual(expectedTrackedOrderId, result);
            _mockDatabaseProvider.Verify(provider => provider.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AddOrderCheckpointAsync_WhenNegativeReturnValue_ThrowsException_()

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

            // Use "dbParameter" as descriptive name
            Mock<IDbDataParameter> dbParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(command => command.CreateParameter()).Returns(dbParameter.Object);

            var outputParameter = new SqlParameter("@newCheckpointID", SqlDbType.Int) { Value = 0 };
            _mockCommand.Setup(command => command.Parameters["@newCheckpointID"]).Returns(outputParameter);

            _mockConnection.Setup(connection => connection.Open());
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);

            // Act
            await _trackedOrderModel.AddOrderCheckpointAsync(checkpoint);

            // Assert - Exception is expected
        }

        [TestMethod]
        public async Task DeleteTrackedOrderAsync_WhenValidID_ReturnsTrue_()
        {
            // Arrange
            int trackOrderID = 123;
            Mock<IDbDataParameter> dbParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(command => command.CreateParameter()).Returns(dbParameter.Object);

            _mockConnection.Setup(connection => connection.Open());
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);


            // Act
            bool result = await _trackedOrderModel.DeleteTrackedOrderAsync(trackOrderID);

            // Assert
            Assert.IsTrue(result);
            _mockDatabaseProvider.Verify(provider => provider.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(connection => connection.CreateCommand(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            _mockCommand.VerifySet(command => command.CommandText = "uspDeleteTrackedOrder");
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(command => command.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(collection => collection.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteTrackedOrderAsync_WhenNoRowsAffected_ReturnsFalse_()
        {
            // Arrange
            int trackOrderID = 123;
            Mock<IDbDataParameter> dbParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(command => command.CreateParameter()).Returns(dbParameter.Object);

            _mockConnection.Setup(connection => connection.Open());
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(0);


            // Act
            bool result = await _trackedOrderModel.DeleteTrackedOrderAsync(trackOrderID);

            // Assert
            Assert.IsFalse(result);
            _mockDatabaseProvider.Verify(provider => provider.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(connection => connection.CreateCommand(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

        }

        [TestMethod]
        public async Task GetAllTrackedOrdersAsync_ReturnsOrderList()
        {
            // Arrange
            SetupMockDataReaderForTrackedOrders();
            _mockConnection.Setup(connection => connection.Open());


            // Act
            List<TrackedOrder> result = await _trackedOrderModel.GetAllTrackedOrdersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(101, result[0].TrackedOrderID);
            Assert.AreEqual(201, result[0].OrderID);
            Assert.AreEqual(OrderStatus.PROCESSING, result[0].CurrentStatus);

            _mockCommand.VerifySet(command => command.CommandText = "SELECT * FROM TrackedOrders");
        }

        [TestMethod]
        public async Task GetAllOrderCheckpointsAsync_WhenValidTrackedOrderID_ReturnsCheckpointList_()

        {
            // Arrange
            int trackedOrderID = 101;
            SetupMockDataReaderForOrderCheckpoints();

            Mock<IDbDataParameter> dbParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(command => command.CreateParameter()).Returns(dbParameter.Object);

            _mockConnection.Setup(connection => connection.Open());


            // Act
            List<OrderCheckpoint> result = await _trackedOrderModel.GetAllOrderCheckpointsAsync(trackedOrderID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(201, result[0].CheckpointID);
            Assert.AreEqual("Distribution Center", result[0].Location);
            Assert.AreEqual("Order processed", result[0].Description);
            Assert.AreEqual(OrderStatus.PROCESSING, result[0].Status);

            _mockCommand.VerifySet(command => command.CommandText = "SELECT * FROM OrderCheckpoints WHERE TrackedOrderID = @trackedOrderID");
            _mockCommand.Verify(command => command.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(collection => collection.Add(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public async Task GetTrackedOrderByIdAsync_WhenValidID_ReturnsTrackedOrder_()

        {
            // Arrange
            int trackedOrderID = 101;
            SetupMockDataReaderForTrackedOrders();

            Mock<IDbDataParameter> dbParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(command => command.CreateParameter()).Returns(dbParameter.Object);

            _mockConnection.Setup(connection => connection.Open());


            // Act
            TrackedOrder result = await _trackedOrderModel.GetTrackedOrderByIdAsync(trackedOrderID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(101, result.TrackedOrderID);
            Assert.AreEqual(201, result.OrderID);
            Assert.AreEqual(OrderStatus.PROCESSING, result.CurrentStatus);

            _mockCommand.VerifySet(command => command.CommandText = "SELECT * FROM TrackedOrders WHERE TrackedOrderID = @trackedOrderID");
            _mockCommand.Verify(command => command.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(collection => collection.Add(It.IsAny<object>()), Times.Once);

        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetTrackedOrderByIdAsync_WhenNonExistentID_ThrowsException_()
        {
            // Arrange
            int trackedOrderID = 999;
            var mockDataReader = new Mock<IDataReader>();
            mockDataReader.Setup(reader => reader.Read()).Returns(false);

            Mock<IDbDataParameter> dbParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(command => command.CreateParameter()).Returns(dbParameter.Object);
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(mockDataReader.Object);
            _mockConnection.Setup(connection => connection.Open());

            // Act
            await _trackedOrderModel.GetTrackedOrderByIdAsync(trackedOrderID);

            // Assert - Exception is expected
        }

        [TestMethod]
        public async Task GetOrderCheckpointByIdAsync_WhenValidID_ReturnsOrderCheckpoint_()

        {
            // Arrange
            int checkpointID = 201;
            SetupMockDataReaderForOrderCheckpoints();

            Mock<IDbDataParameter> dbParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(command => command.CreateParameter()).Returns(dbParameter.Object);

            _mockConnection.Setup(connection => connection.Open());


            // Act
            OrderCheckpoint result = await _trackedOrderModel.GetOrderCheckpointByIdAsync(checkpointID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.CheckpointID);
            Assert.AreEqual("Distribution Center", result.Location);
            Assert.AreEqual("Order processed", result.Description);
            Assert.AreEqual(OrderStatus.PROCESSING, result.Status);

            _mockCommand.VerifySet(command => command.CommandText = "SELECT * FROM OrderCheckpoints WHERE CheckpointID = @checkpointID");
            _mockCommand.Verify(command => command.CreateParameter(), Times.Once);
            _mockParameterCollection.Verify(collection => collection.Add(It.IsAny<object>()), Times.Once);

        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetOrderCheckpointByIdAsync_WhenNonExistentID_ThrowsException_()
        {
            // Arrange
            int checkpointID = 999;
            var mockDataReader = new Mock<IDataReader>();
            mockDataReader.Setup(reader => reader.Read()).Returns(false);

            Mock<IDbDataParameter> dbParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(command => command.CreateParameter()).Returns(dbParameter.Object);
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(mockDataReader.Object);
            _mockConnection.Setup(connection => connection.Open());

            // Act
            await _trackedOrderModel.GetOrderCheckpointByIdAsync(checkpointID);

            // Assert - Exception is expected
        }

        [TestMethod]
        public async Task UpdateTrackedOrderAsync_WhenValidParameters_ExecutesStoredProcedure_()

        {
            // Arrange
            int trackedOrderID = 101;
            DateOnly estimatedDeliveryDate = new DateOnly(2025, 4, 20);
            OrderStatus currentStatus = OrderStatus.SHIPPED;

            Mock<IDbDataParameter> dbParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(command => command.CreateParameter()).Returns(dbParameter.Object);
            _mockConnection.Setup(connection => connection.Open());
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);


            // Act
            await _trackedOrderModel.UpdateTrackedOrderAsync(trackedOrderID, estimatedDeliveryDate, currentStatus);

            // Assert
            _mockDatabaseProvider.Verify(provider => provider.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(connection => connection.CreateCommand(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            _mockCommand.VerifySet(command => command.CommandText = "uspUpdateTrackedOrder");
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(command => command.CreateParameter(), Times.Exactly(3));
            _mockParameterCollection.Verify(collection => collection.Add(It.IsAny<object>()), Times.Exactly(3));
        }

        [TestMethod]
        public async Task UpdateOrderCheckpointAsync_WhenValidParameters_ExecutesStoredProcedure_()

        {
            // Arrange
            int checkpointID = 201;
            DateTime timestamp = new DateTime(2025, 4, 15, 14, 30, 0);
            string location = "Regional Hub";
            string description = "Order in transit";
            OrderStatus status = OrderStatus.SHIPPED;

            Mock<IDbDataParameter> dbParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(command => command.CreateParameter()).Returns(dbParameter.Object);
            _mockConnection.Setup(connection => connection.Open());
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);


            // Act
            await _trackedOrderModel.UpdateOrderCheckpointAsync(checkpointID, timestamp, location, description, status);

            // Assert
            _mockDatabaseProvider.Verify(provider => provider.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(connection => connection.CreateCommand(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            _mockCommand.VerifySet(command => command.CommandText = "uspUpdateOrderCheckpoint");
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(command => command.CreateParameter(), Times.Exactly(5));
            _mockParameterCollection.Verify(collection => collection.Add(It.IsAny<object>()), Times.Exactly(5));
        }

        [TestMethod]
        public async Task UpdateOrderCheckpointAsync_WhenNullLocation_HandlesNullValue_()

        {
            // Arrange
            int checkpointID = 201;
            DateTime timestamp = new DateTime(2025, 4, 15, 14, 30, 0);
            string location = null;
            string description = "Order in transit";
            OrderStatus status = OrderStatus.SHIPPED;

            Mock<IDbDataParameter> dbParameter = new Mock<IDbDataParameter>();
            _mockCommand.Setup(command => command.CreateParameter()).Returns(dbParameter.Object);
            _mockConnection.Setup(connection => connection.Open());
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);


            // Act
            await _trackedOrderModel.UpdateOrderCheckpointAsync(checkpointID, timestamp, location, description, status);

            // Assert
            _mockDatabaseProvider.Verify(provider => provider.CreateConnection(ConnectionString), Times.Once);
            _mockConnection.Verify(connection => connection.CreateCommand(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
        }


        private void SetupMockDataReaderForTrackedOrders()
        {
            // Create a mock data reader that returns one tracked order record.
            var mockDataReader = new Mock<IDataReader>();
            int readCallCount = 0;
            mockDataReader.Setup(reader => reader.Read()).Returns(() => readCallCount++ == 0);

            // Setup column ordinal mapping.
            mockDataReader.Setup(reader => reader.GetOrdinal("TrackedOrderID")).Returns(0);
            mockDataReader.Setup(reader => reader.GetOrdinal("OrderID")).Returns(1);
            mockDataReader.Setup(reader => reader.GetOrdinal("OrderStatus")).Returns(2);
            mockDataReader.Setup(reader => reader.GetOrdinal("EstimatedDeliveryDate")).Returns(3);
            mockDataReader.Setup(reader => reader.GetOrdinal("DeliveryAddress")).Returns(4);

            // Setup field value retrieval.
            mockDataReader.Setup(reader => reader.GetInt32(0)).Returns(101);
            mockDataReader.Setup(reader => reader.GetInt32(1)).Returns(201);
            mockDataReader.Setup(reader => reader.GetString(2)).Returns(OrderStatus.PROCESSING.ToString());
            mockDataReader.Setup(reader => reader.GetDateTime(3)).Returns(new DateTime(2025, 4, 15));
            mockDataReader.Setup(reader => reader.GetString(4)).Returns("123 Test St, Test City");

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(mockDataReader.Object);

        }

        private void SetupMockDataReaderForOrderCheckpoints()
        {
            // Create a mock data reader that returns one order checkpoint record.
            var mockDataReader = new Mock<IDataReader>();
            int callCount = 0;
            mockDataReader.Setup(reader => reader.Read()).Returns(() => callCount++ == 0);

            // Setup column ordinal mapping.
            mockDataReader.Setup(reader => reader.GetOrdinal("CheckpointID")).Returns(0);
            mockDataReader.Setup(reader => reader.GetOrdinal("Timestamp")).Returns(1);
            mockDataReader.Setup(reader => reader.GetOrdinal("Location")).Returns(2);
            mockDataReader.Setup(reader => reader.GetOrdinal("Description")).Returns(3);
            mockDataReader.Setup(reader => reader.GetOrdinal("CheckpointStatus")).Returns(4);
            mockDataReader.Setup(reader => reader.GetOrdinal("TrackedOrderID")).Returns(5);

            // Setup IsDBNull for nullable fields.
            mockDataReader.Setup(reader => reader.IsDBNull(2)).Returns(false);

            // Setup field value retrieval.
            mockDataReader.Setup(reader => reader.GetInt32(0)).Returns(201);
            mockDataReader.Setup(reader => reader.GetDateTime(1)).Returns(new DateTime(2025, 4, 10, 12, 0, 0));
            mockDataReader.Setup(reader => reader.GetString(2)).Returns("Distribution Center");
            mockDataReader.Setup(reader => reader.GetString(3)).Returns("Order processed");
            mockDataReader.Setup(reader => reader.GetString(4)).Returns(OrderStatus.PROCESSING.ToString());
            mockDataReader.Setup(reader => reader.GetInt32(5)).Returns(101);

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(mockDataReader.Object);
        }
    }
}

