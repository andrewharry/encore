using Encore.Testing.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Encore.Testing
{
    public abstract class TestWithEfCore : TestWithLogs
    {
        [NotNull]
        private DataAccessHelper? DataAccess { get; set; }

        [NotNull]
        private DbContextResolver? DbContextResolver { get; set; }

        /// <summary>
        /// Creates a InMemory Test DbContext.  Needs to be called prior to SetupResolver
        /// </summary>
        protected virtual void CreateDatabase<TDbContext>(ServiceLifetime lifetime) where TDbContext : DbContext
        {
            if (DataAccess != null)
                throw new NullReferenceException($"{nameof(CreateDatabase)} needs to be called before {nameof(SetupResolver)}");

            DbContextResolver ??= new DbContextResolver();

            var type = typeof(TDbContext);
            var databaseName = type.Name;
            DbContextResolver.Add(type);

            Registry.Register(type, lifetime);
            Registry.ServiceCollection.AddDbContextFactory<TDbContext>(option => SetOptions(databaseName, option), ServiceLifetime.Singleton);
            Registry.ServiceCollection.AddDbContext<TDbContext>(option => SetOptions(databaseName, option), lifetime);
        }

        protected override void SetupResolver()
        {
            if (DataAccess == null) { 
                base.SetupResolver();
#pragma warning disable CS8604 // Possible null reference argument.
                DbContextResolver.SetupResolver(SutAssembly, Resolver);
#pragma warning restore CS8604 // Possible null reference argument.
                DataAccess = new DataAccessHelper(Resolver, DbContextResolver);
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
            DbContextResolver?.Dispose();
        }

        protected Option<TEntity> SetItem<TEntity>(TEntity item) where TEntity : class
        {
            return DataAccess.SetItem(item);
        }

        protected void SetItems<TEntity>(IEnumerable<TEntity> items) where TEntity : class
        {
            DataAccess.SetItems(items.ToSafeArray());
        }

        protected void SetItems<TEntity>(params TEntity[] items) where TEntity : class
        {
            DataAccess.SetItems(items);
        }

        protected IEnumerable<TEntity> GetItems<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            return DataAccess.GetItems(where);
        }

        protected TEntity? FirstOrDefault<TEntity>() where TEntity : class
        {
            return DataAccess.FirstOrDefault<TEntity>(v => true);
        }
    }
}

