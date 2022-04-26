using Microsoft.Extensions.Logging;
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
            base.OnSetup();
        }

        protected override TInterface RegisterMock<TInterface>() where TInterface : class
        {
            var type = typeof(TInterface);

            if (logger != null)
            {
                if (type == typeof(ILogger<>))
                    return (TInterface)logger;

                if (type == typeof(ILogger))
                    return (TInterface)logger;
            }

            return base.RegisterMock<TInterface>();
        }

        protected void ClearLogs()
        {
            logger?.ClearReceivedCalls();
        }

        protected void ExpectLogCriticalWithException(int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Critical, Arg.Any<string>(), Arg.Any<Exception>());
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
            logger?.Received(count).Log(LogLevel.Information, Arg.Any<string>());
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
