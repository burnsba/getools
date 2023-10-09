using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gebug64.Win.ViewModels;

namespace Gebug64.Win.Controls
{
    /// <summary>
    /// Interaction logic for LogControl.xaml
    /// </summary>
    public partial class LogControl : UserControl
    {
        private readonly MainWindowViewModel _vm;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogControl"/> class.
        /// </summary>
        /// <param name="vm">Reference to main viewmodel.</param>
        public LogControl(MainWindowViewModel vm)
        {
            InitializeComponent();

            _vm = vm;

            DataContext = _vm;

            _vm.LogMessages.CollectionChanged += (a, b) =>
            {
                if (VisualTreeHelper.GetChildrenCount(LogBox) > 0)
                {
                    Border border = (Border)VisualTreeHelper.GetChild(LogBox, 0);
                    ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                    scrollViewer.ScrollToBottom();
                }
            };
        }
    }
}
