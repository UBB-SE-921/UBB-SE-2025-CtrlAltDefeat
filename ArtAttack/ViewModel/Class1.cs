// WaitListNotifier.cs
using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Linq;

// WaitListNotifier.cs
public class WaitListNotifier
{
    private readonly WaitListModel _waitListModel;
    private readonly NotificationDataAdapter _notificationAdapter;
    private readonly string _connectionString;

    public WaitListNotifier(string connectionString)
    {
        _waitListModel = new WaitListModel(connectionString);
        _notificationAdapter = new NotificationDataAdapter(connectionString);
        _connectionString = connectionString;
    }

    public void ScheduleRestockAlerts(int productId, DateTime restockDate)
    {
        int waitlistProductId = GetWaitlistProductId(productId);
        if (waitlistProductId <= 0) return;

        var users = _waitListModel.GetUsersInWaitlist(waitlistProductId)
                     .OrderBy(u => u.positionInQueue)
                     .ToList();

        for (int i = 0; i < users.Count; i++)
        {
            var notification = new ProductAvailableNotification(
                recipientId: users[i].userID,
                timestamp: CalculateNotifyTime(restockDate, i),
                productId: productId,
                isRead: false)
            { };
            _notificationAdapter.AddNotification(notification);
        }
    }

    private int GetWaitlistProductId(int productId)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT WaitListProductID FROM WaitListProduct WHERE ProductID = @ProductId",
            conn))
        {
            cmd.Parameters.AddWithValue("@ProductId", productId);
            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }
    }

    private DateTime CalculateNotifyTime(DateTime restockDate, int position)
    {
        return position switch
        {
            0 => restockDate.AddHours(-48), // First in queue
            1 => restockDate.AddHours(-24), // Second in queue
            _ => restockDate.AddHours(-12)  // Everyone else
        };
    }

    private string GetNotificationMessage(int position, DateTime restockDate)
    {
        string timeDescription = (restockDate - DateTime.Now).TotalHours > 24
            ? $"on {restockDate:MMM dd}"
            : $"in {(int)(restockDate - DateTime.Now).TotalHours} hours";

        return position switch
        {
            1 => $"You're FIRST in line! Product restocking {timeDescription}.",
            2 => $"You're SECOND in line. Product restocking {timeDescription}.",
            _ => $"You're #{position} in queue. Product restocking {timeDescription}."
        };
    }

    /*public void CheckAndSendNotifications(int productId, DateTime endDate)
    {
        int waitlistProductId = GetWaitlistProductId(productId);
        if (waitlistProductId <= 0) return;

        var users = _waitListModel.GetUsersInWaitlist(waitlistProductId)
                     .OrderBy(u => u.positionInQueue)
                     .ToList();

        for (int i = 0; i < users.Count; i++)
        {
            var notifyTime = CalculateNotifyTime(endDate, i);

            // If we're within 5 minutes of the notification time (buffer)
            if (Math.Abs((DateTime.Now - notifyTime).TotalMinutes) <= 5)
            {
                SendNotification(users[i].userID, productId, i + 1, endDate);
            }
        }
    }

    private void SendNotification(int userId, int productId, int position, DateTime restockDate)
    {
        // Check if notification was already sent
        if (_notificationAdapter.NotificationExists(userId, productId, position))
            return;

        var notification = new ProductAvailableNotification(
            recipientId: userId,
            timestamp: DateTime.Now,
            productId: productId,
            isRead: false)
        {
            Message = GetNotificationMessage(position, restockDate)
        };

        _notificationAdapter.AddNotification(notification);

        // Optionally: Send real notification (email, push, etc.)
    }

    public bool NotificationExists(int userId, int productId, int position)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(
            @"SELECT COUNT(*) FROM Notifications 
          WHERE RecipientID = @UserId 
          AND ProductID = @ProductId 
          AND PositionInQueue = @Position",
            conn))
        {
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@ProductId", productId);
            cmd.Parameters.AddWithValue("@Position", position);

            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }
    }*/
}