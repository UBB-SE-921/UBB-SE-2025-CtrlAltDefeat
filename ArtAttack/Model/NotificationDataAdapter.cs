using ArtAttack.Domain;
using ArtAttack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

public class NotificationDataAdapter : IDisposable, INotificationDataAdapter
{
    private SqlConnection _sqlConnection;

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
    public List<Notification> GetNotificationsForUser(int recipientId)
    {
        var notificationList = new List<Notification>();

        SqlCommand sqlCommand = new SqlCommand("GetNotificationsByRecipient", _sqlConnection);
        sqlCommand.CommandType = CommandType.StoredProcedure;

        sqlCommand.Parameters.AddWithValue("@RecipientId", recipientId);
        using (var sqlDataReader = sqlCommand.ExecuteReader())
        {
            while (sqlDataReader.Read())
            {
                notificationList.Add(NotificationFactory.CreateFromDataReader(sqlDataReader));
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

            int rowsAffected = sqlCommand.ExecuteNonQuery();
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
    public void AddNotification(Notification notification)
    {
        using (var sqlCommand = new SqlCommand("AddNotification", _sqlConnection))
        {
            sqlCommand.CommandType = CommandType.StoredProcedure;

            sqlCommand.Parameters.AddWithValue("@recipientID", notification.getRecipientID());
            sqlCommand.Parameters.AddWithValue("@category", notification.getCategory().ToString());

            switch (notification)
            {
                case ContractRenewalAnswerNotification contractRenewalAnswerNotification:
                    sqlCommand.Parameters.AddWithValue("@contractID", contractRenewalAnswerNotification.getContractID());
                    sqlCommand.Parameters.AddWithValue("@isAccepted", contractRenewalAnswerNotification.getIsAccepted());
                    break;

                case ContractRenewalWaitlistNotification contractRenewalWaitlistNotification:
                    sqlCommand.Parameters.AddWithValue("@productID", contractRenewalWaitlistNotification.getProductID());
                    break;

                case OutbiddedNotification outbidNotification:
                    sqlCommand.Parameters.AddWithValue("@productID", outbidNotification.getProductID());
                    break;

                case OrderShippingProgressNotification shippingProgressNotification:
                    sqlCommand.Parameters.AddWithValue("@orderID", shippingProgressNotification.getOrderID());
                    sqlCommand.Parameters.AddWithValue("@shippingState", shippingProgressNotification.getShippingState());
                    sqlCommand.Parameters.AddWithValue("@deliveryDate", shippingProgressNotification.getDeliveryDate());
                    break;

                case PaymentConfirmationNotification paymentConfirmationNotification:
                    sqlCommand.Parameters.AddWithValue("@orderID", paymentConfirmationNotification.getOrderID());
                    sqlCommand.Parameters.AddWithValue("@productID", paymentConfirmationNotification.getProductID());
                    break;

                case ProductRemovedNotification productRemovedNotification:
                    sqlCommand.Parameters.AddWithValue("@productID", productRemovedNotification.getProductID());
                    break;

                case ProductAvailableNotification productAvailableNotification:
                    sqlCommand.Parameters.AddWithValue("@productID", productAvailableNotification.getProductID());
                    break;

                case ContractRenewalRequestNotification contractRenewalRequestNotification:
                    sqlCommand.Parameters.AddWithValue("@contractID", contractRenewalRequestNotification.getContractID());
                    break;

                case ContractExpirationNotification contractExpirationNotification:
                    sqlCommand.Parameters.AddWithValue("@contractID", contractExpirationNotification.getContractID());
                    sqlCommand.Parameters.AddWithValue("@expirationDate", contractExpirationNotification.getContractID());
                    break;

                default:
                    throw new ArgumentException($"Unknown notification type: {notification.GetType()}");
            }

            SetNullParametersForUnusedFields(sqlCommand, notification);

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
    private void SetNullParametersForUnusedFields(SqlCommand sqlCommand, Notification notification)
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