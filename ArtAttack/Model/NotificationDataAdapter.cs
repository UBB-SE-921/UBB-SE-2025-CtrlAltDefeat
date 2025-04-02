using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

public class NotificationDataAdapter : IDisposable
{
    private readonly SqlConnection _connection;

    public NotificationDataAdapter(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        _connection.Open();
    }

    public List<INotification> GetNotificationsForUser(int recipientId)
    {
        var notifications = new List<INotification>();

        using (var command = new SqlCommand("GetNotificationsByRecipient", _connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@RecipientId", recipientId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    notifications.Add(NotificationFactory.CreateFromDataReader(reader));
                }
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

            command.ExecuteNonQuery();
        }
    }

    public void AddNotification(INotification notification)
    {
        using (var command = new SqlCommand("AddNotification", _connection))
        {
            command.CommandType = CommandType.StoredProcedure;

            // Common parameters
            command.Parameters.AddWithValue("@recipientID", notification.RecipientID);
            command.Parameters.AddWithValue("@category", notification.Category.ToString());

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

            SetNullParametersForUnusedFields(command);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            command.ExecuteNonQuery();
        }
    }

    private void SetNullParametersForUnusedFields(SqlCommand command)
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

