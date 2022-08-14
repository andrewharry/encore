using Microsoft.Extensions.Logging;
using System;

namespace Encore.Testing.Services
{
    public class TestLoggerT<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new <see cref="Logger{T}"/>.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public TestLoggerT(TestLogFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _logger = factory.CreateLogger(typeof(T).FullName);
        }

        /// <inheritdoc />
        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }

        /// <inheritdoc />
        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        /// <inheritdoc />
        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
