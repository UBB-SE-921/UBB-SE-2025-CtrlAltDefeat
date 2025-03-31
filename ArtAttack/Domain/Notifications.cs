using System;

namespace ArtAttack.Domain
{
    abstract public class Notification
    {
        protected int notificationID;
        protected int recipientID;
        protected NotificationCategory category;
        protected bool isRead;

        public int getNotificationID()
        {
            return notificationID;
        }

        public int getRecipientID()
        {
            return recipientID;
        }

        public NotificationCategory getCategory()
        {
            return category;
        }

        public bool getIsRead()
        {
            return isRead;
        }

        public void setIsRead(bool value)
        {
            isRead = value;
        }
        public void markAsRead()
        {
            isRead = true;
        }
        public abstract string Title { get; }
        public abstract string Subtitle { get; }
        public abstract string Content { get; }
        public DateTime timestamp { get; set; }
        public bool IsNotRead => !isRead;

    }

    //TODO
    public class ContractRenewalAnswerNotification : Notification
    {
        private int contractID;
        private bool isAccepted;
        public ContractRenewalAnswerNotification(int recipientID, DateTime timestamp, int contractID, bool isAccepted, bool isRead = false, int notificationId = 0)
        {
            notificationID = notificationId;
            this.recipientID = recipientID;
            this.timestamp = timestamp;
            this.isRead = isRead;
            this.contractID = contractID;
            category = NotificationCategory.CONTRACT_RENEWAL_ANS;
            this.isAccepted = isAccepted;
        }
        public int getContractID()
        {
            return contractID;
        }

        public bool getIsAccepted()
        {
            return isAccepted;
        }

        public override string Content => isAccepted ? $"Contract: {contractID} has been renewed!\r\n You can download it from below!" : $"Unfortunately, contract: {contractID} has not been renewed!\r\n The owner refused the renewal request :(";
        public override string Title => "Contract Renewal Answer";
        public override string Subtitle => $"You have received an answer on the renewal request for contract: {contractID}.";

    }


    public class ContractRenewalWaitlistNotification : Notification
    {
        private int productID;
        public ContractRenewalWaitlistNotification(int recipientID, DateTime timestamp, int productID, bool isRead = false, int notificationId = 0)
        {
            notificationID = notificationId;
            this.recipientID = recipientID;
            this.timestamp = timestamp;
            this.isRead = isRead;
            this.productID = productID;
            category = NotificationCategory.CONTRACT_RENEWAL_WAITLIST;
        }

        public int getProductID()
        {
            return productID;
        }

        public override string Content => $"The user that borrowed product: {productID} that you are part of the waitlist for, has renewed its contract.";
        public override string Title => "Contract Renewal in Waitlist";
        public override string Subtitle => "A user has extended their contract.";

    }


