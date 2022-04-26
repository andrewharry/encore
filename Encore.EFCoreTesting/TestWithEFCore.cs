using Encore.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Encore.EFCoreTesting
{
    public abstract class TestWithEFCore : TestWithLogs
    {
        protected Dictionary<string, Type> contexts = new();

        /// <summary>
        /// Creates a InMemory Test DbContext
        /// </summary>
        protected virtual void CreateDatabase<T>(bool useDbContextFactory = false) where T : DbContext
        {
            var databaseName = typeof(T).Name;

            contexts.Add(databaseName, typeof(T));

            if (useDbContextFactory)
            {
                Registry.ServiceCollection.AddDbContextFactory<T>(option => SetupOption(databaseName, option), ServiceLifetime.Singleton);
                return;
            }

            Registry.ServiceCollection.AddDbContext<T>(option => SetupOption(databaseName, option), ServiceLifetime.Scoped);
        }

        protected override void SetupResolver()
        {
            base.SetupResolver();
            repositoryHelper = new RepositoryHelper(Resolver);
        }

        private static void SetupOption(string databaseName, DbContextOptionsBuilder options)
        {
            options
                .UseInMemoryDatabase(databaseName)
                .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: false)
                .LogTo(Console.WriteLine, LogLevel.Warning);
        }
    }
}
