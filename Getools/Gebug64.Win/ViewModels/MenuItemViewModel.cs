using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gebug64.Win.Mvvm;
using Gebug64.Win.Ui;

namespace Gebug64.Win.ViewModels
{
    /// <summary>
    /// Helper viewmodel for window menu items.
    /// </summary>
    public class MenuItemViewModel : ViewModelBase, IIsCheckedabled
    {
        private bool _isChecked = false;
        private bool _isEnabled = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItemViewModel"/> class.
        /// </summary>
        public MenuItemViewModel()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Unique identifier.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Optional display text.
        /// </summary>
        public string? Header { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the menu item has a checkmark.
        /// </summary>
        public bool IsChecked
        {
            get => _isChecked;

            set
            {
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the menu item can be checked.
        /// </summary>
        public bool IsCheckable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the menu item is enabled or not.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;

            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        /// <summary>
        /// Gets or sets the menu item command.
        /// </summary>
        public ICommand? Command { get; set; }

        /// <summary>
        /// Gets or sets an associated value on the menu item.
        /// </summary>
        public object? Value { get; set; }
    }
}
