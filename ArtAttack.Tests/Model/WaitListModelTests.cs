using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data;
using ArtAttack.Model;
using ArtAttack.Domain;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ArtAttack.Shared;

namespace ArtAttack.Tests.Model
{
    [TestClass]
    public class WaitListModelTests
    {
        private Mock<IDbConnection> _mockConnection;
        private Mock<IDbCommand> _mockCommand;
        private Mock<IDataReader> _mockReader;
        private Mock<IDataParameterCollection> _mockParameters;
        private Mock<IDbDataParameter> _mockParameter;
        private string _testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";
        private WaitListModel _waitListModel;
        private MockDatabase _mockDatabase;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockReader = new Mock<IDataReader>();
            _mockParameters = new Mock<IDataParameterCollection>();
            _mockParameter = new Mock<IDbDataParameter>();

            // Setup the parameter collection mock
            _mockParameters.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

            // Setup the command mock
            _mockCommand.Setup(c => c.CreateParameter()).Returns(_mockParameter.Object);
            _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object);

            // Setup the connection mock
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

            // Setup the mock database - make sure this is always used
            _mockDatabase = new MockDatabase();
            _mockDatabase.SetupMockConnection(_mockConnection.Object);

            // Initialize the model with the mock database
            _waitListModel = new WaitListModel(_testConnectionString, _mockDatabase);
        }

        [TestMethod]
        public void Constructor_SetsConnectionString()
        {
            // Arrange & Act
            var model = new WaitListModel(_testConnectionString);

            // Assert - using reflection to access private field
            var field = typeof(WaitListModel).GetField("connectionString",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var value = field.GetValue(model);

            Assert.AreEqual(_testConnectionString, value);
        }

        [TestMethod]
        public void AddUserToWaitlist_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            _waitListModel.AddUserToWaitlist(userId, productWaitListId);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "AddUserToWaitlist");
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockConnection.Verify(c => c.Close(), Times.Once);
        }

        [TestMethod]
        public void AddUserToWaitlist_AddsCorrectParameters()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;

            // Clear any previous setup and prepare for parameter check
            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Capture parameters as they are created and set
            List<(string Name, object Value)> capturedParams = new List<(string, object)>();
            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(name => capturedParams.Add((name, null)));
            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(val => capturedParams[capturedParams.Count - 1] = (capturedParams[capturedParams.Count - 1].Name, val));

            // Act
            _waitListModel.AddUserToWaitlist(userId, productWaitListId);

            // Assert
            Assert.AreEqual(2, capturedParams.Count);
            Assert.AreEqual("@UserID", capturedParams[0].Name);
            Assert.AreEqual(userId, capturedParams[0].Value);
            Assert.AreEqual("@ProductWaitListID", capturedParams[1].Name);
            Assert.AreEqual(productWaitListId, capturedParams[1].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void AddUserToWaitlist_ThrowsException_WhenExecutionFails()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;

            // Setup the command to throw an exception when executed
            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Throws(new Exception("Database error"));

            // Act & Assert
            _waitListModel.AddUserToWaitlist(userId, productWaitListId);
            // The ExpectedException attribute will handle the assertion
        }


        #region RemoveUserFromWaitlist Tests

        [TestMethod]
        public void RemoveUserFromWaitlist_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Act
            _waitListModel.RemoveUserFromWaitlist(userId, productWaitListId);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "RemoveUserFromWaitlist");
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            _mockConnection.Verify(c => c.Close(), Times.Once);
        }

        [TestMethod]
        public void RemoveUserFromWaitlist_AddsCorrectParameters()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

            // Capture parameters
            List<(string Name, object Value)> capturedParams = new List<(string, object)>();
            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(name => capturedParams.Add((name, null)));
            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(val => capturedParams[capturedParams.Count - 1] = (capturedParams[capturedParams.Count - 1].Name, val));

            // Act
            _waitListModel.RemoveUserFromWaitlist(userId, productWaitListId);

            // Assert
            Assert.AreEqual(2, capturedParams.Count);
            Assert.AreEqual("@UserID", capturedParams[0].Name);
            Assert.AreEqual(userId, capturedParams[0].Value);
            Assert.AreEqual("@ProductWaitListID", capturedParams[1].Name);
            Assert.AreEqual(productWaitListId, capturedParams[1].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void RemoveUserFromWaitlist_ThrowsException_WhenExecutionFails()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(c => c.ExecuteNonQuery()).Throws(new Exception("Database error"));

            // Act & Assert
            _waitListModel.RemoveUserFromWaitlist(userId, productWaitListId);
            // ExpectedException attribute handles the assertion
        }

        #endregion

        #region GetUsersInWaitlist Tests

        [TestMethod]
        public void GetUsersInWaitlist_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int waitlistProductId = 101;

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            SetupMockDataReader(new List<Dictionary<string, object>>());

            // Act
            var result = _waitListModel.GetUsersInWaitlist(waitlistProductId);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "GetUsersInWaitlist");
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
        }


        [TestMethod]
        public void GetUsersInWaitlist_ReturnsCorrectData()
        {
            // Arrange
            int waitlistProductId = 101;
            DateTime today = DateTime.Today;

            // Setup column ordinals
            _mockReader.Setup(r => r.GetOrdinal("userID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("positionInQueue")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("joinedTime")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("productWaitListID")).Returns(3); // Add this line

            // Setup Read() method to return true twice then false
            _mockReader.SetupSequence(r => r.Read())
                .Returns(true)  // First call - first record
                .Returns(true)  // Second call - second record
                .Returns(false); // Third call - no more records

            // Setup data for each column with explicit indices
            // First user
            _mockReader.SetupSequence(r => r.GetInt32(0))  // userID column
                .Returns(1)  // First row
                .Returns(2); // Second row

            _mockReader.SetupSequence(r => r.GetInt32(1))  // positionInQueue column
                .Returns(1)  // First row
                .Returns(2); // Second row

            _mockReader.SetupSequence(r => r.GetDateTime(2))  // joinedTime column
                .Returns(today)  // First row
                .Returns(today.AddDays(-1)); // Second row

            // Set productWaitListID value to be the same as the input parameter
            _mockReader.Setup(r => r.GetInt32(3)).Returns(waitlistProductId); // Add this line

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var result = _waitListModel.GetUsersInWaitlist(waitlistProductId);

            // Assert
            Assert.AreEqual(2, result.Count);

            // First user
            Assert.AreEqual(1, result[0].UserID);
            Assert.AreEqual(1, result[0].PositionInQueue);
            Assert.AreEqual(today, result[0].JoinedTime);
            Assert.AreEqual(waitlistProductId, result[0].ProductWaitListID);

            // Second user
            Assert.AreEqual(2, result[1].UserID);
            Assert.AreEqual(2, result[1].PositionInQueue);
            Assert.AreEqual(today.AddDays(-1), result[1].JoinedTime);
            Assert.AreEqual(waitlistProductId, result[1].ProductWaitListID);
        }


        [TestMethod]
        public void GetUsersInWaitlist_ReturnsEmptyList_WhenNoData()
        {
            // Arrange
            int waitlistProductId = 101;
            SetupMockDataReader(new List<Dictionary<string, object>>());

            // Act
            var result = _waitListModel.GetUsersInWaitlist(waitlistProductId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetUsersInWaitlist_AddsCorrectParameter()
        {
            // Arrange
            int waitlistProductId = 101;
            SetupMockDataReader(new List<Dictionary<string, object>>());

            List<(string Name, object Value)> capturedParams = new List<(string, object)>();
            _mockParameter.SetupSet(p => p.ParameterName = It.IsAny<string>())
                .Callback<string>(name => capturedParams.Add((name, null)));
            _mockParameter.SetupSet(p => p.Value = It.IsAny<object>())
                .Callback<object>(val => capturedParams[capturedParams.Count - 1] = (capturedParams[capturedParams.Count - 1].Name, val));

            // Act
            _waitListModel.GetUsersInWaitlist(waitlistProductId);

            // Assert
            Assert.AreEqual(1, capturedParams.Count);
            Assert.AreEqual("@WaitListProductID", capturedParams[0].Name);
            Assert.AreEqual(waitlistProductId, capturedParams[0].Value);
        }

        #endregion

        #region GetUserWaitlists Tests

        [TestMethod]
        public void GetUserWaitlists_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int userId = 42;

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            SetupMockDataReader(new List<Dictionary<string, object>>());

            // Act
            var result = _waitListModel.GetUserWaitlists(userId);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "GetUserWaitlists");
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
        }

        [TestMethod]
        public void GetUserWaitlists_ReturnsCorrectData()
        {
            // Arrange
            int userId = 42;
            DateTime today = DateTime.Today;

            var testData = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "productWaitListID", 101 },
                    { "positionInQueue", 1 },
                    { "joinedTime", today }
                },
                new Dictionary<string, object>
                {
                    { "productWaitListID", 102 },
                    { "positionInQueue", 3 },
                    { "joinedTime", today.AddDays(-2) }
                }
            };

            // For each key in each dictionary, set up specific return value
            _mockReader.Setup(r => r.GetOrdinal("productWaitListID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("positionInQueue")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("joinedTime")).Returns(2);

            _mockReader.Setup(r => r.Read())
                .Returns(() => testData.Count > 0)
                .Callback(() => {
                    if (testData.Count > 0) testData.RemoveAt(0);
                });

            // Set up sequenced returns 
            _mockReader.SetupSequence(r => r.GetInt32(0))
                .Returns(101)  // First call returns productWaitListID = 101
                .Returns(102); // Second call returns productWaitListID = 102

            _mockReader.SetupSequence(r => r.GetInt32(1))
                .Returns(1)   // First call returns positionInQueue = 1 
                .Returns(3);  // Second call returns positionInQueue = 3

            _mockReader.SetupSequence(r => r.GetDateTime(2))
                .Returns(today)  // First call
                .Returns(today.AddDays(-2)); // Second call

            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var result = _waitListModel.GetUserWaitlists(userId);

            // Assert
            Assert.AreEqual(2, result.Count);

            // First waitlist
            Assert.AreEqual(userId, result[0].UserID);
            Assert.AreEqual(101, result[0].ProductWaitListID);
            Assert.AreEqual(1, result[0].PositionInQueue);  // Fix here - It should be 1, not 101
            Assert.AreEqual(today, result[0].JoinedTime);

            // Second waitlist
            Assert.AreEqual(userId, result[1].UserID);
            Assert.AreEqual(102, result[1].ProductWaitListID);
            Assert.AreEqual(3, result[1].PositionInQueue);
            Assert.AreEqual(today.AddDays(-2), result[1].JoinedTime);
        }


        #endregion

        #region GetWaitlistSize Tests

        [TestMethod]
        public void GetWaitlistSize_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int productWaitListId = 101;
            int expectedSize = 5;

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            SetupOutputParameter("@TotalUsers", expectedSize);

            // Act
            var result = _waitListModel.GetWaitlistSize(productWaitListId);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "GetWaitlistSize");
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
            Assert.AreEqual(expectedSize, result);
        }

        [TestMethod]
        public void GetWaitlistSize_ReturnsZero_WhenWaitlistEmpty()
        {
            // Arrange
            int productWaitListId = 101;
            int expectedSize = 0;

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            SetupOutputParameter("@TotalUsers", expectedSize);

            // Act
            var result = _waitListModel.GetWaitlistSize(productWaitListId);

            // Assert
            Assert.AreEqual(expectedSize, result);
        }

        [TestMethod]
        public void GetWaitlistSize_ReturnsZero_WhenOutputParameterIsDBNull()
        {
            // Arrange
            int productWaitListId = 101;

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);

            // Setup output parameter to return DBNull.Value
            _mockParameter.Setup(p => p.Direction).Returns(ParameterDirection.Output);
            _mockParameter.SetupProperty(p => p.Value);
            _mockParameter.Setup(p => p.ParameterName).Returns("@TotalUsers");

            _mockCommand.Setup(c => c.ExecuteNonQuery())
                .Callback(() => {
                    _mockParameter.Object.Value = DBNull.Value;
                })
                .Returns(1);

            // Act
            var result = _waitListModel.GetWaitlistSize(productWaitListId);

            // Assert
            Assert.AreEqual(0, result, "Method should return 0 when output parameter is DBNull.Value");
            _mockCommand.VerifySet(c => c.CommandText = "GetWaitlistSize");
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
        }


        #endregion

        #region IsUserInWaitlist Tests

        [TestMethod]
        public void IsUserInWaitlist_ReturnsTrueWhenUserExists()
        {
            // Arrange
            int userId = 42;
            int productId = 101;

            _mockCommand.Setup(c => c.ExecuteScalar()).Returns(1); // Non-null value

            // Act
            var result = _waitListModel.IsUserInWaitlist(userId, productId);

            // Assert
            Assert.IsTrue(result);
            _mockCommand.VerifySet(c => c.CommandText = "CheckUserInProductWaitlist");
            _mockCommand.Verify(c => c.ExecuteScalar(), Times.Once);
        }

        [TestMethod]
        public void IsUserInWaitlist_ReturnsFalseWhenUserDoesntExist()
        {
            // Arrange
            int userId = 42;
            int productId = 101;

            _mockCommand.Setup(c => c.ExecuteScalar()).Returns(null); // Null result

            // Act
            var result = _waitListModel.IsUserInWaitlist(userId, productId);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region GetUserWaitlistPosition Tests

        [TestMethod]
        public void GetUserWaitlistPosition_ReturnsPositionWhenUserInWaitlist()
        {
            // Arrange
            int userId = 42;
            int productId = 101;
            int expectedPosition = 3;

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            SetupOutputParameter("@Position", expectedPosition);

            // Act
            var result = _waitListModel.GetUserWaitlistPosition(userId, productId);

            // Assert
            _mockCommand.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(c => c.CommandText = "GetUserWaitlistPosition");
            _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
            Assert.AreEqual(expectedPosition, result);
        }

        [TestMethod]
        public void GetUserWaitlistPosition_ReturnsNegativeOneWhenUserNotInWaitlist()
        {
            // Arrange
            int userId = 42;
            int productId = 101;

            _mockCommand.Setup(c => c.CommandType).Returns(CommandType.StoredProcedure);
            SetupOutputParameter("@Position", DBNull.Value);

            // Act
            var result = _waitListModel.GetUserWaitlistPosition(userId, productId);

            // Assert
            Assert.AreEqual(-1, result);
        }

        #endregion

        #region GetUsersInWaitlistOrdered Tests

        [TestMethod]
        public void GetUsersInWaitlistOrdered_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int productId = 101;
            SetupMockDataReader(new List<Dictionary<string, object>>());

            // Act
            var result = _waitListModel.GetUsersInWaitlistOrdered(productId);

            // Assert
            _mockCommand.VerifySet(c => c.CommandText = "GetOrderedWaitlistUsers");
            _mockCommand.Verify(c => c.ExecuteReader(), Times.Once);
            _mockConnection.Verify(c => c.Open(), Times.Once);
        }

        [TestMethod]
        public void GetUsersInWaitlistOrdered_ReturnsCorrectData()
        {
            // Arrange
            int productId = 101;
            DateTime today = DateTime.Today;

            var testData = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "productWaitListID", 101 },
                    { "userID", 1 },
                    { "joinedTime", today },
                    { "positionInQueue", 1 }
                },
                new Dictionary<string, object>
                {
                    { "productWaitListID", 101 },
                    { "userID", 2 },
                    { "joinedTime", today.AddDays(-1) },
                    { "positionInQueue", 2 }
                }
            };

            // For each key in each dictionary, set up specific return value
            _mockReader.Setup(r => r.GetOrdinal("productWaitListID")).Returns(0);
            _mockReader.Setup(r => r.GetOrdinal("userID")).Returns(1);
            _mockReader.Setup(r => r.GetOrdinal("joinedTime")).Returns(2);
            _mockReader.Setup(r => r.GetOrdinal("positionInQueue")).Returns(3);

            _mockReader.Setup(r => r.Read())
                .Returns(() => testData.Count > 0)
                .Callback(() => {
                    if (testData.Count > 0) testData.RemoveAt(0);
                });

            // Set up sequenced returns 
            _mockReader.SetupSequence(r => r.GetInt32(0))   // productWaitListID
                .Returns(101)
                .Returns(101);

            _mockReader.SetupSequence(r => r.GetInt32(1))   // userID
                .Returns(1)
                .Returns(2);

            _mockReader.SetupSequence(r => r.GetDateTime(2)) // joinedTime
                .Returns(today)
                .Returns(today.AddDays(-1));

            _mockReader.SetupSequence(r => r.GetInt32(3))   // positionInQueue
                .Returns(1)
                .Returns(2);

            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var result = _waitListModel.GetUsersInWaitlistOrdered(productId);

            // Assert
            Assert.AreEqual(2, result.Count);

            // First user
            Assert.AreEqual(1, result[0].UserID);
            Assert.AreEqual(101, result[0].ProductWaitListID);
            Assert.AreEqual(1, result[0].PositionInQueue);
            Assert.AreEqual(today, result[0].JoinedTime);

            // Second user
            Assert.AreEqual(2, result[1].UserID);
            Assert.AreEqual(101, result[1].ProductWaitListID);
            Assert.AreEqual(2, result[1].PositionInQueue);
            Assert.AreEqual(today.AddDays(-1), result[1].JoinedTime);
        }


        #endregion

        [TestMethod]
        public void CreateConnection_ReturnsNewSqlConnection_WithCorrectConnectionString()
        {
            // Arrange
            string testConnectionString = "Data Source=test;Initial Catalog=TestDB;Integrated Security=True";
            var provider = new SqlDatabaseProvider();

            // Act
            var connection = provider.CreateConnection(testConnectionString) as SqlConnection;

            // Assert
            Assert.IsNotNull(connection, "Connection should not be null");
            Assert.IsInstanceOfType(connection, typeof(SqlConnection), "Connection should be of type SqlConnection");
            Assert.AreEqual(testConnectionString, connection.ConnectionString, "Connection string should match the provided one");
        }

        #region Helper Methods

        private void SetupMockDataReader(List<Dictionary<string, object>> data)
        {
            _mockReader.Setup(r => r.Read())
                .Returns(() => data.Count > 0)
                .Callback(() => {
                    if (data.Count > 0) data.RemoveAt(0);
                });

            foreach (var record in data)
            {
                foreach (var column in record.Keys)
                {
                    _mockReader.Setup(r => r.GetOrdinal(column)).Returns(0);

                    if (record[column] is int intValue)
                        _mockReader.Setup(r => r.GetInt32(0)).Returns(intValue);

                    if (record[column] is DateTime dateValue)
                        _mockReader.Setup(r => r.GetDateTime(0)).Returns(dateValue);

                    _mockReader.Setup(r => r[column]).Returns(record[column]);
                }
            }

            _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);
        }

        private void SetupOutputParameter(string paramName, object value)
        {
            _mockParameter.Setup(p => p.Direction).Returns(ParameterDirection.Output);
            _mockParameter.SetupProperty(p => p.Value);
            _mockParameter.Setup(p => p.ParameterName).Returns(paramName);

            _mockCommand.Setup(c => c.ExecuteNonQuery())
                .Callback(() => {
                    _mockParameter.Object.Value = value;
                })
                .Returns(1);
        }

        #endregion
    }


    // Helper class for database mocking
    public class MockDatabase : IDatabaseProvider
    {
        private IDbConnection _mockConnection;

        public IDbConnection CreateConnection(string connectionString)
        {
            return _mockConnection;
        }

        public void SetupMockConnection(IDbConnection mockConnection)
        {
            _mockConnection = mockConnection;
        }

        public void SetupParameterCheck(string paramName, SqlDbType type, object value)
        {
            // Implementation not needed for current tests
        }

        public void VerifyParameterAdded(string paramName, object value)
        {
            // Implementation not needed for current tests
        }

        public void VerifyConnectionOpenAndClose()
        {
            // Implementation not needed for current tests
        }
    }

    // Helper class for database mocking
    //public class MockDatabase : ArtAttack.Model.IDatabaseProvider
    //{
    //    private IDbConnection _mockConnection;
    //    private Mock<IDbCommand> _mockCommand = new Mock<IDbCommand>();
    //    private Mock<IDataParameterCollection> _mockParameters = new Mock<IDataParameterCollection>();
    //    private Dictionary<string, (SqlDbType Type, object Value)> _expectedParameters =
    //        new Dictionary<string, (SqlDbType, object)>();

    //    public IDbConnection CreateConnection(string connectionString)
    //    {
    //        return _mockConnection;
    //    }

    //    public void SetupMockConnection(IDbConnection mockConnection)
    //    {
    //        _mockConnection = mockConnection;

    //        // Setup the mock command
    //        _mockCommand.Setup(c => c.Parameters).Returns(_mockParameters.Object);

    //        // Setup the mock connection to return our mock command
    //        Mock.Get(_mockConnection)
    //            .Setup(c => c.CreateCommand())
    //            .Returns(_mockCommand.Object);
    //    }

    //    public void SetupParameterCheck(string paramName, SqlDbType type, object value)
    //    {
    //        _expectedParameters[paramName] = (type, value);

    //        // Setup parameter mock
    //        var mockParam = new Mock<IDbDataParameter>();
    //        mockParam.SetupAllProperties();

    //        // Setup parameters collection to return parameter when asked
    //        _mockParameters.Setup(p => p.Add(It.Is<IDbDataParameter>(
    //            param => param.ParameterName == paramName &&
    //                     param.Value.Equals(value))))
    //            .Returns(0);
    //    }

    //    public void VerifyParameterAdded(string paramName, object value)
    //    {
    //        _mockParameters.Verify(p => p.Add(It.Is<IDbDataParameter>(
    //            param => param.ParameterName == paramName &&
    //                     param.Value.Equals(value))), Times.Once);
    //    }

    //    public void VerifyConnectionOpenAndClose()
    //    {
    //        Mock.Get(_mockConnection).Verify(c => c.Open(), Times.Once);
    //        Mock.Get(_mockConnection).Verify(c => c.Close(), Times.AtLeastOnce);
    //    }
    //}
}