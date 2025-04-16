using System;
using System.Diagnostics.CodeAnalysis;

namespace ArtAttack.Domain
{
    [ExcludeFromCodeCoverage]
    public class Notification : INotification
    {
        public int NotificationID { get; set; }
        public int RecipientID { get; set; }
        public string? Message { get; set; }
        public string? NotificationType { get; set; }
        public NotificationCategory Category { get; set; }
        public bool IsRead { get; set; }
        public DateTime Timestamp { get; set; }

        public bool IsNotRead
        {
            get => !IsRead;
            set => IsRead = !value;
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }

        public string Title { get; }
        public string Subtitle { get; }
        public string Content { get; }
    }

    public class ContractRenewalAnswerNotification : Notification
    {
        private int ContractID { get; }
        private bool IsAccepted { get; }
        public ContractRenewalAnswerNotification(int recipientID, DateTime timestamp, int contractID, bool isAccepted, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            this.RecipientID = recipientID;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.ContractID = contractID;
            Category = NotificationCategory.CONTRACT_RENEWAL_ACCEPTED;
            this.IsAccepted = isAccepted;
        }
        [ExcludeFromCodeCoverage]
        public int GetContractID()
        {
            return ContractID;
        }
        [ExcludeFromCodeCoverage]
        public bool GetIsAccepted()
        {
            return IsAccepted;
        }
        [ExcludeFromCodeCoverage]
        public string Content => IsAccepted ? $"Contract: {ContractID} has been renewed!\reader\n You can download it from below!" : $"Unfortunately, contract: {ContractID} has not been renewed!\reader\n The owner refused the renewal request :(";
        [ExcludeFromCodeCoverage]
        public string Title => "Contract Renewal Answer";
        [ExcludeFromCodeCoverage]
        public string Subtitle => $"You have received an answer on the renewal request for contract: {ContractID}.";
    }

    public class ContractRenewalWaitlistNotification : Notification
    {
        private int ProductID { get; }
        public ContractRenewalWaitlistNotification(int recipientID, DateTime timestamp, int productID, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            this.RecipientID = recipientID;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            this.ProductID = productID;
            Category = NotificationCategory.CONTRACT_RENEWAL_WAITLIST;
        }

        [ExcludeFromCodeCoverage]
        public int GetProductID()
        {
            return ProductID;
        }

        [ExcludeFromCodeCoverage]
        public string Content => $"The user that borrowed product: {ProductID} that you are part of the waitlist for, has renewed its contract.";
        [ExcludeFromCodeCoverage]
        public string Title => "Contract Renewal in Waitlist";
        [ExcludeFromCodeCoverage]
        public string Subtitle => "A user has extended their contract.";
    }

