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
        private readonly string _testConnectionString = "Server=testserver;Database=testdb;User Id=testuser;Password=testpass;";
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
            _mockParameters.Setup(parameterCollection => parameterCollection.Add(It.IsAny<object>())).Returns(0);

            // Setup the command mock for creating parameters and exposing the parameter collection
            _mockCommand.Setup(command => command.CreateParameter()).Returns(_mockParameter.Object);
            _mockCommand.Setup(command => command.Parameters).Returns(_mockParameters.Object);

            // Setup the connection mock to return the command mock
            _mockConnection.Setup(connection => connection.CreateCommand()).Returns(_mockCommand.Object);

            // Setup the mock database - ensure this is always used
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

            // Assert - using reflection to access private field "connectionString"
            FieldInfo connectionStringField = typeof(WaitListModel).GetField("connectionString",
                BindingFlags.NonPublic | BindingFlags.Instance);
            object actualValue = connectionStringField.GetValue(model);

            Assert.AreEqual(_testConnectionString, actualValue);
        }

        [TestMethod]
        public void AddUserToWaitlist_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);

            // Act
            _waitListModel.AddUserToWaitlist(userId, productWaitListId);

            // Assert
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "AddUserToWaitlist");
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockConnection.Verify(connection => connection.Close(), Times.Once);
        }

        [TestMethod]
        public void AddUserToWaitlist_AddsCorrectParameters()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);

            // Capture parameters as they are created and set
            List<(string Name, object Value)> capturedParameters = new List<(string, object)>();

            _mockParameter.SetupSet(dbParameter => dbParameter.ParameterName = It.IsAny<string>())
                .Callback<string>(parameterName => capturedParameters.Add((parameterName, null)));
            _mockParameter.SetupSet(dbParameter => dbParameter.Value = It.IsAny<object>())
                .Callback<object>(parameterValue =>
                {
                    var lastIndex = capturedParameters.Count - 1;
                    capturedParameters[lastIndex] = (capturedParameters[lastIndex].Name, parameterValue);
                });

            // Act
            _waitListModel.AddUserToWaitlist(userId, productWaitListId);

            // Assert
            Assert.AreEqual(2, capturedParameters.Count);
            Assert.AreEqual("@UserID", capturedParameters[0].Name);
            Assert.AreEqual(userId, capturedParameters[0].Value);
            Assert.AreEqual("@ProductWaitListID", capturedParameters[1].Name);
            Assert.AreEqual(productWaitListId, capturedParameters[1].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void AddUserToWaitlist_WhenExecutionFails_ThrowsException_()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Throws(new Exception("Database error"));

            // Act & Assert
            _waitListModel.AddUserToWaitlist(userId, productWaitListId);
        }



        [TestMethod]
        public void RemoveUserFromWaitlist_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);

            // Act
            _waitListModel.RemoveUserFromWaitlist(userId, productWaitListId);

            // Assert
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "RemoveUserFromWaitlist");
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            _mockConnection.Verify(connection => connection.Close(), Times.Once);
        }

        [TestMethod]
        public void RemoveUserFromWaitlist_AddsCorrectParameters()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Returns(1);

            // Capture parameters as they are created
            List<(string Name, object Value)> capturedParameters = new List<(string, object)>();

            _mockParameter.SetupSet(dbParameter => dbParameter.ParameterName = It.IsAny<string>())
                .Callback<string>(parameterName => capturedParameters.Add((parameterName, null)));
            _mockParameter.SetupSet(dbParameter => dbParameter.Value = It.IsAny<object>())
                .Callback<object>(parameterValue =>
                {
                    var lastIndex = capturedParameters.Count - 1;
                    capturedParameters[lastIndex] = (capturedParameters[lastIndex].Name, parameterValue);
                });

            // Act
            _waitListModel.RemoveUserFromWaitlist(userId, productWaitListId);

            // Assert
            Assert.AreEqual(2, capturedParameters.Count);
            Assert.AreEqual("@UserID", capturedParameters[0].Name);
            Assert.AreEqual(userId, capturedParameters[0].Value);
            Assert.AreEqual("@ProductWaitListID", capturedParameters[1].Name);
            Assert.AreEqual(productWaitListId, capturedParameters[1].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void RemoveUserFromWaitlist_WhenExecutionFails_ThrowsException_()
        {
            // Arrange
            int userId = 42;
            int productWaitListId = 101;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(command => command.ExecuteNonQuery()).Throws(new Exception("Database error"));

            // Act & Assert
            _waitListModel.RemoveUserFromWaitlist(userId, productWaitListId);
        }

        [TestMethod]
        public void GetUsersInWaitlist_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int waitlistProductId = 101;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            SetupMockDataReader(new List<Dictionary<string, object>>());

            // Act
            var result = _waitListModel.GetUsersInWaitlist(waitlistProductId);

            // Assert
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "GetUsersInWaitlist");
            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
        }

        [TestMethod]
        public void GetUsersInWaitlist_ReturnsCorrectData()
        {
            // Arrange
            int waitlistProductId = 101;
            DateTime today = DateTime.Today;

            _mockReader.Setup(reader => reader.GetOrdinal("userID")).Returns(0);
            _mockReader.Setup(reader => reader.GetOrdinal("positionInQueue")).Returns(1);
            _mockReader.Setup(reader => reader.GetOrdinal("joinedTime")).Returns(2);
            _mockReader.Setup(reader => reader.GetOrdinal("productWaitListID")).Returns(3);

            _mockReader.SetupSequence(reader => reader.Read())
                .Returns(true)
                .Returns(true)
                .Returns(false);

            _mockReader.SetupSequence(reader => reader.GetInt32(0))
                .Returns(1)
                .Returns(2);
            _mockReader.SetupSequence(reader => reader.GetInt32(1))
                .Returns(1)
                .Returns(2);
            _mockReader.SetupSequence(reader => reader.GetDateTime(2))
                .Returns(today)
                .Returns(today.AddDays(-1));
            _mockReader.Setup(reader => reader.GetInt32(3)).Returns(waitlistProductId);

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

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

            List<(string Name, object Value)> capturedParameters = new List<(string, object)>();
            _mockParameter.SetupSet(dbParameter => dbParameter.ParameterName = It.IsAny<string>())
                .Callback<string>(parameterName => capturedParameters.Add((parameterName, null)));
            _mockParameter.SetupSet(dbParameter => dbParameter.Value = It.IsAny<object>())
                .Callback<object>(parameterValue =>
                {
                    var lastIndex = capturedParameters.Count - 1;
                    capturedParameters[lastIndex] = (capturedParameters[lastIndex].Name, parameterValue);
                });

            // Act
            _waitListModel.GetUsersInWaitlist(waitlistProductId);

            // Assert
            Assert.AreEqual(1, capturedParameters.Count);
            Assert.AreEqual("@WaitListProductID", capturedParameters[0].Name);
            Assert.AreEqual(waitlistProductId, capturedParameters[0].Value);
        }

        [TestMethod]
        public void GetUserWaitlists_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int userId = 42;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            SetupMockDataReader(new List<Dictionary<string, object>>());

            // Act
            var result = _waitListModel.GetUserWaitlists(userId);

            // Assert
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "GetUserWaitlists");
            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
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

            _mockReader.Setup(reader => reader.GetOrdinal("productWaitListID")).Returns(0);
            _mockReader.Setup(reader => reader.GetOrdinal("positionInQueue")).Returns(1);
            _mockReader.Setup(reader => reader.GetOrdinal("joinedTime")).Returns(2);

            _mockReader.Setup(reader => reader.Read())
                .Returns(() => testData.Count > 0)
                .Callback(() =>
                {
                    if (testData.Count > 0)
                        testData.RemoveAt(0);
                });

            _mockReader.SetupSequence(reader => reader.GetInt32(0))
                .Returns(101)
                .Returns(102);

            _mockReader.SetupSequence(reader => reader.GetInt32(1))
                .Returns(1)
                .Returns(3);

            _mockReader.SetupSequence(reader => reader.GetDateTime(2))
                .Returns(today)
                .Returns(today.AddDays(-2));

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var result = _waitListModel.GetUserWaitlists(userId);

            // Assert
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(userId, result[0].UserID);
            Assert.AreEqual(101, result[0].ProductWaitListID);
            Assert.AreEqual(1, result[0].PositionInQueue);
            Assert.AreEqual(today, result[0].JoinedTime);

            Assert.AreEqual(userId, result[1].UserID);
            Assert.AreEqual(102, result[1].ProductWaitListID);
            Assert.AreEqual(3, result[1].PositionInQueue);
            Assert.AreEqual(today.AddDays(-2), result[1].JoinedTime);
        }

        [TestMethod]
        public void GetWaitlistSize_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int productWaitListId = 101;
            int expectedSize = 5;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            SetupOutputParameter("@TotalUsers", expectedSize);

            // Act
            var result = _waitListModel.GetWaitlistSize(productWaitListId);

            // Assert
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "GetWaitlistSize");
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
            Assert.AreEqual(expectedSize, result);
        }

        [TestMethod]
        public void GetWaitlistSize_WhenWaitlistEmpty_ReturnsZero_()
        {
            // Arrange
            int productWaitListId = 101;
            int expectedSize = 0;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            SetupOutputParameter("@TotalUsers", expectedSize);

            // Act
            var result = _waitListModel.GetWaitlistSize(productWaitListId);

            // Assert
            Assert.AreEqual(expectedSize, result);
        }

        [TestMethod]
        public void GetWaitlistSize_WhenOutputParameterIsDBNull_ReturnsZero_()
        {
            // Arrange
            int productWaitListId = 101;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);

            _mockParameter.Setup(dbParameter => dbParameter.Direction).Returns(ParameterDirection.Output);
            _mockParameter.SetupProperty(dbParameter => dbParameter.Value);
            _mockParameter.Setup(dbParameter => dbParameter.ParameterName).Returns("@TotalUsers");

            _mockCommand.Setup(command => command.ExecuteNonQuery())
                .Callback(() =>
                {
                    _mockParameter.Object.Value = DBNull.Value;
                })
                .Returns(1);

            // Act
            var result = _waitListModel.GetWaitlistSize(productWaitListId);

            // Assert
            Assert.AreEqual(0, result, "Method should return 0 when output parameter is DBNull.Value");
            _mockCommand.VerifySet(command => command.CommandText = "GetWaitlistSize");
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
        }

        [TestMethod]
        public void IsUserInWaitlist_WhenUserExists_ReturnsTrue_()
        {
            // Arrange
            int userId = 42;
            int productId = 101;

            _mockCommand.Setup(command => command.ExecuteScalar()).Returns(1);

            // Act
            var result = _waitListModel.IsUserInWaitlist(userId, productId);

            // Assert
            Assert.IsTrue(result);
            _mockCommand.VerifySet(command => command.CommandText = "CheckUserInProductWaitlist");
            _mockCommand.Verify(command => command.ExecuteScalar(), Times.Once);
        }

        [TestMethod]
        public void IsUserInWaitlist_WhenUserDoesntExist_ReturnsFalse_()
        {
            // Arrange
            int userId = 42;
            int productId = 101;

            _mockCommand.Setup(command => command.ExecuteScalar()).Returns(null);

            // Act
            var result = _waitListModel.IsUserInWaitlist(userId, productId);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetUserWaitlistPosition_WhenUserInWaitlist_ReturnsPosition_()
        {
            // Arrange
            int userId = 42;
            int productId = 101;
            int expectedPosition = 3;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            SetupOutputParameter("@Position", expectedPosition);

            // Act
            var result = _waitListModel.GetUserWaitlistPosition(userId, productId);

            // Assert
            _mockCommand.VerifySet(command => command.CommandType = CommandType.StoredProcedure);
            _mockCommand.VerifySet(command => command.CommandText = "GetUserWaitlistPosition");
            _mockCommand.Verify(command => command.ExecuteNonQuery(), Times.Once);
            Assert.AreEqual(expectedPosition, result);
        }

        [TestMethod]
        public void GetUserWaitlistPosition_WhenUserNotInWaitlist_ReturnsNegativeOne_()
        {
            // Arrange
            int userId = 42;
            int productId = 101;

            _mockCommand.Setup(command => command.CommandType).Returns(CommandType.StoredProcedure);
            SetupOutputParameter("@Position", DBNull.Value);

            // Act
            var result = _waitListModel.GetUserWaitlistPosition(userId, productId);

            // Assert
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void GetUsersInWaitlistOrdered_ExecutesCorrectStoredProcedure()
        {
            // Arrange
            int productId = 101;
            SetupMockDataReader(new List<Dictionary<string, object>>());

            // Act
            var result = _waitListModel.GetUsersInWaitlistOrdered(productId);

            // Assert
            _mockCommand.VerifySet(command => command.CommandText = "GetOrderedWaitlistUsers");
            _mockCommand.Verify(command => command.ExecuteReader(), Times.Once);
            _mockConnection.Verify(connection => connection.Open(), Times.Once);
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

            _mockReader.Setup(reader => reader.GetOrdinal("productWaitListID")).Returns(0);
            _mockReader.Setup(reader => reader.GetOrdinal("userID")).Returns(1);
            _mockReader.Setup(reader => reader.GetOrdinal("joinedTime")).Returns(2);
            _mockReader.Setup(reader => reader.GetOrdinal("positionInQueue")).Returns(3);

            _mockReader.Setup(reader => reader.Read())
                .Returns(() => testData.Count > 0)
                .Callback(() =>
                {
                    if (testData.Count > 0)
                        testData.RemoveAt(0);
                });

            _mockReader.SetupSequence(reader => reader.GetInt32(0))
                .Returns(101)
                .Returns(101);

            _mockReader.SetupSequence(reader => reader.GetInt32(1))
                .Returns(1)
                .Returns(2);

            _mockReader.SetupSequence(reader => reader.GetDateTime(2))
                .Returns(today)
                .Returns(today.AddDays(-1));

            _mockReader.SetupSequence(reader => reader.GetInt32(3))
                .Returns(1)
                .Returns(2);

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);

            // Act
            var result = _waitListModel.GetUsersInWaitlistOrdered(productId);

            // Assert
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(1, result[0].UserID);
            Assert.AreEqual(101, result[0].ProductWaitListID);
            Assert.AreEqual(1, result[0].PositionInQueue);
            Assert.AreEqual(today, result[0].JoinedTime);

            Assert.AreEqual(2, result[1].UserID);
            Assert.AreEqual(101, result[1].ProductWaitListID);
            Assert.AreEqual(2, result[1].PositionInQueue);
            Assert.AreEqual(today.AddDays(-1), result[1].JoinedTime);
        }

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

        private void SetupMockDataReader(List<Dictionary<string, object>> data)
        {
            _mockReader.Setup(reader => reader.Read())
                .Returns(() => data.Count > 0)
                .Callback(() =>
                {
                    if (data.Count > 0)
                        data.RemoveAt(0);
                });

            foreach (var record in data)
            {
                foreach (var column in record.Keys)
                {
                    _mockReader.Setup(reader => reader.GetOrdinal(column)).Returns(0);

                    if (record[column] is int intValue)
                        _mockReader.Setup(reader => reader.GetInt32(0)).Returns(intValue);

                    if (record[column] is DateTime dateValue)
                        _mockReader.Setup(reader => reader.GetDateTime(0)).Returns(dateValue);

                    _mockReader.Setup(reader => reader[column]).Returns(record[column]);
                }
            }

            _mockCommand.Setup(command => command.ExecuteReader()).Returns(_mockReader.Object);
        }

        private void SetupOutputParameter(string paramName, object value)
        {
            _mockParameter.Setup(dbParameter => dbParameter.Direction).Returns(ParameterDirection.Output);
            _mockParameter.SetupProperty(dbParameter => dbParameter.Value);
            _mockParameter.Setup(dbParameter => dbParameter.ParameterName).Returns(paramName);

            _mockCommand.Setup(command => command.ExecuteNonQuery())
                .Callback(() =>
                {
                    _mockParameter.Object.Value = value;
                })
                .Returns(1);
        }

        private void AssertParameterValue(List<IDbDataParameter> parameters, string parameterName, object expectedValue)
        {
            var parameter = parameters.FirstOrDefault(parameter => parameter.ParameterName == parameterName);
            Assert.IsNotNull(parameter, $"Parameter {parameterName} not found");
            Assert.AreEqual(expectedValue, parameter.Value, $"Parameter {parameterName} has an incorrect value");
        }

        private void AssertOrderRecordEquality(dynamic expected, dynamic actual)
        {
            // Using a single assertion to check all values via a composite error message.
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
}
