using Microsoft.Extensions.Logging;

namespace ButlerCore
{
    public sealed class CustomFileLoggerProvider : ILoggerProvider
    {
        private StreamWriter? _logFileWriter;
        private bool _disposed;

        public CustomFileLoggerProvider(StreamWriter logFileWriter)
        {
            _logFileWriter = logFileWriter
                ?? throw new ArgumentNullException(nameof(logFileWriter));
        }

        public ILogger CreateLogger(string categoryName)
            => new CustomFileLogger(categoryName, _logFileWriter!);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _logFileWriter?.Flush();
            _logFileWriter?.Dispose();
            _logFileWriter = null;
            GC.SuppressFinalize(this);
        }
    }


    // Customized ILogger, writes logs to text files
    public class CustomFileLogger : ILogger
    {
        private readonly StreamWriter _logFileWriter;

        public CustomFileLogger(
            string categoryName,
            StreamWriter logFileWriter)
        {
            _logFileWriter = logFileWriter;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // required by the ILogger interface
            return null;
        }

        public bool IsEnabled(
            LogLevel logLevel)
        {
            // Ensure that only information level and higher logs are recorded
            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            // Ensure that only information level and higher logs are recorded
            if (!IsEnabled(logLevel))
            {
                return;
            }

            // Get the formatted log message
            var message = formatter(state, exception);

            // Write log messages to text file
            _logFileWriter.WriteLine($"[{DateTime.Now:hh:mm:ss}]:{message}");
            _logFileWriter.Flush();
        }
    }
}