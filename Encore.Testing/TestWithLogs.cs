using Encore.Testing.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Encore.Testing
{
    public abstract class TestWithLogs : TestWithRegistry
    {
        protected ILogger? logger;

        /// <summary>
        /// Override if you want to register a different logger etc
        /// </summary>
        protected override void OnSetup()
        {
            logger = RegisterMock<ILogger>();
            Registry.ServiceCollection.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory>(new TestLogFactory(logger)));
            Registry.ServiceCollection.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
            base.OnSetup();
        }

        protected void ClearLogs()
        {
            logger?.ClearReceivedCalls();
        }

        protected void ExpectLogCriticalWithException(int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Critical, Arg.Any<EventId>(), Arg.Any<string>(), Arg.Any<Exception>());
        }

        protected void ExpectLogCritical(int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Critical, Arg.Any<string>());
        }

        protected void ExpectLogError(int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Error, Arg.Any<string>());
        }

        protected void ExpectLogError(string message, int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Error, message);
        }

        protected void ExpectLogError(string message, Exception exception, int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Error, message, exception);
        }

        protected void ExpectLogWarning(int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Warning, Arg.Any<string>());
        }

        protected void ExpectLogWarning(string message, int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Warning, message);
        }

        protected void ExpectLogInfo(int count = 1)
        {
            logger?.Received(count).Log(Arg.Any<LogLevel>(), Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(), Arg.Any<Func<object, Exception?, string>>());
        }

        protected void ExpectLogInfo(string message, int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Information, message);
        }

        protected void ExpectLogInfo(Func<string, bool> expected, int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Information, Arg.Is<string>(v => expected(v)));
        }

        protected void ExpectLogDebug(string message, int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Debug, message);
        }

        protected void ExpectLogDebug(int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Debug, Arg.Any<string>());
        }

        protected void ExpectLogDebug(Func<string, bool> assert)
        {
            logger?.Received().Log(LogLevel.Debug, Arg.Is<string>(v => assert(v)));
        }
    }
}
