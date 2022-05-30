using Microsoft.Extensions.Logging;

namespace Encore.Testing.Services
{
    internal class TestLogFactory : ILoggerFactory
    {
        private readonly ILogger logger;

        public TestLogFactory(ILoggerInterceptor loggerInterceptor)
        {
            this.logger = new TestLogger(loggerInterceptor);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            /* Does Nothing */
        }

        public ILogger CreateLogger(string categoryName)
        {
            return logger;
        }

        public void Dispose()
        {
            /* Does Nothing */
        }
    }
}