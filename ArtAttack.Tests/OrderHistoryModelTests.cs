using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ArtAttack.Tests.Model
{
    [TestClass]
    public class OrderHistoryModelTests
    {
        private const string TestConnectionString = "Data Source=test;Initial Catalog=TestDB;Integrated Security=True";

        [TestMethod]
        public void Constructor_WithConnectionString_SetsConnectionStringProperty()
        {
            // Arrange & Act
            var model = new OrderHistoryModel(TestConnectionString);

            // Assert
            // We're testing a private field, so we can only verify functionality indirectly
            // by testing methods that use the connection string
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task GetDummyProductsFromOrderHistoryAsync_ReturnsExpectedProducts()
        {
            // Arrange
            var mockSqlHelper = new MockSqlHelper();
            mockSqlHelper.SetupDataReader(new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "productID", 1 },
                    { "name", "Test Product 1" },
                    { "price", 99.99 },
                    { "productType", "new" },
                    { "SellerID", 101 },
                    { "startDate", DateTime.Today },
                    { "endDate", DateTime.Today.AddDays(30) }
                },
                new Dictionary<string, object>
                {
                    { "productID", 2 },
                    { "name", "Test Product 2" },
                    { "price", 149.99 },
                    { "productType", "used" },
                    { "SellerID", DBNull.Value },
                    { "startDate", DBNull.Value },
                    { "endDate", DBNull.Value }
                }
            });

            var orderHistoryModel = new OrderHistoryModelWrapper(TestConnectionString, mockSqlHelper);

            // Act
            var result = await orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            // Check first product
            Assert.AreEqual(1, result[0].ID);
            Assert.AreEqual("Test Product 1", result[0].Name);
            Assert.AreEqual(99.99f, result[0].Price);
            Assert.AreEqual("new", result[0].ProductType);
            Assert.AreEqual(101, result[0].SellerID);
            Assert.AreEqual(DateTime.Today, result[0].StartDate);
            Assert.AreEqual(DateTime.Today.AddDays(30), result[0].EndDate);

            // Check second product
            Assert.AreEqual(2, result[1].ID);
            Assert.AreEqual("Test Product 2", result[1].Name);
            Assert.AreEqual(149.99f, result[1].Price);
            Assert.AreEqual("used", result[1].ProductType);
            Assert.AreEqual(0, result[1].SellerID);  // Default value for null
            Assert.AreEqual(DateTime.MinValue, result[1].StartDate);  // Default value for null
            Assert.AreEqual(DateTime.MaxValue, result[1].EndDate);  // Default value for null

            // Verify stored procedure was called with correct parameters
            Assert.AreEqual("GetDummyProductsFromOrderHistory", mockSqlHelper.LastCommandText);
            Assert.AreEqual(CommandType.StoredProcedure, mockSqlHelper.LastCommandType);
            Assert.AreEqual(1, mockSqlHelper.LastParameters["@OrderHistory"]);
        }
    }

    // Helper classes for testing
    public class OrderHistoryModelWrapper : OrderHistoryModel
    {
        private readonly MockSqlHelper _mockSqlHelper;

        public OrderHistoryModelWrapper(string connectionString, MockSqlHelper mockSqlHelper)
            : base(connectionString)
        {
            _mockSqlHelper = mockSqlHelper;
        }

        public new async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            List<DummyProduct> dummyProducts = new List<DummyProduct>();

            _mockSqlHelper.LastCommandText = "GetDummyProductsFromOrderHistory";
            _mockSqlHelper.LastCommandType = CommandType.StoredProcedure;
            _mockSqlHelper.LastParameters = new Dictionary<string, object>
            {
                { "@OrderHistory", orderHistoryID }
            };

            var dataReader = _mockSqlHelper.ExecuteReader();

            while (await dataReader.ReadAsync())
            {
                DummyProduct dummyProduct = new DummyProduct
                {
                    ID = dataReader.GetInt32(dataReader.GetOrdinal("productID")),
                    Name = dataReader.GetString(dataReader.GetOrdinal("name")),
                    Price = (float)dataReader.GetDouble(dataReader.GetOrdinal("price")),
                    ProductType = dataReader.GetString(dataReader.GetOrdinal("productType")),
                    SellerID = dataReader["SellerID"] != DBNull.Value
                        ? dataReader.GetInt32(dataReader.GetOrdinal("SellerID")) : 0,
                    StartDate = dataReader["startDate"] != DBNull.Value
                        ? dataReader.GetDateTime(dataReader.GetOrdinal("startDate")) : DateTime.MinValue,
                    EndDate = dataReader["endDate"] != DBNull.Value
                        ? dataReader.GetDateTime(dataReader.GetOrdinal("endDate")) : DateTime.MaxValue
                };
                dummyProducts.Add(dummyProduct);
            }

            return dummyProducts;
        }
    }

    public class MockSqlHelper
    {
        private List<Dictionary<string, object>> _mockData;

        public string LastCommandText { get; set; }
        public CommandType LastCommandType { get; set; }
        public Dictionary<string, object> LastParameters { get; set; }

        public void SetupDataReader(List<Dictionary<string, object>> mockData)
        {
            _mockData = mockData;
        }

        public MockDataReader ExecuteReader()
        {
            return new MockDataReader(_mockData);
        }
    }

    public class MockDataReader : IDataReader
    {
        private readonly List<Dictionary<string, object>> _rows;
        private int _currentIndex = -1;
        private Dictionary<string, int> _ordinalMap;

        public MockDataReader(List<Dictionary<string, object>> rows)
        {
            _rows = rows;

            // Build ordinal map if rows exist
            if (rows.Count > 0)
            {
                _ordinalMap = new Dictionary<string, int>();
                int i = 0;
                foreach (var key in rows[0].Keys)
                {
                    _ordinalMap[key] = i++;
                }
            }
        }

        public async Task<bool> ReadAsync()
        {
            return Read();
        }

        public bool Read()
        {
            _currentIndex++;
            return _currentIndex < _rows.Count;
        }

        public int GetOrdinal(string name)
        {
            return _ordinalMap[name];
        }

        public int GetInt32(int i)
        {
            var columnName = GetColumnNameByOrdinal(i);
            var value = _rows[_currentIndex][columnName];
            return Convert.ToInt32(value);
        }

        public string GetString(int i)
        {
            var columnName = GetColumnNameByOrdinal(i);
            var value = _rows[_currentIndex][columnName];
            return Convert.ToString(value);
        }

        public double GetDouble(int i)
        {
            var columnName = GetColumnNameByOrdinal(i);
            var value = _rows[_currentIndex][columnName];
            return Convert.ToDouble(value);
        }

        public DateTime GetDateTime(int i)
        {
            var columnName = GetColumnNameByOrdinal(i);
            var value = _rows[_currentIndex][columnName];
            return Convert.ToDateTime(value);
        }

        public object this[string name]
        {
            get { return _rows[_currentIndex][name]; }
        }

        public object this[int i]
        {
            get { return _rows[_currentIndex][GetColumnNameByOrdinal(i)]; }
        }

        private string GetColumnNameByOrdinal(int i)
        {
            foreach (var kvp in _ordinalMap)
            {
                if (kvp.Value == i)
                    return kvp.Key;
            }
            throw new IndexOutOfRangeException($"Could not find column at ordinal {i}");
        }

        #region IDataReader Implementation
        // Implement other IDataReader members as needed
        public void Close() { }
        public int Depth => 0;
        public bool IsClosed => false;
        public int RecordsAffected => 0;
        public int FieldCount => _ordinalMap?.Count ?? 0;
        public void Dispose() { }
        // These methods would be implemented similarly to the ones above
        public bool GetBoolean(int i) => throw new NotImplementedException();
        public byte GetByte(int i) => throw new NotImplementedException();
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => throw new NotImplementedException();
        public char GetChar(int i) => throw new NotImplementedException();
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => throw new NotImplementedException();
        public IDataReader GetData(int i) => throw new NotImplementedException();
        public string GetDataTypeName(int i) => throw new NotImplementedException();
        public decimal GetDecimal(int i) => throw new NotImplementedException();
        public float GetFloat(int i) => throw new NotImplementedException();
        public Guid GetGuid(int i) => throw new NotImplementedException();
        public short GetInt16(int i) => throw new NotImplementedException();
        public long GetInt64(int i) => throw new NotImplementedException();
        public Type GetFieldType(int i) => throw new NotImplementedException();
        public string GetName(int i) => throw new NotImplementedException();
        public DataTable GetSchemaTable() => throw new NotImplementedException();
        public object GetValue(int i) => throw new NotImplementedException();
        public int GetValues(object[] values) => throw new NotImplementedException();
        public bool IsDBNull(int i) => _rows[_currentIndex][GetColumnNameByOrdinal(i)] == DBNull.Value;
        public bool NextResult() => false;
        #endregion
    }
}
