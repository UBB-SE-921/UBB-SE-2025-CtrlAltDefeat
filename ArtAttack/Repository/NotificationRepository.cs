using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Shared;

namespace ArtAttack.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDatabaseProvider databaseProvider;
        private readonly string connectionString;
        private IDbConnection connection;

        [ExcludeFromCodeCoverage]
        public NotificationRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public NotificationRepository(string connectionString, IDatabaseProvider databaseProvider)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (databaseProvider == null)
            {
                throw new ArgumentNullException(nameof(databaseProvider));
            }

            this.connectionString = connectionString;
            this.databaseProvider = databaseProvider;

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
                        notifications.Add(this.CreateFromDataReader(reader));
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
                        AddParameter(command, "@contractID", ans.ContractID);
                        AddParameter(command, "@isAccepted", ans.IsAccepted);
                        break;

                    case ContractRenewalWaitlistNotification waitlist:
                        AddParameter(command, "@productID", waitlist.ProductID);
                        break;

                    case OutbiddedNotification outbid:
                        AddParameter(command, "@productID", outbid.ProductID);
                        break;

                    case OrderShippingProgressNotification shipping:
                        AddParameter(command, "@orderID", shipping.OrderID);
                        AddParameter(command, "@shippingState", shipping.ShippingState);
                        AddParameter(command, "@deliveryDate", shipping.DeliveryDate);
                        break;

                    case PaymentConfirmationNotification payment:
                        AddParameter(command, "@orderID", payment.OrderID);
                        AddParameter(command, "@productID", payment.ProductID);
                        break;

                    case ProductRemovedNotification removed:
                        AddParameter(command, "@productID", removed.ProductID);
                        break;

                    case ProductAvailableNotification available:
                        AddParameter(command, "@productID", available.ProductID);
                        break;

                    case ContractRenewalRequestNotification request:
                        AddParameter(command, "@contractID", request.ContractID);
                        break;

                    case ContractExpirationNotification expiration:
                        AddParameter(command, "@contractID", expiration.ContractID);
                        AddParameter(command, "@expirationDate", expiration.ExpirationDate);
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

            parameter.Value = value;

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

        /// <summary>
        /// Creates a Notification object from a data reader
        /// </summary>
        /// <param name="reader">The reader to create a notification from</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Notification CreateFromDataReader(IDataReader reader)
        {
            int notificationId = reader.GetInt32(reader.GetOrdinal("notificationID"));
            int recipientId = reader.GetInt32(reader.GetOrdinal("recipientID"));
            DateTime timestamp = reader.GetDateTime(reader.GetOrdinal("timestamp"));
            bool isRead = reader.GetBoolean(reader.GetOrdinal("isRead"));
            string category = reader.GetString(reader.GetOrdinal("category"));

            switch (category)
            {
                case "CONTRACT_RENEWAL_ACCEPTED":
                    int contractId = reader.GetInt32(reader.GetOrdinal("contractID"));
                    bool isAccepted = reader.GetBoolean(reader.GetOrdinal("isAccepted"));
                    return new ContractRenewalAnswerNotification(recipientId, timestamp, contractId, isAccepted, isRead, notificationId);

                case "CONTRACT_RENEWAL_WAITLIST":
                    int productIdWaitlist = reader.GetInt32(reader.GetOrdinal("productID"));
                    return new ContractRenewalWaitlistNotification(recipientId, timestamp, productIdWaitlist, isRead, notificationId);

                case "OUTBIDDED":
                    int productIdOutbidded = reader.GetInt32(reader.GetOrdinal("productID"));
                    return new OutbiddedNotification(recipientId, timestamp, productIdOutbidded, isRead, notificationId);

                case "ORDER_SHIPPING_PROGRESS":
                    int orderId = reader.GetInt32(reader.GetOrdinal("orderID"));
                    string shippingState = reader.GetString(reader.GetOrdinal("shippingState"));
                    DateTime deliveryDate = reader.GetDateTime(reader.GetOrdinal("deliveryDate"));
                    return new OrderShippingProgressNotification(recipientId, timestamp, orderId, shippingState, deliveryDate, isRead, notificationId);

                case "PAYMENT_CONFIRMATION":
                    int productIdPayment = reader.GetInt32(reader.GetOrdinal("productID"));
                    int orderIdPayment = reader.GetInt32(reader.GetOrdinal("orderID"));
                    return new PaymentConfirmationNotification(recipientId, timestamp, productIdPayment, orderIdPayment, isRead, notificationId);

                case "PRODUCT_REMOVED":
                    int productIdRemoved = reader.GetInt32(reader.GetOrdinal("productID"));
                    return new ProductRemovedNotification(recipientId, timestamp, productIdRemoved, isRead, notificationId);

                case "PRODUCT_AVAILABLE":
                    int productIdAvailable = reader.GetInt32(reader.GetOrdinal("productID"));
                    return new ProductAvailableNotification(recipientId, timestamp, productIdAvailable, isRead, notificationId);

                case "CONTRACT_RENEWAL_REQUEST":
                    int contractIdReq = reader.GetInt32(reader.GetOrdinal("contractID"));
                    return new ContractRenewalRequestNotification(recipientId, timestamp, contractIdReq, isRead, notificationId);

                case "CONTRACT_EXPIRATION":
                    int contractIdExp = reader.GetInt32(reader.GetOrdinal("contractID"));
                    DateTime expirationDate = reader.GetDateTime(reader.GetOrdinal("expirationDate"));
                    return new ContractExpirationNotification(recipientId, timestamp, contractIdExp, expirationDate, isRead, notificationId);

                default:
                    throw new ArgumentException($"Unknown notification category: {category}");
            }
        }
    }
}
