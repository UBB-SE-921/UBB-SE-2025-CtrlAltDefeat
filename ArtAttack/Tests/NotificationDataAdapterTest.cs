using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtAttack.Model;
using ArtAttack.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient; // Updated namespace
using Moq;

[TestClass]
public class NotificationDataAdapterTests
{
    private Mock<SqlConnection> _mockConnection;
    private Mock<SqlCommand> _mockCommand;
    private Mock<SqlDataReader> _mockReader;
    private NotificationDataAdapter _adapter;

    [TestInitialize]
    public void Setup()
    {
        _mockConnection = new Mock<SqlConnection>();
        _mockCommand = new Mock<SqlCommand>();
        _mockReader = new Mock<SqlDataReader>();
        _adapter = new NotificationDataAdapter("YourConnectionString");
    }

    [TestMethod]
    public void GetNotificationsForUser_ShouldReturnNotifications()
    {
        // Arrange
        int recipientId = 1;

        // Setup mock data reader
        _mockReader.SetupSequence(r => r.Read())
                   .Returns(true)
                   .Returns(false);
        _mockReader.Setup(r => r.GetInt32(It.IsAny<int>())).Returns(1);
        _mockReader.Setup(r => r.GetString(It.IsAny<int>())).Returns("CONTRACT_RENEWAL_ANS");
        _mockReader.Setup(r => r.GetDateTime(It.IsAny<int>())).Returns(DateTime.Now);
        _mockReader.Setup(r => r.GetBoolean(It.IsAny<int>())).Returns(false);

        // Setup mock command
        _mockCommand.Setup(c => c.ExecuteReader()).Returns(_mockReader.Object);

        // Setup mock connection
        _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

        // Act
        var notifications = _adapter.GetNotificationsForUser(recipientId);

        // Assert
        Assert.IsNotNull(notifications);
        Assert.AreEqual(1, notifications.Count);
    }

    [TestMethod]
    public void MarkAsRead_ShouldExecuteNonQuery()
    {
        // Arrange
        int notificationId = 1;

        // Setup mock command
        _mockCommand.Setup(c => c.ExecuteNonQuery()).Verifiable();

        // Setup mock connection
        _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

        // Act
        _adapter.MarkAsRead(notificationId);

        // Assert
        _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
    }

    [TestMethod]
    public void AddNotification_ShouldExecuteNonQuery()
    {
        // Arrange
        var notification = new ContractRenewalAnswerNotification(1, DateTime.Now, 1, true);

        // Setup mock command
        _mockCommand.Setup(c => c.ExecuteNonQuery()).Verifiable();

        // Setup mock connection
        _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

        // Act
        _adapter.AddNotification(notification);

        // Assert
        _mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _adapter.Dispose();
    }
}
