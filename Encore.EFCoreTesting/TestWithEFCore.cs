using Encore.EFCoreTesting.Services;
using Encore.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Encore.EFCoreTesting
{
    public abstract class TestWithEFCore : TestWithLogs
    {
        [NotNull]
        private DataAccessHelper? dataAccess { get; set; }

        /// <summary>
        /// Creates a InMemory Test DbContext.  Needs to be called prior to SetupResolver
        /// </summary>
        protected virtual void CreateDatabase<TDbContext>() where TDbContext : DbContext
        {
            var type = typeof(TDbContext);
            var databaseName = type.Name;
            dataAccess.CreateDatabase<TDbContext>();

            Registry.Register(type, ServiceLifetime.Transient);
            Registry.ServiceCollection.AddDbContextFactory<TDbContext>(option => SetOptions(databaseName, option), ServiceLifetime.Singleton);
            Registry.ServiceCollection.AddDbContext<TDbContext>(option => SetOptions(databaseName, option), ServiceLifetime.Transient);
        }

        protected override void SetupResolver()
        {
            if (dataAccess == null) { 
                base.SetupResolver();
                dataAccess = new DataAccessHelper(Resolver);
            }
        }

        protected virtual void SetOptions(string databaseName, DbContextOptionsBuilder options)
        {
            options
                .UseInMemoryDatabase(databaseName)
                .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: false)
                .LogTo(Console.WriteLine, LogLevel.Warning);
        }

        protected override void OnPreCleanup()
        {
            base.OnPreCleanup();
            dataAccess?.Dispose();
        }

        protected void SetItems<TEntity>(IEnumerable<TEntity> items) where TEntity : class
        {
            dataAccess.SetItems(items.ToSafeArray());
        }

        protected void SetItems<TEntity>(params TEntity[] items) where TEntity : class
        {
            dataAccess.SetItems(items);
        }

        protected IEnumerable<TEntity> GetItems<TEntity>(Func<TEntity, bool> where) where TEntity : class
        {
            return dataAccess.GetItems(where);
        }

        protected TEntity? FirstOrDefault<TEntity>() where TEntity : class
        {
            return dataAccess.FirstOrDefault<TEntity>(v => true);
        }
    }
}
