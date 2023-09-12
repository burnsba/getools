using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gebug64.Win.Mvvm;

namespace Gebug64.Win.ViewModels
{
    /// <summary>
    /// ViewModel for error information.
    /// </summary>
    public class ErrorWindowViewModel : WindowViewModelBase
    {
        private bool _exitOnClose = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorWindowViewModel"/> class.
        /// </summary>
        public ErrorWindowViewModel()
        {
            CloseCommand = new RelayCommand<ICloseable>(CloseWindow);

            TextContent = String.Empty;
            WindowTitle = "Error";
            ButtonText = "Ok";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorWindowViewModel"/> class.
        /// </summary>
        /// <param name="ex">Exception to display information about.</param>
        public ErrorWindowViewModel(Exception ex)
            : this()
        {
            TextContent = Converters.ExceptionConverter.DefaultToString(ex);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorWindowViewModel"/> class.
        /// </summary>
        /// <param name="header">Header message text.</param>
        /// <param name="ex">Exception to display information about.</param>
        public ErrorWindowViewModel(string header, Exception ex)
            : this()
        {
            HeaderMessage = header;
            TextContent = Converters.ExceptionConverter.DefaultToString(ex);
        }

        /// <summary>
        /// Gets or sets ok button command.
        /// </summary>
        public ICommand CloseCommand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the exception information is from an unhandled exception.
        /// If set to true, when the error window is closed the application will exit.
        /// </summary>
        public bool ExitOnClose
        {
            get
            {
                return _exitOnClose;
            }

            set
            {
                _exitOnClose = value;

                if (_exitOnClose)
                {
                    CloseCommand = new RelayCommand<ICloseable>(w => { Environment.Exit(1); });
                }
                else
                {
                    CloseCommand = new RelayCommand<ICloseable>(CloseWindow);
                }
            }
        }

        /// <summary>
        /// Gets or sets the header message.
        /// </summary>
        public string HeaderMessage { get; set; } = "Unhandled exception";

        /// <summary>
        /// Gets or sets the body content.
        /// </summary>
        public string TextContent { get; set; }

        /// <summary>
        /// Windows title.
        /// </summary>
        public string WindowTitle { get; set; }

        /// <summary>
        /// Window button text.
        /// </summary>
        public string ButtonText { get; set; }
    }
}
