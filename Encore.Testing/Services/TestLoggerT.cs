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

#pragma warning disable CS8604 // Possible null reference argument.
            _logger = factory.CreateLogger(typeof(T).FullName);
#pragma warning restore CS8604 // Possible null reference argument.
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
#pragma warning disable CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
#pragma warning restore CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            _logger.Log(logLevel, eventId, state, exception, formatter);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        }
    }
}
