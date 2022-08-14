using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

        public IEnumerable<TEntity> GetItems<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            using var context = GetContext<TEntity>();
            var set = context.Set<TEntity>();
            return set.AsNoTracking().Where(where).ToArray();
        }

        public TEntity? FirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            using var context = GetContext<TEntity>();
            var set = context.Set<TEntity>();
            return set.AsNoTracking().FirstOrDefault(where);
        }

        public TResult? Transform<TEntity, TResult>(Func<DbSet<TEntity>, TResult?> transform) where TEntity : class
        {
            using var context = GetContext<TEntity>();
            var set = context.Set<TEntity>();
            return transform(set);
        }

        public bool DeleteItem<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            using var context = GetContext<TEntity>();
            var set = context.Set<TEntity>();
            var record = set.AsNoTracking().FirstOrDefault(where);

            if (record == null)
                return false;

            set.Remove(record);
            return context.SaveChanges() > 0;
        }

        public Option<TEntity> SetItem<TEntity>(Func<TEntity, bool> where, TEntity entity) where TEntity : class
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
                return exists;
            }

            context.Set<TEntity>().Add(entity);
            context.SaveChanges();
            return entity;
        }

        public Option<TEntity> SetItem<TEntity>(TEntity entity) where TEntity : class
        {
            using var context = GetContext<TEntity>();
            {
                var result = Upsert(context, entity);
                context.SaveChanges();
                return result;
            } 
        }

        public TEntity[] SetItems<TEntity>(params TEntity[] items) where TEntity : class
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
            return items;
        }

        private DbContext GetContext<TEntity>() where TEntity : class
        {
            if (dependencyResolver == null)
                throw new NotSupportedException("GetContext can only be called on OnPostInitialise");

            return dbContextResolver.GetForEntity<TEntity>();
        }

        private Option<TEntity> Upsert<TEntity>(DbContext context, TEntity entity) where TEntity : class
        {
            var keys = EntityPropertyLookup.GetPrimaryKeyValues(context, entity);
            var exists = context.Find<TEntity>(keys);

            if (exists != null)
            {
                var values = EntityPropertyLookup.GetNonPrimaryKeyValues(context, entity);
                context.Entry(exists).CurrentValues.SetValues(values);
                return exists;
            }

            context.Set<TEntity>().Add(entity);
            return entity;
        }
    }
}
