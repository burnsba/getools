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
    public class MenuItemViewModel : ViewModelBase, IIsCheckedabled
    {
        private bool _isChecked = false;
        private bool _isEnabled = true;

        public MenuItemViewModel()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; init; }
        public string Header { get; set; }
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }
        public bool IsCheckable { get; set; }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        public ICommand Command { get; set; }
        public object Value { get; set; }
    }
}
