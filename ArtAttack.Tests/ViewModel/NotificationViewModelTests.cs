//using ArtAttack.Domain;
//using ArtAttack.ViewModel;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Input;

//namespace ArtAttack.Tests.ViewModel
//{
//    [TestClass]
//    public class NotificationViewModelTests
//    {
//        private Mock<INotificationDataAdapter> mockDataAdapter;
//        private NotificationViewModel viewModel;
//        private int testUserId = 1;
//        private List<Notification> testNotifications;

//        [TestInitialize]
//        public void Setup()
//        {
//            // Setup mock notifications
//            testNotifications = new List<Notification>
//                {
//                    CreateMockNotification(1, testUserId, false),
//                    CreateMockNotification(2, testUserId, true),
//                    CreateMockNotification(3, testUserId, false)
//                };

//            // Setup mock data adapter
//            mockDataAdapter = new Mock<INotificationDataAdapter>();
//            mockDataAdapter.Setup(m => m.GetNotificationsForUser(testUserId))
//                .Returns(testNotifications);

//            // Create a helper method to create the view model with the mock adapter
//            CreateViewModelWithMockAdapter();
//        }

//        private void CreateViewModelWithMockAdapter()
//        {
//            // Inject mock adapter with a custom constructor that overrides the real one
//            viewModel = new TestNotificationViewModel(testUserId, mockDataAdapter.Object);
//        }

//        [TestMethod]
//        public async Task LoadNotificationsAsync_ShouldLoadNotificationsAndSetUnreadCount()
//        {
//            // Act
//            await viewModel.LoadNotificationsAsync(testUserId);

//            // Assert
//            Assert.AreEqual(3, viewModel.Notifications.Count, "Should load all notifications");
//            Assert.AreEqual(2, viewModel.UnreadCount, "Should correctly count unread notifications");
//            Assert.AreEqual("You've got #2 unread notifications.", viewModel.UnReadNotificationsCountText);
//            mockDataAdapter.Verify(m => m.GetNotificationsForUser(testUserId), Times.Once);
//        }

//        [TestMethod]
//        public async Task LoadNotificationsAsync_ShouldHandleExceptionGracefully()
//        {
//            // Arrange
//            mockDataAdapter.Setup(m => m.GetNotificationsForUser(testUserId))
//                .Throws(new Exception("Test exception"));

//            // Act - This should not throw
//            await viewModel.LoadNotificationsAsync(testUserId);

//            // Assert
//            Assert.IsFalse(viewModel.IsLoading, "IsLoading should be reset to false even when exception occurs");
//        }

//        [TestMethod]
//        public async Task MarkAsReadAsync_ShouldCallAdapterCorrectly()
//        {
//            // Arrange
//            int notificationId = 1;

//            // Act
//            await viewModel.MarkAsReadAsync(notificationId);

//            // Assert
//            mockDataAdapter.Verify(m => m.MarkAsRead(notificationId), Times.Once);
//        }

//        [TestMethod]
//        public async Task MarkAsReadAsync_ShouldHandleExceptionGracefully()
//        {
//            // Arrange
//            int notificationId = 1;
//            mockDataAdapter.Setup(m => m.MarkAsRead(notificationId))
//                .Throws(new Exception("Test exception"));

//            // Act - This should not throw
//            await viewModel.MarkAsReadAsync(notificationId);

//            // No assert needed - just verifying no exception is thrown
//        }

//        [TestMethod]
//        public async Task AddNotificationAsync_ShouldAddToLocalCollectionWhenRecipientMatches()
//        {
//            // Arrange
//            var notification = CreateMockNotification(4, testUserId, false);
//            bool popupShown = false;
//            viewModel.ShowPopup += (message) => popupShown = true;

//            // Act
//            await viewModel.AddNotificationAsync(notification);

//            // Assert
//            mockDataAdapter.Verify(m => m.AddNotification(notification), Times.Once);
//            Assert.AreEqual(1, viewModel.Notifications.Count, "Should add to local collection");
//            Assert.AreEqual(1, viewModel.UnreadCount, "Should increment unread count");
//            Assert.IsTrue(popupShown, "Should show popup");
//        }

//        [TestMethod]
//        public async Task AddNotificationAsync_ShouldNotAddToLocalCollectionWhenRecipientDiffers()
//        {
//            // Arrange
//            var notification = CreateMockNotification(4, testUserId + 1, false); // Different recipient
//            bool popupShown = false;
//            viewModel.ShowPopup += (message) => popupShown = true;

//            // Act
//            await viewModel.AddNotificationAsync(notification);

