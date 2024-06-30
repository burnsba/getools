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

namespace Gebug64.Win.Windows.Mdi
{
    /// <summary>
    /// MDI Child window for Map.
    /// </summary>
    public partial class MapControl : UserControl
    {
        private readonly MapWindowViewModel _vm;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapControl"/> class.
        /// </summary>
        /// <param name="vm">Reference to main viewmodel.</param>
        public MapControl(MapWindowViewModel vm)
        {
            InitializeComponent();

            _vm = vm;

            DataContext = _vm;
        }
    }
}
