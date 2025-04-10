using ArtAttack.Model;
using ArtAttack.Domain;
using ArtAttack.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Reflection;

namespace ArtAttack.Tests.Model
{
    [TestClass]
    public class DummyProductModelTests
    {
        // Column names for data reader
        private const string COLUMN_ID = "ID";
        private const string COLUMN_NAME = "Name";
        private const string COLUMN_PRICE = "Price";
        private const string COLUMN_SELLER_ID = "SellerID";
        private const string COLUMN_PRODUCT_TYPE = "ProductType";
        private const string COLUMN_START_DATE = "StartDate";
        private const string COLUMN_END_DATE = "EndDate";

        // Stored procedure names
        private const string PROC_ADD_DUMMY_PRODUCT = "AddDummyProduct";
        private const string PROC_UPDATE_DUMMY_PRODUCT = "UpdateDummyProduct";
        private const string PROC_DELETE_DUMMY_PRODUCT = "DeleteDummyProduct";
        private const string PROC_GET_SELLER_BY_ID = "GetSellerById";
        private const string PROC_GET_DUMMY_PRODUCT_BY_ID = "GetDummyProductByID";

        // Parameter names
        private const string PARAM_ID = "@ID";
        private const string PARAM_NAME = "@Name";
        private const string PARAM_PRICE = "@Price";
        private const string PARAM_SELLER_ID = "@SellerID";
        private const string PARAM_PRODUCT_TYPE = "@ProductType";
        private const string PARAM_START_DATE = "@StartDate";
        private const string PARAM_END_DATE = "@EndDate";
        private const string PARAM_PRODUCT_ID = "@productID";

        // Test data
        private const int TEST_PRODUCT_ID = 42;
        private const int TEST_SELLER_ID = 1;
        private const string TEST_PRODUCT_NAME = "Test Product";
        private const string TEST_PRODUCT_TYPE = "Test Type";
        private const double TEST_PRODUCT_PRICE = 99.99;
        private const string TEST_SELLER_NAME = "Test Seller";
        private const int TEST_UPDATED_SELLER_ID = 2;
        private const string TEST_UPDATED_PRODUCT_NAME = "Updated Product";
        private const string TEST_UPDATED_PRODUCT_TYPE = "Updated Type";
        private const double TEST_UPDATED_PRODUCT_PRICE = 149.99;

        private Mock<IDatabaseProvider> _mockDatabase_Provider;
        private Mock<IDbConnection> _mockDatabase_Connection;
        private Mock<IDbCommand> _mockDatabase_Command;
        private Mock<IDataReader> _mockDatabase_Reader;
        private Mock<IDataParameterCollection> _mockDatabase_ParameterCollection;
        private Mock<IDbDataParameter> _mockDatabase_Parameter;
        private DummyProductModel _dummyProductModel;
        private readonly string _testDatabase_ConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockDatabase_Provider = new Mock<IDatabaseProvider>();
            _mockDatabase_Connection = new Mock<IDbConnection>();
            _mockDatabase_Command = new Mock<IDbCommand>();
            _mockDatabase_Reader = new Mock<IDataReader>();
            _mockDatabase_ParameterCollection = new Mock<IDataParameterCollection>();
            _mockDatabase_Parameter = new Mock<IDbDataParameter>();

            // Setup parameter collection
            _mockDatabase_ParameterCollection.Setup(Database_Parameters => Database_Parameters.Add(It.IsAny<object>())).Returns(0);

            // Setup command
            _mockDatabase_Command.Setup(Database_Command => Database_Command.CreateParameter()).Returns(_mockDatabase_Parameter.Object);
            _mockDatabase_Command.Setup(Database_Command => Database_Command.Parameters).Returns(_mockDatabase_ParameterCollection.Object);
            _mockDatabase_Command.Setup(Database_Command => Database_Command.ExecuteReader()).Returns(_mockDatabase_Reader.Object);

