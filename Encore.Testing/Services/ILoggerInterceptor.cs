using Microsoft.Extensions.Logging;
using System;

public interface ILoggerInterceptor
{
    void Log(LogLevel level, string message);
    void Log(LogLevel level, string message, Exception exception);
}