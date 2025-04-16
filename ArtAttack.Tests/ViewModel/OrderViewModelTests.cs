using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using ArtAttack.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        private string _testConnectionString;
        private OrderViewModel _viewModel;
        private Mock<IOrderModel> _mockOrderModel;

        [TestInitialize]
        public void InitializeTestEnvironment()
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
                .Setup(parameters => parameters.Add(It.IsAny<object>()))
                .Returns(0);

            // Setup the command mock
            _mockCommand.Setup(command => command.CreateParameter()).Returns(_mockParameter.Object);
            _mockCommand.Setup(command => command.Parameters).Returns(_mockParameters.Object);

            // Setup the connection mock
            _mockConnection.Setup(connection => connection.CreateCommand()).Returns(_mockCommand.Object);

            // Setup the database provider mock
            _mockDatabaseProvider.Setup(provider => provider.CreateConnection(It.IsAny<string>())).Returns(_mockConnection.Object);

            // Initialize test connection string and view model with the mock database provider
            _testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";
            _viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);
        }

        [TestMethod]
        public async Task GetOrderSummaryAsync_ReturnsCorrectOrderSummaryForGivenId()
        {
            // Arrange

            OrderSummary expectedOrderSummary = CreateSampleOrderSummary();
            ConfigureMockReaderForOrderSummary(expectedOrderSummary);

            // Act
            OrderSummary actualOrderSummary = await _viewModel.GetOrderSummaryAsync(expectedOrderSummary.ID);

            // Assert
            VerifyOrderSummaryIsCorrect(expectedOrderSummary, actualOrderSummary);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ThrowsArgumentNullExceptionWhenConnectionStringIsNull()
        {
            // Arrange
            string nullConnectionString = null;

            // Act
            new OrderViewModel(nullConnectionString, _mockDatabaseProvider.Object);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ThrowsArgumentNullExceptionWhenDatabaseProviderIsNull()
        {
            // Arrange
            string validConnectionString = _testConnectionString;

            // Act
            new OrderViewModel(validConnectionString, null);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetOrderSummaryAsync_ThrowsKeyNotFoundExceptionWhenOrderSummaryDoesNotExist()
        {
            // Arrange
            int nonExistentOrderSummaryId = 999;

            _mockReader.Setup(reader => reader.Read()).Returns(false);
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            await _viewModel.GetOrderSummaryAsync(nonExistentOrderSummaryId);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        public async Task GetOrdersWithProductInfoAsync_ReturnsCorrectProductInformationList()
        {
            // Arrange
            int userId = 42;
            string searchText = "test";

            List<OrderDisplayInfo> expectedProductInfoList = SetupMockReaderForOrdersWithProductInfo();

            // Act
            List<OrderDisplayInfo> actualProductInfoList = await _viewModel.GetOrdersWithProductInfoAsync(userId, searchText);

            // Assert
            Assert.AreEqual(expectedProductInfoList.Count, actualProductInfoList.Count);
            VerifyOrderDisplayInfoIsCorrect(expectedProductInfoList[0], actualProductInfoList[0]);
            VerifyOrderDisplayInfoIsCorrect(expectedProductInfoList[1], actualProductInfoList[1]);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
        }

        [TestMethod]
        public async Task GetProductCategoryTypesAsync_ReturnsCorrectCategoryMappings()
        {
            // Arrange
            int userId = 42;

            Dictionary<int, string> expectedCategoryTypes = SetupMockReaderForProductCategoryTypes();

            // Act
            Dictionary<int, string> actualCategoryTypes = await _viewModel.GetProductCategoryTypesAsync(userId);

            // Assert
            Assert.AreEqual(expectedCategoryTypes.Count, actualCategoryTypes.Count);
            Assert.AreEqual(expectedCategoryTypes[101], actualCategoryTypes[101]); // "new" category
            Assert.AreEqual(expectedCategoryTypes[102], actualCategoryTypes[102]); // "used" categorized as "new"
            Assert.AreEqual(expectedCategoryTypes[103], actualCategoryTypes[103]); // "borrowed" category
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetOrdersWithProductInfoAsync_ThrowsArgumentExceptionWhenUserIdIsInvalid()
        {
            // Arrange
            int invalidUserId = -1;

            // Act
            await _viewModel.GetOrdersWithProductInfoAsync(invalidUserId);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetProductCategoryTypesAsync_ThrowsArgumentExceptionWhenUserIdIsInvalid()
        {
            // Arrange
            int invalidUserId = 0;

            // Act
            await _viewModel.GetProductCategoryTypesAsync(invalidUserId);

            // Assert is handled by ExpectedException attribute
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_ReturnsOrderWhenFoundInBorrowedOrders()
        {
            // Arrange
            int orderId = 101;
            Order expectedOrder = new Order { OrderID = orderId, ProductType = 1 };

            var viewModel = CreateViewModelWithMockedOrderModel();
            SetupMockOrderModelForGetOrderByIdAsync_BorrowedOrders(expectedOrder);

            // Act
            Order actualOrder = await viewModel.GetOrderByIdAsync(orderId);

            // Assert
            Assert.IsNotNull(actualOrder);
            Assert.AreEqual(expectedOrder.OrderID, actualOrder.OrderID);
            _mockOrderModel.Verify(model => model.GetBorrowedOrderHistoryAsync(0), Times.Once);
            _mockOrderModel.Verify(model => model.GetNewOrUsedOrderHistoryAsync(0), Times.Never);
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_ReturnsOrderWhenFoundInNewOrUsedOrders()
        {
            // Arrange
            int orderId = 102;
            Order expectedOrder = new Order { OrderID = orderId, ProductType = 2 };

            var viewModel = CreateViewModelWithMockedOrderModel();
            SetupMockOrderModelForGetOrderByIdAsync_NewOrUsedOrders(expectedOrder);

            // Act
            Order actualOrder = await viewModel.GetOrderByIdAsync(orderId);

            // Assert
            Assert.IsNotNull(actualOrder);
            Assert.AreEqual(expectedOrder.OrderID, actualOrder.OrderID);
            _mockOrderModel.Verify(model => model.GetBorrowedOrderHistoryAsync(0), Times.Once);
            _mockOrderModel.Verify(model => model.GetNewOrUsedOrderHistoryAsync(0), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_ReturnsNullWhenOrderNotFound()
        {
            // Arrange
            int nonExistentOrderId = 999;

            var viewModel = CreateViewModelWithMockedOrderModel();
            SetupMockOrderModelForGetOrderByIdAsync_OrderNotFound();

            // Act
            Order result = await viewModel.GetOrderByIdAsync(nonExistentOrderId);

            // Assert
            Assert.IsNull(result);
            _mockOrderModel.Verify(model => model.GetBorrowedOrderHistoryAsync(0), Times.Once);
            _mockOrderModel.Verify(model => model.GetNewOrUsedOrderHistoryAsync(0), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_ReturnsThreeMonthsOrdersWhenFilterIsThreeMonths()
        {
            // Arrange
            int buyerId = 42;
            string filterThreeMonths = "3months";
            List<Order> expectedOrders = new List<Order> { new Order { OrderID = 101 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetOrdersFromLastThreeMonths(buyerId))
                .Returns(expectedOrders);

            // Act
            List<Order> actualOrders = await viewModel.GetCombinedOrderHistoryAsync(buyerId, filterThreeMonths);

            // Assert
            Assert.AreEqual(expectedOrders.Count, actualOrders.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, actualOrders[0].OrderID);
            _mockOrderModel.Verify(model => model.GetOrdersFromLastThreeMonths(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_ReturnsSixMonthsOrdersWhenFilterIsSixMonths()
        {
            // Arrange
            int buyerId = 42;
            string filterSixMonths = "6months";
            List<Order> expectedOrders = new List<Order> { new Order { OrderID = 201 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetOrdersFromLastSixMonths(buyerId))
                .Returns(expectedOrders);

            // Act
            List<Order> actualOrders = await viewModel.GetCombinedOrderHistoryAsync(buyerId, filterSixMonths);

            // Assert
            Assert.AreEqual(expectedOrders.Count, actualOrders.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, actualOrders[0].OrderID);
            _mockOrderModel.Verify(model => model.GetOrdersFromLastSixMonths(buyerId), Times.Once);
        }

        [TestMethod]
        public void Constructor_WithConnectionStringInitializesCorrectly()
        {
            // Arrange
            string connectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";

            // Act
            var viewModel = new OrderViewModel(connectionString);

            // Assert
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        public async Task AddOrderAsync_CallsModelWithCorrectParameters()
        {
            // Arrange
            int productId = 1;
            int buyerId = 2;
            int productType = 3;
            string paymentMethod = "Credit Card";
            int orderSummaryId = 4;
            DateTime orderDate = DateTime.Now;

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate))
                .Returns(Task.CompletedTask);

            // Act
            await viewModel.AddOrderAsync(productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate);

            // Assert
            _mockOrderModel.Verify(model => model.AddOrderAsync(
                productId, buyerId, productType, paymentMethod, orderSummaryId, orderDate), Times.Once);
        }

        [TestMethod]
        public async Task UpdateOrderAsync_CallsModelWithCorrectParameters()
        {
            // Arrange
            int orderId = 1;
            int productType = 3;
            string paymentMethod = "PayPal";
            DateTime orderDate = DateTime.Now;

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate))
                .Returns(Task.CompletedTask);

            // Act
            await viewModel.UpdateOrderAsync(orderId, productType, paymentMethod, orderDate);

            // Assert
            _mockOrderModel.Verify(model => model.UpdateOrderAsync(
                orderId, productType, paymentMethod, orderDate), Times.Once);
        }

        [TestMethod]
        public async Task DeleteOrderAsync_CallsModelWithCorrectOrderId()
        {
            // Arrange
            int orderId = 1;

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.DeleteOrderAsync(orderId))
                .Returns(Task.CompletedTask);

            // Act
            await viewModel.DeleteOrderAsync(orderId);

            // Assert
            _mockOrderModel.Verify(model => model.DeleteOrderAsync(orderId), Times.Once);
        }

        [TestMethod]
        public async Task GetBorrowedOrderHistoryAsync_ReturnsCorrectBorrowedOrders()
        {
            // Arrange
            int buyerId = 1;
            List<Order> expectedOrders = new List<Order> { new Order { OrderID = 101 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetBorrowedOrderHistoryAsync(buyerId))
                .ReturnsAsync(expectedOrders);

            // Act
            List<Order> actualOrders = await viewModel.GetBorrowedOrderHistoryAsync(buyerId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, actualOrders.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, actualOrders[0].OrderID);
            _mockOrderModel.Verify(model => model.GetBorrowedOrderHistoryAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetNewOrUsedOrderHistoryAsync_ReturnsCorrectNewOrUsedOrders()
        {
            // Arrange
            int buyerId = 1;
            List<Order> expectedOrders = new List<Order> { new Order { OrderID = 201 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetNewOrUsedOrderHistoryAsync(buyerId))
                .ReturnsAsync(expectedOrders);

            // Act
            List<Order> actualOrders = await viewModel.GetNewOrUsedOrderHistoryAsync(buyerId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, actualOrders.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, actualOrders[0].OrderID);
            _mockOrderModel.Verify(model => model.GetNewOrUsedOrderHistoryAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromLastThreeMonthsAsync_ReturnsCorrectOrdersFromThreeMonths()
        {
            // Arrange
            int buyerId = 1;
            List<Order> expectedOrders = new List<Order> { new Order { OrderID = 301 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetOrdersFromLastThreeMonths(buyerId))
                .Returns(expectedOrders);

            // Act
            List<Order> actualOrders = await viewModel.GetOrdersFromLastThreeMonthsAsync(buyerId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, actualOrders.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, actualOrders[0].OrderID);
            _mockOrderModel.Verify(model => model.GetOrdersFromLastThreeMonths(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromLastSixMonthsAsync_ReturnsCorrectOrdersFromSixMonths()
        {
            // Arrange
            int buyerId = 1;
            List<Order> expectedOrders = new List<Order> { new Order { OrderID = 401 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetOrdersFromLastSixMonths(buyerId))
                .Returns(expectedOrders);

            // Act
            List<Order> actualOrders = await viewModel.GetOrdersFromLastSixMonthsAsync(buyerId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, actualOrders.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, actualOrders[0].OrderID);
            _mockOrderModel.Verify(model => model.GetOrdersFromLastSixMonths(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFrom2024Async_ReturnsCorrectOrdersFrom2024()
        {
            // Arrange
            int buyerId = 1;
            List<Order> expectedOrders = new List<Order> { new Order { OrderID = 501 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetOrdersFrom2024(buyerId))
                .Returns(expectedOrders);

            // Act
            List<Order> actualOrders = await viewModel.GetOrdersFrom2024Async(buyerId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, actualOrders.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, actualOrders[0].OrderID);
            _mockOrderModel.Verify(model => model.GetOrdersFrom2024(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFrom2025Async_ReturnsCorrectOrdersFrom2025()
        {
            // Arrange
            int buyerId = 1;
            List<Order> expectedOrders = new List<Order> { new Order { OrderID = 601 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetOrdersFrom2025(buyerId))
                .Returns(expectedOrders);

            // Act
            List<Order> actualOrders = await viewModel.GetOrdersFrom2025Async(buyerId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, actualOrders.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, actualOrders[0].OrderID);
            _mockOrderModel.Verify(model => model.GetOrdersFrom2025(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersByNameAsync_ReturnsCorrectOrdersMatchingName()
        {
            // Arrange
            int buyerId = 1;
            string searchText = "test";
            List<Order> expectedOrders = new List<Order> { new Order { OrderID = 701 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetOrdersByName(buyerId, searchText))
                .Returns(expectedOrders);

            // Act
            List<Order> actualOrders = await viewModel.GetOrdersByNameAsync(buyerId, searchText);

            // Assert
            Assert.AreEqual(expectedOrders.Count, actualOrders.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, actualOrders[0].OrderID);
            _mockOrderModel.Verify(model => model.GetOrdersByName(buyerId, searchText), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersFromOrderHistoryAsync_ReturnsCorrectOrdersFromHistory()
        {
            // Arrange
            int orderHistoryId = 42;
            List<Order> expectedOrders = new List<Order> { new Order { OrderID = 801 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetOrdersFromOrderHistoryAsync(orderHistoryId))
                .ReturnsAsync(expectedOrders);

            // Act
            List<Order> actualOrders = await viewModel.GetOrdersFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.AreEqual(expectedOrders.Count, actualOrders.Count);
            Assert.AreEqual(expectedOrders[0].OrderID, actualOrders[0].OrderID);
            _mockOrderModel.Verify(model => model.GetOrdersFromOrderHistoryAsync(orderHistoryId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_ReturnsAllOrdersWhenFilterIsAll()
        {
            // Arrange
            int buyerId = 42;
            string filterAll = "all";
            List<Order> borrowedOrders = new List<Order> { new Order { OrderID = 101 } };
            List<Order> newUsedOrders = new List<Order> { new Order { OrderID = 102 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetBorrowedOrderHistoryAsync(buyerId))
                .ReturnsAsync(borrowedOrders);
            _mockOrderModel.Setup(model => model.GetNewOrUsedOrderHistoryAsync(buyerId))
                .ReturnsAsync(newUsedOrders);

            // Act
            List<Order> combinedOrders = await viewModel.GetCombinedOrderHistoryAsync(buyerId, filterAll);

            // Assert
            Assert.AreEqual(2, combinedOrders.Count);
            Assert.IsTrue(combinedOrders.Any(order => order.OrderID == 101));
            Assert.IsTrue(combinedOrders.Any(order => order.OrderID == 102));
            _mockOrderModel.Verify(model => model.GetBorrowedOrderHistoryAsync(buyerId), Times.Once);
            _mockOrderModel.Verify(model => model.GetNewOrUsedOrderHistoryAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_ReturnsAllOrdersWhenFilterIsDefault()
        {
            // Arrange
            int buyerId = 42;
            List<Order> borrowedOrders = new List<Order> { new Order { OrderID = 101 } };
            List<Order> newUsedOrders = new List<Order> { new Order { OrderID = 102 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetBorrowedOrderHistoryAsync(buyerId))
                .ReturnsAsync(borrowedOrders);
            _mockOrderModel.Setup(model => model.GetNewOrUsedOrderHistoryAsync(buyerId))
                .ReturnsAsync(newUsedOrders);

            // Act
            List<Order> combinedOrders = await viewModel.GetCombinedOrderHistoryAsync(buyerId); // Default filter

            // Assert
            Assert.AreEqual(2, combinedOrders.Count);
            Assert.IsTrue(combinedOrders.Any(order => order.OrderID == 101));
            Assert.IsTrue(combinedOrders.Any(order => order.OrderID == 102));
            _mockOrderModel.Verify(model => model.GetBorrowedOrderHistoryAsync(buyerId), Times.Once);
            _mockOrderModel.Verify(model => model.GetNewOrUsedOrderHistoryAsync(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_Returns2024OrdersWhenFilterIs2024()
        {
            // Arrange
            int buyerId = 42;
            string filter2024 = "2024";
            List<Order> orders2024 = new List<Order> { new Order { OrderID = 301 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetOrdersFrom2024(buyerId))
                .Returns(orders2024);

            // Act
            List<Order> actualOrders = await viewModel.GetCombinedOrderHistoryAsync(buyerId, filter2024);

            // Assert
            Assert.AreEqual(1, actualOrders.Count);
            Assert.AreEqual(301, actualOrders[0].OrderID);
            _mockOrderModel.Verify(model => model.GetOrdersFrom2024(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetCombinedOrderHistoryAsync_Returns2025OrdersWhenFilterIs2025()
        {
            // Arrange
            int buyerId = 42;
            string filter2025 = "2025";
            List<Order> orders2025 = new List<Order> { new Order { OrderID = 401 } };

            var viewModel = CreateViewModelWithMockedOrderModel();
            _mockOrderModel.Setup(model => model.GetOrdersFrom2025(buyerId))
                .Returns(orders2025);

            // Act
            List<Order> actualOrders = await viewModel.GetCombinedOrderHistoryAsync(buyerId, filter2025);

            // Assert
            Assert.AreEqual(1, actualOrders.Count);
            Assert.AreEqual(401, actualOrders[0].OrderID);
            _mockOrderModel.Verify(model => model.GetOrdersFrom2025(buyerId), Times.Once);
        }

        [TestMethod]
        public async Task GetOrdersWithProductInfoAsync_AppliesLastThreeMonthsFilterWhenSpecified()
        {
            // Arrange
            int userId = 42;
            string timePeriodLastThreeMonths = "Last 3 Months";
            OrderDisplayInfo expectedOrderInfo = SetupMockReaderForOrdersWithTimeFilter(DateTime.Now.AddMonths(-2));

            // Act
            List<OrderDisplayInfo> resultOrders = await _viewModel.GetOrdersWithProductInfoAsync(userId, null, timePeriodLastThreeMonths);

            // Assert
            Assert.AreEqual(1, resultOrders.Count);
            VerifySingleOrderDisplayInfo(expectedOrderInfo, resultOrders[0]);
        }

        [TestMethod]
        public async Task GetOrdersWithProductInfoAsync_AppliesLastSixMonthsFilterWhenSpecified()
        {
            // Arrange
            int userId = 42;
            string timePeriodLastSixMonths = "Last 6 Months";
            OrderDisplayInfo expectedOrderInfo = SetupMockReaderForOrdersWithTimeFilter(DateTime.Now.AddMonths(-4));

            // Act
            List<OrderDisplayInfo> resultOrders = await _viewModel.GetOrdersWithProductInfoAsync(userId, null, timePeriodLastSixMonths);

            // Assert
            Assert.AreEqual(1, resultOrders.Count);
            VerifySingleOrderDisplayInfo(expectedOrderInfo, resultOrders[0]);
        }

        [TestMethod]
        public async Task GetOrdersWithProductInfoAsync_AppliesThisYearFilterWhenSpecified()
        {
            // Arrange
            int userId = 42;
            string timePeriodThisYear = "This Year";
            OrderDisplayInfo expectedOrderInfo = SetupMockReaderForOrdersWithTimeFilter(new DateTime(DateTime.Now.Year, 1, 15));

            // Act
            List<OrderDisplayInfo> resultOrders = await _viewModel.GetOrdersWithProductInfoAsync(userId, null, timePeriodThisYear);

            // Assert
            Assert.AreEqual(1, resultOrders.Count);
            VerifySingleOrderDisplayInfo(expectedOrderInfo, resultOrders[0]);
        }

        #region Helper Methods

        private OrderSummary CreateSampleOrderSummary()
        {
            return new OrderSummary
            {
                ID = 123,
                Subtotal = 100.50f,
                WarrantyTax = 10.05f,
                DeliveryFee = 5.99f,
                FinalTotal = 116.54f,
                FullName = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                Address = "123 Main St",
                PostalCode = "12345",
                AdditionalInfo = "Leave at door",
                ContractDetails = "Contract details here"
            };
        }

        private void ConfigureMockReaderForOrderSummary(OrderSummary orderSummary)
        {
            // Setup column ordinals

            _mockReader.Setup(reader => reader.GetOrdinal("ID")).Returns(0);
            _mockReader.Setup(reader => reader.GetOrdinal("Subtotal")).Returns(1);
            _mockReader.Setup(reader => reader.GetOrdinal("WarrantyTax")).Returns(2);
            _mockReader.Setup(reader => reader.GetOrdinal("DeliveryFee")).Returns(3);
            _mockReader.Setup(reader => reader.GetOrdinal("FinalTotal")).Returns(4);
            _mockReader.Setup(reader => reader.GetOrdinal("FullName")).Returns(5);
            _mockReader.Setup(reader => reader.GetOrdinal("Email")).Returns(6);
            _mockReader.Setup(reader => reader.GetOrdinal("PhoneNumber")).Returns(7);
            _mockReader.Setup(reader => reader.GetOrdinal("Address")).Returns(8);
            _mockReader.Setup(reader => reader.GetOrdinal("PostalCode")).Returns(9);
            _mockReader.Setup(reader => reader.GetOrdinal("AdditionalInfo")).Returns(10);
            _mockReader.Setup(reader => reader.GetOrdinal("ContractDetails")).Returns(11);

            // Setup Read calls - first return true then false
            int readCount = 0;

            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ == 0);

            // Setup IsDBNull for nullable fields
            _mockReader.Setup(reader => reader.IsDBNull(10)).Returns(false);
            _mockReader.Setup(reader => reader.IsDBNull(11)).Returns(false);

            // Setup field values
            _mockReader.Setup(reader => reader.GetInt32(0)).Returns(orderSummary.ID);
            _mockReader.Setup(reader => reader.GetDouble(1)).Returns(Convert.ToDouble(orderSummary.Subtotal));
            _mockReader.Setup(reader => reader.GetDouble(2)).Returns(Convert.ToDouble(orderSummary.WarrantyTax));
            _mockReader.Setup(reader => reader.GetDouble(3)).Returns(Convert.ToDouble(orderSummary.DeliveryFee));
            _mockReader.Setup(reader => reader.GetDouble(4)).Returns(Convert.ToDouble(orderSummary.FinalTotal));
            _mockReader.Setup(reader => reader.GetString(5)).Returns(orderSummary.FullName);
            _mockReader.Setup(reader => reader.GetString(6)).Returns(orderSummary.Email);
            _mockReader.Setup(reader => reader.GetString(7)).Returns(orderSummary.PhoneNumber);
            _mockReader.Setup(reader => reader.GetString(8)).Returns(orderSummary.Address);
            _mockReader.Setup(reader => reader.GetString(9)).Returns(orderSummary.PostalCode);
            _mockReader.Setup(reader => reader.GetString(10)).Returns(orderSummary.AdditionalInfo);
            _mockReader.Setup(reader => reader.GetString(11)).Returns(orderSummary.ContractDetails);

            // Setup command to return reader
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);
        }

        private void VerifyOrderSummaryIsCorrect(OrderSummary expected, OrderSummary actual)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.ID, actual.ID);
            Assert.AreEqual(expected.Subtotal, actual.Subtotal);
            Assert.AreEqual(expected.WarrantyTax, actual.WarrantyTax);
            Assert.AreEqual(expected.DeliveryFee, actual.DeliveryFee);
            Assert.AreEqual(expected.FinalTotal, actual.FinalTotal);
            Assert.AreEqual(expected.FullName, actual.FullName);
            Assert.AreEqual(expected.Email, actual.Email);
            Assert.AreEqual(expected.PhoneNumber, actual.PhoneNumber);
            Assert.AreEqual(expected.Address, actual.Address);
            Assert.AreEqual(expected.PostalCode, actual.PostalCode);
            Assert.AreEqual(expected.AdditionalInfo, actual.AdditionalInfo);
            Assert.AreEqual(expected.ContractDetails, actual.ContractDetails);
        }

        private List<OrderDisplayInfo> SetupMockReaderForOrdersWithProductInfo()
        {
            DateTime orderDate = new DateTime(2023, 10, 10);
            List<OrderDisplayInfo> expectedProductInfo = new List<OrderDisplayInfo>
            {
                new OrderDisplayInfo
                {
                    OrderID = 101,
                    ProductName = "Test Product 1",
                    ProductTypeName = "new",
                    OrderDate = orderDate.ToString("yyyy-MM-dd"),
                    PaymentMethod = "Credit Card",
                    OrderSummaryID = 201,
                    ProductCategory = "new"
                },
                new OrderDisplayInfo
                {
                    OrderID = 102,
                    ProductName = "Test Product 2",
                    ProductTypeName = "borrowed",
                    OrderDate = orderDate.AddDays(-1).ToString("yyyy-MM-dd"),
                    PaymentMethod = "PayPal",
                    OrderSummaryID = 202,
                    ProductCategory = "borrowed"
                }
            };

            // Setup column ordinals
            _mockReader.Setup(reader => reader.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(reader => reader.GetOrdinal("ProductName")).Returns(1);
            _mockReader.Setup(reader => reader.GetOrdinal("ProductType")).Returns(2);
            _mockReader.Setup(reader => reader.GetOrdinal("ProductTypeName")).Returns(3);
            _mockReader.Setup(reader => reader.GetOrdinal("OrderDate")).Returns(4);
            _mockReader.Setup(reader => reader.GetOrdinal("PaymentMethod")).Returns(5);
            _mockReader.Setup(reader => reader.GetOrdinal("OrderSummaryID")).Returns(6);

            // Setup mock reader with sequential reads
            int readCount = 0;
            _mockReader.Setup(reader => reader.Read())
                .Returns(() => {
                    return readCount++ < 2; // Return true for first 2 calls, then false
                });

            // First product
            _mockReader.Setup(reader => reader.GetInt32(0))
                .Returns(() => readCount == 1 ? 101 : 102);

            _mockReader.Setup(reader => reader.GetString(1))
                .Returns(() => readCount == 1 ? "Test Product 1" : "Test Product 2");

            _mockReader.Setup(reader => reader.GetInt32(2))
                .Returns(() => readCount == 1 ? 1 : 2);

            _mockReader.Setup(reader => reader.GetString(3))
                .Returns(() => readCount == 1 ? "new" : "borrowed");

            _mockReader.Setup(reader => reader.GetDateTime(4))
                .Returns(() => readCount == 1 ? orderDate : orderDate.AddDays(-1));

            _mockReader.Setup(reader => reader.GetString(5))
                .Returns(() => readCount == 1 ? "Credit Card" : "PayPal");

            _mockReader.Setup(reader => reader.GetInt32(6))
                .Returns(() => readCount == 1 ? 201 : 202);

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Capture parameters
            List<IDbDataParameter> capturedParameters = new List<IDbDataParameter>();
            _mockParameters
                .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
                .Callback<object>(param => capturedParameters.Add((IDbDataParameter)param))
                .Returns(0);

            return expectedProductInfo;
        }

        private void VerifyOrderDisplayInfoIsCorrect(OrderDisplayInfo expected, OrderDisplayInfo actual)
        {
            Assert.AreEqual(expected.OrderID, actual.OrderID);
            Assert.AreEqual(expected.ProductName, actual.ProductName);
            Assert.AreEqual(expected.ProductTypeName, actual.ProductTypeName);
            Assert.AreEqual(expected.OrderDate, actual.OrderDate);
            Assert.AreEqual(expected.PaymentMethod, actual.PaymentMethod);
            Assert.AreEqual(expected.OrderSummaryID, actual.OrderSummaryID);
            Assert.AreEqual(expected.ProductCategory, actual.ProductCategory);
        }

        private Dictionary<int, string> SetupMockReaderForProductCategoryTypes()
        {
            Dictionary<int, string> expectedCategoryTypes = new Dictionary<int, string>
            {
                { 101, "new" },
                { 102, "new" },
                { 103, "borrowed" }
            };

            // Setup column ordinals

            _mockReader.Setup(reader => reader.GetOrdinal("OrderSummaryID")).Returns(0);
            _mockReader.Setup(reader => reader.GetOrdinal("productType")).Returns(1);

            // Setup sequential reads with counter
            int readCount = 0;

            _mockReader.Setup(reader => reader.Read())
                .Returns(() => {
                    return readCount++ < 3; // Return true for first 3 calls, then false
                });

            // Setup field values based on read count
            _mockReader.Setup(reader => reader.GetInt32(0))
                .Returns(() => {
                    switch (readCount)
                    {
                        case 1: return 101;
                        case 2: return 102;
                        case 3: return 103;
                        default: return 0;
                    }
                });

            _mockReader.Setup(reader => reader.GetString(1))
                .Returns(() => {
                    switch (readCount)
                    {
                        case 1: return "new";
                        case 2: return "used";
                        case 3: return "borrowed";
                        default: return string.Empty;
                    }
                });

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            return expectedCategoryTypes;
        }

        private OrderViewModel CreateViewModelWithMockedOrderModel()
        {
            var viewModel = new OrderViewModel(_testConnectionString, _mockDatabaseProvider.Object);

            // Use reflection to replace the private model field with our mock
            var modelField = typeof(OrderViewModel).GetField("model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (modelField == null)
            {
                throw new InvalidOperationException("Could not find the model field in OrderViewModel class. Check the field name.");
            }
            modelField.SetValue(viewModel, _mockOrderModel.Object);

            return viewModel;
        }

        private void SetupMockOrderModelForGetOrderByIdAsync_BorrowedOrders(Order mockOrder)
        {
            var borrowedOrders = new List<Order> { mockOrder };
            _mockOrderModel.Setup(model => model.GetBorrowedOrderHistoryAsync(0))
                .Returns(Task.FromResult(borrowedOrders));
        }

        private void SetupMockOrderModelForGetOrderByIdAsync_NewOrUsedOrders(Order mockOrder)
        {
            var borrowedOrders = new List<Order>();
            var newUsedOrders = new List<Order> { mockOrder };

            _mockOrderModel.Setup(model => model.GetBorrowedOrderHistoryAsync(0))
                .Returns(Task.FromResult(borrowedOrders));
            _mockOrderModel.Setup(model => model.GetNewOrUsedOrderHistoryAsync(0))
                .Returns(Task.FromResult(newUsedOrders));
        }

        private void SetupMockOrderModelForGetOrderByIdAsync_OrderNotFound()
        {
            var borrowedOrders = new List<Order> { new Order { OrderID = 101 } };
            var newUsedOrders = new List<Order> { new Order { OrderID = 102 } };

            _mockOrderModel.Setup(model => model.GetBorrowedOrderHistoryAsync(0))
                .Returns(Task.FromResult(borrowedOrders));
            _mockOrderModel.Setup(model => model.GetNewOrUsedOrderHistoryAsync(0))
                .Returns(Task.FromResult(newUsedOrders));
        }

        private OrderDisplayInfo SetupMockReaderForOrdersWithTimeFilter(DateTime orderDate)
        {
            OrderDisplayInfo expectedOrderInfo = new OrderDisplayInfo
            {
                OrderID = 101,
                ProductName = "Test Product",
                ProductTypeName = "new",
                OrderDate = orderDate.ToString("yyyy-MM-dd"),
                PaymentMethod = "Credit Card",
                OrderSummaryID = 201,
                ProductCategory = "new"
            };

            // Setup column ordinals
            _mockReader.Setup(reader => reader.GetOrdinal("OrderID")).Returns(0);
            _mockReader.Setup(reader => reader.GetOrdinal("ProductName")).Returns(1);
            _mockReader.Setup(reader => reader.GetOrdinal("ProductType")).Returns(2);
            _mockReader.Setup(reader => reader.GetOrdinal("ProductTypeName")).Returns(3);
            _mockReader.Setup(reader => reader.GetOrdinal("OrderDate")).Returns(4);
            _mockReader.Setup(reader => reader.GetOrdinal("PaymentMethod")).Returns(5);
            _mockReader.Setup(reader => reader.GetOrdinal("OrderSummaryID")).Returns(6);

            // Setup read results
            int readCount = 0;
            _mockReader.Setup(reader => reader.Read()).Returns(() => readCount++ < 1);

            // Setup field values
            _mockReader.Setup(reader => reader.GetInt32(0)).Returns(101);
            _mockReader.Setup(reader => reader.GetString(1)).Returns("Test Product");
            _mockReader.Setup(reader => reader.GetInt32(2)).Returns(1);
            _mockReader.Setup(reader => reader.GetString(3)).Returns("new");

            _mockReader.Setup(reader => reader.GetDateTime(4)).Returns(orderDate);
            _mockReader.Setup(reader => reader.GetString(5)).Returns("Credit Card");
            _mockReader.Setup(reader => reader.GetInt32(6)).Returns(201);

            // Setup command to return reader
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            return expectedOrderInfo;
        }

        private void VerifySingleOrderDisplayInfo(OrderDisplayInfo expected, OrderDisplayInfo actual)
        {
            Assert.AreEqual(expected.OrderID, actual.OrderID);
            Assert.AreEqual(expected.ProductName, actual.ProductName);
            Assert.AreEqual(expected.ProductTypeName, actual.ProductTypeName);
            Assert.AreEqual(expected.PaymentMethod, actual.PaymentMethod);
            Assert.AreEqual(expected.OrderSummaryID, actual.OrderSummaryID);
        }

        #endregion
    }
}
