using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using ArtAttack.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class NotificationViewModelTests
    {
        private Mock<INotificationDataAdapter> mockNotificationDataAdapter;
        private NotificationViewModel notificationViewModel;
        private readonly int testUserId = 5;
        private List<Notification> mockNotifications;

        // Note: We need to create a factory method for the NotificationDataAdapter
        // to avoid direct instantiation in the ViewModel constructor
        private static class NotificationDataAdapterFactory
        {
            public static INotificationDataAdapter CreateAdapter(string connectionString)
            {
                // During tests, this would return our mock
                return null;
            }
        }

        [TestInitialize]
        public void Setup()
        {
            // Create mock data adapter
            mockNotificationDataAdapter = new Mock<INotificationDataAdapter>();

            // Sample notifications for testing
            mockNotifications = new List<Notification>
            {
                new ProductAvailableNotification(5, DateTime.Now, 101) { NotificationID = 1, IsRead = false },
                new ContractExpirationNotification(5, DateTime.Now, 201, DateTime.Now.AddMonths(1)) { NotificationID = 2, IsRead = true },
                new OutbiddedNotification(5, DateTime.Now, 301) { NotificationID = 3, IsRead = false }
            };

            // Setup the mock adapter
            mockNotificationDataAdapter.Setup(mockNotificationAdapter => mockNotificationAdapter.GetNotificationsForUser(testUserId))
                .Returns(mockNotifications);

            // Create fresh view model for each test
            notificationViewModel = new NotificationViewModel(testUserId);

            // Use reflection to replace the data adapter with our mock
            var field = typeof(NotificationViewModel).GetField("dataAdapter",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(notificationViewModel, mockNotificationDataAdapter.Object);

            // For tests, start with empty notifications and reset properties
            notificationViewModel.Notifications.Clear();
            notificationViewModel.IsLoading = false;
            notificationViewModel.UnreadCount = 0;
        }

        [TestMethod]
        public void Constructor_InitializesProperties()
        {
            // Assert
            Assert.IsNotNull(notificationViewModel.Notifications);
            Assert.IsNotNull(notificationViewModel.MarkAsReadCommand);
            Assert.IsFalse(notificationViewModel.IsLoading);
        }

        [TestMethod]
        public void Constructor_SetsCurrentUserIdCorrectly()
        {
            // Arrange - Use reflection to check private field
            var field = typeof(NotificationViewModel).GetField("currentUserId",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var userId = (int)field.GetValue(notificationViewModel);

            // Assert
            Assert.AreEqual(testUserId, userId);
        }

        [TestMethod]
        public async Task LoadNotificationsAsync_LoadsAndCountsUnreadNotifications()
        {
            // No need to clear invocations if we're using autoLoad = false

            // Act
            await notificationViewModel.LoadNotificationsAsync(testUserId);

            // Assert
            Assert.AreEqual(3, notificationViewModel.Notifications.Count);
            Assert.AreEqual(2, notificationViewModel.UnreadCount); // 2 out of 3 are unread
            Assert.IsFalse(notificationViewModel.IsLoading);
        }



        [TestMethod]
        public async Task LoadNotificationsAsync_SetsIsLoadingTrueWhileExecuting()
        {
            // Arrange
            bool loadingPropertyChanged = false;
            bool wasLoadingTrue = false;

            notificationViewModel.PropertyChanged += (sender, propertyChangedArguments) => {
                if (propertyChangedArguments.PropertyName == nameof(notificationViewModel.IsLoading))
                {
                    loadingPropertyChanged = true;
                    if (notificationViewModel.IsLoading)
                    {
                        wasLoadingTrue = true;
                    }
                }
            };

            // Act
            await notificationViewModel.LoadNotificationsAsync(testUserId);

            // Assert
            Assert.IsTrue(loadingPropertyChanged, "IsLoading property should have changed");
            Assert.IsTrue(wasLoadingTrue, "IsLoading should have been true during execution");
            Assert.IsFalse(notificationViewModel.IsLoading, "IsLoading should be false after completion");
        }



        [TestMethod]
        public async Task LoadNotificationsAsync_HandlesException()
        {
            // Arrange - Setup mock to throw
            mockNotificationDataAdapter.Setup(mockNotificationAdapter => mockNotificationAdapter.GetNotificationsForUser(testUserId))
                .Throws(new Exception("Test exception"));

            // Act - Should not throw
            await notificationViewModel.LoadNotificationsAsync(testUserId);

            // Assert
            Assert.AreEqual(0, notificationViewModel.Notifications.Count);
            Assert.AreEqual(0, notificationViewModel.UnreadCount);
            Assert.IsFalse(notificationViewModel.IsLoading);
        }

        [TestMethod]
        public async Task MarkAsReadAsync_CallsDataAdapter()
        {
            // Arrange
            int notificationId = 42;

            // Act
            await notificationViewModel.MarkAsReadAsync(notificationId);

            // Assert
            mockNotificationDataAdapter.Verify(mockNotificationAdapter => mockNotificationAdapter.MarkAsRead(notificationId), Times.Once);
        }

        [TestMethod]
        public async Task MarkAsReadAsync_HandlesException()
        {
            // Arrange
            int notificationId = 42;
            mockNotificationDataAdapter.Setup(mockNotificationAdapter => mockNotificationAdapter.MarkAsRead(notificationId))
                .Throws(new Exception("Test exception"));

            // Act - Should not throw an exception
            await notificationViewModel.MarkAsReadAsync(notificationId);

            // Assert
            mockNotificationDataAdapter.Verify(mockNotificationAdapter => mockNotificationAdapter.MarkAsRead(notificationId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNotificationAsync_ThrowsArgumentNullException_WhenNotificationIsNull()
        {
            // Act
            await notificationViewModel.AddNotificationAsync(null);
        }

        [TestMethod]
        public async Task AddNotificationAsync_CallsDataAdapter_AndUpdatesCollection_WhenRecipientMatches()
        {
            // Create a new view model with empty collection to ensure clean state
            var mockDataAdapter = new Mock<INotificationDataAdapter>();
            var viewModel = new NotificationViewModel(testUserId, false); // don't auto-load

            // Use reflection to replace adapter
            var field = typeof(NotificationViewModel).GetField("dataAdapter",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(viewModel, mockDataAdapter.Object);

            // Ensure collection is empty
            viewModel.Notifications = new ObservableCollection<Notification>();
            viewModel.UnreadCount = 0;

            // Arrange test data
            var notification = new ProductAvailableNotification(testUserId, DateTime.Now, 501) { NotificationID = 4 };
            bool popupInvoked = false;
            string popupMessage = null;

            viewModel.ShowPopup += (message) => {
                popupInvoked = true;
                popupMessage = message;
            };

            // Act
            await viewModel.AddNotificationAsync(notification);

            // Assert
            mockDataAdapter.Verify(mockNotificationAdapter => mockNotificationAdapter.AddNotification(notification), Times.Once);
            Assert.AreEqual(1, viewModel.Notifications.Count);
            Assert.AreSame(notification, viewModel.Notifications[0]);
            Assert.AreEqual(1, viewModel.UnreadCount);
            Assert.IsTrue(popupInvoked);
            Assert.AreEqual("Notification sent!", popupMessage);
        }

        [TestMethod]
        public async Task AddNotificationAsync_CallsDataAdapter_ButDoesNotUpdateCollection_WhenRecipientDoesNotMatch()
        {
            // Create a new view model with empty collection to ensure clean state
            var mockDataAdapter = new Mock<INotificationDataAdapter>();
            var viewModel = new NotificationViewModel(testUserId, false); // don't auto-load

            // Use reflection to replace adapter
            var field = typeof(NotificationViewModel).GetField("dataAdapter",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(viewModel, mockDataAdapter.Object);

            // Ensure collection is empty
            viewModel.Notifications = new ObservableCollection<Notification>();
            viewModel.UnreadCount = 0;

            // Arrange test data
            var notification = new ProductAvailableNotification(testUserId + 1, DateTime.Now, 501) { NotificationID = 4 };
            bool popupInvoked = false;

            viewModel.ShowPopup += (message) => { popupInvoked = true; };

            // Act
            await viewModel.AddNotificationAsync(notification);

            // Assert
            mockDataAdapter.Verify(mockNotificationAdapter => mockNotificationAdapter.AddNotification(notification), Times.Once);
            Assert.AreEqual(0, viewModel.Notifications.Count);
            Assert.AreEqual(0, viewModel.UnreadCount);
            Assert.IsFalse(popupInvoked);
        }


        [TestMethod]
        public async Task AddNotificationAsync_HandlesException()
        {
            // Create a new view model with empty collection to ensure clean state
            var mockDataAdapter = new Mock<INotificationDataAdapter>();
            var viewModel = new NotificationViewModel(testUserId, false); // don't auto-load

            // Use reflection to replace adapter
            var field = typeof(NotificationViewModel).GetField("dataAdapter",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(viewModel, mockDataAdapter.Object);

            // Ensure collection is empty
            viewModel.Notifications = new ObservableCollection<Notification>();
            viewModel.UnreadCount = 0;

            // Setup the test scenario
            var notification = new ProductAvailableNotification(testUserId, DateTime.Now, 501) { NotificationID = 4 };
            mockDataAdapter.Setup(mockNotificationAdapter => mockNotificationAdapter.AddNotification(It.IsAny<Notification>()))
                .Throws(new Exception("Test exception"));

            // Act
            await viewModel.AddNotificationAsync(notification);

            // Assert
            mockDataAdapter.Verify(mockNotificationAdapter => mockNotificationAdapter.AddNotification(It.IsAny<Notification>()), Times.Once);
            Assert.AreEqual(0, viewModel.Notifications.Count);
        }


        [TestMethod]
        public void Notifications_PropertyChangeNotification()
        {
            // Arrange
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            notificationViewModel.PropertyChanged += (sender, propertyChangedArguments) => {
                propertyChangedRaised = true;
                changedPropertyName = propertyChangedArguments.PropertyName;
            };

            // Act
            notificationViewModel.Notifications = new ObservableCollection<Notification>();

            // Assert
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(nameof(notificationViewModel.Notifications), changedPropertyName);
        }

        [TestMethod]
        public void OnPropertyChanged_RaisesPropertyChangedEvent()
        {
            // Arrange
            string propertyChangedName = null;
            notificationViewModel.PropertyChanged += (sender, propertyChangedArguments) => { propertyChangedName = propertyChangedArguments.PropertyName; };

            // Act
            notificationViewModel.IsLoading = true;

            // Assert
            Assert.AreEqual(nameof(notificationViewModel.IsLoading), propertyChangedName);
        }

        [TestMethod]
        public void UnReadNotificationsCountText_ReturnsCorrectFormat()
        {
            // Arrange
            notificationViewModel.UnreadCount = 5;

            // Act
            string result = notificationViewModel.UnReadNotificationsCountText;

            // Assert
            Assert.AreEqual("You've got #5 unread notifications.", result);
        }

        [TestMethod]
        public void UnreadCount_TriggersPropertyChanged_ForBothUnreadCountAndText()
        {
            // Arrange
            var propertyChangedNames = new List<string>();
            notificationViewModel.PropertyChanged += (sender, propertyChangedArguments) => { propertyChangedNames.Add(propertyChangedArguments.PropertyName); };

            // Act
            notificationViewModel.UnreadCount = 10;

            // Assert
            Assert.AreEqual(2, propertyChangedNames.Count);
            Assert.AreEqual(nameof(notificationViewModel.UnreadCount), propertyChangedNames[0]);
            Assert.AreEqual(nameof(notificationViewModel.UnReadNotificationsCountText), propertyChangedNames[1]);
        }

        [TestMethod]
        public void MarkAsReadCommand_CanExecute()
        {
            // Assert
            Assert.IsTrue(notificationViewModel.MarkAsReadCommand.CanExecute(42));
        }

        [TestMethod]
        public async Task MarkAsReadCommand_ExecutesMarkAsReadAsync()
        {
            // Arrange
            int notificationId = 42;
            var taskCompletionSource = new TaskCompletionSource<bool>();

            // Setup the mock to signal when MarkAsRead is called
            mockNotificationDataAdapter
                .Setup(mockNotificationAdapter => mockNotificationAdapter.MarkAsRead(notificationId))
                .Callback(() => taskCompletionSource.SetResult(true));

            // Act
            notificationViewModel.MarkAsReadCommand.Execute(notificationId);

            // Wait for the command to complete with a timeout
            var completedTask = await Task.WhenAny(taskCompletionSource.Task, Task.Delay(3000));

            // Assert
            if (completedTask == taskCompletionSource.Task)
            {
                mockNotificationDataAdapter.Verify(mockNotificationAdapter => mockNotificationAdapter.MarkAsRead(notificationId), Times.Once);
            }
            else
            {
                Assert.Fail("Command execution timed out after 3 seconds");
            }
        }
    }

    // Wrapper class for testing the private UpdateUnreadCount method if needed
    public class TestableNotificationViewModel : NotificationViewModel
    {
        public TestableNotificationViewModel(int currentUserId) : base(currentUserId)
        {
        }

        public void TestUpdateUnreadCount()
        {
            // Call private method through reflection since it's not part of public API
            var methodInfo = typeof(NotificationViewModel).GetMethod("UpdateUnreadCount",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo != null)
            {
                methodInfo.Invoke(this, null);
            }
        }
    }
}
