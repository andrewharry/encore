using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Encore.Testing.Services
{
    public class DataAccessHelper
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
            using var context = GetContext<TEntity>();
            var set = context.Set<TEntity>();
            return set.Where(where);
        }

        public TEntity? FirstOrDefault<TEntity>(Func<TEntity, bool> where) where TEntity : class
        {
            using var context = GetContext<TEntity>();
            var set = context.Set<TEntity>();
            return set.Where(where).FirstOrDefault();
        }

        public void SetItem<TEntity>(Func<TEntity, bool> where, TEntity entity) where TEntity : class
        {
            if (dependencyResolver == null)
                throw new NotSupportedException("Set Item can only be called on OnPostInitialise");

            using var context = GetContext<TEntity>();

            var set = context.Set<TEntity>();
            var exists = set.Where(where).FirstOrDefault();

            if (exists != null)
            {
                var values = EntityPropertyLookup.GetNonPrimaryKeyValues(context, entity);
                context.Entry(exists).CurrentValues.SetValues(values);
                return;
            }

            context.Set<TEntity>().Add(entity);
            context.SaveChanges();
        }


        public void SetItems<TEntity>(params TEntity[] items) where TEntity : class
        {
            if (dependencyResolver == null)
                throw new NotSupportedException("Set Items can only be called on OnPostInitialise");

            using var context = GetContext<TEntity>();
            var records = items.ToSafeArray();

            foreach (var record in records)
            {
                Upsert(context, record);
            }

            context.SaveChanges();
        }

        private DbContext GetContext<TEntity>() where TEntity : class
        {
            if (dependencyResolver == null)
                throw new NotSupportedException("GetContext can only be called on OnPostInitialise");

            return dbContextResolver.GetForEntity<TEntity>();
        }

        private void Upsert<TEntity>(DbContext context, TEntity entity) where TEntity : class
        {
            var keys = EntityPropertyLookup.GetPrimaryKeyValues(context, entity);
            var exists = context.Find<TEntity>(keys);

            if (exists != null)
            {
                var values = EntityPropertyLookup.GetNonPrimaryKeyValues(context, entity);
                context.Entry(exists).CurrentValues.SetValues(values);
                return;
            }

            context.Set<TEntity>().Add(entity);
        }
    }
}
