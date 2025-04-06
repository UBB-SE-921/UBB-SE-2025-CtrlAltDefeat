using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtAttack.Domain;
using System;

namespace ArtAttack.Tests
{
    [TestClass]
    public class NotificationTests
    {
        [TestMethod]
        public void ContractRenewalAnswerNotification_ShouldReturnCorrectContent_WhenAccepted()
        {
            // Arrange
            var notification = new ContractRenewalAnswerNotification(1, DateTime.Now, 123, true);

            // Act
            var content = notification.Content;

            // Assert
            Assert.AreEqual("Contract: 123 has been renewed!\r\n You can download it from below!", content);
        }

        [TestMethod]
        public void ContractRenewalAnswerNotification_ShouldReturnCorrectContent_WhenNotAccepted()
        {
            // Arrange
            var notification = new ContractRenewalAnswerNotification(1, DateTime.Now, 123, false);

            // Act
            var content = notification.Content;

            // Assert
            Assert.AreEqual("Unfortunately, contract: 123 has not been renewed!\r\n The owner refused the renewal request :(", content);
        }

        [TestMethod]
        public void ContractRenewalWaitlistNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var notification = new ContractRenewalWaitlistNotification(1, DateTime.Now, 456);

            // Act
            var content = notification.Content;

            // Assert
            Assert.AreEqual("The user that borrowed product: 456 that you are part of the waitlist for, has renewed its contract.", content);
        }

        [TestMethod]
        public void OutbiddedNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var notification = new OutbiddedNotification(1, DateTime.Now, 789);

            // Act
            var content = notification.Content;

            // Assert
            Assert.AreEqual("You've been outbid! Another buyer has placed a higher bid on product: 789. Place a new bid now!", content);
        }

        [TestMethod]
        public void OrderShippingProgressNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var notification = new OrderShippingProgressNotification(1, DateTime.Now, 101, "Shipped", DateTime.Now.AddDays(3));

            // Act
            var content = notification.Content;

            // Assert
            Assert.AreEqual($"Your order: 101 has reached the Shipped stage. Estimated delivery is on {DateTime.Now.AddDays(3)}.", content);
        }

        [TestMethod]
        public void PaymentConfirmationNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var notification = new PaymentConfirmationNotification(1, DateTime.Now, 202, 303);

            // Act
            var content = notification.Content;

            // Assert
            Assert.AreEqual("Thank you for your purchase! Your order: 303 for product: 202 has been successfully processed.", content);
        }

        [TestMethod]
        public void ProductRemovedNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var notification = new ProductRemovedNotification(1, DateTime.Now, 404);

            // Act
            var content = notification.Content;

            // Assert
            Assert.AreEqual("Unfortunately, the product: 404 that you were waiting for was removed from the marketplace.", content);
        }

        [TestMethod]
        public void ProductAvailableNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var notification = new ProductAvailableNotification(1, DateTime.Now, 505);

            // Act
            var content = notification.Content;

            // Assert
            Assert.AreEqual("Good news! The product: 505 that you were waiting for is now back in stock.", content);
        }

        [TestMethod]
        public void ContractRenewalRequestNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var notification = new ContractRenewalRequestNotification(1, DateTime.Now, 606);

            // Act
            var content = notification.Content;

            // Assert
            Assert.AreEqual("User 1 would like to renew contract: 606. Please respond promptly.", content);
        }

        [TestMethod]
        public void ContractExpirationNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var notification = new ContractExpirationNotification(1, DateTime.Now, 707, DateTime.Now.AddDays(10));

            // Act
            var content = notification.Content;

            // Assert
            Assert.AreEqual($"Contract: 707 is set to expire on {DateTime.Now.AddDays(10)}.", content);
        }
    }
}