//            // Assert
//            mockDataAdapter.Verify(m => m.AddNotification(notification), Times.Once);
//            Assert.AreEqual(0, viewModel.Notifications.Count, "Should not add to local collection");
//            Assert.AreEqual(0, viewModel.UnreadCount, "Should not increment unread count");
//            Assert.IsFalse(popupShown, "Should not show popup");
//        }

//        [TestMethod]
//        public async Task AddNotificationAsync_ShouldHandleExceptionGracefully()
//        {
//            // Arrange
//            var notification = CreateMockNotification(4, testUserId, false);
//            mockDataAdapter.Setup(m => m.AddNotification(notification))
//                .Throws(new Exception("Test exception"));

//            // Act - This should not throw
//            await viewModel.AddNotificationAsync(notification);

//            // Assert
//            Assert.AreEqual(0, viewModel.Notifications.Count, "Should not add to local collection on error");
//        }

//        [TestMethod]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public async Task AddNotificationAsync_ShouldThrowArgumentNullExceptionWhenNotificationIsNull()
//        {
//            // Act
//            await viewModel.AddNotificationAsync(null);

//            // Assert: ExpectedException attribute handles this
//        }

//        [TestMethod]
//        public void MarkAsReadCommand_ShouldBeInitialized()
//        {
//            // Assert
//            Assert.IsNotNull(viewModel.MarkAsReadCommand, "MarkAsReadCommand should be initialized");
//            Assert.IsInstanceOfType(viewModel.MarkAsReadCommand, typeof(ICommand), "MarkAsReadCommand should implement ICommand");
//        }

//        [TestMethod]
//        public void PropertyChanged_ShouldBeTriggeredWhenPropertiesChange()
//        {
//            // Arrange
//            string propertyChanged = null;
//            viewModel.PropertyChanged += (sender, e) => propertyChanged = e.PropertyName;

//            // Act - Test each property
//            viewModel.IsLoading = true;
//            Assert.AreEqual("IsLoading", propertyChanged);

//            propertyChanged = null;
//            viewModel.UnreadCount = 5;

//            // UnreadCount changes both UnreadCount and UnReadNotificationsCountText
//            Assert.AreEqual("UnReadNotificationsCountText", propertyChanged);

//            propertyChanged = null;
//            viewModel.Notifications = new System.Collections.ObjectModel.ObservableCollection<Notification>();
//            Assert.AreEqual("Notifications", propertyChanged);
//        }

//        [TestMethod]
//        public void UpdateUnreadCount_ShouldRecalculateUnreadCount()
//        {
//            // Arrange
//            var notifications = new List<Notification>
//                {
//                    CreateMockNotification(1, testUserId, false),
//                    CreateMockNotification(2, testUserId, true),
//                    CreateMockNotification(3, testUserId, false)
//                };
//            viewModel.Notifications = new System.Collections.ObjectModel.ObservableCollection<Notification>(notifications);

//            // Act
//            // Call the private method using reflection
//            var methodInfo = typeof(NotificationViewModel).GetMethod("UpdateUnreadCount",
//                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//            methodInfo.Invoke(viewModel, null);

//            // Assert
//            Assert.AreEqual(2, viewModel.UnreadCount, "Should correctly count unread notifications");
//        }

//        [TestMethod]
//        public void Constructor_ShouldInitializePropertiesAndStartLoading()
//        {
//            // Constructor already called in setup, so just verify state
//            Assert.IsNotNull(viewModel.Notifications);
//            Assert.IsNotNull(viewModel.MarkAsReadCommand);
//        }

//        // Helper method to create mock notifications
//        private Notification CreateMockNotification(int id, int recipientId, bool isRead)
//        {
//            var mockNotification = new Mock<Notification>();
//            mockNotification.Setup(n => n.NotificationID).Returns(id);
//            mockNotification.Setup(n => n.RecipientID).Returns(recipientId);
//            mockNotification.Setup(n => n.IsRead).Returns(isRead);

//            // Setup a proper Title/Content/Subtitle implementation
//            mockNotification.Setup(n => n.Title).Returns("Test Title");
//            mockNotification.Setup(n => n.Content).Returns("Test Content");
//            mockNotification.Setup(n => n.Subtitle).Returns("Test Subtitle");
//            mockNotification.Setup(n => n.Category).Returns(NotificationCategory.PAYMENT_CONFIRMATION);

//            return mockNotification.Object;
//        }

//        // Helper class to inject mock dependencies
//        private class TestNotificationViewModel : NotificationViewModel
//        {
//            public TestNotificationViewModel(int userId, INotificationDataAdapter mockAdapter)
//                : base(userId)
//            {
//                // Use reflection to set the private field
//                var field = typeof(NotificationViewModel).GetField("dataAdapter",
//                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//                field.SetValue(this, mockAdapter);
//            }
//        }
//    }
//}
