using Microsoft.Extensions.Logging;

namespace Encore.Testing.Services;

public class TestLogger : ILogger
{
    private readonly ILoggerInterceptor interceptor;

    public TestLogger(ILoggerInterceptor interceptor)
    {
        this.interceptor = interceptor;
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.Debug)
            return;
        
        var msg = $"{formatter(state, exception)}";
        
        Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}] {msg}");
        
        if (exception != null)
            interceptor.Log(logLevel, msg, exception);
        else interceptor.Log(logLevel, msg);
    }
}