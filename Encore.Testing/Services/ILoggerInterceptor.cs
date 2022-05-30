using Microsoft.Extensions.Logging;

public interface ILoggerInterceptor
{
    void Log(LogLevel level, string message);
    void Log(LogLevel level, string message, Exception exception);
}