    public class OutbiddedNotification : Notification
    {
        private int ProductID { get; }
        public OutbiddedNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            ProductID = productId;
            Category = NotificationCategory.OUTBIDDED;
        }
        [ExcludeFromCodeCoverage]
        public int GetProductID()
        {
            return ProductID;
        }
        [ExcludeFromCodeCoverage]
        public string Content => $"You've been outbid! Another buyer has placed a higher bid on product: {ProductID}. Place a new bid now!";
        [ExcludeFromCodeCoverage]
        public string Title => "Outbidded";
        [ExcludeFromCodeCoverage]
        public string Subtitle => $"You've been outbidded on product: {ProductID}.";
    }

    public class OrderShippingProgressNotification : Notification
    {
        private int OrderID { get; }
        private string ShippingState { get; }
        private DateTime DeliveryDate { get; }

        public OrderShippingProgressNotification(int recipientId, DateTime timestamp, int id, string state, DateTime deliveryDate, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            OrderID = id;
            ShippingState = state;
            Category = NotificationCategory.ORDER_SHIPPING_PROGRESS;
            this.DeliveryDate = deliveryDate;
        }

        [ExcludeFromCodeCoverage]
        public int GetOrderID()
        {
            return OrderID;
        }

        [ExcludeFromCodeCoverage]
        public DateTime GetDeliveryDate()
        {
            return DeliveryDate;
        }

        [ExcludeFromCodeCoverage]
        public string GetShippingState()
        {
            return ShippingState;
        }

        [ExcludeFromCodeCoverage]
        public string Content => $"Your order: {OrderID} has reached the {ShippingState} stage. Estimated delivery is on {DeliveryDate}.";
        [ExcludeFromCodeCoverage]
        public string Title => "Order Shipping Update";
        [ExcludeFromCodeCoverage]
        public string Subtitle => $"New info on order: {OrderID} is available.";
    }

    public class PaymentConfirmationNotification : Notification
    {
        private int ProductID { get; }
        private int OrderID { get; }

        public PaymentConfirmationNotification(int recipientId, DateTime timestamp, int productId, int orderId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            this.IsRead = isRead;
            ProductID = productId;
            OrderID = orderId;
            Category = NotificationCategory.PAYMENT_CONFIRMATION;
        }

        [ExcludeFromCodeCoverage]
        public int GetProductID()
        {
            return ProductID;
        }

        [ExcludeFromCodeCoverage]
        public int GetOrderID()
        {
            return OrderID;
        }

        [ExcludeFromCodeCoverage]
        public string Content => $"Thank you for your purchase! Your order: {OrderID} for product: {ProductID} has been successfully processed.";
        [ExcludeFromCodeCoverage]
        public string Title => "Payment Confirmation";
        [ExcludeFromCodeCoverage]
        public string Subtitle => $"Order: {OrderID} has been processed successfully!";
    }

    public class ProductRemovedNotification : Notification
    {
        private int ProductID { get; }

        public ProductRemovedNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            ProductID = productId;
            this.IsRead = isRead;
            Category = NotificationCategory.PRODUCT_REMOVED;
        }

        [ExcludeFromCodeCoverage]
        public int GetProductID()
        {
            return ProductID;
        }

        [ExcludeFromCodeCoverage]
        public string Content => $"Unfortunately, the product: {ProductID} that you were waiting for was removed from the marketplace.";
        [ExcludeFromCodeCoverage]
        public string Title => "Product Removed";
        [ExcludeFromCodeCoverage]
        public string Subtitle => $"Product: {ProductID} was removed from the marketplace!";
    }

    public class ProductAvailableNotification : Notification
    {
        private int ProductID { get; }

        public ProductAvailableNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            ProductID = productId;
            this.IsRead = isRead;
            Category = NotificationCategory.PRODUCT_AVAILABLE;
        }

        [ExcludeFromCodeCoverage]
        public int GetProductID()
        {
            return ProductID;
        }

        [ExcludeFromCodeCoverage]
        public string Content => $"Good news! The product: {ProductID} that you were waiting for is now back in stock.";
        [ExcludeFromCodeCoverage]
        public string Title => "Product Available";
        [ExcludeFromCodeCoverage]
        public string Subtitle => $"Product: {ProductID} is available now!";
    }

    public class ContractRenewalRequestNotification : Notification
    {
        private int ContractID { get; }

        public ContractRenewalRequestNotification(int recipientId, DateTime timestamp, int contractId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            ContractID = contractId;
            this.IsRead = isRead;
            Category = NotificationCategory.CONTRACT_RENEWAL_REQUEST;
        }

        [ExcludeFromCodeCoverage]
        public int GetContractID()
        {
            return ContractID;
        }

        [ExcludeFromCodeCoverage]
        public string Content => $"User {RecipientID} would like to renew contract: {ContractID}. Please respond promptly.";
        [ExcludeFromCodeCoverage]
        public string Title => "Contract Renewal Request";
        [ExcludeFromCodeCoverage]
        public string Subtitle => $"User {RecipientID} wants to renew contract: {ContractID}.";
    }

    public class ContractExpirationNotification : Notification
    {
        private int ContractID { get; }
        private DateTime ExpirationDate { get; }

        public ContractExpirationNotification(int recipientId, DateTime timestamp, int contractId, DateTime expirationDate, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            this.Timestamp = timestamp;
            ContractID = contractId;
            this.IsRead = isRead;
            Category = NotificationCategory.CONTRACT_EXPIRATION;
            this.ExpirationDate = expirationDate;
        }

        [ExcludeFromCodeCoverage]
        public int GetContractID()
        {
            return ContractID;
        }
        public DateTime GetExpirationDate()
        {
            return ExpirationDate;
        }

        [ExcludeFromCodeCoverage]
        public string Content => $"Contract: {ContractID} is set to expire on {ExpirationDate}.";
        [ExcludeFromCodeCoverage]
        public string Title => "Contract Expiration";
        [ExcludeFromCodeCoverage]
        public string Subtitle => $"Contract: {ContractID} is about to expire!";
    }
}