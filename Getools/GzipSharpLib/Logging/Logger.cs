using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GzipSharpLib.Logging
{
    public class Logger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

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

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string msg = formatter?.Invoke(state, exception) ?? string.Empty;

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
