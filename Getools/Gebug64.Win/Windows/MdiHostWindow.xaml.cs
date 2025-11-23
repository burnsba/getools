using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Gebug64.Win.Controls;
using Gebug64.Win.Extensions;
using Gebug64.Win.ViewModels;
using Gebug64.Win.ViewModels.CategoryTabs;
using Gebug64.Win.ViewModels.Config;
using Gebug64.Win.Windows.Mdi;
using Microsoft.Extensions.DependencyInjection;
using WpfMdiCore;

namespace Gebug64.Win.Windows
{
    /// <summary>
    /// Interaction logic for MdiHostWindow.xaml
    /// </summary>
    public partial class MdiHostWindow : Window, ILayoutWindow
    {
        private const int _saveUiStateDelay = 1000; // ms

        private readonly string _typeName;

        private MdiHostViewModel _vm;
        private AppConfigViewModel _appConfig;

        private bool _ready = false;

        private object _timerLock = new object();
        private bool _saveUiStateTimerActive = false;
        private System.Timers.Timer _saveUiStateTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MdiHostWindow"/> class.
        /// </summary>
        /// <param name="vm">Main host viewmodel.</param>
        public MdiHostWindow(MdiHostViewModel vm)
        {
            _typeName = GetType().FullName!;

            InitializeComponent();

            _appConfig = (AppConfigViewModel)Workspace.Instance.ServiceProvider.GetService(typeof(AppConfigViewModel))!;

            _saveUiStateTimer = new System.Timers.Timer();
            _saveUiStateTimer.Interval = _saveUiStateDelay; // ms
            _saveUiStateTimer.Enabled = false;
            _saveUiStateTimer.AutoReset = false;
            _saveUiStateTimer.Elapsed += NotifySaveUiState;

            _vm = vm;

            _vm.SetFocusChildCallback(FocusCreateChild);

            DataContext = _vm;

            Container.Children.CollectionChanged += (o, e) => Menu_RefreshWindows();

            Menu_RefreshWindows();
        }

        /// <inheritdoc />
        public string TypeName => _typeName;

        /// <summary>
        /// Helper method to focus on a child control, or create it if it doesn't exist.
        /// </summary>
        /// <param name="controlType">Internal <see cref="MdiChild.Content"/> type.</param>
        /// <param name="title">Optional. Title of child window; only used if the child is created.</param>
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

            if (typeof(ILayoutWindow).IsAssignableFrom(controlType))
            {
                newChild.TryLoadWindowLayoutState(_appConfig);
            }

            newChild.Resize += _vm.ResizeChildHandler;
            newChild.Move += _vm.MoveChildHandler;
            newChild.Closed += _vm.CloseChildHandler;

            Container.Children.Add(newChild);
        }

        /// <summary>
        /// Unconditionally creates a new <see cref="MdiChild"/> with the given content, then
        /// disables <see cref="MdiChild.CloseBox"/>.
        /// </summary>
        /// <param name="content">Internal <see cref="MdiChild.Content"/> type.</param>
        /// <param name="title">Title of child window.</param>
        /// <param name="configCallback">Optional callback. Called with newly created <see cref="MdiChild"/>. Use to set config parameters, etc.</param>
        public void AddPermanentMdiChild(Control content, string title, Action<MdiChild>? configCallback = null)
        {
            var child = new MdiChild()
            {
                Content = content,
                Title = title,
            };

            if (content.MinHeight > 0)
            {
                child.MinHeight = content.MinHeight;
            }

            if (content.MinWidth > 0)
            {
                child.MinWidth = content.MinWidth;
            }

            child.Resizable = true;

            if (configCallback != null)
            {
                configCallback(child);
            }

            child.CloseBox = false;

            if (typeof(ILayoutWindow).IsAssignableFrom(content.GetType()))
            {
                child.TryLoadWindowLayoutState(_appConfig);
            }

            child.Resize += _vm.ResizeChildHandler;
            child.Move += _vm.MoveChildHandler;
            child.Closed += _vm.CloseChildHandler;

            Container.Children.Add(child);
        }

        /// <summary>
        /// Sets a flag to allow UI state changes to write to the appconfig file.
        /// (That is, startup/setup is ignored and won't write to appconfig).
        /// </summary>
        public void DoneInit()
        {
            _ready = true;
            _vm.DoneInit();
        }

        /// <summary>
        /// Load any saved child windows from the appconfig and restore them to the prior
        /// position and window state.
        /// </summary>
        public void LoadSavedChildLayout()
        {
            var configWindowTypeNames = _appConfig.LayoutState.Windows.Select(x => x.TypeName);

            // Closeable windows should be tagged with ITransientChild.
            var assemblyTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            var mdiChildTypesToLoad = assemblyTypes.Where(x =>
                typeof(ITransientChild).IsAssignableFrom(x)
                && !x.IsAbstract
                && x.IsClass
                && configWindowTypeNames.Contains(x.FullName));

            foreach (var child in mdiChildTypesToLoad)
            {
                FocusCreateChild(child, Gebug64.Win.Ui.Lang.GetDefaultWindowTitle(child));
            }
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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            StartExtendNotifySaveUiTimer();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            StartExtendNotifySaveUiTimer();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            StartExtendNotifySaveUiTimer();
        }

        /// <summary>
        /// Assuming <see cref="DoneInit"/> has already been called, timer event handler
        /// to save UI state to appconfig.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void NotifySaveUiState(object? sender, ElapsedEventArgs e)
        {
            _saveUiStateTimerActive = false;

            if (!_ready)
            {
                return;
            }

            Workspace.Instance.SaveUiState();
        }

        /// <summary>
        /// Event handler called when window changes state, moves, or resizes.
        /// Starts a timer to call <see cref="NotifySaveUiState"/>, or if the timer
        /// is already running stops it and restarts it to extend the timeout.
        /// </summary>
        private void StartExtendNotifySaveUiTimer()
        {
            if (!_ready)
            {
                return;
            }

            lock (_timerLock)
            {
                if (!_saveUiStateTimerActive)
                {
                    _saveUiStateTimerActive = true;
                    _saveUiStateTimer.Start();
                    return;
                }

                _saveUiStateTimer.Stop();
                _saveUiStateTimer.Start();
            }
        }
    }
}
