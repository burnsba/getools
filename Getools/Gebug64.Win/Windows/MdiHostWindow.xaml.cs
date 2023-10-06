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
using System.Windows.Shapes;
using Gebug64.Win.Controls;
using Gebug64.Win.ViewModels;
using WPF.MDI;

namespace Gebug64.Win.Windows
{
    /// <summary>
    /// Interaction logic for MdiHostWindow.xaml
    /// </summary>
    public partial class MdiHostWindow : Window
    {
        private MdiHostViewModel _vm;

        public MdiHostWindow(MdiHostViewModel vm)
        {
            InitializeComponent();

            _vm = vm;

            _vm.SetFocusChildCallback(FocusCreateChild);

            DataContext = _vm;

            Container.Children.CollectionChanged += (o, e) => Menu_RefreshWindows();

            Menu_RefreshWindows();
        }

        public void FocusCreateChild(Type controlType, string title)
        {
            foreach (var child in Container.Children)
            {
                if (child.Content.GetType() == controlType)
                {
                    child.Focus();
                    return;
                }
            }

            var control = (Control)Workspace.Instance.ServiceProvider.GetService(controlType)!;

            var newChild = new MdiChild()
            {
                Content = control,
                Title = title,
            };

            Container.Children.Add(newChild);
        }

        public void AddPermanentMdiChild(Control content, string title, Action<MdiChild>? configCallback = null)
        {
            var child = new MdiChild()
            {
                Content = content,
                Title = title,
            };

            if (configCallback != null)
            {
                configCallback(child);
            }

            child.CloseBox = false;

            Container.Children.Add(child);
        }

        /// <summary>
        /// Refresh windows list
        /// </summary>
        private void Menu_RefreshWindows()
        {
            WindowsMenu.Items.Clear();
            MenuItem mi;
            for (int i = 0; i < Container.Children.Count; i++)
            {
                MdiChild child = Container.Children[i];
                mi = new MenuItem { Header = child.Title };
                mi.Click += (o, e) => child.Focus();
                WindowsMenu.Items.Add(mi);
            }

            WindowsMenu.Items.Add(new Separator());
            WindowsMenu.Items.Add(mi = new MenuItem { Header = "Minimize All" });
            mi.Click += (o, e) => Container.Children.ToList().ForEach(x => x.WindowState = WindowState.Minimized);

            WindowsMenu.Items.Add(mi = new MenuItem { Header = "Cascade" });
            mi.Click += (o, e) => Container.MdiLayout = MdiLayout.Cascade;
            WindowsMenu.Items.Add(mi = new MenuItem { Header = "Horizontally" });
            mi.Click += (o, e) => Container.MdiLayout = MdiLayout.TileHorizontal;
            WindowsMenu.Items.Add(mi = new MenuItem { Header = "Vertically" });
            mi.Click += (o, e) => Container.MdiLayout = MdiLayout.TileVertical;

            WindowsMenu.Items.Add(new Separator());
            WindowsMenu.Items.Add(mi = new MenuItem { Header = "Close all" });
            mi.Click += (o, e) =>
            {
                var removeChildren = Container.Children.Where(x => x.CloseBox).ToList();
                foreach (var child in removeChildren)
                {
                    Container.Children.Remove(child);
                }
            };
        }
    }
}
