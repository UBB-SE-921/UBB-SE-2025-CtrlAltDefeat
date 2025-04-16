using System;
using System.Diagnostics.CodeAnalysis;

namespace ArtAttack.Domain
{
    [ExcludeFromCodeCoverage]
    abstract public class Notification
    {
        public int NotificationID { get; set; }
        public int RecipientID { get; set; }
        public NotificationCategory Category { get; set; }
        public bool IsRead { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ContractRenewalAnswerNotification : Notification
    {
        public int ContractID { get; }
        public bool IsAccepted { get; }

        public ContractRenewalAnswerNotification(int recipientID, DateTime timestamp, int contractID, bool isAccepted, bool isRead = false, int notificationId = 0)
        {
            this.NotificationID = notificationId;
            this.RecipientID = recipientID;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.ContractID = contractID;
            Category = NotificationCategory.CONTRACT_RENEWAL_ACCEPTED;
            this.IsAccepted = isAccepted;
        }
    }

    public class ContractRenewalWaitlistNotification : Notification
    {
        public int ProductID { get; }

        public ContractRenewalWaitlistNotification(int recipientID, DateTime timestamp, int productID, bool isRead = false, int notificationId = 0)
        {
            this.NotificationID = notificationId;
            this.RecipientID = recipientID;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.ProductID = productID;
            Category = NotificationCategory.CONTRACT_RENEWAL_WAITLIST;
        }
    }

    public class OutbiddedNotification : Notification
    {
        public int ProductID { get; }

        public OutbiddedNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.ProductID = productId;
            Category = NotificationCategory.OUTBIDDED;
        }
    }

    public class OrderShippingProgressNotification : Notification
    {
        public int OrderID { get; }
        public string ShippingState { get; }
        public DateTime DeliveryDate { get; }

        public OrderShippingProgressNotification(int recipientId, DateTime timestamp, int id, string state, DateTime deliveryDate, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.OrderID = id;
            this.ShippingState = state;
            Category = NotificationCategory.ORDER_SHIPPING_PROGRESS;
            this.DeliveryDate = deliveryDate;
        }
    }

    public class PaymentConfirmationNotification : Notification
    {
        public int ProductID { get; }
        public int OrderID { get; }

        public PaymentConfirmationNotification(int recipientId, DateTime timestamp, int productId, int orderId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.ProductID = productId;
            this.OrderID = orderId;
            Category = NotificationCategory.PAYMENT_CONFIRMATION;
        }
    }

    public class ProductRemovedNotification : Notification
    {
        public int ProductID { get; }

        public ProductRemovedNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.ProductID = productId;
            this.IsRead = isRead;
            Category = NotificationCategory.PRODUCT_REMOVED;
        }
    }

    public class ProductAvailableNotification : Notification
    {
        public int ProductID { get; }

        public ProductAvailableNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.ProductID = productId;
            this.IsRead = isRead;
            Category = NotificationCategory.PRODUCT_AVAILABLE;
        }
    }

    public class ContractRenewalRequestNotification : Notification
    {
        public int ContractID { get; }

        public ContractRenewalRequestNotification(int recipientId, DateTime timestamp, int contractId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.ContractID = contractId;
            this.IsRead = isRead;
            Category = NotificationCategory.CONTRACT_RENEWAL_REQUEST;
        }
    }

    public class ContractExpirationNotification : Notification
    {
        public int ContractID { get; }
        public DateTime ExpirationDate { get; }

        public ContractExpirationNotification(int recipientId, DateTime timestamp, int contractId, DateTime expirationDate, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.ContractID = contractId;
            this.IsRead = isRead;
            Category = NotificationCategory.CONTRACT_EXPIRATION;
            this.ExpirationDate = expirationDate;
        }
    }
}