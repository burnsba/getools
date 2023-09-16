using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Gebug64.Win.ViewModels;
using Gebug64.Win.Windows;
using System.Windows.Threading;
using Gebug64.Win.Session;
using Gebug64.Win.Config;
using Gebug64.Win.ViewModels.Config;
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

        public void SaveAppSettings()
        {
            var translateService = (TranslateService)ServiceProvider.GetService(typeof(TranslateService))!;
            var appConfig = (AppConfigViewModel)ServiceProvider.GetService(typeof(AppConfigViewModel))!;

            var settings = translateService.Translate<AppConfigViewModel, AppConfigSettings>(appConfig);

            var jobj = JObject.FromObject(settings);
            // parent the C# class into a container called "AppConfigSettings"
            var container = JObject.FromObject(new { AppConfigSettings = jobj });

            string json = JsonConvert.SerializeObject(container, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(AppConfigSettings.DefaultFilename, json);
        }
    }
}
