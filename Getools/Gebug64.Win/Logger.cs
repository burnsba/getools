using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win
{
    /// <summary>
    /// Standard app logger singleton.
    /// </summary>
    public class Logger : ILogger
    {
        private const string TimestampFormat = "yyyyMMdd-HHmmss";

        private static Logger? _instance = null;

        private static object _singleton = new object();

        private bool _consoleAdded = false;
        private List<Action<LogLevel, string>> _callbacks = new List<Action<LogLevel, string>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        private Logger()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the logger.
        /// </summary>
        public static Logger? Instance
        {
            get
            {
                if (object.ReferenceEquals(null, _instance))
                {
                    CreateInstance();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets the option to prefix log messages with the current timestamp.
        /// </summary>
        public bool PrefixTimestamp { get; set; } = true;

        /// <summary>
        /// Instantiates the singleton instance of the logger.
        /// </summary>
        public static void CreateInstance()
        {
            lock (_singleton)
            {
                if (object.ReferenceEquals(null, _instance))
                {
                    _instance = new Logger();
                }
            }
        }

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) =>
            logLevel switch
            {
                LogLevel.None => true,
                LogLevel.Trace => true,
                LogLevel.Debug => true,
                LogLevel.Information => true,
                LogLevel.Warning => true,
                LogLevel.Critical => true,
                LogLevel.Error => true,
                _ => throw new NotImplementedException(),
            };

        /// <summary>
        /// Adds method to the logger to send received messages to callback.
        /// </summary>
        /// <param name="callback">Callback action to accept log message.</param>
        public void AddCallback(Action<LogLevel, string> callback)
        {
            _callbacks.Add(callback);
        }

        /// <summary>
        /// Removes logger callback method.
        /// </summary>
        /// <param name="callback">Callback action to remove.</param>
        public void RemoveCallback(Action<LogLevel, string> callback)
        {
            _callbacks.Remove(callback);
        }

        /// <summary>
        /// Adds method to the logger to write received messages to console stdout.
        /// </summary>
        public void AddConsoleLogger()
        {
            if (_consoleAdded)
            {
                return;
            }

            AddCallback(ConsoleLogger);

            _consoleAdded = true;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string msg = formatter?.Invoke(state, exception) ?? string.Empty;

            if (PrefixTimestamp)
            {
                var prefix = DateTime.Now.ToString(TimestampFormat) + ": ";
                msg = prefix + msg;
            }

            foreach (var callback in _callbacks)
            {
                callback(logLevel, msg);
            }
        }

        private void ConsoleLogger(LogLevel logLevel, string msg)
        {
            if (logLevel == LogLevel.Debug
                || logLevel == LogLevel.Trace
                || logLevel == LogLevel.Information
                || logLevel == LogLevel.Warning)
            {
                Console.Out.WriteLine(msg);
            }
            else if (logLevel == LogLevel.Critical
                || logLevel == LogLevel.Error)
            {
                Console.Error.WriteLine(msg);
            }
        }
    }
}
