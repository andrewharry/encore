using Encore.Helpers;
using Encore.Testing.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Encore.Testing.Services
{
    [SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "<Pending>")]
    public class DbContextResolver : IDisposable
    {
        [NotNull]
        private IServiceResolver? Resolver;

        public HashSet<Type> DbContexts { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DbContextResolver()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            DbContexts = new HashSet<Type>();
        }

        public void SetupResolver(Assembly assembly, IServiceResolver serviceResolver, bool autoPopulate = false)
        {
            this.Resolver = serviceResolver;

            if (autoPopulate)
            {
                var dbContexts = AssemblyHelper.GetTypesByBaseClass(typeof(DbContext), assembly);

                foreach (Type dbContext in dbContexts)
                {
                    Add(dbContext);
                }
            }
        }

        public void Add(Type dbContextType)
        {
            ValidateType(dbContextType);
            DbContexts.Add(dbContextType);
        }
        
        public IEnumerable<DbContext> GetAll()
        {
            foreach (var contextType in DbContexts)
            {
                if (Resolver.TryResolve(contextType) is not DbContext context)
                    continue;

                yield return context;
            }
        }

        public DbContext GetForEntity<TEntity>() where TEntity : class
        {
            if (DbContexts.IsNullOrEmpty())
                throw new ArgumentException("No DbContexts Registered");

            if (DbContexts.OnlyOne())
            {
                // There is only one DbContext registered - so defaults to this one
                return ResolveDbType(DbContexts.First());
            }

            // Need to determine which dbContext has the TEntity class mapped
            // NOTE: This might be expensive - Probably should cache the mapping

            var entityType = typeof(TEntity);

            foreach (var dbContextType in DbContexts)
            {
                var dbContext = ResolveDbType(dbContextType);

                if (dbContext == null)
                    continue;

                var match = dbContext.Model.FindEntityType(entityType);

                if (match != null)
                    return dbContext;
            }

            // Not mapped - default to first dbContextType
            return ResolveDbType(DbContexts.First());
        }


        private DbContext ResolveDbType(Type? dbContextType)
        {
            if (!ValidateType(dbContextType))
                throw new ArgumentException("Invalid dbContextType");

            var context = Resolver.TryResolve(dbContextType) as DbContext;

            return context ?? throw new ArgumentException("No DbContexts Registered");
        }

        private static bool ValidateType([NotNull] Type? dbContextType)
        {
            if (dbContextType == null)
                throw new ArgumentNullException(nameof(dbContextType));

            if (dbContextType.BaseType != typeof(DbContext))
                throw new ArgumentException($"Type {dbContextType.Name} is not a valid DbContext");

            return true;
        }

        [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
        public void Dispose()
        {
            foreach (var context in GetAll())
            {
                if (context.Database.IsInMemory())
                {
                    context.Database.EnsureDeleted();
                }
            }

            DbContexts.Clear();
        }
    }
}
