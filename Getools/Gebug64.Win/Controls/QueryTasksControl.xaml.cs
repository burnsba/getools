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
    /// Interaction logic for QueryTasksControl.xaml
    /// </summary>
    public partial class QueryTasksControl : UserControl
    {
        private readonly MainWindowViewModel _vm;

        public QueryTasksControl(MainWindowViewModel vm)
        {
            InitializeComponent();

            _vm = vm;

            DataContext = _vm;
        }
    }
}
