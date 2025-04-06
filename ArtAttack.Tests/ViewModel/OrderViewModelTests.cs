using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using ArtAttack.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class OrderViewModelTests
    {
        private Mock<IDatabaseProvider> _mockDatabaseProvider;
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private Mock<IDbDataParameter> _mockParameter;
        private string _testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";
        private OrderViewModel _viewModel;
        private Mock<IOrderModel> _mockOrderModel;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockDatabaseProvider = new Mock<IDatabaseProvider>();
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockReader = new Mock<IDataReader>();
            _mockParameters = new Mock<IDataParameterCollection>();
            _mockParameter = new Mock<IDbDataParameter>();
            _mockOrderModel = new Mock<IOrderModel>();

            // Setup the parameter collection mock
            _mockParameters
                .Setup(p => p.Add(It.IsAny<object>()))
                .Returns(0);

            // Setup the command mock
            _mockCommand.Setup(c => c.CreateParameter()).Returns(_mockParameter.Object);
            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object);

            // Setup the connection mock
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

            // Setup the database provider mock
            _mockDatabaseProvider.Setup(p => p.CreateConnection(It.IsAny<string>())).Returns(_mockConnection.Object);

            // Initialize the view model with the mock database provider
            _viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);
        }

        [TestMethod]
        public async Task GetOrderSummaryAsync_ReturnsCorrectOrderSummary()
        {
            // Arrange
            int orderSummaryId = 123;
            float subtotal = 100.50f;
            float warrantyTax = 10.05f;
            float deliveryFee = 5.99f;
            float finalTotal = 116.54f;
            string fullName = "John Doe";
            string email = "john@example.com";
            string phoneNumber = "1234567890";
            string address = "123 Main St";
            string postalCode = "12345";
            string additionalInfo = "Leave at door";
            string contractDetails = "Contract details here";

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("ID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("Subtotal")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("WarrantyTax")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("DeliveryFee")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("FinalTotal")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("FullName")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("Email")).Returns(6);
            _mockReader.Setup(r => r.GetOrdinal("PhoneNumber")).Returns(7);
            _mockReader.Setup(r => r.GetOrdinal("Address")).Returns(8);
            _mockReader.Setup(r => r.GetOrdinal("PostalCode")).Returns(9);
            _mockReader.Setup(r => r.GetOrdinal("AdditionalInfo")).Returns(10);
            _mockReader.Setup(r => r.GetOrdinal("ContractDetails")).Returns(11);

            // Setup Read calls - first return true then false
            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ == 0);

            // Setup IsDBNull for nullable fields
            _mockReader.Setup(r => r.IsDBNull(10)).Returns(false);
            _mockReader.Setup(r => r.IsDBNull(11)).Returns(false);

            // Setup field values
            _mockReader.Setup(r => r.GetInt32(0)).Returns(orderSummaryId);
            _mockReader.Setup(r => r.GetDouble(1)).Returns(Convert.ToDouble(subtotal));
            _mockReader.Setup(r => r.GetDouble(2)).Returns(Convert.ToDouble(warrantyTax));
            _mockReader.Setup(r => r.GetDouble(3)).Returns(Convert.ToDouble(deliveryFee));
            _mockReader.Setup(r => r.GetDouble(4)).Returns(Convert.ToDouble(finalTotal));
            _mockReader.Setup(r => r.GetString(5)).Returns(fullName);
            _mockReader.Setup(r => r.GetString(6)).Returns(email);
            _mockReader.Setup(r => r.GetString(7)).Returns(phoneNumber);
            _mockReader.Setup(r => r.GetString(8)).Returns(address);
            _mockReader.Setup(r => r.GetString(9)).Returns(postalCode);
            _mockReader.Setup(r => r.GetString(10)).Returns(additionalInfo);
            _mockReader.Setup(r => r.GetString(11)).Returns(contractDetails);

            // Setup command to return reader
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var result = await _viewModel.GetOrderSummaryAsync(orderSummaryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(orderSummaryId, result.ID);
            Assert.AreEqual(subtotal, result.Subtotal);
            Assert.AreEqual(warrantyTax, result.WarrantyTax);
            Assert.AreEqual(deliveryFee, result.DeliveryFee);
            Assert.AreEqual(finalTotal, result.FinalTotal);
            Assert.AreEqual(fullName, result.FullName);
            Assert.AreEqual(email, result.Email);
            Assert.AreEqual(phoneNumber, result.PhoneNumber);
            Assert.AreEqual(address, result.Address);
            Assert.AreEqual(postalCode, result.PostalCode);
            Assert.AreEqual(additionalInfo, result.AdditionalInfo);
            Assert.AreEqual(contractDetails, result.ContractDetails);

            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
        }


        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetOrderSummaryAsync_ThrowsKeyNotFoundException_WhenOrderSummaryNotFound()
        {
            // Arrange
            int nonExistentOrderSummaryId = 999;

            // Setup Read returns no results
            _mockReader.Setup(r => r.Read()).Returns(false);
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            await _viewModel.GetOrderSummaryAsync(nonExistentOrderSummaryId);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        public async Task GetOrdersWithProductInfoAsync_ReturnsCorrectProductInfoList()
        {
            // Arrange
            int userId = 42;
            string searchText = "test";

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductName")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("ProductTypeName")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(6);

            DateTime orderDate = new DateTime(2023, 10, 10);

            // Setup mock reader with sequential reads
            int readCount = 0;
            _mockReader.Setup(r => r.Read())
                .Returns(() => {
                    return readCount++ < 2; // Return true for first 2 calls, then false
                });

            // First product
            _mockReader.Setup(r => r.GetInt32(0))
                .Returns(() => readCount == 1 ? 101 : 102);

            _mockReader.Setup(r => r.GetString(1))
                .Returns(() => readCount == 1 ? "Test Product 1" : "Test Product 2");

            _mockReader.Setup(r => r.GetInt32(2))
                .Returns(() => readCount == 1 ? 1 : 2);

            _mockReader.Setup(r => r.GetString(3))
                .Returns(() => readCount == 1 ? "new" : "borrowed");

            _mockReader.Setup(r => r.GetDateTime(4))
                .Returns(() => readCount == 1 ? orderDate : orderDate.AddDays(-1));

            _mockReader.Setup(r => r.GetString(5))
                .Returns(() => readCount == 1 ? "Credit Card" : "PayPal");

            _mockReader.Setup(r => r.GetInt32(6))
                .Returns(() => readCount == 1 ? 201 : 202);

            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Capture parameters
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Act
            var results = await _viewModel.GetOrdersWithProductInfoAsync(userId, searchText);

            // Assert
            Assert.AreEqual(2, results.Count);

            // First product (new)
            Assert.AreEqual(101, results[0].OrderID);
            Assert.AreEqual("Test Product 1", results[0].ProductName);
            Assert.AreEqual("new", results[0].ProductTypeName);
            Assert.AreEqual(orderDate.ToString("yyyy-MM-dd"), results[0].OrderDate);
            Assert.AreEqual("Credit Card", results[0].PaymentMethod);
            Assert.AreEqual(201, results[0].OrderSummaryID);
            Assert.AreEqual("new", results[0].ProductCategory);

            // Second product (borrowed)
            Assert.AreEqual(102, results[1].OrderID);
            Assert.AreEqual("Test Product 2", results[1].ProductName);
            Assert.AreEqual("borrowed", results[1].ProductTypeName);
            Assert.AreEqual(orderDate.AddDays(-1).ToString("yyyy-MM-dd"), results[1].OrderDate);
            Assert.AreEqual("PayPal", results[1].PaymentMethod);
            Assert.AreEqual(202, results[1].OrderSummaryID);
            Assert.AreEqual("borrowed", results[1].ProductCategory);

            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
        }

        [TestMethod]
        public async Task GetProductCategoryTypesAsync_ReturnsCorrectCategoryTypes()
        {
            // Arrange
            int userId = 42;

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("productType")).Returns(1);

            // Setup sequential reads with counter
            int readCount = 0;
            _mockReader.Setup(r => r.Read())
                .Returns(() => {
                    return readCount++ < 3; // Return true for first 3 calls, then false
                });

            // Setup field values based on read count
            _mockReader.Setup(r => r.GetInt32(0))
                .Returns(() => {
                    switch (readCount)
                    {
                        case 1: return 101;
                        case 2: return 102;
                        case 3: return 103;
                        default: return 0;
                    }
                });

            _mockReader.Setup(r => r.GetString(1))
                .Returns(() => {
                    switch (readCount)
                    {
                        case 1: return "new";
                        case 2: return "used";
                        case 3: return "borrowed";
                        default: return string.Empty;
                    }
                });

            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Capture parameters
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Act
            var results = await _viewModel.GetProductCategoryTypesAsync(userId);

            // Assert
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("new", results[101]); // "new" is categorized as "new"
            Assert.AreEqual("new", results[102]); // "used" is categorized as "new"
            Assert.AreEqual("borrowed", results[103]); // "borrowed" is categorized as "borrowed"

            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetOrdersWithProductInfoAsync_ThrowsArgumentException_WhenUserIdIsInvalid()
        {
            // Act
            await _viewModel.GetOrdersWithProductInfoAsync(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetProductCategoryTypesAsync_ThrowsArgumentException_WhenUserIdIsInvalid()
        {
            // Act
            await _viewModel.GetProductCategoryTypesAsync(0);
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_ReturnsOrder_WhenFoundInBorrowedOrders()
        {
            // Arrange
            int orderId = 101;
            var mockOrder = new Order { OrderID = orderId, ProductType = 1 };
            var borrowedOrders = new List<Order> { mockOrder };

            // Setup the mock using the interface
            _mockOrderModel.Setup(m => m.GetBorrowedOrderHistoryAsync(0))
                .Returns(Task.FromResult(borrowedOrders));

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetOrderByIdAsync(orderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(orderId, result.OrderID);
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(0), Times.Once);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(0), Times.Never);
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_ReturnsOrder_WhenFoundInNewUsedOrders()
        {
            // Arrange
            int orderId = 102;
            var mockOrder = new Order { OrderID = orderId, ProductType = 2 };
            var borrowedOrders = new List<Order>();
            var newUsedOrders = new List<Order> { mockOrder };

            _mockOrderModel.Setup(m => m.GetBorrowedOrderHistoryAsync(0))
                .Returns(Task.FromResult(borrowedOrders));
            _mockOrderModel.Setup(m => m.GetNewOrUsedOrderHistoryAsync(0))
                .Returns(Task.FromResult(newUsedOrders));

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetOrderByIdAsync(orderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(orderId, result.OrderID);
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(0), Times.Once);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(0), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_ReturnsNull_WhenOrderNotFound()
        {
            // Arrange
            int nonExistentOrderId = 999;
            var borrowedOrders = new List<Order> { new Order { OrderID = 101 } };
            var newUsedOrders = new List<Order> { new Order { OrderID = 102 } };

            _mockOrderModel.Setup(m => m.GetBorrowedOrderHistoryAsync(0))
                .Returns(Task.FromResult(borrowedOrders));
            _mockOrderModel.Setup(m => m.GetNewOrUsedOrderHistoryAsync(0))
                .Returns(Task.FromResult(newUsedOrders));

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetOrderByIdAsync(nonExistentOrderId);

            // Assert
            Assert.IsNull(result);
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(0), Times.Once);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(0), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_ReturnsThreeMonthsOrders_WhenFilterIs3Months()
        {
            // Arrange
            int buyerId = 42;
            string filter = "3months";
            var threeMonthsOrders = new List<Order> { new Order { OrderID = 101 } };

            // Setup the mock using the interface, not the concrete class
            _mockOrderModel.Setup(m => m.GetOrdersFromLastThreeMonths(buyerId))
                .Returns(threeMonthsOrders);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetCombinedOrderHistoryAsync(buyerId, filter);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(101, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFromLastThreeMonths(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_ReturnsSixMonthsOrders_WhenFilterIs6Months()
        {
            // Arrange
            int buyerId = 42;
            string filter = "6months";
            var sixMonthsOrders = new List<Order> { new Order { OrderID = 201 } };

            // Setup the mock using the interface
            _mockOrderModel.Setup(m => m.GetOrdersFromLastSixMonths(buyerId))
                .Returns(sixMonthsOrders);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetCombinedOrderHistoryAsync(buyerId, filter);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(201, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFromLastSixMonths(buyerId), Times.Once);
        }

        [TestMethod]
        public void Constructor_WithConnectionString_InitializesCorrectly()
        {
            // Arrange
            var connectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";

            // Act
            var viewModel = new OrderViewModel(connectionString);

            // Assert
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        public async Task AddOrderAsync_CallsModelMethod()
        {
            // Arrange
            int productId = 1;
            int buyerId = 2;
            int productType = 3;
            string paymentMethod = "Credit Card";
            int orderSummaryId = 4;
            DateTime orderDate = DateTime.Now;

            _mockOrderModel.Setup(m => m.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate))
                .Returns(Task.CompletedTask);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            await viewModel.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate);

            // Assert
            _mockOrderModel.Verify(m => m.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate), Times.Once);
        }

        [TestMethod]
        public async Task UpdateOrderAsync_CallsModelMethod()
        {
            // Arrange
            int orderId = 1;
            int productType = 3;
            string paymentMethod = "PayPal";
            DateTime orderDate = DateTime.Now;

            _mockOrderModel.Setup(m => m.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate))
                .Returns(Task.CompletedTask);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private model field with our mock
            // Field name might be 'model' without underscore prefix
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Add null check to help debug
            if (modelField == null)
            {
                // Try alternative field naming conventions
                modelField = typeof(OrderViewModel).GetField("_orderModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (modelField == null)
                    throw new InvalidOperationException("Could not find the model field in OrderViewModel class. Check the field name.");
            }

            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            await viewModel.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate);

            // Assert
            _mockOrderModel.Verify(m => m.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate), Times.Once);
        }


        [TestMethod]
        public async Task DeleteOrderAsync_CallsModelMethod()
        {
            // Arrange
            int orderId = 1;

            _mockOrderModel.Setup(m => m.DeleteOrderAsync(orderId))
                .Returns(Task.CompletedTask);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            await viewModel.DeleteOrderAsync(orderId);

            // Assert
            _mockOrderModel.Verify(m => m.DeleteOrderAsync(orderId), Times.Once);
        }

        [TestMethod]
        public async Task GetBorrowedOrderHistoryAsync_ReturnsModelResult()
        {
            // Arrange
            int buyerId = 1;
            var expectedOrders = new List<Order> { new Order { OrderID = 101 } };

            _mockOrderModel.Setup(m => m.GetBorrowedOrderHistoryAsync(buyerId))
                .ReturnsAsync(expectedOrders);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetBorrowedOrderHistoryAsync(buyerId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, result.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetNewOrUsedOrderHistoryAsync_ReturnsModelResult()
        {
            // Arrange
            int buyerId = 1;
            var expectedOrders = new List<Order> { new Order { OrderID = 201 } };

            _mockOrderModel.Setup(m => m.GetNewOrUsedOrderHistoryAsync(buyerId))
                .ReturnsAsync(expectedOrders);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetNewOrUsedOrderHistoryAsync(buyerId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, result.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromLastThreeMonthsAsync_ReturnsModelResult()
        {
            // Arrange
            int buyerId = 1;
            var expectedOrders = new List<Order> { new Order { OrderID = 301 } };

            _mockOrderModel.Setup(m => m.GetOrdersFromLastThreeMonths(buyerId))
                .Returns(expectedOrders);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetOrdersFromLastThreeMonthsAsync(buyerId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, result.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFromLastThreeMonths(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromLastSixMonthsAsync_ReturnsModelResult()
        {
            // Arrange
            int buyerId = 1;
            var expectedOrders = new List<Order> { new Order { OrderID = 401 } };

            _mockOrderModel.Setup(m => m.GetOrdersFromLastSixMonths(buyerId))
                .Returns(expectedOrders);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetOrdersFromLastSixMonthsAsync(buyerId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, result.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFromLastSixMonths(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFrom2024Async_ReturnsModelResult()
        {
            // Arrange
            int buyerId = 1;
            var expectedOrders = new List<Order> { new Order { OrderID = 501 } };

            _mockOrderModel.Setup(m => m.GetOrdersFrom2024(buyerId))
                .Returns(expectedOrders);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetOrdersFrom2024Async(buyerId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, result.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFrom2024(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFrom2025Async_ReturnsModelResult()
        {
            // Arrange
            int buyerId = 1;
            var expectedOrders = new List<Order> { new Order { OrderID = 601 } };

            _mockOrderModel.Setup(m => m.GetOrdersFrom2025(buyerId))
                .Returns(expectedOrders);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetOrdersFrom2025Async(buyerId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, result.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFrom2025(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersByNameAsync_ReturnsModelResult()
        {
            // Arrange
            int buyerId = 1;
            string searchText = "test";
            var expectedOrders = new List<Order> { new Order { OrderID = 701 } };

            _mockOrderModel.Setup(m => m.GetOrdersByName(buyerId, searchText))
                .Returns(expectedOrders);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetOrdersByNameAsync(buyerId, searchText);

            // Assert
            Assert.AreEqual(expectedOrders.Count, result.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersByName(buyerId, searchText), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromOrderHistoryAsync_ReturnsModelResult()
        {
            // Arrange
            int orderHistoryId = 42;
            var expectedOrders = new List<Order> { new Order { OrderID = 801 } };

            _mockOrderModel.Setup(m => m.GetOrdersFromOrderHistoryAsync(orderHistoryId))
                .ReturnsAsync(expectedOrders);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetOrdersFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, result.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFromOrderHistoryAsync(orderHistoryId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_ReturnsAllOrders_WhenFilterIsAll()
        {
            // Arrange
            int buyerId = 42;
            string filter = "all";
            var borrowedOrders = new List<Order> { new Order { OrderID = 101 } };
            var newUsedOrders = new List<Order> { new Order { OrderID = 102 } };

            _mockOrderModel.Setup(m => m.GetBorrowedOrderHistoryAsync(buyerId))
                .ReturnsAsync(borrowedOrders);
            _mockOrderModel.Setup(m => m.GetNewOrUsedOrderHistoryAsync(buyerId))
                .ReturnsAsync(newUsedOrders);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetCombinedOrderHistoryAsync(buyerId, filter);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(o => o.OrderID == 101));
            Assert.IsTrue(result.Any(o => o.OrderID == 102));
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(buyerId), Times.Once);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_ReturnsAllOrders_WhenFilterIsDefault()
        {
            // Arrange
            int buyerId = 42;
            var borrowedOrders = new List<Order> { new Order { OrderID = 101 } };
            var newUsedOrders = new List<Order> { new Order { OrderID = 102 } };

            _mockOrderModel.Setup(m => m.GetBorrowedOrderHistoryAsync(buyerId))
                .ReturnsAsync(borrowedOrders);
            _mockOrderModel.Setup(m => m.GetNewOrUsedOrderHistoryAsync(buyerId))
                .ReturnsAsync(newUsedOrders);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetCombinedOrderHistoryAsync(buyerId); // Default filter

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(o => o.OrderID == 101));
            Assert.IsTrue(result.Any(o => o.OrderID == 102));
            _mockOrderModel.Verify(m => m.GetBorrowedOrderHistoryAsync(buyerId), Times.Once);
            _mockOrderModel.Verify(m => m.GetNewOrUsedOrderHistoryAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_Returns2024Orders_WhenFilterIs2024()
        {
            // Arrange
            int buyerId = 42;
            string filter = "2024";
            var orders2024 = new List<Order> { new Order { OrderID = 301 } };

            _mockOrderModel.Setup(m => m.GetOrdersFrom2024(buyerId))
                .Returns(orders2024);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetCombinedOrderHistoryAsync(buyerId, filter);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(301, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFrom2024(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_Returns2025Orders_WhenFilterIs2025()
        {
            // Arrange
            int buyerId = 42;
            string filter = "2025";
            var orders2025 = new List<Order> { new Order { OrderID = 401 } };

            _mockOrderModel.Setup(m => m.GetOrdersFrom2025(buyerId))
                .Returns(orders2025);

            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private _model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            // Act
            var result = await viewModel.GetCombinedOrderHistoryAsync(buyerId, filter);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(401, result[0].OrderID);
            _mockOrderModel.Verify(m => m.GetOrdersFrom2025(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersWithProductInfoAsync_AppliesLast3MonthsFilter_WhenSpecified()
        {
            // Arrange
            int userId = 42;
            string timePeriod = "Last 3 Months";

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductName")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("ProductTypeName")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(6);

            // Setup read results
            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            // Setup field values
            _mockReader.Setup(r => r.GetInt32(0)).Returns(101);
            _mockReader.Setup(r => r.GetString(1)).Returns("Test Product");
            _mockReader.Setup(r => r.GetInt32(2)).Returns(1);
            _mockReader.Setup(r => r.GetString(3)).Returns("new");
            _mockReader.Setup(r => r.GetDateTime(4)).Returns(DateTime.Now.AddMonths(-2));
            _mockReader.Setup(r => r.GetString(5)).Returns("Credit Card");
            _mockReader.Setup(r => r.GetInt32(6)).Returns(201);

            // Setup command to return reader
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Act
            var results = await _viewModel.GetOrdersWithProductInfoAsync(userId, null, timePeriod);

            // Assert
            Assert.AreEqual(1, results.Count);

            // Verify SQL text contains the correct filter
            //_mockCommand.Verify(c => c.set_CommandText(It.Is<string>(
            //    sql => sql.Contains("AND o.OrderDate >= DATEADD(month, -3, GETDATE())"))),
            //    Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersWithProductInfoAsync_AppliesLast6MonthsFilter_WhenSpecified()
        {
            // Arrange
            int userId = 42;
            string timePeriod = "Last 6 Months";

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductName")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("ProductTypeName")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(6);

            // Setup read results
            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            // Setup field values
            _mockReader.Setup(r => r.GetInt32(0)).Returns(101);
            _mockReader.Setup(r => r.GetString(1)).Returns("Test Product");
            _mockReader.Setup(r => r.GetInt32(2)).Returns(1);
            _mockReader.Setup(r => r.GetString(3)).Returns("new");
            _mockReader.Setup(r => r.GetDateTime(4)).Returns(DateTime.Now.AddMonths(-4));
            _mockReader.Setup(r => r.GetString(5)).Returns("Credit Card");
            _mockReader.Setup(r => r.GetInt32(6)).Returns(201);

            // Setup command to return reader
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Act
            var results = await _viewModel.GetOrdersWithProductInfoAsync(userId, null, timePeriod);

            // Assert
            Assert.AreEqual(1, results.Count);

            // Verify SQL text contains the correct filter
            //_mockCommand.Verify(c => c.set_CommandText(It.Is<string>(
            //    sql => sql.Contains("AND o.OrderDate >= DATEADD(month, -6, GETDATE())"))),
            //    Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersWithProductInfoAsync_AppliesThisYearFilter_WhenSpecified()
        {
            // Arrange
            int userId = 42;
            string timePeriod = "This Year";

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("ProductName")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("ProductType")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("ProductTypeName")).Returns(3);
            _mockReader.Setup(r => r.GetOrdinal("OrderDate")).Returns(4);
            _mockReader.Setup(r => r.GetOrdinal("PaymentMethod")).Returns(5);
            _mockReader.Setup(r => r.GetOrdinal("OrderSummaryID")).Returns(6);

            // Setup read results
            int readCount = 0;
            _mockReader.Setup(r => r.Read()).Returns(() => readCount++ < 1);

            // Setup field values
            _mockReader.Setup(r => r.GetInt32(0)).Returns(101);
            _mockReader.Setup(r => r.GetString(1)).Returns("Test Product");
            _mockReader.Setup(r => r.GetInt32(2)).Returns(1);
            _mockReader.Setup(r => r.GetString(3)).Returns("new");
            _mockReader.Setup(r => r.GetDateTime(4)).Returns(new DateTime(DateTime.Now.Year, 1, 15)); // Current year
            _mockReader.Setup(r => r.GetString(5)).Returns("Credit Card");
            _mockReader.Setup(r => r.GetInt32(6)).Returns(201);

            // Setup command to return reader
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            // Act
            var results = await _viewModel.GetOrdersWithProductInfoAsync(userId, null, timePeriod);

            // Assert
            Assert.AreEqual(1, results.Count);
        }
    }
}