    public class OutbiddedNotification : Notification
    {
        private int productID;
        public OutbiddedNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            notificationID = notificationId;
            recipientID = recipientId;
            this.timestamp = timestamp;
            this.isRead = isRead;
            productID = productId;
            category = NotificationCategory.OUTBIDDED;
        }
        public int getProductID()
        {
            return productID;
        }
        public override string Content => $"You've been outbid! Another buyer has placed a higher bid on product: {productID}. Place a new bid now!";
        public override string Title => "Outbidded";
        public override string Subtitle => $"You've been outbidded on product: {productID}.";

    }

    public class OrderShippingProgressNotification : Notification
    {
        private int orderID;
        private string shippingState;
        private DateTime deliveryDate;

        public OrderShippingProgressNotification(int recipientId, DateTime timestamp, int id, string state, DateTime deliveryDate, bool isRead = false, int notificationId = 0)
        {
            notificationID = notificationId;
            recipientID = recipientId;
            this.timestamp = timestamp;
            this.isRead = isRead;
            orderID = id;
            shippingState = state;
            category = NotificationCategory.ORDER_SHIPPING_PROGRESS;
            this.deliveryDate = deliveryDate;
        }

        public int getOrderID()
        {
            return orderID;
        }

        public DateTime getDeliveryDate()
        {
            return deliveryDate;
        }

        public string getShippingState()
        {
            return shippingState;
        }

        public override string Content => $"Your order: {orderID} has reached the {shippingState} stage. Estimated delivery is on {deliveryDate}.";
        public override string Title => "Order Shipping Update";
        public override string Subtitle => $"New info on order: {orderID} is available.";
    }

    public class PaymentConfirmationNotification : Notification
    {
        private int productID;
        private int orderID;

        public PaymentConfirmationNotification(int recipientId, DateTime timestamp, int productId, int orderId, bool isRead = false, int notificationId = 0)
        {
            notificationID = notificationId;
            recipientID = recipientId;
            this.timestamp = timestamp;
            this.isRead = isRead;
            productID = productId;
            orderID = orderId;
            category = NotificationCategory.PAYMENT_CONFIRMATION;
        }

        public int getProductID()
        {
            return productID;
        }

        public int getOrderID()
        {
            return orderID;
        }

        public override string Content => $"Thank you for your purchase! Your order: {orderID} for product: {productID} has been successfully processed.";
        public override string Title => "Payment Confirmation";
        public override string Subtitle => $"Order: {orderID} has been processed successfully!";


    }

    public class ProductRemovedNotification : Notification
    {
        private int productID;

        public ProductRemovedNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            notificationID = notificationId;
            recipientID = recipientId;
            this.timestamp = timestamp;
            productID = productId;
            this.isRead = isRead;
            category = NotificationCategory.PRODUCT_REMOVED;
        }

        public int getProductID()
        {
            return productID;
        }

        public override string Content => $"Unfortunately, the product: {productID} that you were waiting for was removed from the marketplace.";
        public override string Title => "Product Removed";
        public override string Subtitle => $"Product: {productID} was removed from the marketplace!";


    }

    public class ProductAvailableNotification : Notification
    {
        private int productID;

        public ProductAvailableNotification(int recipientId, DateTime timestamp, int productId, bool isRead = false, int notificationId = 0)
        {
            notificationID = notificationId;
            recipientID = recipientId;
            this.timestamp = timestamp;
            productID = productId;
            this.isRead = isRead;
            category = NotificationCategory.PRODUCT_AVAILABLE;
        }

        public int getProductID()
        {
            return productID;
        }

        public override string Content => $"Good news! The product: {productID} that you were waiting for is now back in stock.";
        public override string Title => "Product Available";
        public override string Subtitle => $"Product: {productID} is available now!";

    }

    public class ContractRenewalRequestNotification : Notification
    {
        private int contractID;

        public ContractRenewalRequestNotification(int recipientId, DateTime timestamp, int contractId, bool isRead = false, int notificationId = 0)
        {
            notificationID = notificationId;
            recipientID = recipientId;
            this.timestamp = timestamp;
            contractID = contractId;
            this.isRead = isRead;
            category = NotificationCategory.CONTRACT_RENEWAL_REQ;
        }

        public int getContractID()
        {
            return contractID;
        }

        public override string Content => $"User {recipientID} would like to renew contract: {contractID}. Please respond promptly.";
        public override string Title => "Contract Renewal Request";
        public override string Subtitle => $"User {recipientID} wants to renew contract: {contractID}.";


    }

    public class ContractExpirationNotification : Notification
    {
        private int contractID;
        private DateTime expirationDate;

        public ContractExpirationNotification(int recipientId, DateTime timestamp, int contractId, DateTime expirationDate, bool isRead = false, int notificationId = 0)
        {
            notificationID = notificationId;
            recipientID = recipientId;
            this.timestamp = timestamp;
            contractID = contractId;
            this.isRead = isRead;
            category = NotificationCategory.CONTRACT_EXPIRATION;
            this.expirationDate = expirationDate;
        }

        public int getContractID()
        {
            return contractID;
        }

        public override string Content => $"Contract: {contractID} is set to expire on {expirationDate}.";
        public override string Title => "Contract Expiration";
        public override string Subtitle => $"Contract: {contractID} is about to expire!";

    }


}
