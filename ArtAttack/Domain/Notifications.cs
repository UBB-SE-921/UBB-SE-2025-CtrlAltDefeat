using System;

namespace ArtAttack.Domain
{
    abstract public class Notification : INotification
    {
        public int NotificationID { get; set; }
        public int RecipientID { get; set; }
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

        public abstract string Title { get; }
        public abstract string Subtitle { get; }
        public abstract string Content { get; }
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
            Category = NotificationCategory.CONTRACT_RENEWAL_ANS;
            this.IsAccepted = isAccepted;
        }
        public int GetContractID()
        {
            return ContractID;
        }

        public bool GetIsAccepted()
        {
            return IsAccepted;
        }

        public override string Content => IsAccepted ? $"Contract: {ContractID} has been renewed!\r\n You can download it from below!" : $"Unfortunately, contract: {ContractID} has not been renewed!\r\n The owner refused the renewal request :(";
        public override string Title => "Contract Renewal Answer";
        public override string Subtitle => $"You have received an answer on the renewal request for contract: {ContractID}.";
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

        public int GetProductID()
        {
            return ProductID;
        }

        public override string Content => $"The user that borrowed product: {ProductID} that you are part of the waitlist for, has renewed its contract.";
        public override string Title => "Contract Renewal in Waitlist";
        public override string Subtitle => "A user has extended their contract.";
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
        public int GetProductID()
        {
            return ProductID;
        }
        public override string Content => $"You've been outbid! Another buyer has placed a higher bid on product: {ProductID}. Place a new bid now!";
        public override string Title => "Outbidded";
        public override string Subtitle => $"You've been outbidded on product: {ProductID}.";
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

        public int GetOrderID()
        {
            return OrderID;
        }

        public DateTime GetDeliveryDate()
        {
            return DeliveryDate;
        }

        public string GetShippingState()
        {
            return ShippingState;
        }

        public override string Content => $"Your order: {OrderID} has reached the {ShippingState} stage. Estimated delivery is on {DeliveryDate}.";
        public override string Title => "Order Shipping Update";
        public override string Subtitle => $"New info on order: {OrderID} is available.";
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

        public int GetProductID()
        {
            return ProductID;
        }

        public int GetOrderID()
        {
            return OrderID;
        }

        public override string Content => $"Thank you for your purchase! Your order: {OrderID} for product: {ProductID} has been successfully processed.";
        public override string Title => "Payment Confirmation";
        public override string Subtitle => $"Order: {OrderID} has been processed successfully!";
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

        public int GetProductID()
        {
            return ProductID;
        }

        public override string Content => $"Unfortunately, the product: {ProductID} that you were waiting for was removed from the marketplace.";
        public override string Title => "Product Removed";
        public override string Subtitle => $"Product: {ProductID} was removed from the marketplace!";
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

        public int GetProductID()
        {
            return ProductID;
        }

        public override string Content => $"Good news! The product: {ProductID} that you were waiting for is now back in stock.";
        public override string Title => "Product Available";
        public override string Subtitle => $"Product: {ProductID} is available now!";
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
            Category = NotificationCategory.CONTRACT_RENEWAL_REQ;
        }

        public int GetContractID()
        {
            return ContractID;
        }

        public override string Content => $"User {RecipientID} would like to renew contract: {ContractID}. Please respond promptly.";
        public override string Title => "Contract Renewal Request";
        public override string Subtitle => $"User {RecipientID} wants to renew contract: {ContractID}.";
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

        public int GetContractID()
        {
            return ContractID;
        }
        public DateTime GetExpirationDate()
        {
            return ExpirationDate;
        }

        public override string Content => $"Contract: {ContractID} is set to expire on {ExpirationDate}.";
        public override string Title => "Contract Expiration";
        public override string Subtitle => $"Contract: {ContractID} is about to expire!";
    }
}