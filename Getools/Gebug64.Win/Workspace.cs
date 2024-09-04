using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using AutoMapper;
using Gebug64.Win.Config;
using Gebug64.Win.Controls;
using Gebug64.Win.Extensions;
using Gebug64.Win.Session;
using Gebug64.Win.ViewModels;
using Gebug64.Win.ViewModels.Config;
using Gebug64.Win.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gebug64.Win
{
    /// <summary>
    /// Application context.
    /// </summary>
    public class Workspace
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private static Workspace _instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private static object _singleton = new object();
        private Dispatcher _dispatcher;

        private Dictionary<string, Control> _registeredControls = new Dictionary<string, Control>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Workspace"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        private Workspace(IServiceProvider serviceProvider)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the workspace instance.
        /// </summary>
        public static Workspace Instance => _instance;

        /// <summary>
        /// Application service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Creates the workspace and sets global options/config.
        /// </summary>
        /// <param name="configuration">Configuration provider (appsettings, environment, command line).</param>
        /// <param name="serviceProvider">Depenency injection service provider.</param>
        public static void CreateInstance(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            lock (_singleton)
            {
                if (object.ReferenceEquals(null, _instance))
                {
                    _instance = new Workspace(serviceProvider);
                }
            }
        }

        /// <summary>
        /// Shows a window of the given type. If a window with the same type already exists,
        /// it will be shown. If no window with the given type exists a new one will
        /// be created and shown.
        /// </summary>
        /// <typeparam name="T">Type of window to create.</typeparam>
        /// <param name="args">Window constructor arguments.</param>
        public void CreateSingletonWindow<T>(params object[] args)
        {
            if (!typeof(System.Windows.Window).IsAssignableFrom(typeof(T)))
            {
                throw new ArgumentException($"{typeof(T).FullName} must be System.Windows.Window");
            }

            _dispatcher.Invoke(() =>
            {
                foreach (System.Windows.Window w in System.Windows.Application.Current.Windows)
                {
                    if (w.GetType() == typeof(T))
                    {
                        w.Show();
                        w.Activate();
                        return;
                    }
                }

                var obj = Activator.CreateInstance(typeof(T), args);

                if (object.ReferenceEquals(null, obj))
                {
                    throw new NullReferenceException(nameof(obj));
                }

                var window = (System.Windows.Window)obj;

                window.Show();
            });
        }

        /// <summary>
        /// Shows a window of the given type. If a window with the same type already exists,
        /// it will be closed and recreated. If no window with the given type exists a new one will
        /// be created and shown.
        /// </summary>
        /// <typeparam name="T">Type of window to create.</typeparam>
        /// <param name="args">Window constructor arguments.</param>
        public void RecreateSingletonWindow<T>(params object[] args)
        {
            if (!typeof(System.Windows.Window).IsAssignableFrom(typeof(T)))
            {
                throw new ArgumentException($"{typeof(T).FullName} must be System.Windows.Window");
            }

            _dispatcher.Invoke(() =>
            {
                foreach (System.Windows.Window w in System.Windows.Application.Current.Windows)
                {
                    if (w.GetType() == typeof(T))
                    {
                        w.Close();
                    }
                }

                var obj = Activator.CreateInstance(typeof(T), args);

                if (object.ReferenceEquals(null, obj))
                {
                    throw new NullReferenceException(nameof(obj));
                }

                var window = (System.Windows.Window)obj;

                window.Show();
            });
        }

        /// <summary>
        /// Closes all windows of the given type.
        /// </summary>
        /// <typeparam name="T">Type of window to close.</typeparam>
        public void CloseWindows<T>()
        {
            if (!typeof(System.Windows.Window).IsAssignableFrom(typeof(T)))
            {
                throw new ArgumentException($"{typeof(T).FullName} must be System.Windows.Window");
            }

            _dispatcher.Invoke(() =>
            {
                foreach (System.Windows.Window w in System.Windows.Application.Current.Windows)
                {
                    if (w.GetType() == typeof(T))
                    {
                        w.Close();
                    }
                }
            });
        }

        /// <summary>
        /// Adds a control to the workspace collection so that it can be resolved globally.
        /// </summary>
        /// <param name="name">Name of the control. This should be unique across
        /// the entire application.</param>
        /// <param name="control">Control to track.</param>
        /// <remarks>
        /// This is used to fix the XAML failure related to the context menu
        /// in the data grid for the command to scroll a seperate data grid
        /// to the currently selected item.
        /// </remarks>
        public void RegisterControl(string name, Control control)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException($"Parameter \"{name}\" is required.");
            }

            if (_registeredControls.ContainsKey(name))
            {
                if (!object.ReferenceEquals(_registeredControls[name], control))
                {
                    throw new InvalidOperationException($"Object with key \"{name}\" already registered.");
                }
            }

            _registeredControls.Add(name, control);
        }

        /// <summary>
        /// Adds a control to the workspace collection so that it can be resolved globally.
        /// If the key already exists, the current element will be replaced
        /// with the new value.
        /// </summary>
        /// <param name="name">Name of the control. This should be unique across
        /// the entire application.</param>
        /// <param name="control">Control to track.</param>
        /// <remarks>
        /// This is used to fix the XAML failure related to the context menu
        /// in the data grid for the command to scroll a seperate data grid
        /// to the currently selected item.
        /// </remarks>
        public void RegisterOverwriteControl(string name, Control control)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException($"Parameter \"{name}\" is required.");
            }

            if (_registeredControls.ContainsKey(name))
            {
                _registeredControls.Remove(name);
            }

            _registeredControls.Add(name, control);
        }

        /// <summary>
        /// Removes a control from the global collection. If the key
        /// isn't found, nothing happens.
        /// </summary>
        /// <param name="name">Name of control to remove. Required.</param>
        public void UnregisterControl(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException($"Parameter \"{name}\" is required.");
            }

            if (_registeredControls.ContainsKey(name))
            {
                _registeredControls.Remove(name);
            }
        }

        /// <summary>
        /// Gets a control from the global collection. Throws on failure
        /// to find element.
        /// </summary>
        /// <param name="name">Name of item.</param>
        /// <returns>Control.</returns>
        public Control GetControl(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException($"Parameter \"{name}\" is required.");
            }

            return _registeredControls[name];
        }

        /// <summary>
        /// Gets a control from the global collection. If the control
        /// cannot be found, null is returned.
        /// </summary>
        /// <param name="name">Name of item.</param>
        /// <returns>Control or null.</returns>
        public Control? GetControlSafe(string name)
        {
            if (_registeredControls.ContainsKey(name))
            {
                return _registeredControls[name];
            }

            return null;
        }

        /// <summary>
        /// Exception handler when a task throws.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="title">Window title.</param>
        public void ShowTaskException(Task task, string title)
        {
            if (object.ReferenceEquals(null, task))
            {
                return;
            }

            var exc = task.Exception;
            if (!object.ReferenceEquals(null, exc))
            {
                var flattened = exc.Flatten();

                var ewvm = new ErrorWindowViewModel(title, flattened)
                {
                    ButtonText = "Ok",
                };

                Workspace.Instance.RecreateSingletonWindow<ErrorWindow>(ewvm);
            }
        }

        /// <summary>
        /// Serializes runtime app settings and saves to default json file.
        /// </summary>
        public void SaveAppSettings()
        {
            var appConfig = (AppConfigViewModel)ServiceProvider.GetService(typeof(AppConfigViewModel))!;

            var mapper = (IMapper)ServiceProvider.GetService(typeof(IMapper))!;
            var settings = mapper.Map<AppConfigSettings>(appConfig);

            var jobj = JObject.FromObject(settings);

            // parent the C# class into a container called "AppConfigSettings"
            var container = JObject.FromObject(new { AppConfigSettings = jobj });

            string json = JsonConvert.SerializeObject(container, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(AppConfigSettings.DefaultFilename, json);
        }

        /// <summary>
        /// Collect current UI state information and update runtime appconfig.
        /// If state has changed, call <see cref="SaveAppSettings"/>.
        /// </summary>
        public void SaveUiState()
        {
            var appConfig = (AppConfigViewModel)ServiceProvider.GetService(typeof(AppConfigViewModel))!;

            // Make a copy of the existing UI state for comparison.
            var oldLayoutList = new List<UiWindowState>();
            foreach (var x in appConfig.LayoutState.Windows)
            {
                oldLayoutList.Add(x with { });
            }

            appConfig.LayoutState.Windows.Clear();

            var host = (MdiHostWindow)ServiceProvider.GetService(typeof(MdiHostWindow))!;

            // Collect the current UI state.
            _dispatcher.Invoke(() =>
            {
                appConfig.LayoutState.Windows.Add(host.GetWindowLayoutState());

                foreach (var child in host.Container.Children)
                {
                    appConfig.LayoutState.Windows.Add(child.GetWindowLayoutState());
                }
            });

            bool needToSave = false;

            // If the current UI state is the same as the starting state, no need to save.
            if (appConfig.LayoutState.Windows.Count == oldLayoutList.Count)
            {
                var len = oldLayoutList.Count;
                for (int i = 0; i < len; i++)
                {
                    if (appConfig.LayoutState.Windows[i] != oldLayoutList[i])
                    {
                        needToSave = true;
                        break;
                    }
                }
            }
            else
            {
                needToSave = true;
            }

            if (needToSave)
            {
                SaveAppSettings();
            }
        }

        /// <summary>
        /// Helper method to set a property on an object from a directory picker dialog.
        /// </summary>
        /// <param name="instance">Object containing property.</param>
        /// <param name="propertyName">Name of property to set. Also reads this for the starting value.</param>
        /// <param name="getDefaultValue">If property isn't set, method to get default value.</param>
        public void SetDirectoryCommandHandler(object instance, string propertyName, Func<string> getDefaultValue)
        {
            // TODO: Use the new folder open dialog in .net 8.
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            if (object.ReferenceEquals(null, instance))
            {
                return;
            }

            var pi = instance.GetType().GetProperty(propertyName);
            if (object.ReferenceEquals(null, pi))
            {
                return;
            }

            string startDir = (string?)pi.GetValue(instance) ?? string.Empty;

            if (string.IsNullOrEmpty(startDir) || !System.IO.Directory.Exists(startDir))
            {
                if (getDefaultValue != null)
                {
                    startDir = getDefaultValue();
                }
            }

            dialog.InitialDirectory = startDir;
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrEmpty(dialog.FileName))
            {
                string dir = dialog.FileName!;

                if (!System.IO.Directory.Exists(dir) && System.IO.File.Exists(dir))
                {
                    // ignore possible null
                    dir = System.IO.Path.GetDirectoryName(dir)!;
                }

                if (System.IO.Directory.Exists(dir))
                {
                    pi.SetValue(instance, dir);
                }
            }
        }

        /// <summary>
        /// Helper method to set a property on an object from a file picker dialog.
        /// </summary>
        /// <param name="instance">Object containing property.</param>
        /// <param name="propertyName">Name of property to set. Also reads this for the starting value.</param>
        /// <param name="getDefaultValue">If property isn't set, method to get default value.</param>
        public void SetFileCommandHandler(object instance, string propertyName, Func<string> getDefaultValue)
        {
            // TODO: Use the new folder open dialog in .net 8.
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            if (object.ReferenceEquals(null, instance))
            {
                return;
            }

            var pi = instance.GetType().GetProperty(propertyName);
            if (object.ReferenceEquals(null, pi))
            {
                return;
            }

            string startDir = (string?)pi.GetValue(instance) ?? string.Empty;

            if (System.IO.File.Exists(startDir))
            {
                startDir = System.IO.Path.GetDirectoryName(startDir)!;
            }

            if (string.IsNullOrEmpty(startDir) || !System.IO.Directory.Exists(startDir))
            {
                if (getDefaultValue != null)
                {
                    startDir = getDefaultValue();

                    if (System.IO.File.Exists(startDir))
                    {
                        startDir = System.IO.Path.GetDirectoryName(startDir)!;
                    }
                }
            }

            dialog.InitialDirectory = startDir;
            dialog.IsFolderPicker = false;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrEmpty(dialog.FileName))
            {
                string file = dialog.FileName!;

                if (System.IO.File.Exists(file))
                {
                    pi.SetValue(instance, file);
                }
            }
        }
    }
}
