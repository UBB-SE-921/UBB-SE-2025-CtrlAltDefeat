using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

public class NotificationDataAdapter : IDisposable
{
    private SqlConnection _connection;

    public NotificationDataAdapter(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        _connection.Open();
    }

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

    public void MarkAsRead(int notificationId)
    {
        using (var command = new SqlCommand("MarkNotificationAsRead", _connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@notificationID", notificationId);

            int rowsAffected = command.ExecuteNonQuery();
        }
    }


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

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}