using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

public class NotificationDataAdapter : IDisposable
{
    private SqlConnection _connection;

    /// <summary>
    /// Constructor for the NotificationDataAdapter class.
    /// </summary>
    /// <param name="connectionString"> The connection String for accessing the database</param>
    public NotificationDataAdapter(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        _connection.Open();
    }

    /// <summary>
    /// Retrieves all notifications for a given user.
    /// </summary>
    /// <param name="recipientId"> The id of the recipient, user that receives the notification </param>
    /// <returns></returns>
    public List<Notification> GetNotificationsForUser(int recipientId)
    {
        var notifications = new List<Notification>();

        SqlCommand command = new SqlCommand("GetNotificationsByRecipient", _connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@RecipientId", recipientId);
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                notifications.Add(NotificationFactory.CreateFromDataReader(reader));
            }
        }
        return notifications;
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="notificationId"> The notification which will pe marked as read </param>
    public void MarkAsRead(int notificationId)
    {
        using (var command = new SqlCommand("MarkNotificationAsRead", _connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@notificationID", notificationId);

            int rowsAffected = command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Adds a new notification to the database.
    /// </summary>
    /// <param name="notification"> Notification which will be added </param>
    /// <exception cref="ArgumentException"></exception>
    public void AddNotification(Notification notification)
    {
        using (var command = new SqlCommand("AddNotification", _connection))
        {
            command.CommandType = CommandType.StoredProcedure;

            // Common parameters
            command.Parameters.AddWithValue("@recipientID", notification.getRecipientID());
            command.Parameters.AddWithValue("@category", notification.getCategory().ToString());

            switch (notification)
            {
                case ContractRenewalAnswerNotification ans:
                    command.Parameters.AddWithValue("@contractID", ans.getContractID());
                    command.Parameters.AddWithValue("@isAccepted", ans.getIsAccepted());
                    break;

                case ContractRenewalWaitlistNotification waitlist:
                    command.Parameters.AddWithValue("@productID", waitlist.getProductID());
                    break;

                case OutbiddedNotification outbid:
                    command.Parameters.AddWithValue("@productID", outbid.getProductID());
                    break;

                case OrderShippingProgressNotification shipping:
                    command.Parameters.AddWithValue("@orderID", shipping.getOrderID());
                    command.Parameters.AddWithValue("@shippingState", shipping.getShippingState());
                    command.Parameters.AddWithValue("@deliveryDate", shipping.getDeliveryDate());
                    break;

                case PaymentConfirmationNotification payment:
                    command.Parameters.AddWithValue("@orderID", payment.getOrderID());
                    command.Parameters.AddWithValue("@productID", payment.getProductID());
                    break;

                case ProductRemovedNotification removed:
                    command.Parameters.AddWithValue("@productID", removed.getProductID());
                    break;

                case ProductAvailableNotification available:
                    command.Parameters.AddWithValue("@productID", available.getProductID());
                    break;

                case ContractRenewalRequestNotification request:
                    command.Parameters.AddWithValue("@contractID", request.getContractID());
                    break;

                case ContractExpirationNotification expiration:
                    command.Parameters.AddWithValue("@contractID", expiration.getContractID());
                    command.Parameters.AddWithValue("@expirationDate", expiration.getContractID());
                    break;

                default:
                    throw new ArgumentException($"Unknown notification type: {notification.GetType()}");
            }

            SetNullParametersForUnusedFields(command, notification);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Sets null parameters for unused fields in the AddNotification stored procedure.
    /// </summary>
    /// <param name="command">The SqlCommand object used to execute the AddNotification stored procedure.</param>
    /// <param name="notification">The Notification object containing the data to be added to the database.</param>
    private void SetNullParametersForUnusedFields(SqlCommand command, Notification notification)
    {
        var allParams = new[] { "@contractID", "@isAccepted", "@productID", "@orderID",
                          "@shippingState", "@deliveryDate", "@expirationDate" };

        foreach (var param in allParams)
        {
            if (!command.Parameters.Contains(param))
            {
                command.Parameters.AddWithValue(param, DBNull.Value);
            }
        }
    }

    /// <summary>
    /// Disposes of the SqlConnection object.
    /// </summary>
    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}