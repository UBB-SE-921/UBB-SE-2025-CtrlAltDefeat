using System;

namespace ArtAttack.Domain
{
    public interface INotification
    {
        int NotificationID { get; }
        int RecipientID { get; }
        NotificationCategory Category { get; }
        bool IsRead { get; set; }
        string Title { get; }
        string Subtitle { get; }
        string Content { get; }
        DateTime Timestamp { get; set; }
        bool IsNotRead { get; }
        void MarkAsRead();
    }

    public abstract class NotificationBase : INotification
    {
        public int NotificationID { get; protected set; }
        public int RecipientID { get; protected set; }
        public NotificationCategory Category { get; protected set; }
        public bool IsRead { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsNotRead => !IsRead;

        public void MarkAsRead()
        {
            IsRead = true;
        }

        public abstract string Title { get; }
        public abstract string Subtitle { get; }
        public abstract string Content { get; }
    }

    public class ContractRenewalAnswerNotification : NotificationBase
    {
        private readonly int _contractID;
        private readonly bool _isAccepted;

        public ContractRenewalAnswerNotification(int recipientID, DateTime timestamp, int contractID, bool isAccepted, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientID;
            Timestamp = timestamp;
            IsRead = isRead;
            _contractID = contractID;
            Category = NotificationCategory.CONTRACT_RENEWAL_ANS;
            _isAccepted = isAccepted;
        }

        public int GetContractID()
        {
            return _contractID;
        }

        public bool GetIsAccepted()
        {
            return _isAccepted;
        }

        public override string Content => _isAccepted
            ? $"Contract: {_contractID} has been renewed!\r\n You can download it from below!"
            : $"Unfortunately, contract: {_contractID} has not been renewed!\r\n The owner refused the renewal request :(";

        public override string Title => "Contract Renewal Answer";
        public override string Subtitle => $"You have received an answer on the renewal request for contract: {_contractID}.";
    }

    public class ContractRenewalWaitlistNotification : Notification
    {
        private readonly int _productID;

        public ContractRenewalWaitlistNotification(int recipientID, DateTime timestamp, int productID, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientID;
            Timestamp = timestamp;
            IsRead = isRead;
            _productID = productID;
            Category = NotificationCategory.CONTRACT_RENEWAL_WAITLIST;
        }

        public int GetProductID()
        {
            return _productID;
        }

        public override string Content => $"The user that borrowed product: {_productID} that you are part of the waitlist for, has renewed its contract.";
        public override string Title => "Contract Renewal in Waitlist";
        public override string Subtitle => "A user has extended their contract.";
    }

    public class OutbiddedNotification : NotificationBase
    {
        private readonly int _productID;

        public OutbiddedNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            Timestamp = timestamp;
            IsRead = isRead;
            _productID = productId;
            Category = NotificationCategory.OUTBIDDED;
        }

        public int GetProductID()
        {
            return _productID;
        }

        public override string Content => $"You've been outbid! Another buyer has placed a higher bid on product: {_productID}. Place a new bid now!";
        public override string Title => "Outbidded";
        public override string Subtitle => $"You've been outbidded on product: {_productID}.";
    }

    public class OrderShippingProgressNotification : NotificationBase
    {
        private readonly int _orderID;
        private readonly string _shippingState;
        private readonly DateTime _deliveryDate;

        public OrderShippingProgressNotification(int recipientId, DateTime timestamp, int orderId, string shippingState, DateTime deliveryDate, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            Timestamp = timestamp;
            IsRead = isRead;
            _orderID = orderId;
            _shippingState = shippingState;
            Category = NotificationCategory.ORDER_SHIPPING_PROGRESS;
            _deliveryDate = deliveryDate;
        }

        public int GetOrderID()
        {
            return _orderID;
        }

        public DateTime GetDeliveryDate()
        {
            return _deliveryDate;
        }

        public string GetShippingState()
        {
            return _shippingState;
        }

        public override string Content => $"Your order: {_orderID} has reached the {_shippingState} stage. Estimated delivery is on {_deliveryDate}.";
        public override string Title => "Order Shipping Update";
        public override string Subtitle => $"New info on order: {_orderID} is available.";
    }

    public class PaymentConfirmationNotification : NotificationBase
    {
        private readonly int _productID;
        private readonly int _orderID;

        public PaymentConfirmationNotification(int recipientId, DateTime timestamp, int productId, int orderId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            Timestamp = timestamp;
            IsRead = isRead;
            _productID = productId;
            _orderID = orderId;
            Category = NotificationCategory.PAYMENT_CONFIRMATION;
        }

        public int GetProductID()
        {
            return _productID;
        }

        public int GetOrderID()
        {
            return _orderID;
        }

        public override string Content => $"Thank you for your purchase! Your order: {_orderID} for product: {_productID} has been successfully processed.";
        public override string Title => "Payment Confirmation";
        public override string Subtitle => $"Order: {_orderID} has been processed successfully!";
    }

    public class ProductRemovedNotification : NotificationBase
    {
        private readonly int _productID;

        public ProductRemovedNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            Timestamp = timestamp;
            IsRead = isRead;
            _productID = productId;
            Category = NotificationCategory.PRODUCT_REMOVED;
        }

        public int GetProductID()
        {
            return _productID;
        }

        public override string Content => $"Unfortunately, the product: {_productID} that you were waiting for was removed from the marketplace.";
        public override string Title => "Product Removed";
        public override string Subtitle => $"Product: {_productID} was removed from the marketplace!";
    }

    public class ProductAvailableNotification : NotificationBase
    {
        private readonly int _productID;

        public ProductAvailableNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            Timestamp = timestamp;
            IsRead = isRead;
            _productID = productId;
            Category = NotificationCategory.PRODUCT_AVAILABLE;
        }

        public int GetProductID()
        {
            return _productID;
        }

        public override string Content => $"Good news! The product: {_productID} that you were waiting for is now back in stock.";
        public override string Title => "Product Available";
        public override string Subtitle => $"Product: {_productID} is available now!";
    }

    public class ContractRenewalRequestNotification : NotificationBase
    {
        private readonly int _contractID;

        public ContractRenewalRequestNotification(int recipientId, DateTime timestamp, int contractId, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            Timestamp = timestamp;
            IsRead = isRead;
            _contractID = contractId;
            Category = NotificationCategory.CONTRACT_RENEWAL_REQ;
        }

        public int GetContractID()
        {
            return _contractID;
        }

        public override string Content => $"User {RecipientID} would like to renew contract: {_contractID}. Please respond promptly.";
        public override string Title => "Contract Renewal Request";
        public override string Subtitle => $"User {RecipientID} wants to renew contract: {_contractID}.";
    }

    public class ContractExpirationNotification : NotificationBase
    {
        private readonly int _contractID;
        private readonly DateTime _expirationDate;

        public ContractExpirationNotification(int recipientId, DateTime timestamp, int contractId, DateTime expirationDate, bool isRead = false, int notificationId = 0)
        {
            NotificationID = notificationId;
            RecipientID = recipientId;
            Timestamp = timestamp;
            IsRead = isRead;
            _contractID = contractId;
            Category = NotificationCategory.CONTRACT_EXPIRATION;
            _expirationDate = expirationDate;
        }

        public int GetContractID()
        {
            return _contractID;
        }

        public override string Content => $"Contract: {_contractID} is set to expire on {_expirationDate}.";
        public override string Title => "Contract Expiration";
        public override string Subtitle => $"Contract: {_contractID} is about to expire!";
    }
}
