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
        private readonly int contractID;
        private readonly bool isAccepted;

        public int ContractID
        {
            get { return contractID; }
        }
        public bool IsAccepted
        {
            get { return isAccepted; }
        }

        public ContractRenewalAnswerNotification(int recipientID, DateTime timestamp, int contractID, bool isAccepted, bool isRead = false, int notificationId = 0)
        {
            this.NotificationID = notificationId;
            this.RecipientID = recipientID;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.contractID = contractID;
            Category = NotificationCategory.CONTRACT_RENEWAL_ACCEPTED;
            this.isAccepted = isAccepted;
        }
    }

    public class ContractRenewalWaitlistNotification : Notification
    {
        private readonly int productID;
        public int ProductID
        {
            get { return productID; }
        }

        public ContractRenewalWaitlistNotification(int recipientID, DateTime timestamp, int productID, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            this.RecipientID = recipientID;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.productID = productID;
            Category = NotificationCategory.CONTRACT_RENEWAL_WAITLIST;
        }
    }

    public class OutbiddedNotification : Notification
    {
        private readonly int productID;
        public int ProductID
        {
            get { return productID; }
        }

        public OutbiddedNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.productID = productId;
            Category = NotificationCategory.OUTBIDDED;
        }
    }

    public class OrderShippingProgressNotification : Notification
    {
        private readonly int orderID;
        private readonly string shippingState;
        private readonly DateTime deliveryDate;

        public int OrderID
        {
            get { return orderID; }
        }

        public string ShippingState
        {
            get { return shippingState; }
        }

        public DateTime DeliveryDate
        {
            get { return deliveryDate; }
        }

        public OrderShippingProgressNotification(int recipientId, DateTime timestamp, int id, string state, DateTime deliveryDate, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.orderID = id;
            this.shippingState = state;
            Category = NotificationCategory.ORDER_SHIPPING_PROGRESS;
            this.deliveryDate = deliveryDate;
        }
    }

    public class PaymentConfirmationNotification : Notification
    {
        private readonly int productID;
        private readonly int orderID;

        public int ProductID
        {
            get { return productID; }
        }

        public int OrderID
        {
            get { return orderID; }
        }

        public PaymentConfirmationNotification(int recipientId, DateTime timestamp, int productId, int orderId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.productID = productId;
            this.orderID = orderId;
            Category = NotificationCategory.PAYMENT_CONFIRMATION;
        }
    }

    public class ProductRemovedNotification : Notification
    {
        private readonly int productID;

        public int ProductID
        {
            get { return productID; }
        }

        public ProductRemovedNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.productID = productId;
            this.IsRead = isRead;
            Category = NotificationCategory.PRODUCT_REMOVED;
        }
    }

    public class ProductAvailableNotification : Notification
    {
        private readonly int productID;

        public int ProductID
        {
            get { return productID; }
        }

        public ProductAvailableNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.productID = productId;
            this.IsRead = isRead;
            Category = NotificationCategory.PRODUCT_AVAILABLE;
        }
    }

    public class ContractRenewalRequestNotification : Notification
    {
        private readonly int contractID;

        public int ContractID
        {
            get { return contractID; }
        }

        public ContractRenewalRequestNotification(int recipientId, DateTime timestamp, int contractId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.contractID = contractId;
            this.IsRead = isRead;
            Category = NotificationCategory.CONTRACT_RENEWAL_REQUEST;
        }
    }

    public class ContractExpirationNotification : Notification
    {
        private readonly int contractID;
        private readonly DateTime expirationDate;

        public int ContractID
        {
            get { return contractID; }
        }

        public DateTime ExpirationDate
        {
            get { return expirationDate; }
        }

        public ContractExpirationNotification(int recipientId, DateTime timestamp, int contractId, DateTime expirationDate, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.contractID = contractId;
            this.IsRead = isRead;
            Category = NotificationCategory.CONTRACT_EXPIRATION;
            this.expirationDate = expirationDate;
        }
    }
}