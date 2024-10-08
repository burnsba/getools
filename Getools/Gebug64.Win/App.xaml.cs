﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Antlr4.Runtime.Atn;
using AutoMapper;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Flashcart;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.SerialPort;
using Gebug64.Win.Config;
using Gebug64.Win.Controls;
using Gebug64.Win.Extensions;
using Gebug64.Win.Session;
using Gebug64.Win.ViewModels;
using Gebug64.Win.ViewModels.CategoryTabs;
using Gebug64.Win.ViewModels.Config;
using Gebug64.Win.ViewModels.Map;
using Gebug64.Win.Windows;
using Gebug64.Win.Windows.Mdi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Gebug64.Win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ILogger? _theLogger;
        private AppConfigViewModel? _appConfigViewModel = null;
        private MessageBus<IGebugMessage>? _appGebugMessageBus;

        private void App_Startup(object sender, StartupEventArgs e)
        {
            ServiceCollection serviceCollection;
            IConfiguration configuration;
            ServiceProvider serviceProvider;

            Logger.CreateInstance();
            _theLogger = Logger.Instance!;

            AppConfigSettings.EnsureDefaultDirectory();
            var settingsDirectory = AppConfigSettings.GetDefaultDirectory();

            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(settingsDirectory)
                    .AddJsonFile(AppConfigSettings.DefaultFilename, optional: true, reloadOnChange: false);

                configuration = builder.Build();

                serviceCollection = new ServiceCollection();

                ConfigureServices(serviceCollection, configuration);

                serviceProvider = serviceCollection.BuildServiceProvider();

                Workspace.CreateInstance(configuration, serviceProvider);
            }
            catch (Exception ex)
            {
                ShowUnhandledStartupException(ex.Message);
            }

            Dispatcher.UnhandledException += (s, e) =>
            {
                e.Handled = true;
                ShowUnhandledException(e.Exception, "Dispatcher.UnhandledException");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                e.SetObserved();
                ShowUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
            };

            MainWindowViewModel vm = (MainWindowViewModel)Workspace.Instance.ServiceProvider.GetService(typeof(MainWindowViewModel))!;

            /*** begin resolve tabs */

            // Look up the tabs to add based on the interface ICategoryTabViewModel.
            // This will instantiate instances of the viewmodels, so needs to happen
            // outside the constructor of the MainWindowViewModel, otherwise any references in the tabs
            // to the MainWindowViewModel will create a circular construction chain (a very slow stack overflow).
            var assemblyTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            var tabViewmodelTypes = assemblyTypes.Where(x =>
                typeof(ICategoryTabViewModel).IsAssignableFrom(x)
                && !x.IsAbstract
                && x.IsClass);
            var tabViewmodels = new List<TabViewModelBase>();
            foreach (var t in tabViewmodelTypes)
            {
                tabViewmodels.Add((TabViewModelBase)Workspace.Instance.ServiceProvider.GetService(t)!);
            }

            foreach (var t in tabViewmodels.OrderBy(x => x.DisplayOrder))
            {
                vm.Tabs.Add(t);
            }

            /*** end resolve tabs */

            var host = (MdiHostWindow)Workspace.Instance.ServiceProvider.GetService(typeof(MdiHostWindow))!;

            MainControl main = (MainControl)Workspace.Instance.ServiceProvider.GetService(typeof(MainControl))!;
            host.AddPermanentMdiChild(main, Gebug64.Win.Ui.Lang.Window_MessageCenterTitle);

            // restore position and state.
            host.TryLoadWindowLayoutState(_appConfigViewModel!);

            // Load children from appconfig.
            host.LoadSavedChildLayout();

            // Enable events to write UI state updates to appconfig file.
            host.DoneInit();

            host.Show();

            _theLogger.Log(LogLevel.Information, "Application started");
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var mapeprConfiguration = new MapperConfiguration(x => Session.MapperConfig.SetConfiguration(x));
            var mapper = mapeprConfiguration.CreateMapper();
            services.AddSingleton(typeof(IMapper), mapper);

            services.AddSingleton(typeof(IConfiguration), configuration);

            var appSettings = new AppConfigSettings(configuration);
            _appConfigViewModel = mapper.Map<AppConfigViewModel>(appSettings);

            services.AddSingleton<AppConfigViewModel>(_appConfigViewModel);

            services.AddSingleton<ILogger>(_theLogger!);

            var typeGetter = new SerialPortFactoryTypeGetter(typeof(WrappedSerialPort));
            services.AddSingleton<SerialPortFactoryTypeGetter>(typeGetter);

            services.AddSingleton<SerialPortProvider>();
            services.AddTransient<SerialPortFactory>();

            services.AddSingleton<IConnectionServiceProviderResolver>(new ConnectionServiceProviderResolver());
            services.AddTransient<Everdrive>();

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MapWindowViewModel>();
            services.AddSingleton<MemoryWindowViewModel>();

            // MdiChild windows
            services.AddTransient<MainControl>();
            services.AddTransient<LogControl>();
            services.AddTransient<QueryTasksControl>();
            services.AddTransient<MemoryControl>();
            services.AddTransient<MapControl>();

            // Parent MDI object
            services.AddSingleton<MdiHostViewModel>();

            // Parent MDI window
            services.AddSingleton<MdiHostWindow>();

            var assemblyTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            var tabViewmodelTypes = assemblyTypes.Where(x =>
                typeof(ICategoryTabViewModel).IsAssignableFrom(x)
                && !x.IsAbstract
                && x.IsClass);

            foreach (var t in tabViewmodelTypes)
            {
                services.AddTransient(t);
            }

            _appGebugMessageBus = new MessageBus<IGebugMessage>();
            services.AddSingleton<MessageBus<IGebugMessage>>(_appGebugMessageBus);
        }

        /// <summary>
        /// Shows error window for uncaught exceptions. Closes application once
        /// the error window is closed.
        /// </summary>
        /// <param name="ex">Exception to display.</param>
        /// <param name="source">Source of exception.</param>
        private void ShowUnhandledException(Exception ex, string source)
        {
            var ewvm = new ErrorWindowViewModel($"Fatal: Unhandled exception in application: {source}", ex)
            {
                ExitOnClose = true,
                ButtonText = "Exit",
            };

            Workspace.Instance.RecreateSingletonWindow<ErrorWindow>(ewvm);
        }

        private void ShowUnhandledStartupException(string message)
        {
            System.Windows.MessageBox.Show(message);
            Logger.Instance!.Shutdown();
            Environment.Exit(1);
        }
    }
}
