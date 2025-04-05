using System;
using System.Collections.Generic;
using System.Data;
using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;

public class NotificationDataAdapter : IDisposable
{
    private SqlConnection connection;

    public NotificationDataAdapter(string connectionString)
    {
        connection = new SqlConnection(connectionString);
        connection.Open();
    }

    public List<Notification> GetNotificationsForUser(int recipientId)
    {
        var notifications = new List<Notification>();

        SqlCommand command = new SqlCommand("GetNotificationsByRecipient", connection);
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
        using (var command = new SqlCommand("MarkNotificationAsRead", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@notificationID", notificationId);

            int rowsAffected = command.ExecuteNonQuery();
        }
    }

    public void AddNotification(Notification notification)
    {
        using (var command = new SqlCommand("AddNotification", connection))
        {
            command.CommandType = CommandType.StoredProcedure;

            // Common parameters
            command.Parameters.AddWithValue("@recipientID", notification.RecipientID);
            command.Parameters.AddWithValue("@category", notification.RecipientID.ToString());

            switch (notification)
            {
                case ContractRenewalAnswerNotification ans:
                    command.Parameters.AddWithValue("@contractID", ans.GetContractID());
                    command.Parameters.AddWithValue("@isAccepted", ans.GetIsAccepted());
                    break;

                case ContractRenewalWaitlistNotification waitlist:
                    command.Parameters.AddWithValue("@productID", waitlist.GetProductID());
                    break;

                case OutbiddedNotification outbid:
                    command.Parameters.AddWithValue("@productID", outbid.GetProductID());
                    break;

                case OrderShippingProgressNotification shipping:
                    command.Parameters.AddWithValue("@orderID", shipping.GetOrderID());
                    command.Parameters.AddWithValue("@shippingState", shipping.GetShippingState());
                    command.Parameters.AddWithValue("@deliveryDate", shipping.GetDeliveryDate());
                    break;

                case PaymentConfirmationNotification payment:
                    command.Parameters.AddWithValue("@orderID", payment.GetOrderID());
                    command.Parameters.AddWithValue("@productID", payment.GetProductID());
                    break;

                case ProductRemovedNotification removed:
                    command.Parameters.AddWithValue("@productID", removed.GetProductID());
                    break;

                case ProductAvailableNotification available:
                    command.Parameters.AddWithValue("@productID", available.GetProductID());
                    break;

                case ContractRenewalRequestNotification request:
                    command.Parameters.AddWithValue("@contractID", request.GetContractID());
                    break;

                case ContractExpirationNotification expiration:
                    command.Parameters.AddWithValue("@contractID", expiration.GetContractID());
                    command.Parameters.AddWithValue("@expirationDate", expiration.GetContractID());
                    break;

                default:
                    throw new ArgumentException($"Unknown notification type: {notification.GetType()}");
            }

            SetNullParametersForUnusedFields(command, notification);

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            command.ExecuteNonQuery();
        }
    }

    private void SetNullParametersForUnusedFields(SqlCommand command, Notification notification)
    {
        var allParams = new[] { "@contractID", "@isAccepted", "@productID", "@orderID", "@shippingState", "@deliveryDate", "@expirationDate" };

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
        connection?.Close();
        connection?.Dispose();
    }
}