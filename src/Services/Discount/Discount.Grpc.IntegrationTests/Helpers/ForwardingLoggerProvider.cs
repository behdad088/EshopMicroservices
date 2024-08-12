using Microsoft.Extensions.Logging;

namespace Discount.Grpc.IntegrationTests.Helpers;

internal class ForwardingLoggerProvider(LogMessage logAction) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new ForwardingLogger(categoryName, logAction);
    }

    public void Dispose()
    {
    }

    private class ForwardingLogger(string categoryName, LogMessage logAction) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null!;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            logAction(logLevel, categoryName, eventId, formatter(state, exception), exception);
        }
    }
}