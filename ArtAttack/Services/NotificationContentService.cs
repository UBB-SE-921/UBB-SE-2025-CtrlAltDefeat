using System;
using ArtAttack.Domain;

namespace ArtAttack.Services
{
    public class NotificationContentService : INotificationContentService
    {
        public string GetContent(Notification notification)
        {
            return notification switch
            {
                ContractRenewalAnswerNotification ans => ans.IsAccepted
                    ? $"Contract: {ans.ContractID} has been renewed!\nYou can download it from below!"
                    : $"Unfortunately, contract: {ans.ContractID} has not been renewed!\nThe owner refused the renewal request :(",
                ContractRenewalWaitlistNotification waitlist => $"You have been added to the waitlist for contract renewal of product: {waitlist.ProductID}",
                OutbiddedNotification outbid => $"You have been outbid on product: {outbid.ProductID}",
                OrderShippingProgressNotification shipping => $"Your order {shipping.OrderID} is {shipping.ShippingState}. Expected delivery: {shipping.DeliveryDate:MM/dd/yyyy}",
                PaymentConfirmationNotification payment => $"Payment confirmed for order {payment.OrderID} of product {payment.ProductID}",
                ProductRemovedNotification removed => $"Product {removed.ProductID} has been removed from the platform",
                ProductAvailableNotification available => $"Product {available.ProductID} is now available for purchase",
                ContractRenewalRequestNotification request => $"You have received a renewal request for contract {request.ContractID}",
                ContractExpirationNotification expiration => $"Contract {expiration.ContractID} will expire on {expiration.ExpirationDate:MM/dd/yyyy}",
                _ => throw new ArgumentException($"Unknown notification type: {notification.GetType()}")
            };
        }

        public string GetTitle(Notification notification)
        {
            return notification switch
            {
                ContractRenewalAnswerNotification _ => "Contract Renewal Answer",
                ContractRenewalWaitlistNotification _ => "Contract Renewal Waitlist",
                OutbiddedNotification _ => "Outbid Notification",
                OrderShippingProgressNotification _ => "Order Shipping Update",
                PaymentConfirmationNotification _ => "Payment Confirmation",
                ProductRemovedNotification _ => "Product Removed",
                ProductAvailableNotification _ => "Product Available",
                ContractRenewalRequestNotification _ => "Contract Renewal Request",
                ContractExpirationNotification _ => "Contract Expiration Notice",
                _ => throw new ArgumentException($"Unknown notification type: {notification.GetType()}")
            };
        }

        public string GetSubtitle(Notification notification)
        {
            return notification switch
            {
                ContractRenewalAnswerNotification ans => $"You have received an answer on the renewal request for contract: {ans.ContractID}.",
                ContractRenewalWaitlistNotification waitlist => $"You have been added to the waitlist for product: {waitlist.ProductID}",
                OutbiddedNotification outbid => $"You have been outbid on product: {outbid.ProductID}",
                OrderShippingProgressNotification shipping => $"Update on your order: {shipping.OrderID}",
                PaymentConfirmationNotification payment => $"Payment processed for order: {payment.OrderID}",
                ProductRemovedNotification removed => $"Product {removed.ProductID} has been removed",
                ProductAvailableNotification available => $"Product {available.ProductID} is now available",
                ContractRenewalRequestNotification request => $"New renewal request for contract: {request.ContractID}",
                ContractExpirationNotification expiration => $"Contract {expiration.ContractID} expiration notice",
                _ => throw new ArgumentException($"Unknown notification type: {notification.GetType()}")
            };
        }
    }
}