using System;
using System.Data;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    public class NotificationFactory : INotificationFactory
    {
        /// <summary>
        /// Creates a Notification object from a data reader
        /// </summary>
        /// <param name="reader">The reader to create a notification from</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Notification CreateFromDataReader(IDataReader reader)
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

        /// <summary>
        /// Compares two notifications for equality based on their type and specific properties
        /// </summary>
        /// <param name="actualNotification">The actual notification (usually from a test)</param>
        /// <param name="expectedNotification">The expected notification to compare against</param>
        /// <returns>True if notifications are equal, false otherwise</returns>
        public static bool AreEqual(Notification actualNotification, Notification expectedNotification)
        {
            if (actualNotification == null || expectedNotification == null)
            {
                return actualNotification == expectedNotification;
            }

            // Check common properties
            if (actualNotification.RecipientID != expectedNotification.RecipientID ||
                actualNotification.GetType() != expectedNotification.GetType())
            {
                return false;
            }

            // Type-specific comparisons
            switch (actualNotification)
            {
                case ProductAvailableNotification actualProductAvailableNotification:
                    var expectedProductAvailableNotification = expectedNotification as ProductAvailableNotification;
                    return actualProductAvailableNotification.GetProductID() == expectedProductAvailableNotification.GetProductID();

                case ContractRenewalAnswerNotification actualContractRenewalAnswerNotification:
                    var expectedContractRenewalAnswerNotification = expectedNotification as ContractRenewalAnswerNotification;
                    return actualContractRenewalAnswerNotification.GetContractID() == expectedContractRenewalAnswerNotification.GetContractID() &&
                           actualContractRenewalAnswerNotification.GetIsAccepted() == expectedContractRenewalAnswerNotification.GetIsAccepted();

                case ContractRenewalWaitlistNotification actualContractRenewalWaitlistNotification:
                    var expectedContractRenewalWaitlistNotification = expectedNotification as ContractRenewalWaitlistNotification;
                    return actualContractRenewalWaitlistNotification.GetProductID() == expectedContractRenewalWaitlistNotification.GetProductID();

                case OutbiddedNotification actualOutbiddedNotification:
                    var expectedOutbiddedNotification = expectedNotification as OutbiddedNotification;
                    return actualOutbiddedNotification.GetProductID() == expectedOutbiddedNotification.GetProductID();

                case OrderShippingProgressNotification actualOrderShippingProgressNotification:
                    var expectedOrderShippingProgressNotification = expectedNotification as OrderShippingProgressNotification;
                    return actualOrderShippingProgressNotification.GetOrderID() == expectedOrderShippingProgressNotification.GetOrderID() &&
                           actualOrderShippingProgressNotification.GetShippingState() == expectedOrderShippingProgressNotification.GetShippingState() &&
                           actualOrderShippingProgressNotification.GetDeliveryDate() == expectedOrderShippingProgressNotification.GetDeliveryDate();

                case PaymentConfirmationNotification actualPaymentConfirmationNotification:
                    var expectedPaymentConfirmationNotification = expectedNotification as PaymentConfirmationNotification;
                    return actualPaymentConfirmationNotification.GetProductID() == expectedPaymentConfirmationNotification.GetProductID() &&
                           actualPaymentConfirmationNotification.GetOrderID() == expectedPaymentConfirmationNotification.GetOrderID();

                case ProductRemovedNotification actualProductRemovedNotification:
                    var expectedProductRemovedNotification = expectedNotification as ProductRemovedNotification;
                    return actualProductRemovedNotification.GetProductID() == expectedProductRemovedNotification.GetProductID();

                case ContractRenewalRequestNotification actualContractRenewalRequestNotification:
                    var expectedContractRenewalRequestNotification = expectedNotification as ContractRenewalRequestNotification;
                    return actualContractRenewalRequestNotification.GetContractID() == expectedContractRenewalRequestNotification.GetContractID();

                case ContractExpirationNotification actualContractExpirationNotification:
                    var expectedContractExpirationNotification = expectedNotification as ContractExpirationNotification;
                    return actualContractExpirationNotification.GetContractID() == expectedContractExpirationNotification.GetContractID() &&
                           actualContractExpirationNotification.GetExpirationDate() == expectedContractExpirationNotification.GetExpirationDate();

                default:
                    return false;
            }
        }
    }
}
