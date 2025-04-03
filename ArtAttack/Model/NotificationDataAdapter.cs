using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

public class NotificationDataAdapter : IDisposable, INotificationDataAdapter
{
    private readonly SqlConnection _connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationDataAdapter"/> class with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to the database.</param>
    /// <exception cref="SqlException">Thrown when the connection to the database cannot be established.</exception>
    public NotificationDataAdapter(string connectionString)
    {
        _sqlConnection = new SqlConnection(connectionString);
        _sqlConnection.Open();
    }

/// <summary>
    /// Retrieves a list of notifications for a specific user.
    /// </summary>
    /// <param name="recipientId">The ID of the recipient user.</param>
    /// <returns>A list of <see cref="Notification"/> objects for the specified user.</returns>
    /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the SQL connection is not open.</exception>
    /// <precondition>recipientId must be a valid user ID.</precondition>
    /// <postcondition>A list of notifications for the specified user is returned.</postcondition>
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
        return notificationList;
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to mark as read.</param>
    /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the SQL connection is not open.</exception>
    /// <precondition>notificationId must be a valid notification ID.</precondition>
    /// <postcondition>The specified notification is marked as read in the database.</postcondition>
    public void MarkAsRead(int notificationId)
    {
        using (var sqlCommand = new SqlCommand("MarkNotificationAsRead", _sqlConnection))
        {
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@notificationID", notificationId);

            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Adds a new notification to the database.
    /// </summary>
    /// <param name="notification">The <see cref="Notification"/> object to add.</param>
    /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the SQL connection is not open.</exception>
    /// <exception cref="ArgumentException">Thrown when the notification type is unknown.</exception>
    /// <precondition>notification must be a valid <see cref="Notification"/> object.</precondition>
    /// <postcondition>The specified notification is added to the database.</postcondition>
    public void AddNotification(INotification notification)
    {
        using (var sqlCommand = new SqlCommand("AddNotification", _sqlConnection))
        {
            sqlCommand.CommandType = CommandType.StoredProcedure;

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


            if (_sqlConnection.State != ConnectionState.Open)
                _sqlConnection.Open();

            sqlCommand.ExecuteNonQuery();
        }
    }


    /// <summary>
    /// Sets null values for unused fields in the SQL command parameters.
    /// </summary>
    /// <param name="command">The SQL command to set parameters for.</param>
    /// <param name="notification">The notification object to determine which fields are used.</param>
    /// <precondition>command and notification must be non-null.</precondition>
    /// <postcondition>Unused fields in the SQL command parameters are set to null.</postcondition>
    private void SetNullParametersForUnusedFields(SqlCommand sqlCommand)
    {
        var unusedFields = new[] { "@contractID", "@isAccepted", "@productID", "@orderID",
                          "@shippingState", "@deliveryDate", "@expirationDate" };

        foreach (var unusedField in unusedFields)
        {
            if (!sqlCommand.Parameters.Contains(unusedField))
            {
                sqlCommand.Parameters.AddWithValue(unusedField, DBNull.Value);
            }
        }
    }

    /// <summary>
    /// Disposes the SQL connection.
    /// </summary>
    /// <postcondition>The SQL connection is closed and disposed.</postcondition>
    public void Dispose()
    {
        if (_sqlConnection != null)
        {
            _sqlConnection.Close();
            _sqlConnection.Dispose();
            _sqlConnection = new SqlConnection(); 
        }
    }
}

