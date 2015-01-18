namespace IPFilter.ViewModels
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Implements a command that when executed runs a delegate action.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        readonly Action<object> action;
        readonly Func<object, bool> canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="action">The action to run when the command is executed.</param>
        public DelegateCommand(Action<object> action)
        {
            if (action == null) throw new ArgumentNullException("action");
            this.action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="action">The action to run when the command is executed.</param>
        /// <param name="canExecute">The function to call to evaluate when the command can be executed.</param>
        public DelegateCommand(Action<object> action, Func<object, bool> canExecute)
            : this(action)
        {
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            if (CanExecute(parameter)) action(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}