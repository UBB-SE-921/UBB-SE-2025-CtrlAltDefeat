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
            Action action = () => { };

            // Act
            var command = new NotificationRelayCommand(action);

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
            Action action = () => { };
            Func<bool> canExecute = () => true;

            // Act
            var command = new NotificationRelayCommand(action, canExecute);

            // Assert
            Assert.IsNotNull(command);
        }

        [TestMethod]
        public void CanExecute_WhenCanExecuteIsNull_ReturnsTrue()
        {
            // Arrange
            Action action = () => { };
            var command = new NotificationRelayCommand(action);

            // Act
            bool result = command.CanExecute(null);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanExecute_WhenCanExecuteReturnsTrue_ReturnsTrue()
        {
            // Arrange
            Action action = () => { };
            Func<bool> canExecute = () => true;
            var command = new NotificationRelayCommand(action, canExecute);

            // Act
            bool result = command.CanExecute(null);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanExecute_WhenCanExecuteReturnsFalse_ReturnsFalse()
        {
            // Arrange
            Action action = () => { };
            Func<bool> canExecute = () => false;
            var command = new NotificationRelayCommand(action, canExecute);

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
            Action action = () => { actionCalled = true; };
            var command = new NotificationRelayCommand(action);

            // Act
            command.Execute(null);

            // Assert
            Assert.IsTrue(actionCalled);
        }

        [TestMethod]
        public void RaiseCanExecuteChanged_TriggersCanExecuteChanged()
        {
            // Arrange
            Action action = () => { };
            var command = new NotificationRelayCommand(action);

            bool eventRaised = false;
            command.CanExecuteChanged += (s, e) => { eventRaised = true; };

            // Act
            command.RaiseCanExecuteChanged();

            // Assert
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void RaiseCanExecuteChanged_WhenNoSubscribers_DoesNotThrow()
        {
            // Arrange
            Action action = () => { };
            var command = new NotificationRelayCommand(action);

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
            Action<string> action = (s) => { };

            // Act
            var command = new NotificationRelayCommand<string>(action);

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
            Action<string> action = (s) => { };
            Func<string, bool> canExecute = (s) => true;

            // Act
            var command = new NotificationRelayCommand<string>(action, canExecute);

            // Assert
            Assert.IsNotNull(command);
        }

        [TestMethod]
        public void CanExecute_WhenCanExecuteIsNull_ReturnsTrue()
        {
            // Arrange
            Action<string> action = (s) => { };
            var command = new NotificationRelayCommand<string>(action);

            // Act
            bool result = command.CanExecute("test");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanExecute_WhenCanExecuteReturnsTrue_ReturnsTrue()
        {
            // Arrange
            Action<string> action = (s) => { };
            Func<string, bool> canExecute = (s) => true;
            var command = new NotificationRelayCommand<string>(action, canExecute);

            // Act
            bool result = command.CanExecute("test");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanExecute_WhenCanExecuteReturnsFalse_ReturnsFalse()
        {
            // Arrange
            Action<string> action = (s) => { };
            Func<string, bool> canExecute = (s) => false;
            var command = new NotificationRelayCommand<string>(action, canExecute);

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
            Action<string> action = (s) => { capturedParameter = s; };
            var command = new NotificationRelayCommand<string>(action);

            // Act
            command.Execute("test");

            // Assert
            Assert.AreEqual("test", capturedParameter);
        }

        [TestMethod]
        public void RaiseCanExecuteChanged_TriggersCanExecuteChanged()
        {
            // Arrange
            Action<string> action = (s) => { };
            var command = new NotificationRelayCommand<string>(action);

            bool eventRaised = false;
            command.CanExecuteChanged += (s, e) => { eventRaised = true; };

            // Act
            command.RaiseCanExecuteChanged();

            // Assert
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void RaiseCanExecuteChanged_WhenNoSubscribers_DoesNotThrow()
        {
            // Arrange
            Action<string> action = (s) => { };
            var command = new NotificationRelayCommand<string>(action);

            // Act & Assert (no exception should be thrown)
            command.RaiseCanExecuteChanged();
        }

        [TestMethod]
        public void Execute_WithNullParameter_CastsProperly()
        {
            // Arrange
            object capturedParameter = "not null";
            Action<object> action = (obj) => { capturedParameter = obj; };
            var command = new NotificationRelayCommand<object>(action);

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
            Action<string> action = (s) => { };
            var command = new NotificationRelayCommand<string>(action);

            // Act & Assert - Should throw exception
            command.Execute(123); // Integer cannot be cast to string
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void CanExecute_WithWrongParameterType_ThrowsInvalidCastException()
        {
            // Arrange
            Action<string> action = (s) => { };
            Func<string, bool> canExecute = (s) => true;
            var command = new NotificationRelayCommand<string>(action, canExecute);

            // Act & Assert - Should throw exception
            command.CanExecute(123); // Integer cannot be cast to string
        }
    }
}
