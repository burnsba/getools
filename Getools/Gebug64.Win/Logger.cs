using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
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

        private Thread _logThread;
        private ConcurrentQueue<LogQueueItem> _logMessageQueue = new ConcurrentQueue<LogQueueItem>();

        private bool _shutdown = false;
        private bool _consoleAdded = false;
        private List<Action<LogLevel, string>> _callbacks = new List<Action<LogLevel, string>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        private Logger()
        {
            _logThread = new Thread(ThreadLogMain);
            _logThread.IsBackground = true;
            _logThread.Start();
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

        /// <summary>
        /// Stops logger thread.
        /// </summary>
        public void Shutdown()
        {
            _shutdown = true;
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
            if (_shutdown)
            {
                throw new InvalidOperationException("Logger thread has terminated");
            }

            string msg = formatter?.Invoke(state, exception) ?? string.Empty;

            if (PrefixTimestamp)
            {
                var prefix = DateTime.Now.ToString(TimestampFormat) + ": ";
                msg = prefix + msg;
            }

            _logMessageQueue.Enqueue(new LogQueueItem()
            {
                Message = msg,
                Level = logLevel,
            });
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

        private void ThreadLogMain()
        {
            while (!_shutdown)
            {
                LogQueueItem next;

                while (_logMessageQueue.Any())
                {
                    if (_logMessageQueue.TryDequeue(out next))
                    {
                        foreach (var callback in _callbacks)
                        {
                            callback(next.Level, next.Message);
                        }
                    }
                }

                Thread.Sleep(10);
            }
        }

        private struct LogQueueItem
        {
            public string Message { get; set; }

            public LogLevel Level { get; set; }
        }
    }
}
