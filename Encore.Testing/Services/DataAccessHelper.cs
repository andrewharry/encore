using Encore.Testing.Services;
using Microsoft.EntityFrameworkCore;

namespace Encore.Testing.Services
{
    internal class DataAccessHelper
    {
        private readonly IServiceResolver dependencyResolver;
        private readonly DbContextResolver dbContextResolver;

        public DataAccessHelper(IServiceResolver dependencyResolver, DbContextResolver dbContextResolver)
        {
            this.dependencyResolver = dependencyResolver;
            this.dbContextResolver = dbContextResolver;
        }

        public IEnumerable<TEntity> GetItems<TEntity>(Func<TEntity, bool> where) where TEntity : class
        {
            using (var context = GetContext<TEntity>())
            {
                var set = context.Set<TEntity>();
                return set.Where(where);
            }
        }

        public TEntity? FirstOrDefault<TEntity>(Func<TEntity, bool> where) where TEntity : class
        {
            using (var context = GetContext<TEntity>())
            {
                var set = context.Set<TEntity>();
                return set.Where(where).FirstOrDefault();
            }
        }

        public void SetItems<TEntity>(params TEntity[] items) where TEntity : class
        {
            if (dependencyResolver == null)
                throw new NotSupportedException("SetContextItems can only be called on OnPostInitialise");

            using (var context = GetContext<TEntity>())
            {
                var set = context.Set<TEntity>();

                set.AddRange(items);

                context.SaveChanges();
                context.ChangeTracker.Clear();
            }
        }

        public void SetItem<TEntity>(TEntity item) where TEntity : class
        {
            if (dependencyResolver == null)
                throw new NotSupportedException("Set Items can only be called on OnPostInitialise");

            using (var context = GetContext<TEntity>())
            {
                Upsert(context, item);
                context.SaveChanges();
                context.ChangeTracker.Clear();
            }
        }

        public void SetItems<TEntity>(IEnumerable<TEntity> items) where TEntity : class
        {
            if (dependencyResolver == null)
                throw new NotSupportedException("Set Items can only be called on OnPostInitialise");

            using (var context = GetContext<TEntity>())
            {
                var records = items.ToSafeArray();

                foreach (var record in records)
                {
                    Upsert(context, record);
                }

                context.SaveChanges();
                context.ChangeTracker.Clear();
            }
        }

        private DbContext GetContext<TEntity>() where TEntity : class
        {
            if (dependencyResolver == null)
                throw new NotSupportedException("GetContext can only be called on OnPostInitialise");

            var context = dbContextResolver.GetForEntity<TEntity>();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context;
        }

        private void Upsert<TEntity>(DbContext context, TEntity entity) where TEntity : class
        {
            var entry = context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                context.Attach(entity);
                entry.State = EntityState.Modified;
                return;
            }

            entry.State = EntityState.Added;
            context.Set<TEntity>().Add(entity);
        }
    }
}
