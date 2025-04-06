using System;
using System.Collections.Generic;
using System.Data;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    public class NotificationDataAdapter : IDisposable, INotificationDataAdapter
    {
        private readonly IDatabaseProvider databaseProvider;
        private readonly string connectionString;
        private IDbConnection connection;

        public NotificationDataAdapter(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public NotificationDataAdapter(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
            connection = databaseProvider.CreateConnection(connectionString);
            connection.Open();
        }

        /// <summary>
        /// Retrieves notification for a user based on ID
        /// </summary>
        /// <param name="recipientId">Id of the recipient to for which to retrieve notifications</param>
        /// <returns></returns>
        public List<Notification> GetNotificationsForUser(int recipientId)
        {
            var notifications = new List<Notification>();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "GetNotificationsByRecipient";
                command.CommandType = CommandType.StoredProcedure;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@RecipientId";
                parameter.Value = recipientId;
                command.Parameters.Add(parameter);

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

        /// <summary>
        /// Marks a notification as read in the database using the MarkNotificationAsRead stored procedure
        /// </summary>
        /// <param name="notificationId">Notification for which to retrieve status</param>
        public void MarkAsRead(int notificationId)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "MarkNotificationAsRead";
                command.CommandType = CommandType.StoredProcedure;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@notificationID";
                parameter.Value = notificationId;
                command.Parameters.Add(parameter);

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds a notification to the database
        /// </summary>
        /// <param name="notification">Notification to be added to the database</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddNotification(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "AddNotification";
                command.CommandType = CommandType.StoredProcedure;

                AddParameter(command, "@recipientID", notification.RecipientID);
                AddParameter(command, "@category", notification.Category.ToString());

                switch (notification)
                {
                    case ContractRenewalAnswerNotification ans:
                        AddParameter(command, "@contractID", ans.GetContractID());
                        AddParameter(command, "@isAccepted", ans.GetIsAccepted());
                        break;

                    case ContractRenewalWaitlistNotification waitlist:
                        AddParameter(command, "@productID", waitlist.GetProductID());
                        break;

                    case OutbiddedNotification outbid:
                        AddParameter(command, "@productID", outbid.GetProductID());
                        break;

                    case OrderShippingProgressNotification shipping:
                        AddParameter(command, "@orderID", shipping.GetOrderID());
                        AddParameter(command, "@shippingState", shipping.GetShippingState());
                        AddParameter(command, "@deliveryDate", shipping.GetDeliveryDate());
                        break;

                    case PaymentConfirmationNotification payment:
                        AddParameter(command, "@orderID", payment.GetOrderID());
                        AddParameter(command, "@productID", payment.GetProductID());
                        break;

                    case ProductRemovedNotification removed:
                        AddParameter(command, "@productID", removed.GetProductID());
                        break;

                    case ProductAvailableNotification available:
                        AddParameter(command, "@productID", available.GetProductID());
                        break;

                    case ContractRenewalRequestNotification request:
                        AddParameter(command, "@contractID", request.GetContractID());
                        break;

                    case ContractExpirationNotification expiration:
                        AddParameter(command, "@contractID", expiration.GetContractID());
                        AddParameter(command, "@expirationDate", expiration.GetExpirationDate());
                        break;

                    default:
                        throw new ArgumentException($"Unknown notification type: {notification.GetType()}");
                }

                SetNullParametersForUnusedFields(command);

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds a parameter to an sql command
        /// </summary>
        /// <param name="command">The command to add a parameter to</param>
        /// <param name="name">Name of the parameter to add</param>
        /// <param name="value">Value of the parameter to be added</param>
        private void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Sets null parameters for unused fields in the command
        /// </summary>
        /// <param name="command">Database command to set null fields in</param>
        private void SetNullParametersForUnusedFields(IDbCommand command)
        {
            var allParams = new[] { "@contractID", "@isAccepted", "@productID", "@orderID", "@shippingState", "@deliveryDate", "@expirationDate" };
            var existingParamNames = new HashSet<string>();

            // Collect the names of parameters that already exist
            foreach (IDataParameter param in command.Parameters)
            {
                existingParamNames.Add(param.ParameterName);
            }

            // Add null parameters for any that don't exist
            foreach (var paramName in allParams)
            {
                if (!existingParamNames.Contains(paramName))
                {
                    AddParameter(command, paramName, DBNull.Value);
                }
            }
        }

        /// <summary>
        /// Disposes of the database connection
        /// </summary>
        public void Dispose()
        {
            connection?.Dispose();
            connection = null;
        }
    }
}
