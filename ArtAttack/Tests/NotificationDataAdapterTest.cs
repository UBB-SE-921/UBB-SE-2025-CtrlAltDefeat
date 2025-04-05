using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtAttack.Model;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using Moq;

[TestClass]
public class NotificationDataAdapterTests
{
    private Mock<SqlConnection> mockConnection;
    private Mock<SqlCommand> mockCommand;
    private Mock<SqlDataReader> mockReader;
    private NotificationDataAdapter adapter;

    [TestInitialize]
    public void Setup()
    {
        mockConnection = new Mock<SqlConnection>();
        mockCommand = new Mock<SqlCommand>();
        mockReader = new Mock<SqlDataReader>();
        adapter = new NotificationDataAdapter("YourConnectionString");
    }

    [TestMethod]
    public void GetNotificationsForUser_ShouldReturnNotifications()
    {
        int recipientId = 1;

        mockReader.SetupSequence(r => r.Read())
                   .Returns(true)
                   .Returns(false);
        mockReader.Setup(r => r.GetInt32(It.IsAny<int>())).Returns(1);
        mockReader.Setup(r => r.GetString(It.IsAny<int>())).Returns("CONTRACT_RENEWAL_ANS");
        mockReader.Setup(r => r.GetDateTime(It.IsAny<int>())).Returns(DateTime.Now);
        mockReader.Setup(r => r.GetBoolean(It.IsAny<int>())).Returns(false);

        mockCommand.Setup(c => c.ExecuteReader()).Returns(mockReader.Object);

        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        var notifications = adapter.GetNotificationsForUser(recipientId);

        Assert.IsNotNull(notifications);
        Assert.AreEqual(1, notifications.Count);
    }

    [TestMethod]
    public void MarkAsRead_ShouldExecuteNonQuery()
    {
        int notificationId = 1;

        mockCommand.Setup(c => c.ExecuteNonQuery()).Verifiable();

        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        adapter.MarkAsRead(notificationId);

        mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
    }

    [TestMethod]
    public void AddNotification_ShouldExecuteNonQuery()
    {
        var notification = new ContractRenewalAnswerNotification(1, DateTime.Now, 1, true);

        mockCommand.Setup(c => c.ExecuteNonQuery()).Verifiable();

        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

        adapter.AddNotification(notification);

        mockCommand.Verify(c => c.ExecuteNonQuery(), Times.Once);
    }

    [TestCleanup]
    public void Cleanup()
    {
        adapter.Dispose();
    }
}
