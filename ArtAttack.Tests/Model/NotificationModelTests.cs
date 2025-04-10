using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ArtAttack.Tests.Model
{
    [TestClass]
    public class NotificationFactoryTests
    {
        private Mock<IDataReader> mockDataReader;
        private DateTime timestamp;

        [TestInitialize]
        public void Setup()
        {
            mockDataReader = new Mock<IDataReader>();
            timestamp = DateTime.Now;

            // Setup common ordinal positions
            mockDataReader.Setup(reader => reader.GetOrdinal("notificationID")).Returns(0);
            mockDataReader.Setup(reader => reader.GetOrdinal("recipientID")).Returns(1);
            mockDataReader.Setup(reader => reader.GetOrdinal("timestamp")).Returns(2);
            mockDataReader.Setup(reader => reader.GetOrdinal("isRead")).Returns(3);
            mockDataReader.Setup(reader => reader.GetOrdinal("category")).Returns(4);
            mockDataReader.Setup(reader => reader.GetOrdinal("contractID")).Returns(5);
            mockDataReader.Setup(reader => reader.GetOrdinal("isAccepted")).Returns(6);
            mockDataReader.Setup(reader => reader.GetOrdinal("productID")).Returns(7);
            mockDataReader.Setup(reader => reader.GetOrdinal("orderID")).Returns(8);
            mockDataReader.Setup(reader => reader.GetOrdinal("shippingState")).Returns(9);
            mockDataReader.Setup(reader => reader.GetOrdinal("deliveryDate")).Returns(10);
            mockDataReader.Setup(reader => reader.GetOrdinal("expirationDate")).Returns(11);

            // Setup common column values
            mockDataReader.Setup(reader => reader.GetInt32(0)).Returns(42);  // notificationID
            mockDataReader.Setup(reader => reader.GetInt32(1)).Returns(1);   // recipientID
            mockDataReader.Setup(reader => reader.GetDateTime(2)).Returns(timestamp);  // timestamp
            mockDataReader.Setup(reader => reader.GetBoolean(3)).Returns(false);  // isRead
        }

        [TestMethod]
        public void CreateFromDataReader_ShouldReturnProductAvailableNotification()
        {
            // Setup specific values for this test case
            mockDataReader.Setup(reader => reader.GetString(4)).Returns("PRODUCT_AVAILABLE");
            mockDataReader.Setup(reader => reader.GetInt32(7)).Returns(301);  // productID

            // Act
            var actualNotification = NotificationFactory.CreateFromDataReader(mockDataReader.Object);

            // Create expected notification with the same properties
            var expectedNotification = new ProductAvailableNotification(
                recipientId: 1,
                timestamp: timestamp,
                productId: 301,
                isRead: false,
                notificationId: 42
            );

            // Assert
            Assert.IsTrue(
                NotificationFactory.AreEqual(actualNotification, expectedNotification),
                $"Expected a ProductAvailableNotification with RecipientID=1, ProductID=301"
            );
        }
        [TestMethod]
        public void CreateFromDataReader_ShouldReturnContractRenewalAnswerNotification()
        {
            // Setup specific values for this test case
            mockDataReader.Setup(reader => reader.GetString(4)).Returns("CONTRACT_RENEWAL_ACCEPTED");
            mockDataReader.Setup(reader => reader.GetInt32(5)).Returns(101);  // contractID
            mockDataReader.Setup(reader => reader.GetBoolean(6)).Returns(true);  // isAccepted

            // Act
            var actualNotification = NotificationFactory.CreateFromDataReader(mockDataReader.Object);

            // Create expected notification with the same properties
            var expectedNotification = new ContractRenewalAnswerNotification(
                recipientID: 1,
                timestamp: timestamp,
                isAccepted: true,
                contractID: 101,
                isRead: false,
                notificationId: 42

            );

            // Assert
            Assert.IsTrue(
                NotificationFactory.AreEqual(actualNotification, expectedNotification),
                $"Expected a ContractRenewalAnswerNotification with RecipientID=1, ContractID=101, IsAccepted=true"
            );
        }

        [TestMethod]
        public void CreateFromDataReader_ShouldReturnOrderShippingProgressNotification()
        {
            // Setup specific values for this test case
            DateTime deliveryDate = new DateTime(2025, 4, 15);

            mockDataReader.Setup(reader => reader.GetString(4)).Returns("ORDER_SHIPPING_PROGRESS");
            mockDataReader.Setup(reader => reader.GetInt32(8)).Returns(201);  // orderID
            mockDataReader.Setup(reader => reader.GetString(9)).Returns("SHIPPED");  // shippingState
            mockDataReader.Setup(reader => reader.GetDateTime(10)).Returns(deliveryDate);  // deliveryDate

            // Act
            var actualNotification = NotificationFactory.CreateFromDataReader(mockDataReader.Object);

            // Create expected notification with the same properties
            var expectedNotification = new OrderShippingProgressNotification(
                recipientId: 1,
                timestamp: timestamp,
                id: 201,
                state: "SHIPPED",
                deliveryDate: deliveryDate,
                isRead: false,
                notificationId: 42
            );

            // Assert
            Assert.IsTrue(
                NotificationFactory.AreEqual(actualNotification, expectedNotification),
                $"Expected an OrderShippingProgressNotification with RecipientID=1, OrderID=201, ShippingState=SHIPPED, DeliveryDate={deliveryDate}"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateFromDataReader_UnknownCategoryNotification_ShouldThrowArgumentException()
        {
            // Setup specific values for this test case
            mockDataReader.Setup(reader => reader.GetString(4)).Returns("UNKNOWN_CATEGORY");

            // Act
            NotificationFactory.CreateFromDataReader(mockDataReader.Object);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public void CreateFromDataReader_ShouldReturnContractRenewalWaitlistNotification()
        {
            // Setup specific values for this test case
            mockDataReader.Setup(reader => reader.GetString(4)).Returns("CONTRACT_RENEWAL_WAITLIST");
            mockDataReader.Setup(reader => reader.GetInt32(7)).Returns(201);  // productID

            // Act
            var actualNotification = NotificationFactory.CreateFromDataReader(mockDataReader.Object);

            // Create expected notification with the same properties
            var expectedNotification = new ContractRenewalWaitlistNotification(
                recipientID: 1,
                timestamp: timestamp,
                productID: 201,
                isRead: false,
                notificationId: 42
            );

            // Assert
            Assert.IsTrue(
                NotificationFactory.AreEqual(actualNotification, expectedNotification),
                $"Expected a ContractRenewalWaitlistNotification with RecipientID=1, ProductID=201"
            );
        }

        [TestMethod]
        public void CreateFromDataReader_ShouldReturnPaymentConfirmationNotification()
        {
            // Setup specific values for this test case
            mockDataReader.Setup(reader => reader.GetString(4)).Returns("PAYMENT_CONFIRMATION");
            mockDataReader.Setup(reader => reader.GetInt32(7)).Returns(201);  // productID
            mockDataReader.Setup(reader => reader.GetInt32(8)).Returns(102);  // orderID

            // Act
            var actualNotification = NotificationFactory.CreateFromDataReader(mockDataReader.Object);

            // Create expected notification with the same properties
            var expectedNotification = new PaymentConfirmationNotification(
                recipientId: 1,
                timestamp: timestamp,
                productId: 201,
                orderId: 102,
                isRead: false,
                notificationId: 42
            );

            // Assert
            Assert.IsTrue(
                NotificationFactory.AreEqual(actualNotification, expectedNotification),
                $"Expected a PaymentConfirmationNotification with RecipientID=1, ProductID=201, OrderID=102"
            );
        }

        [TestMethod]
        public void CreateFromDataReader_ShouldReturnProductRemovedNotification()
        {
            // Setup specific values for this test case
            mockDataReader.Setup(reader => reader.GetString(4)).Returns("PRODUCT_REMOVED");
            mockDataReader.Setup(reader => reader.GetInt32(7)).Returns(201);  // productID

            // Act
            var actualNotification = NotificationFactory.CreateFromDataReader(mockDataReader.Object);

            // Create expected notification with the same properties
            var expectedNotification = new ProductRemovedNotification(
                recipientId: 1,
                timestamp: timestamp,
                productId: 201,
                isRead: false,
                notificationId: 42
            );

            // Assert
            Assert.IsTrue(
                NotificationFactory.AreEqual(actualNotification, expectedNotification),
                $"Expected a ProductRemovedNotification with RecipientID=1, ProductID=201"
            );
        }

        [TestMethod]
        public void CreateFromDataReader_ShouldReturnContractRenewalRequestNotification()
        {
            // Setup specific values for this test case
            mockDataReader.Setup(reader => reader.GetString(4)).Returns("CONTRACT_RENEWAL_REQUEST");
            mockDataReader.Setup(reader => reader.GetInt32(5)).Returns(101);  // contractID


            // Act
            var actualNotification = NotificationFactory.CreateFromDataReader(mockDataReader.Object);

            // Create expected notification with the same properties
            var expectedNotification = new ContractRenewalRequestNotification(
                recipientId: 1,
                timestamp: timestamp,
                contractId: 101,
                isRead: false,
                notificationId: 42
            );

            // Assert
            Assert.IsTrue(
                NotificationFactory.AreEqual(actualNotification, expectedNotification),
                $"Expected a ContractRenewalRequestNotification with RecipientID=1, ContractID=101"
            );
        }

        [TestMethod]
        public void CreateFromDataReader_ShouldReturnContractExpirationNotification()
        {
            // Setup specific values for this test case
            DateTime expirationDate = new DateTime(2025, 10, 15);

            mockDataReader.Setup(reader => reader.GetString(4)).Returns("CONTRACT_EXPIRATION");
            mockDataReader.Setup(reader => reader.GetInt32(5)).Returns(101);  // contractID
            mockDataReader.Setup(reader => reader.GetDateTime(11)).Returns(expirationDate);  // expirationDate

            // Act
            var actualNotification = NotificationFactory.CreateFromDataReader(mockDataReader.Object);

            // Create expected notification with the same properties
            var expectedNotification = new ContractExpirationNotification(
                recipientId: 1,
                timestamp: timestamp,
                contractId: 101,
                expirationDate: expirationDate,
                isRead: false,
                notificationId: 42
            );

            // Assert
            Assert.IsTrue(
                NotificationFactory.AreEqual(actualNotification, expectedNotification),
                $"Expected a ContractExpirationNotification with RecipientID=1, ContractID=101, ExpirationDate={expirationDate}"
            );
        }

        [TestMethod]
        public void CreateFromDataReader_ShouldReturnOutbiddedNotification()
        {
            // Setup specific values for this test case
            mockDataReader.Setup(reader => reader.GetString(4)).Returns("OUTBIDDED");
            mockDataReader.Setup(reader => reader.GetInt32(7)).Returns(301);  // productID

            // Act
            var actualNotification = NotificationFactory.CreateFromDataReader(mockDataReader.Object);

            // Create expected notification with the same properties
            var expectedNotification = new OutbiddedNotification(
                recipientId: 1,
                timestamp: timestamp,
                productId: 301,
                isRead: false,
                notificationId: 42
            );

            // Assert
            Assert.IsTrue(
                NotificationFactory.AreEqual(actualNotification, expectedNotification),
                $"Expected an OutbiddedNotification with RecipientID=1, ProductID=301"
            );
        }

    }

    [TestClass]
    public class NotificationDataAdapterTests
    {
        private Mock<IDatabaseProvider> mockDatabaseProvider;
        private Mock<IDbConnection> mockConnection;
        private Mock<IDbCommand> mockCommand;
        private Mock<IDataReader> mockReader;
        private Mock<IDataParameterCollection> mockParameters;
        private NotificationDataAdapter notificationDataAdapter;
        private const string ConnectionString = "test_connection_string";
        private const string TestConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";

        [TestInitialize]
        public void Setup()
        {
            mockDatabaseProvider = new Mock<IDatabaseProvider>();
            mockConnection = new Mock<IDbConnection>();
            mockCommand = new Mock<IDbCommand>();
            mockReader = new Mock<IDataReader>();
            mockParameters = new Mock<IDataParameterCollection>();

            mockCommand.Setup(command => command.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
            mockCommand.Setup(command => command.Parameters).Returns(mockParameters.Object);
            mockCommand.Setup(command => command.ExecuteReader()).Returns(mockReader.Object);

            mockConnection.Setup(command => command.CreateCommand()).Returns(mockCommand.Object);
            mockConnection.Setup(command => command.State).Returns(ConnectionState.Open);

            mockDatabaseProvider.Setup(databaseProvider => databaseProvider.CreateConnection(ConnectionString)).Returns(mockConnection.Object);

            notificationDataAdapter = new NotificationDataAdapter(ConnectionString, mockDatabaseProvider.Object);
        }

        [TestMethod]
        public void Constructor_WithOnlyConnectionString_ShouldInitializCorrectly()
        {
            // This will create a testable version that doesn't actually try to connect to SQL
            var testNotificationDataAdapter = new NotificationDataAdapter(ConnectionString, mockDatabaseProvider.Object);

            // Assert - using reflection to access private field
            var connectionStringField = typeof(NotificationDataAdapter).GetField("connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var connectionStringValue = connectionStringField.GetValue(testNotificationDataAdapter);

            Assert.AreEqual(ConnectionString, connectionStringValue);
        }

        [TestMethod]
        public void Constructor_WithConnectionStringAndDatabaseprovider_ShouldSetConnectionString()
        {
            // Arrange
            // Create a mock database provider that won't actually try to connect
            var mockDatabaseProvider = new Mock<IDatabaseProvider>();
            mockDatabaseProvider.Setup(databaseProvider => databaseProvider.CreateConnection(TestConnectionString)).Returns(mockConnection.Object);

            // Act
            // Use the constructor that accepts both connection string and database provider
            var testNotificationDataAdapter = new NotificationDataAdapter(TestConnectionString, mockDatabaseProvider.Object);

            // Assert - using reflection to access private field
            var connectionStringField = typeof(NotificationDataAdapter).GetField("connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var connectionStringValue = connectionStringField.GetValue(testNotificationDataAdapter);

            Assert.AreEqual(TestConnectionString, connectionStringValue);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConnectionString_ShouldThrowArgumentNullException()
        {
            // Act - this should throw ArgumentNullException
            var testNotificationDataAdapter = new NotificationDataAdapter(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullDatabaseProvider_ShouldThrowArgumentNullException()
        {
            // Act
            var testNotificationDataAdapter = new NotificationDataAdapter(ConnectionString, null);
        }

        [TestMethod]
        public void GetNotificationsForUser_ShouldReturnNotificationsList()
        {
            // Arrange
            int notificationRecieverId = 1;

            // Setup reader to return two notifications
            int readCount = 0;
            mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 2);
            SetupReaderForNotifications(mockReader);

            // Act
            var userNotifications = notificationDataAdapter.GetNotificationsForUser(notificationRecieverId);

            // Verify command was set up correctly
            mockCommand.VerifySet(command => command.CommandText = "GetNotificationsByRecipient");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);

            // Assert
            Assert.AreEqual(2, userNotifications.Count);
        }

        [TestMethod]
        public void MarkNotificationAsRead_ShouldExecuteCommand()
        {
            // Arrange
            int notificationId = 42;

            // Act
            notificationDataAdapter.MarkAsRead(notificationId);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "MarkNotificationAsRead");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNotification_WithNullNotification_ShouldThrowArgumentNullException()
        {
            // Act
            notificationDataAdapter.AddNotification(null);
        }

        [TestMethod]
        public void Dispose_ShouldCloseAndDisposeConnection()
        {
            // Act
            notificationDataAdapter.Dispose();

            // Assert - connection should be disposed
            mockConnection.Verify(command => command.Dispose(), Times.Once);
        }

        [TestMethod]
        public void AddNotification_ShouldAddContractRenewalAnswerNotification()
        {
            // Arrange
            var notification = new ContractRenewalAnswerNotification(
                recipientID: 1,
                timestamp: DateTime.Now,
                contractID: 101,
                isAccepted: true
            );

            // Capture parameters
            var capturedParameters = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParameters);

            // Act
            notificationDataAdapter.AddNotification(notification);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "AddNotification");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            AssertParameterExists(capturedParameters, "@category", "CONTRACT_RENEWAL_ACCEPTED");

        }

        [TestMethod]
        public void AddNotification_ShouldAddProductAvailableNotification()
        {
            // Arrange
            var notification = new ProductAvailableNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                productId: 301
            );

            // Create a list to store the actual parameters
            var capturedParameters = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParameters);

            // Act
            notificationDataAdapter.AddNotification(notification);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "AddNotification");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            AssertParameterExists(capturedParameters, "@category", "PRODUCT_AVAILABLE");
        }


        [TestMethod]
        public void AddNotification_ShouldAddOrderShippingProgressNotification()
        {
            // Arrange
            var deliveryDate = new DateTime(2025, 4, 15);
            var notification = new OrderShippingProgressNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                id: 201,
                state: "SHIPPED",
                deliveryDate: deliveryDate
            );

            // Capture parameters
            var capturedParameters = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParameters);

            // Act
            notificationDataAdapter.AddNotification(notification);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "AddNotification");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            AssertParameterExists(capturedParameters, "@category", "ORDER_SHIPPING_PROGRESS");
        }

        [TestMethod]
        public void AddNotification_ShouldAddContractExpirationNotification()
        {
            // Arrange
            var expirationDate = new DateTime(2025, 10, 15);
            var notification = new ContractExpirationNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                contractId: 101,
                expirationDate: expirationDate
            );

            // Capture parameters
            var capturedParameters = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParameters);

            // Act
            notificationDataAdapter.AddNotification(notification);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "AddNotification");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            AssertParameterExists(capturedParameters, "@category", "CONTRACT_EXPIRATION");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddNotification_UnknownNotificationType_ShouldThrowArgumentException()
        {
            // Arrange
            var notification = new Mock<Notification>().Object;

            // Act - should throw ArgumentException
            notificationDataAdapter.AddNotification(notification);
        }


        [TestMethod]
        public void AddNotification_ShouldAddContractRenewalWaitlistNotification()
        {
            // Arrange
            var notification = new ContractRenewalWaitlistNotification(
                recipientID: 1,
                timestamp: DateTime.Now,
                productID: 201
            );

            // Capture parameters
            var capturedParameters = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParameters);

            // Act
            notificationDataAdapter.AddNotification(notification);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "AddNotification");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            AssertParameterExists(capturedParameters, "@category", "CONTRACT_RENEWAL_WAITLIST");
        }

        [TestMethod]
        public void AddNotification_ShouldAddOutbiddedNotification()
        {
            // Arrange
            var notification = new OutbiddedNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                productId: 301
            );

            // Capture parameters
            var capturedParameters = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParameters);

            // Act
            notificationDataAdapter.AddNotification(notification);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "AddNotification");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            AssertParameterExists(capturedParameters, "@category", "OUTBIDDED");
        }

        [TestMethod]
        public void AddNotification_ShouldAddPaymentConfirmationNotification()
        {
            // Arrange
            var notification = new PaymentConfirmationNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                productId: 301,
                orderId: 501
            );

            // Capture parameters
            var capturedParameters = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParameters);

            // Act
            notificationDataAdapter.AddNotification(notification);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "AddNotification");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            AssertParameterExists(capturedParameters, "@category", "PAYMENT_CONFIRMATION");
        }

        [TestMethod]
        public void AddNotification_ShouldAddProductRemovedNotification()
        {
            // Arrange
            var notification = new ProductRemovedNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                productId: 301
            );

            // Capture parameters
            var capturedParameters = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParameters);

            // Act
            notificationDataAdapter.AddNotification(notification);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "AddNotification");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            AssertParameterExists(capturedParameters, "@category", "PRODUCT_REMOVED");

        }

        [TestMethod]
        public void AddNotification_ShouldAddContractRenewalRequestNotification()
        {
            // Arrange
            var notification = new ContractRenewalRequestNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                contractId: 101
            );

            // Capture parameters
            var capturedParameters = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParameters);

            // Act
            notificationDataAdapter.AddNotification(notification);

            // Assert
            mockCommand.VerifySet(command => command.CommandText = "AddNotification");
            mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            AssertParameterExists(capturedParameters, "@category", "CONTRACT_RENEWAL_REQUEST");

        }

        [TestMethod]
        public void AddNotification_ShouldHandleConnectionClosed()
        {
            // Arrange
            // First, reset interactions with the connection mock
            // to clear the Open() call from the constructor
            Mock.Get(mockConnection.Object).Invocations.Clear();

            // Now set up the closed state for this specific test
            mockConnection.Setup(connection => connection.State).Returns(ConnectionState.Closed);

            var notification = new ProductAvailableNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                productId: 301
            );

            // Capture parameters with proper enumeration support
            var capturedParameters = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParameters);

            // Act
            notificationDataAdapter.AddNotification(notification);

            // Assert
            mockConnection.Verify(command => command.Open(), Times.Once);
            mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);

            AssertParameterExists(capturedParameters, "@category", "PRODUCT_AVAILABLE");
        }

        // Helper methods for testing
        private void SetupParameterCapture(List<(string Name, object Value)> capturedParameters)
        {
            var mockParameter = new Mock<IDbDataParameter>();
            mockParameter.SetupProperty(parameter => parameter.ParameterName);
            mockParameter.SetupProperty(parameter => parameter.Value);

            // Set up a collection to track parameters added to the command
            var parameters = new List<IDbDataParameter>();

            // Setup mock parameters collection to be enumerable
            mockParameters.Setup(parameter => parameter.GetEnumerator())
                .Returns(() => parameters.GetEnumerator());

            // Setup command to return our mock parameter and capture its values
            mockCommand.Setup(command => command.CreateParameter()).Returns(() => {
                var parameterObject = mockParameter.Object;
                parameterObject.ParameterName = null;
                parameterObject.Value = null;
                return parameterObject;
            });

            // Capture parameters when they're added to the collection
            mockParameters.Setup(parameter => parameter.Add(It.IsAny<object>()))
                .Callback<object>(parameterObject => {
                    parameters.Add((IDbDataParameter)parameterObject);
                    var databaseParameter = (IDbDataParameter)parameterObject;
                    capturedParameters.Add((databaseParameter.ParameterName, databaseParameter.Value));
                })
                .Returns(0);
        }

        // Helper method to assert that a parameter exists in the captured parameters
        private void AssertParameterExists(List<(string Name, object Value)> parameters, string parameterName, object expectedParameterValue)
        {
            var firstOrDefaultParameter = parameters.FirstOrDefault(parameter => parameter.Name == parameterName);
            Assert.IsTrue(firstOrDefaultParameter != default, $"Parameter {parameterName} was not found");

            if (expectedParameterValue == DBNull.Value)
            {
                Assert.AreEqual(DBNull.Value, firstOrDefaultParameter.Value, $"Parameter {parameterName} should be DBNull");
            }
            else
            {
                Assert.AreEqual(expectedParameterValue, firstOrDefaultParameter.Value, $"Parameter {parameterName} has incorrect value");
            }
        }

        private void SetupReaderForNotifications(Mock<IDataReader> mockReader)
        {
            mockReader.Setup(reader => reader.GetOrdinal("notificationID")).Returns(0);
            mockReader.Setup(reader => reader.GetOrdinal("recipientID")).Returns(1);
            mockReader.Setup(reader => reader.GetOrdinal("timestamp")).Returns(2);
            mockReader.Setup(reader => reader.GetOrdinal("isRead")).Returns(3);
            mockReader.Setup(reader => reader.GetOrdinal("category")).Returns(4);
            mockReader.Setup(reader => reader.GetOrdinal("contractID")).Returns(5);
            mockReader.Setup(reader => reader.GetOrdinal("isAccepted")).Returns(6);
            mockReader.Setup(reader => reader.GetOrdinal("productID")).Returns(7);
            mockReader.Setup(reader => reader.GetOrdinal("orderID")).Returns(8);
            mockReader.Setup(reader => reader.GetOrdinal("shippingState")).Returns(9);
            mockReader.Setup(reader => reader.GetOrdinal("deliveryDate")).Returns(10);
            mockReader.Setup(reader => reader.GetOrdinal("expirationDate")).Returns(11);

            mockReader.SetupSequence(reader => reader.GetInt32(0)).Returns(42).Returns(43);
            mockReader.Setup(reader => reader.GetInt32(1)).Returns(1);
            mockReader.Setup(reader => reader.GetDateTime(2)).Returns(DateTime.Now);
            mockReader.Setup(reader => reader.GetBoolean(3)).Returns(false);

            // First row: CONTRACT_RENEWAL_ACCEPTED
            // Second row: PRODUCT_AVAILABLE
            mockReader.SetupSequence(reader => reader.GetString(4))
                .Returns("CONTRACT_RENEWAL_ACCEPTED")
                .Returns("PRODUCT_AVAILABLE");

            mockReader.Setup(reader => reader.GetInt32(5)).Returns(101); // contractID
            mockReader.Setup(reader => reader.GetBoolean(6)).Returns(true); // isAccepted
            mockReader.Setup(reader => reader.GetInt32(7)).Returns(201); // productID
        }
    }
}
