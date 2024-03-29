﻿using Encore.Testing.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;

namespace Encore.Testing
{
    public abstract class TestWithLogs : TestWithRegistry
    {
        protected ILoggerInterceptor? logger;

        public ILogger? Logger { get; private set; }

        /// <summary>
        /// Override if you want to register a different logger etc
        /// </summary>
        protected override void OnSetup()
        {
            logger = RegisterMock<ILoggerInterceptor>();
            Registry.Register(new TestLogFactory(logger));
            Registry.Register(typeof(ILogger<>), typeof(TestLoggerT<>));
            Logger = new TestLogger(logger);
            Registry.Register<ILogger>(Logger);
            substitutes.Add(typeof(ILogger), Logger);
            base.OnSetup();
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

        protected void ExpectLogInfo(string message)
        {
            logger?.Received().Log(LogLevel.Information, message);
        }

        protected void ExpectLogInfo(string message, int count)
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
