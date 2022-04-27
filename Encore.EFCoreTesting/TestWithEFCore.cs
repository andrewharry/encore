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
        protected DbContextResolver? dbContextResolver;

        [NotNull]
        private RepositoryHelper? repositoryHelper { get; set; }

        /// <summary>
        /// Creates a InMemory Test DbContext.  Needs to be called prior to SetupResolver
        /// </summary>
        protected virtual void CreateDatabase<T>() where T : DbContext
        {
            if (dbContextResolver == null)
                dbContextResolver = new DbContextResolver(Resolver);

            var type = typeof(T);
            var databaseName = type.Name;
            dbContextResolver.Add(type);

            Registry.Register(type, ServiceLifetime.Transient);
            Registry.ServiceCollection.AddDbContextFactory<T>(option => SetOptions(databaseName, option), ServiceLifetime.Singleton);
            Registry.ServiceCollection.AddDbContext<T>(option => SetOptions(databaseName, option), ServiceLifetime.Transient);
        }

        protected override void SetupResolver()
        {
            base.SetupResolver();
            repositoryHelper = new RepositoryHelper(Resolver, dbContextResolver);
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
            DeleteData();
        }

        private void DeleteData()
        {
            foreach (var context in dbContextResolver.GetAll())
            {
                context.Database.EnsureDeleted();
            }
        }

        protected void SetItems<TEntity>(IEnumerable<TEntity> items) where TEntity : class
        {
            repositoryHelper.SetItems(items.ToSafeArray());
        }

        protected void SetItems<TEntity>(params TEntity[] items) where TEntity : class
        {
            repositoryHelper.SetItems(items);
        }

        protected IEnumerable<TEntity> GetItems<TEntity>(Func<TEntity, bool> where) where TEntity : class
        {
            return repositoryHelper.GetItems(where);
        }

        protected TEntity? FirstOrDefault<TEntity>() where TEntity : class
        {
            return repositoryHelper.FirstOrDefault<TEntity>(v => true);
        }
    }
}
