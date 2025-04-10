using ArtAttack.Domain;

namespace ArtAttack.Tests.Model
{
    [TestClass]
    public class NotificationTests
    {
        [TestMethod]
        public void ContractRenewalAnswerNotification_WhenAccepted_ShouldReturnCorrectContent()
        {
            // Arrange
            var contractRenewalAnswerNotification = new ContractRenewalAnswerNotification(1, DateTime.Now, 123, true);

            // Act
            var notificationContent = contractRenewalAnswerNotification.Content;

            // Assert
            Assert.AreEqual("Contract: 123 has been renewed!\reader\n You can download it from below!", notificationContent);
        }

        [TestMethod]
        public void ContractRenewalAnswerNotification_WhenNotAccepted_ShouldReturnCorrectContent()
        {
            // Arrange
            var contractRenewalAnswerNotification = new ContractRenewalAnswerNotification(1, DateTime.Now, 123, false);

            // Act
            var notificationContent = contractRenewalAnswerNotification.Content;

            // Assert
            Assert.AreEqual("Unfortunately, contract: 123 has not been renewed!\reader\n The owner refused the renewal request :(", notificationContent);
        }

        [TestMethod]
        public void ContractRenewalWaitlistNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var contractRenewalWaitlistNotification = new ContractRenewalWaitlistNotification(1, DateTime.Now, 456);

            // Act
            var notificationContent = contractRenewalWaitlistNotification.Content;

            // Assert
            Assert.AreEqual("The user that borrowed product: 456 that you are part of the waitlist for, has renewed its contract.", notificationContent);
        }

        [TestMethod]
        public void OutbiddedNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var outbiddedNotification = new OutbiddedNotification(1, DateTime.Now, 789);

            // Act
            var notificationContent = outbiddedNotification.Content;

            // Assert
            Assert.AreEqual("You've been outbid! Another buyer has placed a higher bid on product: 789. Place a new bid now!", notificationContent);
        }

        [TestMethod]
        public void OrderShippingProgressNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var orderShippingProgressNotification = new OrderShippingProgressNotification(1, DateTime.Now, 101, "Shipped", DateTime.Now.AddDays(3));

            // Act
            var notificationContent = orderShippingProgressNotification.Content;

            // Assert
            Assert.AreEqual($"Your order: 101 has reached the Shipped stage. Estimated delivery is on {DateTime.Now.AddDays(3)}.", notificationContent);
        }

        [TestMethod]
        public void PaymentConfirmationNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var paymentConfirmationNotification = new PaymentConfirmationNotification(1, DateTime.Now, 202, 303);

            // Act
            var notificationContent = paymentConfirmationNotification.Content;

            // Assert
            Assert.AreEqual("Thank you for your purchase! Your order: 303 for product: 202 has been successfully processed.", notificationContent);
        }

        [TestMethod]
        public void ProductRemovedNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var productRemovedNotification = new ProductRemovedNotification(1, DateTime.Now, 404);

            // Act
            var notificationContent = productRemovedNotification.Content;

            // Assert
            Assert.AreEqual("Unfortunately, the product: 404 that you were waiting for was removed from the marketplace.", notificationContent);
        }

        [TestMethod]
        public void ProductAvailableNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var productAvailableNotification = new ProductAvailableNotification(1, DateTime.Now, 505);

            // Act
            var notificationContent = productAvailableNotification.Content;

            // Assert
            Assert.AreEqual("Good news! The product: 505 that you were waiting for is now back in stock.", notificationContent);
        }

        [TestMethod]
        public void ContractRenewalRequestNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var contractRenewalRequestNotification = new ContractRenewalRequestNotification(1, DateTime.Now, 606);

            // Act
            var notificationContent = contractRenewalRequestNotification.Content;

            // Assert
            Assert.AreEqual("User 1 would like to renew contract: 606. Please respond promptly.", notificationContent);
        }

        [TestMethod]
        public void ContractExpirationNotification_ShouldReturnCorrectContent()
        {
            // Arrange
            var contractExpirationNotification = new ContractExpirationNotification(1, DateTime.Now, 707, DateTime.Now.AddDays(10));

            // Act
            var notificationContent = contractExpirationNotification.Content;

            // Assert
            Assert.AreEqual($"Contract: 707 is set to expire on {DateTime.Now.AddDays(10)}.", notificationContent);
        }
    }
}