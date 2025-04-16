using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ArtAttack.Model;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Tests.Model
{
    [TestClass]
    public class OrderModelTests
    {
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private Mock<IDbDataParameter> _mockParameter;
        private Mock<IDatabaseProvider> _mockDatabaseProvider;
        private string _testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";
        private OrderModel _orderModel;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockReader = new Mock<IDataReader>();
            _mockParameters = new Mock<IDataParameterCollection>();
            _mockParameter = new Mock<IDbDataParameter>();
            _mockDatabaseProvider = new Mock<IDatabaseProvider>();

            // Setup the parameter collection mock with a default Add callback
            _mockParameters
                .Setup(parameterCollection => parameterCollection.Add(It.IsAny<object>()))
                .Returns(0);

            // Setup command mock for creating parameters and exposing the parameter collection
            _mockCommand
                .Setup(command => command.CreateParameter())
                .Returns(_mockParameter.Object);
            _mockCommand
                .Setup(command => command.Parameters)
                .Returns(_mockParameters.Object);

            // Setup connection mock to return the command mock
            _mockConnection
                .Setup(connection => connection.CreateCommand())
                .Returns(_mockCommand.Object);

            // Setup the database provider mock to return the connection mock
            _mockDatabaseProvider
                .Setup(provider => provider.CreateConnection(It.IsAny<string>()))
                .Returns(_mockConnection.Object);

            // Initialize the model with the mock database provider and connection string
            _orderModel = new OrderModel(_testConnectionString, _mockDatabaseProvider.Object);
        }

        /// <summary>
        /// Helper method to capture added parameters.
        /// </summary>
        private List<IDbDataParameter> CaptureParameters()
        {
            var capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(parameterCollection => parameterCollection.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(parameter =>
                {
                    capturedParameters.Add((IDbDataParameter)parameter);
                })
                .Returns(0);
            return capturedParameters;
        }

        /// <summary>
        /// Helper method to setup the standard column ordinals for order retrieval.
        /// </summary>
        private void SetupCommonOrderReaderOrdinals()
        {
            _mockReader.Setup(dataReader => dataReader.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(dataReader => dataReader.GetOrdinal("ProductID")).Returns(1);
            _mockReader.Setup(dataReader => dataReader.GetOrdinal("BuyerID")).Returns(2);
            _mockReader.Setup(dataReader => dataReader.GetOrdinal("OrderSummaryID")).Returns(3);
            _mockReader.Setup(dataReader => dataReader.GetOrdinal("OrderHistoryID")).Returns(4);
            _mockReader.Setup(dataReader => dataReader.GetOrdinal("ProductType")).Returns(5);
            _mockReader.Setup(dataReader => dataReader.GetOrdinal("PaymentMethod")).Returns(6);
            _mockReader.Setup(dataReader => dataReader.GetOrdinal("OrderDate")).Returns(7);
        }

        [TestMethod]
        public async Task AddOrderAsync_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int productId = 1;
            int buyerId = 2;
            int productType = 3;
            string paymentMethod = "Credit Card";
            int orderSummaryId = 4;
            DateTime orderDate = DateTime.Now;

            var capturedParameters = CaptureParameters();

            // Act
            await _orderModel.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate);

            // Assert
            // Verify stored procedure configuration
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "AddOrder");

            // Verify the command execution and connection opening
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);

            // Verify parameters (using one assert per aspect via a helper method)
            Assert.AreEqual(6, capturedParameters.Count, "Should have added 6 parameters");
            AssertParameterValue(capturedParameters, "@ProductID", productId);
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);
            AssertParameterValue(capturedParameters, "@ProductType", productType);
            AssertParameterValue(capturedParameters, "@PaymentMethod", paymentMethod);
            AssertParameterValue(capturedParameters, "@OrderSummaryID", orderSummaryId);
            AssertParameterValue(capturedParameters, "@OrderDate", orderDate);
        }

        [TestMethod]
        public async Task UpdateOrderAsync_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int orderId = 1;
            int productType = 2;
            string paymentMethod = "Credit Card";
            DateTime orderDate = DateTime.Now;

            var capturedParameters = CaptureParameters();

            // Act
            await _orderModel.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate);

            // Assert
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "UpdateOrder");

            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);

            Assert.AreEqual(4, capturedParameters.Count, "Should have added 4 parameters");
            AssertParameterValue(capturedParameters, "@OrderID", orderId);
            AssertParameterValue(capturedParameters, "@ProductType", productType);
            AssertParameterValue(capturedParameters, "@PaymentMethod", paymentMethod);
            AssertParameterValue(capturedParameters, "@OrderDate", orderDate);
        }

        [TestMethod]
        public async Task DeleteOrderAsync_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int orderId = 1;
            var capturedParameters = CaptureParameters();

            // Act
            await _orderModel.DeleteOrderAsync(orderId);

            // Assert
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "DeleteOrder");

            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);

            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@OrderID", orderId);
        }

        [TestMethod]
        public async Task GetBorrowedOrderHistoryAsync_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            DateTime orderDate = DateTime.Now;
            var capturedParameters = CaptureParameters();

            // Setup common column ordinals
            SetupCommonOrderReaderOrdinals();

            // Setup the IDataReader for two records
            _mockReader.SetupSequence(dataReader => dataReader.Read())
                .Returns(true)   // First record
                .Returns(true)   // Second record
                .Returns(false); // End of data

            // Setup values for the first and second order records
            _mockReader.SetupSequence(dataReader => dataReader.GetInt32(0))
                .Returns(101)
                .Returns(102); // OrderID

            _mockReader.SetupSequence(dataReader => dataReader.GetInt32(1))
                .Returns(201)
                .Returns(202); // ProductID

            _mockReader.SetupSequence(dataReader => dataReader.GetInt32(2))
                .Returns(buyerId)
                .Returns(buyerId); // BuyerID

            _mockReader.SetupSequence(dataReader => dataReader.GetInt32(3))
                .Returns(301)
                .Returns(302); // OrderSummaryID

            _mockReader.SetupSequence(dataReader => dataReader.GetInt32(4))
                .Returns(401)
                .Returns(402); // OrderHistoryID

            _mockReader.SetupSequence(dataReader => dataReader.GetInt32(5))
                .Returns(1)
                .Returns(2);   // ProductType

            _mockReader.SetupSequence(dataReader => dataReader.GetString(6))
                .Returns("Credit Card")
                .Returns("PayPal"); // PaymentMethod

            _mockReader.SetupSequence(dataReader => dataReader.GetDateTime(7))
                .Returns(orderDate)
                .Returns(orderDate.AddDays(-1)); // OrderDate

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = await _orderModel.GetBorrowedOrderHistoryAsync(buyerId);

            // Assert (using a single assert per record via a helper comparison)
            Assert.AreEqual(2, results.Count, "Should have returned two orders");

            // First order
            AssertOrderRecordEquality(new
            {
                OrderID = 101,
                ProductID = 201,
                BuyerID = buyerId,
                OrderSummaryID = 301,
                OrderHistoryID = 401,
                ProductType = 1,
                PaymentMethod = "Credit Card",
                OrderDate = orderDate
            }, results[0]);

            // Second order
            AssertOrderRecordEquality(new
            {
                OrderID = 102,
                ProductID = 202,
                BuyerID = buyerId,
                OrderSummaryID = 302,
                OrderHistoryID = 402,
                ProductType = 2,
                PaymentMethod = "PayPal",
                OrderDate = orderDate.AddDays(-1)
            }, results[1]);

            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "get_borrowed_order_history");

            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);

            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
        }

        [TestMethod]
        public async Task GetNewOrUsedOrderHistoryAsync_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            DateTime orderDate = DateTime.Now;
            var capturedParameters = CaptureParameters();

            SetupCommonOrderReaderOrdinals();

            _mockReader.SetupSequence(dataReader => dataReader.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            // Setup values for the record
            _mockReader.Setup(dataReader => dataReader.GetInt32(0)).Returns(101);           // OrderID
            _mockReader.Setup(dataReader => dataReader.GetInt32(1)).Returns(201);           // ProductID
            _mockReader.Setup(dataReader => dataReader.GetInt32(2)).Returns(buyerId);         // BuyerID
            _mockReader.Setup(dataReader => dataReader.GetInt32(3)).Returns(301);             // OrderSummaryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(4)).Returns(401);             // OrderHistoryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(5)).Returns(3);               // ProductType
            _mockReader.Setup(dataReader => dataReader.GetString(6)).Returns("Credit Card");  // PaymentMethod
            _mockReader.Setup(dataReader => dataReader.GetDateTime(7)).Returns(orderDate);      // OrderDate

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = await _orderModel.GetNewOrUsedOrderHistoryAsync(buyerId);

            // Assert
            Assert.AreEqual(1, results.Count, "Should have returned one order");
            AssertOrderRecordEquality(new
            {
                OrderID = 101,
                ProductID = 201,
                BuyerID = buyerId,
                OrderSummaryID = 301,
                OrderHistoryID = 401,
                ProductType = 3,
                PaymentMethod = "Credit Card",
                OrderDate = orderDate
            }, results[0]);

            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "get_new_or_used_order_history");

            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);

            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
        }

        [TestMethod]
        public void GetOrdersFromLastThreeMonths_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            DateTime orderDate = DateTime.Now;
            var capturedParameters = CaptureParameters();

            SetupCommonOrderReaderOrdinals();

            _mockReader.SetupSequence(dataReader => dataReader.Read())
                .Returns(true)    // First record
                .Returns(false);  // End of data

            _mockReader.Setup(dataReader => dataReader.GetInt32(0)).Returns(101);            // OrderID
            _mockReader.Setup(dataReader => dataReader.GetInt32(1)).Returns(201);            // ProductID
            _mockReader.Setup(dataReader => dataReader.GetInt32(2)).Returns(buyerId);          // BuyerID
            _mockReader.Setup(dataReader => dataReader.GetInt32(3)).Returns(301);            // OrderSummaryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(4)).Returns(401);            // OrderHistoryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(5)).Returns(1);              // ProductType
            _mockReader.Setup(dataReader => dataReader.GetString(6)).Returns("Credit Card"); // PaymentMethod
            _mockReader.Setup(dataReader => dataReader.GetDateTime(7)).Returns(orderDate.AddMonths(-2)); // OrderDate

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = _orderModel.GetOrdersFromLastThreeMonths(buyerId);

            // Assert
            Assert.AreEqual(1, results.Count, "Should have returned one order");
            // Here we check only a couple of key fields as an example
            Assert.AreEqual(101, results[0].OrderID, "OrderID mismatch");
            Assert.AreEqual(orderDate.AddMonths(-2), results[0].OrderDate, "OrderDate mismatch");

            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "get_orders_from_last_3_months");

            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);

            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
        }

        [TestMethod]
        public void GetOrdersFromLastSixMonths_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            DateTime orderDate = DateTime.Now;
            var capturedParameters = CaptureParameters();

            SetupCommonOrderReaderOrdinals();

            _mockReader.SetupSequence(dataReader => dataReader.Read())
                .Returns(true)    // First record
                .Returns(false);  // End of data

            _mockReader.Setup(dataReader => dataReader.GetInt32(0)).Returns(101);            // OrderID
            _mockReader.Setup(dataReader => dataReader.GetInt32(1)).Returns(201);            // ProductID
            _mockReader.Setup(dataReader => dataReader.GetInt32(2)).Returns(buyerId);          // BuyerID
            _mockReader.Setup(dataReader => dataReader.GetInt32(3)).Returns(301);            // OrderSummaryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(4)).Returns(401);            // OrderHistoryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(5)).Returns(1);              // ProductType
            _mockReader.Setup(dataReader => dataReader.GetString(6)).Returns("Credit Card"); // PaymentMethod
            _mockReader.Setup(dataReader => dataReader.GetDateTime(7)).Returns(orderDate.AddMonths(-5)); // OrderDate

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = _orderModel.GetOrdersFromLastSixMonths(buyerId);

            // Assert
            Assert.AreEqual(1, results.Count, "Should have returned one order");
            Assert.AreEqual(101, results[0].OrderID, "OrderID mismatch");
            Assert.AreEqual(orderDate.AddMonths(-5), results[0].OrderDate, "OrderDate mismatch");

            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "get_orders_from_last_6_months");

            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);

            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
        }

        [TestMethod]
        public void GetOrdersByName_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            string searchText = "test";
            DateTime orderDate = DateTime.Now;
            var capturedParameters = CaptureParameters();

            SetupCommonOrderReaderOrdinals();

            _mockReader.SetupSequence(dataReader => dataReader.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            _mockReader.Setup(dataReader => dataReader.GetInt32(0)).Returns(101);            // OrderID
            _mockReader.Setup(dataReader => dataReader.GetInt32(1)).Returns(201);            // ProductID
            _mockReader.Setup(dataReader => dataReader.GetInt32(2)).Returns(buyerId);          // BuyerID
            _mockReader.Setup(dataReader => dataReader.GetInt32(3)).Returns(301);            // OrderSummaryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(4)).Returns(401);            // OrderHistoryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(5)).Returns(1);              // ProductType
            _mockReader.Setup(dataReader => dataReader.GetString(6)).Returns("Credit Card"); // PaymentMethod
            _mockReader.Setup(dataReader => dataReader.GetDateTime(7)).Returns(orderDate);     // OrderDate

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = _orderModel.GetOrdersByName(buyerId, searchText);

            // Assert
            Assert.AreEqual(1, results.Count, "Should have returned one order");
            Assert.AreEqual(101, results[0].OrderID, "OrderID mismatch");

            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "get_orders_by_name");

            Assert.AreEqual(2, capturedParameters.Count, "Should have added 2 parameters");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);
            AssertParameterValue(capturedParameters, "@text", searchText);

            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
        }

        [TestMethod]
        public void GetOrdersFrom2024_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            DateTime orderDate = new DateTime(2024, 3, 10);
            var capturedParameters = CaptureParameters();

            SetupCommonOrderReaderOrdinals();

            _mockReader.SetupSequence(dataReader => dataReader.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            _mockReader.Setup(dataReader => dataReader.GetInt32(0)).Returns(101);            // OrderID
            _mockReader.Setup(dataReader => dataReader.GetInt32(1)).Returns(201);            // ProductID
            _mockReader.Setup(dataReader => dataReader.GetInt32(2)).Returns(buyerId);          // BuyerID
            _mockReader.Setup(dataReader => dataReader.GetInt32(3)).Returns(301);            // OrderSummaryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(4)).Returns(401);            // OrderHistoryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(5)).Returns(1);              // ProductType
            _mockReader.Setup(dataReader => dataReader.GetString(6)).Returns("Credit Card"); // PaymentMethod
            _mockReader.Setup(dataReader => dataReader.GetDateTime(7)).Returns(orderDate);     // OrderDate

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = _orderModel.GetOrdersFrom2024(buyerId);

            // Assert
            Assert.AreEqual(1, results.Count, "Should have returned one order");
            Assert.AreEqual(101, results[0].OrderID, "OrderID mismatch");
            Assert.AreEqual(orderDate, results[0].OrderDate, "OrderDate mismatch");

            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "get_orders_from_2024");

            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);

            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
        }

        [TestMethod]
        public void GetOrdersFrom2025_ReturnsCorrectData()
        {
            // Arrange
            int buyerId = 42;
            DateTime orderDate = new DateTime(2025, 6, 15);
            var capturedParameters = CaptureParameters();

            SetupCommonOrderReaderOrdinals();

            _mockReader.SetupSequence(dataReader => dataReader.Read())
                .Returns(true)   // First record
                .Returns(false); // End of data

            _mockReader.Setup(dataReader => dataReader.GetInt32(0)).Returns(101);            // OrderID
            _mockReader.Setup(dataReader => dataReader.GetInt32(1)).Returns(201);            // ProductID
            _mockReader.Setup(dataReader => dataReader.GetInt32(2)).Returns(buyerId);          // BuyerID
            _mockReader.Setup(dataReader => dataReader.GetInt32(3)).Returns(301);            // OrderSummaryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(4)).Returns(401);            // OrderHistoryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(5)).Returns(1);              // ProductType
            _mockReader.Setup(dataReader => dataReader.GetString(6)).Returns("Credit Card"); // PaymentMethod
            _mockReader.Setup(dataReader => dataReader.GetDateTime(7)).Returns(orderDate);     // OrderDate

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = _orderModel.GetOrdersFrom2025(buyerId);

            // Assert
            Assert.AreEqual(1, results.Count, "Should have returned one order");
            Assert.AreEqual(101, results[0].OrderID, "OrderID mismatch");
            Assert.AreEqual(orderDate, results[0].OrderDate, "OrderDate mismatch");

            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "get_orders_from_2025");

            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@BuyerID", buyerId);

            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromOrderHistoryAsync_ReturnsCorrectData()
        {
            // Arrange
            int orderHistoryId = 42;
            DateTime orderDate = DateTime.Now;
            var capturedParameters = CaptureParameters();

            SetupCommonOrderReaderOrdinals();

            // Setup the IDataReader for one record
            _mockReader.SetupSequence(dataReader => dataReader.Read())
                .Returns(true)    // First record
                .Returns(false);  // End of data

            // Setup IsDBNull (assume non-null for these columns)
            _mockReader.Setup(dataReader => dataReader.IsDBNull(6)).Returns(false);
            _mockReader.Setup(dataReader => dataReader.IsDBNull(7)).Returns(false);

            _mockReader.Setup(dataReader => dataReader.GetInt32(0)).Returns(101);            // OrderID
            _mockReader.Setup(dataReader => dataReader.GetInt32(1)).Returns(201);            // ProductID
            _mockReader.Setup(dataReader => dataReader.GetInt32(2)).Returns(301);            // BuyerID
            _mockReader.Setup(dataReader => dataReader.GetInt32(3)).Returns(401);            // OrderSummaryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(4)).Returns(orderHistoryId);   // OrderHistoryID
            _mockReader.Setup(dataReader => dataReader.GetInt32(5)).Returns(1);              // ProductType
            _mockReader.Setup(dataReader => dataReader.GetString(6)).Returns("Credit Card"); // PaymentMethod
            _mockReader.Setup(dataReader => dataReader.GetDateTime(7)).Returns(orderDate);     // OrderDate

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = await _orderModel.GetOrdersFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.AreEqual(1, results.Count, "Should have returned one order");
            AssertOrderRecordEquality(new
            {
                OrderID = 101,
                ProductID = 201,
                BuyerID = 301,
                OrderSummaryID = 401,
                OrderHistoryID = orderHistoryId,
                ProductType = 1,
                PaymentMethod = "Credit Card",
                OrderDate = orderDate
            }, results[0]);

            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "get_orders_from_order_history");

            Assert.AreEqual(1, capturedParameters.Count, "Should have added 1 parameter");
            AssertParameterValue(capturedParameters, "@OrderHistoryID", orderHistoryId);

            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromOrderHistoryAsync_HandlesNullValues_ReturnsCorrectData()
        {
            // Arrange
            int orderHistoryId = 42;

            SetupCommonOrderReaderOrdinals();

            // Setup the IDataReader for one record with null PaymentMethod and OrderDate
            _mockReader.SetupSequence(dataReader => dataReader.Read())
                .Returns(true)
                .Returns(false);

            _mockReader.Setup(dataReader => dataReader.IsDBNull(6)).Returns(true);
            _mockReader.Setup(dataReader => dataReader.IsDBNull(7)).Returns(true);

            _mockReader.Setup(dataReader => dataReader.GetInt32(0)).Returns(101);
            _mockReader.Setup(dataReader => dataReader.GetInt32(1)).Returns(201);
            _mockReader.Setup(dataReader => dataReader.GetInt32(2)).Returns(301);
            _mockReader.Setup(dataReader => dataReader.GetInt32(3)).Returns(401);
            _mockReader.Setup(dataReader => dataReader.GetInt32(4)).Returns(orderHistoryId);
            _mockReader.Setup(dataReader => dataReader.GetInt32(5)).Returns(1);

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var results = await _orderModel.GetOrdersFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.AreEqual(1, results.Count, "Should have returned one order");
            Assert.AreEqual("", results[0].PaymentMethod, "PaymentMethod should be empty for null values");
            Assert.AreEqual(DateTime.MinValue, results[0].OrderDate, "OrderDate should be DateTime.MinValue for null values");
        }

        [TestMethod]
        public void Constructor_SetsConnectionString()
        {
            // Arrange & Act
            var model = new OrderModel(_testConnectionString);

            // Assert using reflection to access the private field
            var fieldInfo = typeof(OrderModel).GetField("connectionString",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var actualValue = fieldInfo.GetValue(model);
            Assert.AreEqual(_testConnectionString, actualValue, "The connection string was not set correctly in the constructor");
        }

        [TestMethod]
        public void ConnectionString_Property_ReturnsCorrectValue()
        {
            // Arrange
            string expectedConnectionString = _testConnectionString;

            // Act
            string actualConnectionString = _orderModel.ConnectionString;

            // Assert
            Assert.AreEqual(expectedConnectionString, actualConnectionString,
                "The ConnectionString property should return the value of the private connectionString field");
        }

        private void AssertParameterValue(List<IDbDataParameter> parameters, string parameterName, object expectedValue)
        {
            var parameter = parameters.FirstOrDefault(parameter => parameter.ParameterName == parameterName);
            Assert.IsNotNull(parameter, $"Parameter {parameterName} not found");
            Assert.AreEqual(expectedValue, parameter.Value, $"Parameter {parameterName} has an incorrect value");
        }

        private void AssertOrderRecordEquality(dynamic expected, dynamic actual)
        {
            Assert.IsTrue(
                expected.OrderID == actual.OrderID &&
                expected.ProductID == actual.ProductID &&
                expected.BuyerID == actual.BuyerID &&
                expected.OrderSummaryID == actual.OrderSummaryID &&
                expected.OrderHistoryID == actual.OrderHistoryID &&
                expected.ProductType == actual.ProductType &&
                expected.PaymentMethod == actual.PaymentMethod &&
                expected.OrderDate == actual.OrderDate,
                $"Order record does not match. Expected: [OrderID={expected.OrderID}, ProductID={expected.ProductID}, BuyerID={expected.BuyerID}, " +
                $"OrderSummaryID={expected.OrderSummaryID}, OrderHistoryID={expected.OrderHistoryID}, ProductType={expected.ProductType}, " +
                $"PaymentMethod={expected.PaymentMethod}, OrderDate={expected.OrderDate}], " +
                $"Actual: [OrderID={actual.OrderID}, ProductID={actual.ProductID}, BuyerID={actual.BuyerID}, OrderSummaryID={actual.OrderSummaryID}, " +
                $"OrderHistoryID={actual.OrderHistoryID}, ProductType={actual.ProductType}, PaymentMethod={actual.PaymentMethod}, OrderDate={actual.OrderDate}]"
            );
        }
    }
}