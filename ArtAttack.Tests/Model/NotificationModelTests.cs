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
        [TestMethod]
        public void CreateFromDataReader_ProductAvailableNotification_ReturnsCorrectType()
        {
            // Arrange
            var mockReader = new Mock<IDataReader>();

            // Setup the ordinal positions
            mockReader.Setup(r => r.GetOrdinal("notificationID")).Returns(0);
            mockReader.Setup(r => r.GetOrdinal("recipientID")).Returns(1);
            mockReader.Setup(r => r.GetOrdinal("timestamp")).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("isRead")).Returns(3);
            mockReader.Setup(r => r.GetOrdinal("category")).Returns(4);
            mockReader.Setup(r => r.GetOrdinal("productID")).Returns(7);

            // Setup the values returned by each column
            mockReader.Setup(r => r.GetInt32(0)).Returns(42);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1);
            mockReader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetBoolean(3)).Returns(false);
            mockReader.Setup(r => r.GetString(4)).Returns("PRODUCT_AVAILABLE");
            mockReader.Setup(r => r.GetInt32(7)).Returns(301);

            // Act
            var notification = NotificationFactory.CreateFromDataReader(mockReader.Object);

            // Assert
            Assert.IsInstanceOfType(notification, typeof(ProductAvailableNotification));
            var typedNotification = notification as ProductAvailableNotification;
            Assert.AreEqual(1, typedNotification.RecipientID);
            Assert.AreEqual(301, typedNotification.GetProductID());
            Assert.AreEqual("Product Available", typedNotification.Title);
        }

        [TestMethod]
        public void CreateFromDataReader_ContractRenewalAnswerNotification_ReturnsCorrectType()
        {
            // Arrange
            var mockReader = new Mock<IDataReader>();

            // Setup the ordinal positions
            mockReader.Setup(r => r.GetOrdinal("notificationID")).Returns(0);
            mockReader.Setup(r => r.GetOrdinal("recipientID")).Returns(1);
            mockReader.Setup(r => r.GetOrdinal("timestamp")).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("isRead")).Returns(3);
            mockReader.Setup(r => r.GetOrdinal("category")).Returns(4);
            mockReader.Setup(r => r.GetOrdinal("contractID")).Returns(5);
            mockReader.Setup(r => r.GetOrdinal("isAccepted")).Returns(6);

            // Setup the values returned by each column
            mockReader.Setup(r => r.GetInt32(0)).Returns(42);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1);
            mockReader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetBoolean(3)).Returns(false);
            mockReader.Setup(r => r.GetString(4)).Returns("CONTRACT_RENEWAL_ACCEPTED");
            mockReader.Setup(r => r.GetInt32(5)).Returns(101);
            mockReader.Setup(r => r.GetBoolean(6)).Returns(true);

            // Act
            var notification = NotificationFactory.CreateFromDataReader(mockReader.Object);

            // Assert
            Assert.IsInstanceOfType(notification, typeof(ContractRenewalAnswerNotification));
            var typedNotification = notification as ContractRenewalAnswerNotification;
            Assert.AreEqual(1, typedNotification.RecipientID);
            Assert.AreEqual(101, typedNotification.GetContractID());
            Assert.IsTrue(typedNotification.GetIsAccepted());
            Assert.AreEqual("Contract Renewal Answer", typedNotification.Title);
        }

        [TestMethod]
        public void CreateFromDataReader_OrderShippingProgressNotification_ReturnsCorrectType()
        {
            // Arrange
            var mockReader = new Mock<IDataReader>();
            DateTime deliveryDate = new DateTime(2025, 4, 15);

            // Setup the ordinal positions
            mockReader.Setup(r => r.GetOrdinal("notificationID")).Returns(0);
            mockReader.Setup(r => r.GetOrdinal("recipientID")).Returns(1);
            mockReader.Setup(r => r.GetOrdinal("timestamp")).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("isRead")).Returns(3);
            mockReader.Setup(r => r.GetOrdinal("category")).Returns(4);
            mockReader.Setup(r => r.GetOrdinal("orderID")).Returns(8);
            mockReader.Setup(r => r.GetOrdinal("shippingState")).Returns(9);
            mockReader.Setup(r => r.GetOrdinal("deliveryDate")).Returns(10);

            // Setup the values returned by each column
            mockReader.Setup(r => r.GetInt32(0)).Returns(42);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1);
            mockReader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetBoolean(3)).Returns(false);
            mockReader.Setup(r => r.GetString(4)).Returns("ORDER_SHIPPING_PROGRESS");
            mockReader.Setup(r => r.GetInt32(8)).Returns(201);
            mockReader.Setup(r => r.GetString(9)).Returns("SHIPPED");
            mockReader.Setup(r => r.GetDateTime(10)).Returns(deliveryDate);

            // Act
            var notification = NotificationFactory.CreateFromDataReader(mockReader.Object);

            // Assert
            Assert.IsInstanceOfType(notification, typeof(OrderShippingProgressNotification));
            var typedNotification = notification as OrderShippingProgressNotification;
            Assert.AreEqual(1, typedNotification.RecipientID);
            Assert.AreEqual(201, typedNotification.GetOrderID());
            Assert.AreEqual("SHIPPED", typedNotification.GetShippingState());
            Assert.AreEqual(deliveryDate, typedNotification.GetDeliveryDate());
            Assert.AreEqual("Order Shipping Update", typedNotification.Title);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateFromDataReader_UnknownCategory_ThrowsArgumentException()
        {
            // Arrange
            var mockReader = new Mock<IDataReader>();
            SetupCommonReaderFields(mockReader, "UNKNOWN_CATEGORY");

            // Act
            NotificationFactory.CreateFromDataReader(mockReader.Object);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public void CreateFromDataReader_ContractRenewalWaitlistNotification_ReturnsCorrectType()
        {
            // Arrange
            var mockReader = new Mock<IDataReader>();

            // Setup the ordinal positions
            mockReader.Setup(r => r.GetOrdinal("notificationID")).Returns(0);
            mockReader.Setup(r => r.GetOrdinal("recipientID")).Returns(1);
            mockReader.Setup(r => r.GetOrdinal("timestamp")).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("isRead")).Returns(3);
            mockReader.Setup(r => r.GetOrdinal("category")).Returns(4);
            mockReader.Setup(r => r.GetOrdinal("productID")).Returns(7);

            // Setup the values returned by each column
            mockReader.Setup(r => r.GetInt32(0)).Returns(42);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1);
            mockReader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetBoolean(3)).Returns(false);
            mockReader.Setup(r => r.GetString(4)).Returns("CONTRACT_RENEWAL_WAITLIST");
            mockReader.Setup(r => r.GetInt32(7)).Returns(201);

            // Act
            var notification = NotificationFactory.CreateFromDataReader(mockReader.Object);

            // Assert
            Assert.IsInstanceOfType(notification, typeof(ContractRenewalWaitlistNotification));
            var typedNotification = notification as ContractRenewalWaitlistNotification;
            Assert.AreEqual(1, typedNotification.RecipientID);
            Assert.AreEqual(201, typedNotification.GetProductID());
            Assert.AreEqual("Contract Renewal in Waitlist", typedNotification.Title);
        }

        [TestMethod]
        public void CreateFromDataReader_PaymentConfirmationNotification_ReturnsCorrectType()
        {
            // Arrange
            var mockReader = new Mock<IDataReader>();

            // Setup the ordinal positions
            mockReader.Setup(r => r.GetOrdinal("notificationID")).Returns(0);
            mockReader.Setup(r => r.GetOrdinal("recipientID")).Returns(1);
            mockReader.Setup(r => r.GetOrdinal("timestamp")).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("isRead")).Returns(3);
            mockReader.Setup(r => r.GetOrdinal("category")).Returns(4);
            mockReader.Setup(r => r.GetOrdinal("productID")).Returns(7);
            mockReader.Setup(r => r.GetOrdinal("orderID")).Returns(8);

            // Setup the values returned by each column
            mockReader.Setup(r => r.GetInt32(0)).Returns(42);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1);
            mockReader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetBoolean(3)).Returns(false);
            mockReader.Setup(r => r.GetString(4)).Returns("PAYMENT_CONFIRMATION");
            mockReader.Setup(r => r.GetInt32(7)).Returns(201); // productID
            mockReader.Setup(r => r.GetInt32(8)).Returns(102); // orderID

            // Act
            var notification = NotificationFactory.CreateFromDataReader(mockReader.Object);

            // Assert
            Assert.IsInstanceOfType(notification, typeof(PaymentConfirmationNotification));
            var typedNotification = notification as PaymentConfirmationNotification;
            Assert.AreEqual(1, typedNotification.RecipientID);
            Assert.AreEqual(201, typedNotification.GetProductID());
            Assert.AreEqual(102, typedNotification.GetOrderID());
            Assert.AreEqual("Payment Confirmation", typedNotification.Title);
        }

        [TestMethod]
        public void CreateFromDataReader_ProductRemovedNotification_ReturnsCorrectType()
        {
            // Arrange
            var mockReader = new Mock<IDataReader>();

            // Setup the ordinal positions
            mockReader.Setup(r => r.GetOrdinal("notificationID")).Returns(0);
            mockReader.Setup(r => r.GetOrdinal("recipientID")).Returns(1);
            mockReader.Setup(r => r.GetOrdinal("timestamp")).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("isRead")).Returns(3);
            mockReader.Setup(r => r.GetOrdinal("category")).Returns(4);
            mockReader.Setup(r => r.GetOrdinal("productID")).Returns(7);

            // Setup the values returned by each column
            mockReader.Setup(r => r.GetInt32(0)).Returns(42);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1);
            mockReader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetBoolean(3)).Returns(false);
            mockReader.Setup(r => r.GetString(4)).Returns("PRODUCT_REMOVED");
            mockReader.Setup(r => r.GetInt32(7)).Returns(201);

            // Act
            var notification = NotificationFactory.CreateFromDataReader(mockReader.Object);

            // Assert
            Assert.IsInstanceOfType(notification, typeof(ProductRemovedNotification));
            var typedNotification = notification as ProductRemovedNotification;
            Assert.AreEqual(1, typedNotification.RecipientID);
            Assert.AreEqual(201, typedNotification.GetProductID());
            Assert.AreEqual("Product Removed", typedNotification.Title);
        }

        [TestMethod]
        public void CreateFromDataReader_ContractRenewalRequestNotification_ReturnsCorrectType()
        {
            // Arrange
            var mockReader = new Mock<IDataReader>();

            // Setup the ordinal positions
            mockReader.Setup(r => r.GetOrdinal("notificationID")).Returns(0);
            mockReader.Setup(r => r.GetOrdinal("recipientID")).Returns(1);
            mockReader.Setup(r => r.GetOrdinal("timestamp")).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("isRead")).Returns(3);
            mockReader.Setup(r => r.GetOrdinal("category")).Returns(4);
            mockReader.Setup(r => r.GetOrdinal("contractID")).Returns(5);

            // Setup the values returned by each column
            mockReader.Setup(r => r.GetInt32(0)).Returns(42);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1);
            mockReader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetBoolean(3)).Returns(false);
            mockReader.Setup(r => r.GetString(4)).Returns("CONTRACT_RENEWAL_REQUEST");
            mockReader.Setup(r => r.GetInt32(5)).Returns(101);

            // Act
            var notification = NotificationFactory.CreateFromDataReader(mockReader.Object);

            // Assert
            Assert.IsInstanceOfType(notification, typeof(ContractRenewalRequestNotification));
            var typedNotification = notification as ContractRenewalRequestNotification;
            Assert.AreEqual(1, typedNotification.RecipientID);
            Assert.AreEqual(101, typedNotification.GetContractID());
            Assert.AreEqual("Contract Renewal Request", typedNotification.Title);
        }

        [TestMethod]
        public void CreateFromDataReader_ContractExpirationNotification_ReturnsCorrectType()
        {
            // Arrange
            var mockReader = new Mock<IDataReader>();
            DateTime expirationDate = new DateTime(2025, 10, 15);

            // Setup the ordinal positions
            mockReader.Setup(r => r.GetOrdinal("notificationID")).Returns(0);
            mockReader.Setup(r => r.GetOrdinal("recipientID")).Returns(1);
            mockReader.Setup(r => r.GetOrdinal("timestamp")).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("isRead")).Returns(3);
            mockReader.Setup(r => r.GetOrdinal("category")).Returns(4);
            mockReader.Setup(r => r.GetOrdinal("contractID")).Returns(5);
            mockReader.Setup(r => r.GetOrdinal("expirationDate")).Returns(11);

            // Setup the values returned by each column
            mockReader.Setup(r => r.GetInt32(0)).Returns(42);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1);
            mockReader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetBoolean(3)).Returns(false);
            mockReader.Setup(r => r.GetString(4)).Returns("CONTRACT_EXPIRATION");
            mockReader.Setup(r => r.GetInt32(5)).Returns(101);
            mockReader.Setup(r => r.GetDateTime(11)).Returns(expirationDate);

            // Act
            var notification = NotificationFactory.CreateFromDataReader(mockReader.Object);

            // Assert
            Assert.IsInstanceOfType(notification, typeof(ContractExpirationNotification));
            var typedNotification = notification as ContractExpirationNotification;
            Assert.AreEqual(1, typedNotification.RecipientID);
            Assert.AreEqual(101, typedNotification.GetContractID());
            Assert.AreEqual(expirationDate, typedNotification.GetExpirationDate());
            Assert.AreEqual("Contract Expiration", typedNotification.Title);
        }

        [TestMethod]
        public void CreateFromDataReader_OutbiddedNotification_ReturnsCorrectType()
        {
            // Arrange
            var mockReader = new Mock<IDataReader>();

            // Setup the ordinal positions
            mockReader.Setup(r => r.GetOrdinal("notificationID")).Returns(0);
            mockReader.Setup(r => r.GetOrdinal("recipientID")).Returns(1);
            mockReader.Setup(r => r.GetOrdinal("timestamp")).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("isRead")).Returns(3);
            mockReader.Setup(r => r.GetOrdinal("category")).Returns(4);
            mockReader.Setup(r => r.GetOrdinal("productID")).Returns(7);

            // Setup the values returned by each column
            mockReader.Setup(r => r.GetInt32(0)).Returns(42);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1);
            mockReader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetBoolean(3)).Returns(false);
            mockReader.Setup(r => r.GetString(4)).Returns("OUTBIDDED");
            mockReader.Setup(r => r.GetInt32(7)).Returns(301);

            // Act
            var notification = NotificationFactory.CreateFromDataReader(mockReader.Object);

            // Assert
            Assert.IsInstanceOfType(notification, typeof(OutbiddedNotification));
            var typedNotification = notification as OutbiddedNotification;
            Assert.AreEqual(1, typedNotification.RecipientID);
            Assert.AreEqual(301, typedNotification.GetProductID());
            Assert.AreEqual("Outbidded", typedNotification.Title);
        }


        private void SetupCommonReaderFields(Mock<IDataReader> mockReader, string category)
        {
            // Setup the ordinal positions
            mockReader.Setup(r => r.GetOrdinal("notificationID")).Returns(0);
            mockReader.Setup(r => r.GetOrdinal("recipientID")).Returns(1);
            mockReader.Setup(r => r.GetOrdinal("timestamp")).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("isRead")).Returns(3);
            mockReader.Setup(r => r.GetOrdinal("category")).Returns(4);

            // Setup the values returned by each column
            mockReader.Setup(r => r.GetInt32(0)).Returns(42);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1);
            mockReader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetBoolean(3)).Returns(false);
            mockReader.Setup(r => r.GetString(4)).Returns(category);
        }

    }

    [TestClass]
    public class NotificationDataAdapterTests
    {
        private Mock<IDatabaseProvider> _mockDbProvider;
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private NotificationDataAdapter _adapter;
        private const string ConnectionString = "test_connection_string";
        private const string TestConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";

        [TestInitialize]
        public void Setup()
        {
            _mockDbProvider = new Mock<IDatabaseProvider>();
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockReader = new Mock<IDataReader>();
            _mockParameters = new Mock<IDataParameterCollection>();

            _mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object);
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);
            _mockConnection.Setup(c => c.State).Returns(ConnectionState.Open);

            _mockDbProvider.Setup(p => p.CreateConnection(ConnectionString)).Returns(_mockConnection.Object);

            _adapter = new NotificationDataAdapter(ConnectionString, _mockDbProvider.Object);
        }

        [TestMethod]
        public void Constructor_WithOnlyConnectionString_InitializesCorrectly()
        {
            // This will create a testable version that doesn't actually try to connect to SQL
            var adapter = new NotificationDataAdapter(ConnectionString, _mockDbProvider.Object);

            // Assert - using reflection to access private field
            var field = typeof(NotificationDataAdapter).GetField("connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var value = field.GetValue(adapter);

            Assert.IsNotNull(value);
            Assert.AreEqual(ConnectionString, value);
        }

        [TestMethod]
        public void Constructor_SetsConnectionString()
        {
            // Arrange
            // Create a mock database provider that won't actually try to connect
            var mockDbProvider = new Mock<IDatabaseProvider>();
            mockDbProvider.Setup(p => p.CreateConnection(TestConnectionString)).Returns(_mockConnection.Object);

            // Act
            // Use the constructor that accepts both connection string and database provider
            var model = new NotificationDataAdapter(TestConnectionString, mockDbProvider.Object);

            // Assert - using reflection to access private field
            var field = typeof(NotificationDataAdapter).GetField("connectionString",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var value = field.GetValue(model);

            Assert.AreEqual(TestConnectionString, value);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Act - this should throw ArgumentNullException
            using var adapter = new NotificationDataAdapter(null);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullDatabaseProvider_ThrowsArgumentNullException()
        {
            // Act
            _ = new NotificationDataAdapter(ConnectionString, null);
        }

        [TestMethod]
        public void GetNotificationsForUser_ReturnsNotificationsList()
        {
            // Arrange
            int recipientId = 1;

            // Setup reader to return two notifications
            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 2);
            SetupReaderForNotifications(_mockReader);

            // Act
            var notifications = _adapter.GetNotificationsForUser(recipientId);

            // Assert
            Assert.IsNotNull(notifications);
            Assert.AreEqual(2, notifications.Count);

            // Verify command was set up correctly
            _mockCommand.VerifySet(c => c.CommandText = "GetNotificationsByRecipient");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
        }

        [TestMethod]
        public void MarkAsRead_ExecutesCommand()
        {
            // Arrange
            int notificationId = 42;

            // Act
            _adapter.MarkAsRead(notificationId);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "MarkNotificationAsRead");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNotification_NullNotification_ThrowsArgumentNullException()
        {
            // Act
            _adapter.AddNotification(null);
        }

        [TestMethod]
        public void Dispose_ClosesAndDisposesConnection()
        {
            // Act
            _adapter.Dispose();

            // Assert - connection should be disposed
            _mockConnection.Verify(c => c.Dispose(), Times.Once);
        }

        [TestMethod]
        public void AddNotification_ContractRenewalAnswerNotification_AddsCorrectParameters()
        {
            // Arrange
            var notification = new ContractRenewalAnswerNotification(
                recipientID: 1,
                timestamp: DateTime.Now,
                contractID: 101,
                isAccepted: true
            );

            // Capture parameters
            var capturedParams = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParams);

            // Act
            _adapter.AddNotification(notification);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "AddNotification");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Check parameters
            AssertParameterExists(capturedParams, "@recipientID", 1);
            AssertParameterExists(capturedParams, "@category", "CONTRACT_RENEWAL_ACCEPTED");
            AssertParameterExists(capturedParams, "@contractID", 101);
            AssertParameterExists(capturedParams, "@isAccepted", true);

            // Check null parameters
            AssertParameterExists(capturedParams, "@productID", DBNull.Value);
            AssertParameterExists(capturedParams, "@orderID", DBNull.Value);
            AssertParameterExists(capturedParams, "@shippingState", DBNull.Value);
            AssertParameterExists(capturedParams, "@deliveryDate", DBNull.Value);
            AssertParameterExists(capturedParams, "@expirationDate", DBNull.Value);
        }

        [TestMethod]
        public void AddNotification_ProductAvailableNotification_AddsCorrectParameters()
        {
            // Arrange
            var notification = new ProductAvailableNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                productId: 301
            );

            // Create a list to store the actual parameters
            var capturedParams = new List<(string Name, object Value)>();

            // Setup mock parameter
            var mockParameter = new Mock<IDbDataParameter>();
            mockParameter.SetupProperty(p => p.ParameterName);
            mockParameter.SetupProperty(p => p.Value);

            // Set up a collection to track parameters added to the command
            var parameters = new List<IDbDataParameter>();

            // Setup mock parameters collection to be enumerable
            _mockParameters.Setup(p => p.GetEnumerator())
                .Returns(() => parameters.GetEnumerator());

            // Setup command to return our mock parameter and capture its values
            _mockCommand.Setup(c => c.CreateParameter()).Returns(() => {
                var param = mockParameter.Object;
                param.ParameterName = null;
                param.Value = null;
                return param;
            });

            // Capture parameters when they're added to the collection
            _mockParameters.Setup(p => p.Add(It.IsAny<object>()))
                .Callback<object>(param => {
                    parameters.Add((IDbDataParameter)param);
                    var dbParam = (IDbDataParameter)param;
                    capturedParams.Add((dbParam.ParameterName, dbParam.Value));
                })
                .Returns(0);

            // Act
            _adapter.AddNotification(notification);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "AddNotification");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Check parameters
            AssertParameterExists(capturedParams, "@recipientID", 1);
            AssertParameterExists(capturedParams, "@category", "PRODUCT_AVAILABLE");
            AssertParameterExists(capturedParams, "@productID", 301);

            // Check null parameters
            AssertParameterExists(capturedParams, "@contractID", DBNull.Value);
            AssertParameterExists(capturedParams, "@isAccepted", DBNull.Value);
            AssertParameterExists(capturedParams, "@orderID", DBNull.Value);
            AssertParameterExists(capturedParams, "@shippingState", DBNull.Value);
            AssertParameterExists(capturedParams, "@deliveryDate", DBNull.Value);
            AssertParameterExists(capturedParams, "@expirationDate", DBNull.Value);
        }


        [TestMethod]
        public void AddNotification_OrderShippingProgressNotification_AddsCorrectParameters()
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
            var capturedParams = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParams);

            // Act
            _adapter.AddNotification(notification);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "AddNotification");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Check parameters
            AssertParameterExists(capturedParams, "@recipientID", 1);
            AssertParameterExists(capturedParams, "@category", "ORDER_SHIPPING_PROGRESS");
            AssertParameterExists(capturedParams, "@orderID", 201);
            AssertParameterExists(capturedParams, "@shippingState", "SHIPPED");
            AssertParameterExists(capturedParams, "@deliveryDate", deliveryDate);

            // Check null parameters
            AssertParameterExists(capturedParams, "@contractID", DBNull.Value);
            AssertParameterExists(capturedParams, "@isAccepted", DBNull.Value);
            AssertParameterExists(capturedParams, "@productID", DBNull.Value);
            AssertParameterExists(capturedParams, "@expirationDate", DBNull.Value);
        }

        [TestMethod]
        public void AddNotification_ContractExpirationNotification_AddsCorrectParameters()
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
            var capturedParams = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParams);

            // Act
            _adapter.AddNotification(notification);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "AddNotification");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Check parameters
            AssertParameterExists(capturedParams, "@recipientID", 1);
            AssertParameterExists(capturedParams, "@category", "CONTRACT_EXPIRATION");
            AssertParameterExists(capturedParams, "@contractID", 101);
            AssertParameterExists(capturedParams, "@expirationDate", expirationDate);

            // Check null parameters
            AssertParameterExists(capturedParams, "@isAccepted", DBNull.Value);
            AssertParameterExists(capturedParams, "@productID", DBNull.Value);
            AssertParameterExists(capturedParams, "@orderID", DBNull.Value);
            AssertParameterExists(capturedParams, "@shippingState", DBNull.Value);
            AssertParameterExists(capturedParams, "@deliveryDate", DBNull.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddNotification_UnknownNotificationType_ThrowsArgumentException()
        {
            // Arrange
            var notification = new Mock<Notification>().Object;

            // Act - should throw ArgumentException
            _adapter.AddNotification(notification);
        }


        [TestMethod]
        public void AddNotification_ContractRenewalWaitlistNotification_AddsCorrectParameters()
        {
            // Arrange
            var notification = new ContractRenewalWaitlistNotification(
                recipientID: 1,
                timestamp: DateTime.Now,
                productID: 201
            );

            // Capture parameters
            var capturedParams = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParams);

            // Act
            _adapter.AddNotification(notification);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "AddNotification");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Check parameters
            AssertParameterExists(capturedParams, "@recipientID", 1);
            AssertParameterExists(capturedParams, "@category", "CONTRACT_RENEWAL_WAITLIST");
            AssertParameterExists(capturedParams, "@productID", 201);

            // Check null parameters
            AssertParameterExists(capturedParams, "@contractID", DBNull.Value);
            AssertParameterExists(capturedParams, "@isAccepted", DBNull.Value);
            AssertParameterExists(capturedParams, "@orderID", DBNull.Value);
            AssertParameterExists(capturedParams, "@shippingState", DBNull.Value);
            AssertParameterExists(capturedParams, "@deliveryDate", DBNull.Value);
            AssertParameterExists(capturedParams, "@expirationDate", DBNull.Value);
        }

        [TestMethod]
        public void AddNotification_OutbiddedNotification_AddsCorrectParameters()
        {
            // Arrange
            var notification = new OutbiddedNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                productId: 301
            );

            // Capture parameters
            var capturedParams = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParams);

            // Act
            _adapter.AddNotification(notification);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "AddNotification");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Check parameters
            AssertParameterExists(capturedParams, "@recipientID", 1);
            AssertParameterExists(capturedParams, "@category", "OUTBIDDED");
            AssertParameterExists(capturedParams, "@productID", 301);

            // Check null parameters
            AssertParameterExists(capturedParams, "@contractID", DBNull.Value);
            AssertParameterExists(capturedParams, "@isAccepted", DBNull.Value);
            AssertParameterExists(capturedParams, "@orderID", DBNull.Value);
            AssertParameterExists(capturedParams, "@shippingState", DBNull.Value);
            AssertParameterExists(capturedParams, "@deliveryDate", DBNull.Value);
            AssertParameterExists(capturedParams, "@expirationDate", DBNull.Value);
        }

        [TestMethod]
        public void AddNotification_PaymentConfirmationNotification_AddsCorrectParameters()
        {
            // Arrange
            var notification = new PaymentConfirmationNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                productId: 301,
                orderId: 501
            );

            // Capture parameters
            var capturedParams = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParams);

            // Act
            _adapter.AddNotification(notification);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "AddNotification");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Check parameters
            AssertParameterExists(capturedParams, "@recipientID", 1);
            AssertParameterExists(capturedParams, "@category", "PAYMENT_CONFIRMATION");
            AssertParameterExists(capturedParams, "@orderID", 501);
            AssertParameterExists(capturedParams, "@productID", 301);

            // Check null parameters
            AssertParameterExists(capturedParams, "@contractID", DBNull.Value);
            AssertParameterExists(capturedParams, "@isAccepted", DBNull.Value);
            AssertParameterExists(capturedParams, "@shippingState", DBNull.Value);
            AssertParameterExists(capturedParams, "@deliveryDate", DBNull.Value);
            AssertParameterExists(capturedParams, "@expirationDate", DBNull.Value);
        }

        [TestMethod]
        public void AddNotification_ProductRemovedNotification_AddsCorrectParameters()
        {
            // Arrange
            var notification = new ProductRemovedNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                productId: 301
            );

            // Capture parameters
            var capturedParams = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParams);

            // Act
            _adapter.AddNotification(notification);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "AddNotification");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Check parameters
            AssertParameterExists(capturedParams, "@recipientID", 1);
            AssertParameterExists(capturedParams, "@category", "PRODUCT_REMOVED");
            AssertParameterExists(capturedParams, "@productID", 301);

            // Check null parameters
            AssertParameterExists(capturedParams, "@contractID", DBNull.Value);
            AssertParameterExists(capturedParams, "@isAccepted", DBNull.Value);
            AssertParameterExists(capturedParams, "@orderID", DBNull.Value);
            AssertParameterExists(capturedParams, "@shippingState", DBNull.Value);
            AssertParameterExists(capturedParams, "@deliveryDate", DBNull.Value);
            AssertParameterExists(capturedParams, "@expirationDate", DBNull.Value);
        }

        [TestMethod]
        public void AddNotification_ContractRenewalRequestNotification_AddsCorrectParameters()
        {
            // Arrange
            var notification = new ContractRenewalRequestNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                contractId: 101
            );

            // Capture parameters
            var capturedParams = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParams);

            // Act
            _adapter.AddNotification(notification);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "AddNotification");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Check parameters
            AssertParameterExists(capturedParams, "@recipientID", 1);
            AssertParameterExists(capturedParams, "@category", "CONTRACT_RENEWAL_REQUEST");
            AssertParameterExists(capturedParams, "@contractID", 101);

            // Check null parameters
            AssertParameterExists(capturedParams, "@isAccepted", DBNull.Value);
            AssertParameterExists(capturedParams, "@productID", DBNull.Value);
            AssertParameterExists(capturedParams, "@orderID", DBNull.Value);
            AssertParameterExists(capturedParams, "@shippingState", DBNull.Value);
            AssertParameterExists(capturedParams, "@deliveryDate", DBNull.Value);
            AssertParameterExists(capturedParams, "@expirationDate", DBNull.Value);
        }

        [TestMethod]
        public void AddNotification_ContractRenewalRequestNotificationWithNull_AddsCorrectParameters()
        {
            // Arrange
            var notification = new ContractRenewalRequestNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                contractId: 101
            );

            // Capture parameters
            var capturedParams = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParams);

            // Act
            _adapter.AddNotification(notification);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "AddNotification");
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Check parameters
            AssertParameterExists(capturedParams, "@recipientID", 1);
            AssertParameterExists(capturedParams, "@category", "CONTRACT_RENEWAL_REQUEST");
            AssertParameterExists(capturedParams, "@contractID", 101);

            // Check null parameters
            AssertParameterExists(capturedParams, "@isAccepted", DBNull.Value);
            AssertParameterExists(capturedParams, "@productID", DBNull.Value);
            AssertParameterExists(capturedParams, "@orderID", DBNull.Value);
            AssertParameterExists(capturedParams, "@shippingState", DBNull.Value);
            AssertParameterExists(capturedParams, "@deliveryDate", DBNull.Value);
            AssertParameterExists(capturedParams, "@expirationDate", DBNull.Value);
        }

        [TestMethod]
        public void AddNotification_HandlesConnectionClosed()
        {
            // Arrange
            // First, reset interactions with the connection mock
            // to clear the Open() call from the constructor
            Mock.Get(_mockConnection.Object).Invocations.Clear();

            // Now set up the closed state for this specific test
            _mockConnection.Setup(c => c.State).Returns(ConnectionState.Closed);

            var notification = new ProductAvailableNotification(
                recipientId: 1,
                timestamp: DateTime.Now,
                productId: 301
            );

            // Capture parameters with proper enumeration support
            var capturedParams = new List<(string Name, object Value)>();
            SetupParameterCapture(capturedParams);

            // Act
            _adapter.AddNotification(notification);

            // Assert
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);

            // Verify the parameters were still set correctly
            AssertParameterExists(capturedParams, "@recipientID", 1);
            AssertParameterExists(capturedParams, "@category", "PRODUCT_AVAILABLE");
            AssertParameterExists(capturedParams, "@productID", 301);
        }



        // Helper methods for testing
        private void SetupParameterCapture(List<(string Name, object Value)> capturedParams)
        {
            var mockParameter = new Mock<IDbDataParameter>();
            mockParameter.SetupProperty(p => p.ParameterName);
            mockParameter.SetupProperty(p => p.Value);

            // Set up a collection to track parameters added to the command
            var parameters = new List<IDbDataParameter>();

            // Setup mock parameters collection to be enumerable
            _mockParameters.Setup(p => p.GetEnumerator())
                .Returns(() => parameters.GetEnumerator());

            // Setup command to return our mock parameter and capture its values
            _mockCommand.Setup(c => c.CreateParameter()).Returns(() => {
                var param = mockParameter.Object;
                param.ParameterName = null;
                param.Value = null;
                return param;
            });

            // Capture parameters when they're added to the collection
            _mockParameters.Setup(p => p.Add(It.IsAny<object>()))
                .Callback<object>(param => {
                    parameters.Add((IDbDataParameter)param);
                    var dbParam = (IDbDataParameter)param;
                    capturedParams.Add((dbParam.ParameterName, dbParam.Value));
                })
                .Returns(0);
        }


        private void AssertParameterExists(List<(string Name, object Value)> parameters, string name, object expectedValue)
        {
            var param = parameters.FirstOrDefault(p => p.Name == name);
            Assert.IsTrue(param != default, $"Parameter {name} was not found");

            if (expectedValue == DBNull.Value)
            {
                Assert.AreEqual(DBNull.Value, param.Value, $"Parameter {name} should be DBNull");
            }
            else
            {
                Assert.AreEqual(expectedValue, param.Value, $"Parameter {name} has incorrect value");
            }
        }


        private void SetupReaderForNotifications(Mock<IDataReader> mockReader)
        {
            mockReader.Setup(r => r.GetOrdinal("notificationID")).Returns(0);
            mockReader.Setup(r => r.GetOrdinal("recipientID")).Returns(1);
            mockReader.Setup(r => r.GetOrdinal("timestamp")).Returns(2);
            mockReader.Setup(r => r.GetOrdinal("isRead")).Returns(3);
            mockReader.Setup(r => r.GetOrdinal("category")).Returns(4);
            mockReader.Setup(r => r.GetOrdinal("contractID")).Returns(5);
            mockReader.Setup(r => r.GetOrdinal("isAccepted")).Returns(6);
            mockReader.Setup(r => r.GetOrdinal("productID")).Returns(7);
            mockReader.Setup(r => r.GetOrdinal("orderID")).Returns(8);
            mockReader.Setup(r => r.GetOrdinal("shippingState")).Returns(9);
            mockReader.Setup(r => r.GetOrdinal("deliveryDate")).Returns(10);
            mockReader.Setup(r => r.GetOrdinal("expirationDate")).Returns(11);

            mockReader.SetupSequence(r => r.GetInt32(0)).Returns(42).Returns(43);
            mockReader.Setup(r => r.GetInt32(1)).Returns(1);
            mockReader.Setup(r => r.GetDateTime(2)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetBoolean(3)).Returns(false);

            // First row: CONTRACT_RENEWAL_ACCEPTED
            // Second row: PRODUCT_AVAILABLE
            mockReader.SetupSequence(r => r.GetString(4))
                .Returns("CONTRACT_RENEWAL_ACCEPTED")
                .Returns("PRODUCT_AVAILABLE");

            mockReader.Setup(r => r.GetInt32(5)).Returns(101); // contractID
            mockReader.Setup(r => r.GetBoolean(6)).Returns(true); // isAccepted
            mockReader.Setup(r => r.GetInt32(7)).Returns(201); // productID
        }
    }
}
