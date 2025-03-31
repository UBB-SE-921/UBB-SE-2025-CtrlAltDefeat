using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using System;

namespace ArtAttack.Model
{
    public static class NotificationFactory
    {
        public static Notification CreateFromDataReader(SqlDataReader reader)
        {
            //Common fields
            int notificationId = reader.GetInt32(reader.GetOrdinal("notificationID"));
            int recipientId = reader.GetInt32(reader.GetOrdinal("recipientID"));
            DateTime timestamp = reader.GetDateTime(reader.GetOrdinal("timestamp"));
            bool isRead = reader.GetBoolean(reader.GetOrdinal("isRead"));
            string category = reader.GetString(reader.GetOrdinal("category"));

            switch (category)
            {
                case "CONTRACT_RENEWAL_ANS":
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

                case "CONTRACT_RENEWAL_REQ":
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
