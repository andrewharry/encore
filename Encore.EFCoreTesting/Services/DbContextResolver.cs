using Encore.Testing.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Encore.EFCoreTesting.Services
{
    public class DbContextResolver
    {
        private readonly IServiceResolver Resolver;

        public HashSet<Type> DbContexts { get; set; }

        public DbContextResolver(IServiceResolver serviceResolver)
        {
            DbContexts = new HashSet<Type>();
            this.Resolver = serviceResolver;
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
                var context = Resolver.TryResolve(contextType) as DbContext;

                if (context == null)
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

        private bool ValidateType([NotNullWhen(true)] Type? dbContextType)
        {
            if (dbContextType == null)
                throw new ArgumentNullException(nameof(dbContextType));

            if (dbContextType.BaseType != typeof(DbContext))
                throw new ArgumentException($"Type {dbContextType.Name} is not a valid DbContext");

            return true;
        }
    }
}
