using Microsoft.Extensions.Logging;

namespace Encore.Testing.Services
{
    internal class TestLogFactory : ILoggerFactory
    {
        private readonly ILogger logger;

        public TestLogFactory(ILogger logger)
        {
            this.logger = logger;
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