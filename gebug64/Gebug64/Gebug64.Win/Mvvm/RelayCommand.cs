using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gebug64.Win.Mvvm
{
    /// <summary>
    /// Command that accepts parameter.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private Action<T>? _actionWithArgs;

        private Func<T, bool>? _canExecuteWithArgs;
        private Func<bool>? _canExecuteEmpty;
        private bool _executeHasArgs = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// </summary>
        /// <param name="action">Action to perform when the command is executed.</param>
        public RelayCommand(Action<T> action)
        {
            _actionWithArgs = action;
            _canExecuteEmpty = () => true;

            _executeHasArgs = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// </summary>
        /// <param name="action">Action to perform when the command is executed.</param>
        /// <param name="canExecute">Function to determine if command can be executed.</param>
        public RelayCommand(Action<T> action, Func<bool> canExecute)
        {
            _actionWithArgs = action;
            _canExecuteEmpty = canExecute;

            _executeHasArgs = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// </summary>
        /// <param name="action">Action to perform when the command is executed.</param>
        /// <param name="canExecute">Function to determine if command can be executed.</param>
        public RelayCommand(Action<T> action, Func<T, bool> canExecute)
        {
            _actionWithArgs = action;
            _canExecuteWithArgs = canExecute;

            _executeHasArgs = true;
        }

        /// <summary>
        /// Event handler.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Gets a value indicating whether the command can be performed.
        /// </summary>
        /// <param name="parameter">Optional CanExecute function parameter.</param>
        /// <returns>Whether command can be performed.</returns>
        public bool CanExecute(T parameter)
        {
            if (_executeHasArgs)
            {
                return _canExecuteWithArgs!(parameter);
            }

            return _canExecuteEmpty!();
        }

        /// <summary>
        /// Gets a value indicating whether the command can be performed.
        /// </summary>
        /// <param name="parameter">Optional CanExecute function parameter.</param>
        /// <returns>Whether command can be performed.</returns>
        public bool CanExecute(object? parameter)
        {
            if (_executeHasArgs && !object.ReferenceEquals(null, parameter))
            {
                return _canExecuteWithArgs!((T)parameter);
            }

            return _canExecuteEmpty!();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Optional action parameter.</param>
        public void Execute(T parameter)
        {
            if (object.ReferenceEquals(null, parameter))
            {
                throw new NullReferenceException(nameof(parameter));
            }

            _actionWithArgs!(parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Optional action parameter.</param>
        public void Execute(object? parameter)
        {
            if (object.ReferenceEquals(null, parameter))
            {
                throw new NullReferenceException(nameof(parameter));
            }

            _actionWithArgs!((T)parameter);
        }
    }
}
