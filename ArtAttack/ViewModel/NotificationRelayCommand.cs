using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace ArtAttack.ViewModel
{
    public class NotificationRelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public event EventHandler CanExecuteChanged;

        [ExcludeFromCodeCoverage]
        public NotificationRelayCommand(Action execute)
           : this(execute, null)
        {
        }
        public NotificationRelayCommand(Action execute, Func<bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        [ExcludeFromCodeCoverage]
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }
        [ExcludeFromCodeCoverage]
        public void Execute(object parameter)
        {
            execute();
        }
        [ExcludeFromCodeCoverage]
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class NotificationRelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Func<T, bool> canExecute;

        public event EventHandler CanExecuteChanged;

        public NotificationRelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public NotificationRelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }
        [ExcludeFromCodeCoverage]
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute((T)parameter);
        }
        [ExcludeFromCodeCoverage]
        public void Execute(object parameter)
        {
            execute((T)parameter);
        }
        [ExcludeFromCodeCoverage]
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
