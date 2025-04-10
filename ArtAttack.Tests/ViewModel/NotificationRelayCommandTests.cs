using ArtAttack.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ArtAttack.Tests.ViewModel
{
    [TestClass]
    public class NotificationRelayCommandTests
    {
        [TestMethod]
        public void Constructor_WithExecuteAction_InitializesCommand()
        {
            // Arrange
            Action testAction = () => { };

            // Act
            var command = new NotificationRelayCommand(testAction);

            // Assert
            Assert.IsNotNull(command);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullExecuteAction_ThrowsArgumentNullException()
        {
            // Act & Assert
            _ = new NotificationRelayCommand(null);
        }

        [TestMethod]
        public void Constructor_WithExecuteActionAndCanExecute_InitializesCommand()
        {
            // Arrange
            Action testAaction = () => { };
            Func<bool> canExecute = () => true;

            // Act
            var command = new NotificationRelayCommand(testAaction, canExecute);

            // Assert
            Assert.IsNotNull(command);
        }

        [TestMethod]
        public void CanExecute_WhenCanExecuteIsNull_ReturnsTrue()
        {
            // Arrange
            Action testAaction = () => { };
            var command = new NotificationRelayCommand(testAaction);

            // Act
            bool result = command.CanExecute(null);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanExecute_WhenCanExecuteReturnsTrue_ReturnsTrue()
        {
            // Arrange
            Action testAaction = () => { };
            Func<bool> canExecute = () => true;
            var command = new NotificationRelayCommand(testAaction, canExecute);

            // Act
            bool result = command.CanExecute(null);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanExecute_WhenCanExecuteReturnsFalse_ReturnsFalse()
        {
            // Arrange
            Action testAaction = () => { };
            Func<bool> canExecute = () => false;
            var command = new NotificationRelayCommand(testAaction, canExecute);

            // Act
            bool result = command.CanExecute(null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Execute_CallsExecuteAction()
        {
            // Arrange
            bool actionCalled = false;
            Action testAaction = () => { actionCalled = true; };
            var command = new NotificationRelayCommand(testAaction);

            // Act
            command.Execute(null);

            // Assert
            Assert.IsTrue(actionCalled);
        }

        [TestMethod]
        public void RaiseCanExecuteChanged_TriggersCanExecuteChanged()
        {
            // Arrange
            Action testAaction = () => { };
            var command = new NotificationRelayCommand(testAaction);

            bool eventRaised = false;
            command.CanExecuteChanged += (sender, eventArguments) => { eventRaised = true; };

            // Act
            command.RaiseCanExecuteChanged();

            // Assert
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void RaiseCanExecuteChanged_WhenNoSubscribers_DoesNotThrow()
        {
            // Arrange
            Action testAaction = () => { };
            var command = new NotificationRelayCommand(testAaction);

            // Act & Assert (no exception should be thrown)
            command.RaiseCanExecuteChanged();
        }
    }

    [TestClass]
    public class NotificationRelayCommandGenericTests
    {
        [TestMethod]
        public void Constructor_WithExecuteAction_InitializesCommand()
        {
            // Arrange
            Action<string> testAaction = (sender) => { };

            // Act
            var command = new NotificationRelayCommand<string>(testAaction);

            // Assert
            Assert.IsNotNull(command);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullExecuteAction_ThrowsArgumentNullException()
        {
            // Act & Assert
            _ = new NotificationRelayCommand<string>(null);
        }

        [TestMethod]
        public void Constructor_WithExecuteActionAndCanExecute_InitializesCommand()
        {
            // Arrange
            Action<string> testAaction = (sender) => { };
            Func<string, bool> canExecute = (sender) => true;

            // Act
            var command = new NotificationRelayCommand<string>(testAaction, canExecute);

            // Assert
            Assert.IsNotNull(command);
        }

        [TestMethod]
        public void CanExecute_WhenCanExecuteIsNull_ReturnsTrue()
        {
            // Arrange
            Action<string> testAaction = (sender) => { };
            var command = new NotificationRelayCommand<string>(testAaction);

            // Act
            bool result = command.CanExecute("test");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanExecute_WhenCanExecuteReturnsTrue_ReturnsTrue()
        {
            // Arrange
            Action<string> testAaction = (sender) => { };
            Func<string, bool> canExecute = (sender) => true;
            var command = new NotificationRelayCommand<string>(testAaction, canExecute);

            // Act
            bool result = command.CanExecute("test");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanExecute_WhenCanExecuteReturnsFalse_ReturnsFalse()
        {
            // Arrange
            Action<string> testAaction = (sender) => { };
            Func<string, bool> canExecute = (sender) => false;
            var command = new NotificationRelayCommand<string>(testAaction, canExecute);

            // Act
            bool result = command.CanExecute("test");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Execute_CallsExecuteAction()
        {
            // Arrange
            string capturedParameter = null;
            Action<string> testAaction = (sender) => { capturedParameter = sender; };
            var command = new NotificationRelayCommand<string>(testAaction);

            // Act
            command.Execute("test");

            // Assert
            Assert.AreEqual("test", capturedParameter);
        }

        [TestMethod]
        public void RaiseCanExecuteChanged_TriggersCanExecuteChanged()
        {
            // Arrange
            Action<string> testAaction = (sender) => { };
            var command = new NotificationRelayCommand<string>(testAaction);

            bool eventRaised = false;
            command.CanExecuteChanged += (sender, eventArguments) => { eventRaised = true; };

            // Act
            command.RaiseCanExecuteChanged();

            // Assert
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void RaiseCanExecuteChanged_WhenNoSubscribers_DoesNotThrow()
        {
            // Arrange
            Action<string> testAaction = (sender) => { };
            var command = new NotificationRelayCommand<string>(testAaction);

            // Act & Assert (no exception should be thrown)
            command.RaiseCanExecuteChanged();
        }

        [TestMethod]
        public void Execute_WithNullParameter_CastsProperly()
        {
            // Arrange
            object capturedParameter = "not null";
            Action<object> testAaction = (caputerdValue) => { capturedParameter = caputerdValue; };
            var command = new NotificationRelayCommand<object>(testAaction);

            // Act - Should not throw an exception
            command.Execute(null);

            // Assert
            Assert.IsNull(capturedParameter);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void Execute_WithWrongParameterType_ThrowsInvalidCastException()
        {
            // Arrange
            Action<string> testAaction = (sender) => { };
            var command = new NotificationRelayCommand<string>(testAaction);

            // Act & Assert - Should throw exception
            command.Execute(123); // Integer cannot be cast to string
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void CanExecute_WithWrongParameterType_ThrowsInvalidCastException()
        {
            // Arrange
            Action<string> testAaction = (sender) => { };
            Func<string, bool> canExecute = (sender) => true;
            var command = new NotificationRelayCommand<string>(testAaction, canExecute);

            // Act & Assert - Should throw exception
            command.CanExecute(123); // Integer cannot be cast to string
        }
    }
}
