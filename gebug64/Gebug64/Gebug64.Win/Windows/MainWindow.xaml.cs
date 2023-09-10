using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using Gebug64.Win.Mvvm;
using Gebug64.Win.ViewModels;

namespace Gebug64.Win.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ICloseable
    {
        private readonly Dispatcher _dispatcher;
        private readonly MainWindowViewModel _vm;

        private int _imageZoomLittleScrollIndex = 0;
        private int _imageZoomBigScrollIndex = 0;

        private Point _panMousePoint;
        private double _verticalPanOffset = 1;
        private double _horizontalPanOffset = 1;

        public MainWindow(MainWindowViewModel vm)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

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

        /// <summary>
        /// When the main window closes, close any other opened windows.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Workspace.Instance.CloseWindows<ErrorWindow>();
        }
    }
}
