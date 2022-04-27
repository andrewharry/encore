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

        public void Add(Type contextType)
        {
            ValidateType(contextType);
            DbContexts.Add(contextType);
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

            if (DbContexts.Count == 1)
            {
                return GetForEntity(DbContexts.FirstOrDefault());

                if (type == null)
                    throw new ArgumentException("Null Type Issue");

                var context = Resolver.TryResolve(type) as DbContext;

                return context ?? throw new ArgumentException("No DbContexts Registered");
            }
        }

        private DbContext GetForEntity(Type? contextType)
        {
            ValidateType(contextType);

            if (DbContexts.IsNullOrEmpty())
                throw new ArgumentException("No DbContexts Registered");

            if (DbContexts.Count == 1)
            {
                var type = DbContexts.FirstOrDefault();

                if (type == null)
                    throw new ArgumentException("Null Type Issue");

                var context = Resolver.TryResolve(type) as DbContext;

                return context ?? throw new ArgumentException("No DbContexts Registered");
            }
        }

        private bool ValidateType([NotNullWhen(true)] Type? contextType)
        {
            if (contextType == null)
                throw new ArgumentNullException(nameof(contextType));

            if (contextType.BaseType != typeof(DbContext))
                throw new ArgumentException($"Type {contextType.Name} is not a valid DbContext");

            return true;
        }
    }
}
