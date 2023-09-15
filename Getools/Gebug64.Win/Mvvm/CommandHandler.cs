using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gebug64.Win.Mvvm
{
    /// <summary>
    /// Command helper.
    /// </summary>
    public class CommandHandler : ICommand
    {
        private Action<object?>? _actionWithArgs;
        private Action? _actionEmpty;
        private bool _actionHasArgs = false;

        private Func<object?, bool>? _canExecuteWithArgs;
        private Func<bool>? _canExecuteEmpty;
        private bool _executeHasArgs = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="action">Action to perform when the command is executed.</param>
        public CommandHandler(Action action)
        {
            _actionEmpty = action;
            _canExecuteEmpty = () => true;

            _actionHasArgs = false;
            _executeHasArgs = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="action">Action to perform when the command is executed.</param>
        public CommandHandler(Action<object?> action)
        {
            _actionWithArgs = action;
            _canExecuteEmpty = () => true;

            _actionHasArgs = true;
            _executeHasArgs = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="action">Action to perform when the command is executed.</param>
        /// <param name="canExecute">Function to determine if command can be executed.</param>
        public CommandHandler(Action action, Func<bool> canExecute)
        {
            _actionEmpty = action;
            _canExecuteEmpty = canExecute;

            _actionHasArgs = false;
            _executeHasArgs = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="action">Action to perform when the command is executed.</param>
        /// <param name="canExecute">Function to determine if command can be executed.</param>
        public CommandHandler(Action action, Func<object?, bool> canExecute)
        {
            _actionEmpty = action;
            _canExecuteWithArgs = canExecute;

            _actionHasArgs = false;
            _executeHasArgs = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="action">Action to perform when the command is executed.</param>
        /// <param name="canExecute">Function to determine if command can be executed.</param>
        public CommandHandler(Action<object?> action, Func<bool> canExecute)
        {
            _actionWithArgs = action;
            _canExecuteEmpty = canExecute;

            _actionHasArgs = true;
            _executeHasArgs = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="action">Action to perform when the command is executed.</param>
        /// <param name="canExecute">Function to determine if command can be executed.</param>
        public CommandHandler(Action<object?> action, Func<object?, bool> canExecute)
        {
            _actionWithArgs = action;
            _canExecuteWithArgs = canExecute;

            _actionHasArgs = true;
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
        public bool CanExecute(object? parameter)
        {
            if (_executeHasArgs)
            {
                return _canExecuteWithArgs!(parameter);
            }

            return _canExecuteEmpty!();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Optional action parameter.</param>
        public void Execute(object? parameter)
        {
            if (_actionHasArgs)
            {
                _actionWithArgs!(parameter);
            }
            else
            {
                _actionEmpty!();
            }
        }
    }
}
