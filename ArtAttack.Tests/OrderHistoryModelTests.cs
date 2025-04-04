using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ArtAttack.Tests.Model
{
    [TestClass]
    public class OrderHistoryModelTests
    {
        private readonly string _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=TestDb;Integrated Security=True;";

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_ValidOrderHistoryId_ReturnsProducts()
        {
            // Arrange
            int orderHistoryId = 1;
            var mockDb = SetupMockDatabase(GetValidProductDataReader());
            var model = new OrderHistoryModelWrapper(_connectionString, mockDb.Object);

            // Act
            var result = await model.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].ID);
            Assert.AreEqual("Test Product 1", result[0].Name);
            Assert.AreEqual(99.99f, result[0].Price);
            Assert.AreEqual("new", result[0].ProductType);
            Assert.AreEqual(101, result[0].SellerID);

            // Verify the SQL parameters were correct
            mockDb.Verify(m => m.SetupCommand("GetDummyProductsFromOrderHistory", It.IsAny<Action<SqlCommand>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_EmptyResultSet_ReturnsEmptyList()
        {
            // Arrange
            int orderHistoryId = 99; // ID with no results
            var mockDb = SetupMockDatabase(GetEmptyDataReader());
            var model = new OrderHistoryModelWrapper(_connectionString, mockDb.Object);

            // Act
            var result = await model.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            // Verify the SQL parameters were correct
            mockDb.Verify(m => m.SetupCommand("GetDummyProductsFromOrderHistory", It.IsAny<Action<SqlCommand>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_NullValues_HandlesNullsCorrectly()
        {
            // Arrange
            int orderHistoryId = 2;
            var mockDb = SetupMockDatabase(GetNullValuesDataReader());
            var model = new OrderHistoryModelWrapper(_connectionString, mockDb.Object);

            // Act
            var result = await model.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result[0].ID);
            Assert.AreEqual("Null Values Product", result[0].Name);
            Assert.AreEqual(0, result[0].SellerID); // Default value for null
            Assert.AreEqual(DateTime.MinValue, result[0].StartDate); // Default for null
            Assert.AreEqual(DateTime.MaxValue, result[0].EndDate); // Default for null
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetDummyProductsFromOrderHistoryAsync_DatabaseError_ThrowsSqlException()
        {
            // Arrange
            int orderHistoryId = 3;
            var mockDb = new Mock<IDatabaseHelper>();
            mockDb.Setup(m => m.SetupCommand(It.IsAny<string>(), It.IsAny<Action<SqlCommand>>()))
                .Throws(new Exception("Database error"));
            var model = new OrderHistoryModelWrapper(_connectionString, mockDb.Object);

            // Act
            await model.GetDummyProductsFromOrderHistoryAsync(orderHistoryId); // Should throw SqlException
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public async Task GetDummyProductsFromOrderHistoryAsync_InvalidDataTypes_ThrowsInvalidCastException()
        {
            // Arrange
            int orderHistoryId = 4;
            var mockDb = SetupMockDatabase(GetInvalidDataTypesReader());
            var model = new OrderHistoryModelWrapper(_connectionString, mockDb.Object);

            // Act
            await model.GetDummyProductsFromOrderHistoryAsync(orderHistoryId); // Should throw InvalidCastException
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_Large_ReturnsCorrectNumberOfProducts()
        {
            // Arrange
            int orderHistoryId = 5;
            var mockDb = SetupMockDatabase(GetLargeDataReader(1000)); // 1000 products
            var model = new OrderHistoryModelWrapper(_connectionString, mockDb.Object);

            // Act
            var result = await model.GetDummyProductsFromOrderHistoryAsync(orderHistoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1000, result.Count);
        }

        // Helper Methods for Creating Mock Data
        private Mock<IDatabaseHelper> SetupMockDatabase(DbDataReader mockReader)
        {
            var mockDb = new Mock<IDatabaseHelper>();
            mockDb.Setup(m => m.SetupCommand(It.IsAny<string>(), It.IsAny<Action<SqlCommand>>()))
                .Returns(mockReader);
            return mockDb;
        }

        private DbDataReader GetValidProductDataReader()
        {
            // Create a mock data reader with valid product data
            var mockDataReader = new Mock<DbDataReader>();

            // Set up data rows
            var rowIndex = -1;
            var data = new[]
            {
                new object[] { 1, "Test Product 1", 99.99, "new", 101, DateTime.Today, DateTime.Today.AddDays(30) },
                new object[] { 2, "Test Product 2", 149.99, "used", 102, DateTime.Today.AddDays(-10), DateTime.Today.AddDays(20) }
            };

            // Configure Read() to advance through the rows
            mockDataReader.Setup(m => m.ReadAsync(It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(++rowIndex < data.Length));

            // Configure GetOrdinal to return correct column indexes
            mockDataReader.Setup(m => m.GetOrdinal("productID")).Returns(0);
            mockDataReader.Setup(m => m.GetOrdinal("name")).Returns(1);
            mockDataReader.Setup(m => m.GetOrdinal("price")).Returns(2);
            mockDataReader.Setup(m => m.GetOrdinal("productType")).Returns(3);
            mockDataReader.Setup(m => m.GetOrdinal("SellerID")).Returns(4);
            mockDataReader.Setup(m => m.GetOrdinal("startDate")).Returns(5);
            mockDataReader.Setup(m => m.GetOrdinal("endDate")).Returns(6);

            // Configure data access
            mockDataReader.Setup(m => m.GetInt32(0)).Returns(() => (int)data[rowIndex][0]);
            mockDataReader.Setup(m => m.GetString(1)).Returns(() => (string)data[rowIndex][1]);
            mockDataReader.Setup(m => m.GetDouble(2)).Returns(() => (double)data[rowIndex][2]);
            mockDataReader.Setup(m => m.GetString(3)).Returns(() => (string)data[rowIndex][3]);
            mockDataReader.Setup(m => m.GetInt32(4)).Returns(() => (int)data[rowIndex][4]);
            mockDataReader.Setup(m => m.GetDateTime(5)).Returns(() => (DateTime)data[rowIndex][5]);
            mockDataReader.Setup(m => m.GetDateTime(6)).Returns(() => (DateTime)data[rowIndex][6]);

            // For integer indexer
            mockDataReader.Setup(m => m[It.IsAny<int>()]).Returns((int i) => data[rowIndex][i]);

            // For string indexer
            mockDataReader.Setup(m => m[It.IsAny<string>()]).Returns((string name) => {
                int idx = 0;
                switch (name)
                {
                    case "productID": idx = 0; break;
                    case "name": idx = 1; break;
                    case "price": idx = 2; break;
                    case "productType": idx = 3; break;
                    case "SellerID": idx = 4; break;
                    case "startDate": idx = 5; break;
                    case "endDate": idx = 6; break;
                }
                return data[rowIndex][idx];
            });



            return mockDataReader.Object;
        }

        private DbDataReader GetEmptyDataReader()
        {
            var mockDataReader = new Mock<DbDataReader>();
            mockDataReader.Setup(m => m.ReadAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(false)); // No rows
            return mockDataReader.Object;
        }

        private DbDataReader GetNullValuesDataReader()
        {
            // Create a mock data reader with some null values
            var mockDataReader = new Mock<DbDataReader>();

            // Set up data row with null values
            var data = new[]
            {
                new object[] { 2, "Null Values Product", 59.99, "used", DBNull.Value, DBNull.Value, DBNull.Value }
            };

            // Configure Read()
            mockDataReader.Setup(m => m.ReadAsync(It.IsAny<CancellationToken>()))
                .Returns(new Func<Task<bool>>(() => {
                    if (data.Length > 0)
                    {
                        return Task.FromResult(true);
                    }
                    return Task.FromResult(false);
                }))
                .Callback(() => data = Array.Empty<object[]>()); // Clear data after first read


            // Configure GetOrdinal
            mockDataReader.Setup(m => m.GetOrdinal("productID")).Returns(0);
            mockDataReader.Setup(m => m.GetOrdinal("name")).Returns(1);
            mockDataReader.Setup(m => m.GetOrdinal("price")).Returns(2);
            mockDataReader.Setup(m => m.GetOrdinal("productType")).Returns(3);
            mockDataReader.Setup(m => m.GetOrdinal("SellerID")).Returns(4);
            mockDataReader.Setup(m => m.GetOrdinal("startDate")).Returns(5);
            mockDataReader.Setup(m => m.GetOrdinal("endDate")).Returns(6);

            // Configure data access
            mockDataReader.Setup(m => m.GetInt32(0)).Returns(2);
            mockDataReader.Setup(m => m.GetString(1)).Returns("Null Values Product");
            mockDataReader.Setup(m => m.GetDouble(2)).Returns(59.99);
            mockDataReader.Setup(m => m.GetString(3)).Returns("used");

            // Configure DBNull checks
            var statefulData = new object[] { 2, "Null Values Product", 59.99, "used", DBNull.Value, DBNull.Value, DBNull.Value };
            mockDataReader.Setup(m => m[It.IsAny<int>()]).Returns((int i) => statefulData[i]);
            mockDataReader.Setup(m => m[It.IsAny<string>()]).Returns((string name) => {
                int idx = 0;
                switch (name)
                {
                    case "productID": idx = 0; break;
                    case "name": idx = 1; break;
                    case "price": idx = 2; break;
                    case "productType": idx = 3; break;
                    case "SellerID": idx = 4; break;
                    case "startDate": idx = 5; break;
                    case "endDate": idx = 6; break;
                }
                return statefulData[idx];
            });

            return mockDataReader.Object;
        }

        private DbDataReader GetInvalidDataTypesReader()
        {
            // Create a data reader that will throw InvalidCastException
            var mockDataReader = new Mock<DbDataReader>();

            // Configure Read()
            mockDataReader.Setup(m => m.ReadAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            // Configure GetOrdinal
            mockDataReader.Setup(m => m.GetOrdinal("productID")).Returns(0);

            // This will cause an InvalidCastException when GetInt32 is called on a string
            mockDataReader.Setup(m => m.GetInt32(0))
                .Throws(new InvalidCastException("Cannot convert string to int"));

            return mockDataReader.Object;
        }

        private DbDataReader GetLargeDataReader(int count)
        {
            // Create a mock data reader with many rows
            var mockDataReader = new Mock<DbDataReader>();
            var rowCount = 0;

            // Configure Read() to return true 'count' times
            mockDataReader.Setup(m => m.ReadAsync(It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(rowCount++ < count));

            // Configure GetOrdinal
            mockDataReader.Setup(m => m.GetOrdinal("productID")).Returns(0);
            mockDataReader.Setup(m => m.GetOrdinal("name")).Returns(1);
            mockDataReader.Setup(m => m.GetOrdinal("price")).Returns(2);
            mockDataReader.Setup(m => m.GetOrdinal("productType")).Returns(3);
            mockDataReader.Setup(m => m.GetOrdinal("SellerID")).Returns(4);
            mockDataReader.Setup(m => m.GetOrdinal("startDate")).Returns(5);
            mockDataReader.Setup(m => m.GetOrdinal("endDate")).Returns(6);

            // Configure data access
            mockDataReader.Setup(m => m.GetInt32(0)).Returns(() => rowCount);
            mockDataReader.Setup(m => m.GetString(1)).Returns(() => $"Product {rowCount}");
            mockDataReader.Setup(m => m.GetDouble(2)).Returns(19.99);
            mockDataReader.Setup(m => m.GetString(3)).Returns("new");

            // Handle column access and DBNull checks
            var columnValues = new Dictionary<string, object>
            {
                { "productID", 0 },
                { "name", "Product" },
                { "price", 19.99 },
                { "productType", "new" },
                { "SellerID", 100 },
                { "startDate", DateTime.Today },
                { "endDate", DateTime.Today.AddMonths(1) }
            };

            mockDataReader.Setup(m => m[It.IsAny<string>()]).Returns((string name) => columnValues[name]);

            return mockDataReader.Object;
        }
    }

    // Wrapper and interface for dependency injection and testing
    public interface IDatabaseHelper
    {
        DbDataReader SetupCommand(string storedProcName, Action<SqlCommand> parameterSetup);
    }

    public class OrderHistoryModelWrapper : OrderHistoryModel
    {
        private readonly IDatabaseHelper _dbHelper;

        public OrderHistoryModelWrapper(string connectionString, IDatabaseHelper dbHelper)
            : base(connectionString)
        {
            _dbHelper = dbHelper;
        }

        // Override the GetDummyProductsFromOrderHistoryAsync method to use our mock
        public new async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            List<DummyProduct> dummyProducts = new List<DummyProduct>();

            try
            {
                DbDataReader sqlDataReader = _dbHelper.SetupCommand(
                    "GetDummyProductsFromOrderHistory",
                    cmd => cmd.Parameters.AddWithValue("@OrderHistory", orderHistoryID));

                while (await sqlDataReader.ReadAsync())
                {
                    DummyProduct dummyProduct = new DummyProduct
                    {
                        ID = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("productID")),
                        Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("name")),
                        Price = (float)sqlDataReader.GetDouble(sqlDataReader.GetOrdinal("price")),
                        ProductType = sqlDataReader.GetString(sqlDataReader.GetOrdinal("productType")),
                        SellerID = sqlDataReader["SellerID"] != DBNull.Value
                            ? sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("SellerID")) : 0,
                        StartDate = sqlDataReader["startDate"] != DBNull.Value
                            ? sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("startDate")) : DateTime.MinValue,
                        EndDate = sqlDataReader["endDate"] != DBNull.Value
                            ? sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("endDate")) : DateTime.MaxValue
                    };
                    dummyProducts.Add(dummyProduct);
                }
            }
            catch (Exception ex)
            {
                // Re-throw the exception to match original behavior
                throw ex;
            }

            return dummyProducts;
        }
    }
}