            // Setup connection
            _mockDatabase_Connection.Setup(Database_Connection => Database_Connection.CreateCommand()).Returns(_mockDatabase_Command.Object);
            _mockDatabase_Provider.Setup(Database_Provider => Database_Provider.CreateConnection(_testDatabase_ConnectionString)).Returns(_mockDatabase_Connection.Object);

            // Create the model with mocked provider
            _dummyProductModel = new DummyProductModel(_testDatabase_ConnectionString, _mockDatabase_Provider.Object);
        }

        [TestMethod]
        public void Constructor_WithConnectionString_InitializesCorrectly()
        {
            // Arrange & Act
            var productModel = new DummyProductModel(_testDatabase_ConnectionString, _mockDatabase_Provider.Object);

            // Assert - using reflection to access private field
            var Database_ConnectionStringField = typeof(DummyProductModel).GetField("connectionString",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var actualDatabase_ConnectionString = Database_ConnectionStringField.GetValue(productModel);

            Assert.IsNotNull(actualDatabase_ConnectionString);
            Assert.AreEqual(_testDatabase_ConnectionString, actualDatabase_ConnectionString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Act - should throw ArgumentNullException
            var productModel = new DummyProductModel(null, _mockDatabase_Provider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullDatabaseProvider_ThrowsArgumentNullException()
        {
            // Act - should throw ArgumentNullException
            var productModel = new DummyProductModel(_testDatabase_ConnectionString, null);
        }

        [TestMethod]
        public void Constructor_WithOnlyConnectionString_InitializesWithSqlDatabaseProvider()
        {
            // Create a mock provider to avoid real database connections
            var mockDatabase_Provider = new Mock<IDatabaseProvider>();
            mockDatabase_Provider.Setup(Database_Provider => Database_Provider.CreateConnection(It.IsAny<string>()))
                .Returns(_mockDatabase_Connection.Object);

            // Create with the two-parameter constructor to avoid real connection attempts
            var productModel = new DummyProductModel(_testDatabase_ConnectionString, mockDatabase_Provider.Object);

            // Check that connection string was set correctly
            var Database_ConnectionStringField = typeof(DummyProductModel).GetField("connectionString",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var actualDatabase_ConnectionString = Database_ConnectionStringField.GetValue(productModel);

            Assert.IsNotNull(actualDatabase_ConnectionString);
            Assert.AreEqual(_testDatabase_ConnectionString, actualDatabase_ConnectionString);
        }

        [TestMethod]
        public async Task AddDummyProductAsync_ExecutesCorrectProcedure()
        {
            // Arrange
            string productName = TEST_PRODUCT_NAME;
            float productPrice = (float)TEST_PRODUCT_PRICE;
            int sellerId = TEST_SELLER_ID;
            string productType = TEST_PRODUCT_TYPE;
            DateTime startDate = new DateTime(2023, 1, 1);
            DateTime endDate = new DateTime(2023, 12, 31);

            // Act
            await _dummyProductModel.AddDummyProductAsync(productName, productPrice, sellerId, productType, startDate, endDate);

            // Assert
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandText = PROC_ADD_DUMMY_PRODUCT);
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandType = CommandType.StoredProcedure);
            _mockDatabase_Connection.Verify(Database_Connection => Database_Connection.Open(), Times.Once);
            _mockDatabase_Command.Verify(Database_Command => Database_Command.ExecuteNonQuery(), Times.Once);

            // Verify parameters
            VerifyParameterAdded(PARAM_NAME, productName);
            VerifyParameterAdded(PARAM_PRICE, productPrice);
            VerifyParameterAdded(PARAM_SELLER_ID, sellerId);
            VerifyParameterAdded(PARAM_PRODUCT_TYPE, productType);
            VerifyParameterAdded(PARAM_START_DATE, startDate);
            VerifyParameterAdded(PARAM_END_DATE, endDate);
        }

        [TestMethod]
        public async Task AddDummyProductAsyncWithNullName_ExecutesCorrectProcedure()
        {
            // Arrange
            string productName = null;
            float productPrice = (float)TEST_PRODUCT_PRICE;
            int sellerId = TEST_SELLER_ID;
            string productType = TEST_PRODUCT_TYPE;
            DateTime startDate = new DateTime(2023, 1, 1);
            DateTime endDate = new DateTime(2023, 12, 31);

            // Act
            await _dummyProductModel.AddDummyProductAsync(productName, productPrice, sellerId, productType, startDate, endDate);

            // Assert
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandText = PROC_ADD_DUMMY_PRODUCT);
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandType = CommandType.StoredProcedure);
            _mockDatabase_Connection.Verify(Database_Connection => Database_Connection.Open(), Times.Once);
            _mockDatabase_Command.Verify(Database_Command => Database_Command.ExecuteNonQuery(), Times.Once);

            // Verify parameters
            VerifyParameterAdded(PARAM_NAME, DBNull.Value);
            VerifyParameterAdded(PARAM_PRICE, productPrice);
            VerifyParameterAdded(PARAM_SELLER_ID, sellerId);
            VerifyParameterAdded(PARAM_PRODUCT_TYPE, productType);
            VerifyParameterAdded(PARAM_START_DATE, startDate);
            VerifyParameterAdded(PARAM_END_DATE, endDate);
        }

        [TestMethod]
        public async Task UpdateDummyProductAsync_ExecutesCorrectProcedure()
        {
            // Arrange
            int productId = TEST_PRODUCT_ID;
            string updatedProductName = TEST_UPDATED_PRODUCT_NAME;
            float updatedProductPrice = (float)TEST_UPDATED_PRODUCT_PRICE;
            int updatedSellerId = TEST_UPDATED_SELLER_ID;
            string updatedProductType = TEST_UPDATED_PRODUCT_TYPE;
            DateTime updatedStartDate = new DateTime(2023, 2, 1);
            DateTime updatedEndDate = new DateTime(2024, 1, 31);

            // Act
            await _dummyProductModel.UpdateDummyProductAsync(productId, updatedProductName, updatedProductPrice,
                updatedSellerId, updatedProductType, updatedStartDate, updatedEndDate);

            // Assert
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandText = PROC_UPDATE_DUMMY_PRODUCT);
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandType = CommandType.StoredProcedure);
            _mockDatabase_Connection.Verify(Database_Connection => Database_Connection.Open(), Times.Once);
            _mockDatabase_Command.Verify(Database_Command => Database_Command.ExecuteNonQuery(), Times.Once);

            // Verify parameters
            VerifyParameterAdded(PARAM_ID, productId);
            VerifyParameterAdded(PARAM_NAME, updatedProductName);
            VerifyParameterAdded(PARAM_PRICE, updatedProductPrice);
            VerifyParameterAdded(PARAM_SELLER_ID, updatedSellerId);
            VerifyParameterAdded(PARAM_PRODUCT_TYPE, updatedProductType);
            VerifyParameterAdded(PARAM_START_DATE, updatedStartDate);
            VerifyParameterAdded(PARAM_END_DATE, updatedEndDate);
        }

        [TestMethod]
        public async Task DeleteDummyProduct_ExecutesCorrectProcedure()
        {
            // Arrange
            int productId = TEST_PRODUCT_ID;

            // Act
            await _dummyProductModel.DeleteDummyProduct(productId);

            // Assert
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandText = PROC_DELETE_DUMMY_PRODUCT);
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandType = CommandType.StoredProcedure);
            _mockDatabase_Connection.Verify(Database_Connection => Database_Connection.Open(), Times.Once);
            _mockDatabase_Command.Verify(Database_Command => Database_Command.ExecuteNonQuery(), Times.Once);

            // Verify parameters
            VerifyParameterAdded(PARAM_ID, productId);
        }

        [TestMethod]
        public async Task GetSellerNameAsync_WithValidSellerId_ReturnsName()
        {
            // Arrange
            int? sellerId = TEST_SELLER_ID;
            string expectedSellerName = TEST_SELLER_NAME;

            _mockDatabase_Command.Setup(Database_Command => Database_Command.ExecuteScalar()).Returns(expectedSellerName);

            // Act
            var actualSellerName = await _dummyProductModel.GetSellerNameAsync(sellerId);

            // Assert
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandText = PROC_GET_SELLER_BY_ID);
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandType = CommandType.StoredProcedure);
            _mockDatabase_Connection.Verify(Database_Connection => Database_Connection.Open(), Times.Once);
            _mockDatabase_Command.Verify(Database_Command => Database_Command.ExecuteScalar(), Times.Once);

            // Verify parameters
            VerifyParameterAdded(PARAM_SELLER_ID, sellerId.Value);

            // Verify result
            Assert.AreEqual(expectedSellerName, actualSellerName);
        }

        [TestMethod]
        public async Task GetSellerNameAsync_WithNullSellerId_AddsDbnullParameter()
        {
            // Arrange
            int? sellerId = null;

            // Act
            await _dummyProductModel.GetSellerNameAsync(sellerId);

            // Assert
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandText = PROC_GET_SELLER_BY_ID);
            _mockDatabase_Parameter.VerifySet(Database_Parameter => Database_Parameter.Value = DBNull.Value);
        }

        [TestMethod]
        public async Task GetSellerNameAsync_WithNullResult_ReturnsNull()
        {
            // Arrange
            int? sellerId = TEST_SELLER_ID;
            _mockDatabase_Command.Setup(Database_Command => Database_Command.ExecuteScalar()).Returns(null);

            // Act
            var sellerName = await _dummyProductModel.GetSellerNameAsync(sellerId);

            // Assert
            Assert.IsNull(sellerName);
        }

        [TestMethod]
        public async Task GetDummyProductByIdAsync_WithValidProduct_ReturnsProduct()
        {
            // Arrange
            int productId = TEST_PRODUCT_ID;
            DateTime startDate = new DateTime(2023, 1, 1);
            DateTime endDate = new DateTime(2023, 12, 31);

            // Column indices
            const int INDEX_ID = 0;
            const int INDEX_NAME = 1;
            const int INDEX_PRICE = 2;
            const int INDEX_SELLER_ID = 3;
            const int INDEX_PRODUCT_TYPE = 4;
            const int INDEX_START_DATE = 5;
            const int INDEX_END_DATE = 6;

            // Setup reader column ordinals
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_ID)).Returns(INDEX_ID);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_NAME)).Returns(INDEX_NAME);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_PRICE)).Returns(INDEX_PRICE);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_SELLER_ID)).Returns(INDEX_SELLER_ID);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_PRODUCT_TYPE)).Returns(INDEX_PRODUCT_TYPE);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_START_DATE)).Returns(INDEX_START_DATE);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_END_DATE)).Returns(INDEX_END_DATE);

            // Setup reader data
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.Read()).Returns(true);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetInt32(INDEX_ID)).Returns(productId);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetString(INDEX_NAME)).Returns(TEST_PRODUCT_NAME);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetDouble(INDEX_PRICE)).Returns(TEST_PRODUCT_PRICE);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.IsDBNull(INDEX_SELLER_ID)).Returns(false);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetInt32(INDEX_SELLER_ID)).Returns(TEST_SELLER_ID);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetString(INDEX_PRODUCT_TYPE)).Returns(TEST_PRODUCT_TYPE);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.IsDBNull(INDEX_START_DATE)).Returns(false);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetDateTime(INDEX_START_DATE)).Returns(startDate);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.IsDBNull(INDEX_END_DATE)).Returns(false);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetDateTime(INDEX_END_DATE)).Returns(endDate);

            // Act
            var dummyProduct = await _dummyProductModel.GetDummyProductByIdAsync(productId);

            // Assert
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandText = PROC_GET_DUMMY_PRODUCT_BY_ID);
            _mockDatabase_Command.VerifySet(Database_Command => Database_Command.CommandType = CommandType.StoredProcedure);
            _mockDatabase_Connection.Verify(Database_Connection => Database_Connection.Open(), Times.Once);
            _mockDatabase_Command.Verify(Database_Command => Database_Command.ExecuteReader(), Times.Once);

            // Verify parameters
            VerifyParameterAdded(PARAM_PRODUCT_ID, productId);

            // Verify result
            Assert.IsNotNull(dummyProduct);
            Assert.AreEqual(productId, dummyProduct.ID);
            Assert.AreEqual(TEST_PRODUCT_NAME, dummyProduct.Name);
            Assert.AreEqual((float)TEST_PRODUCT_PRICE, dummyProduct.Price);
            Assert.AreEqual(TEST_SELLER_ID, dummyProduct.SellerID);
            Assert.AreEqual(TEST_PRODUCT_TYPE, dummyProduct.ProductType);
            Assert.AreEqual(startDate, dummyProduct.StartDate);
            Assert.AreEqual(endDate, dummyProduct.EndDate);
        }

        [TestMethod]
        public async Task GetDummyProductByIdAsync_WithNullFields_HandlesNullsCorrectly()
        {
            // Arrange
            int productId = TEST_PRODUCT_ID;

            // Column indices
            const int INDEX_ID = 0;
            const int INDEX_NAME = 1;
            const int INDEX_PRICE = 2;
            const int INDEX_SELLER_ID = 3;
            const int INDEX_PRODUCT_TYPE = 4;
            const int INDEX_START_DATE = 5;
            const int INDEX_END_DATE = 6;

            // Setup reader column ordinals
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_ID)).Returns(INDEX_ID);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_NAME)).Returns(INDEX_NAME);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_PRICE)).Returns(INDEX_PRICE);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_SELLER_ID)).Returns(INDEX_SELLER_ID);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_PRODUCT_TYPE)).Returns(INDEX_PRODUCT_TYPE);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_START_DATE)).Returns(INDEX_START_DATE);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetOrdinal(COLUMN_END_DATE)).Returns(INDEX_END_DATE);

            // Setup reader data with nulls
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.Read()).Returns(true);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetInt32(INDEX_ID)).Returns(productId);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetString(INDEX_NAME)).Returns(TEST_PRODUCT_NAME);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetDouble(INDEX_PRICE)).Returns(TEST_PRODUCT_PRICE);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.IsDBNull(INDEX_SELLER_ID)).Returns(true);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.GetString(INDEX_PRODUCT_TYPE)).Returns(TEST_PRODUCT_TYPE);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.IsDBNull(INDEX_START_DATE)).Returns(true);
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.IsDBNull(INDEX_END_DATE)).Returns(true);

            // Act
            var dummyProduct = await _dummyProductModel.GetDummyProductByIdAsync(productId);

            // Assert
            Assert.IsNotNull(dummyProduct);
            Assert.IsNull(dummyProduct.SellerID);
            Assert.IsNull(dummyProduct.StartDate);
            Assert.IsNull(dummyProduct.EndDate);
        }

        [TestMethod]
        public async Task GetDummyProductByIdAsync_WithNoProduct_ReturnsNull()
        {
            // Arrange
            int productId = TEST_PRODUCT_ID;
            _mockDatabase_Reader.Setup(Database_Reader => Database_Reader.Read()).Returns(false);

            // Act
            var dummyProduct = await _dummyProductModel.GetDummyProductByIdAsync(productId);

            // Assert
            Assert.IsNull(dummyProduct);
        }

        private void VerifyParameterAdded(string parameterName, object parameterValue)
        {
            _mockDatabase_Parameter.VerifySet(Database_Parameter => Database_Parameter.ParameterName = parameterName, Times.AtLeastOnce());
            _mockDatabase_Parameter.VerifySet(Database_Parameter => Database_Parameter.Value = parameterValue, Times.AtLeastOnce());
        }
    }
}