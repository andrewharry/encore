using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Encore.Testing.Services
{
    public class EntityPropertyLookup
    {
        private static ConcurrentDictionary<Type, IProperty[]> cache = new ConcurrentDictionary<Type, IProperty[]>();

        public static object?[] GetPrimaryKeyValues<TEntity>(DbContext dbContext, TEntity entity) where TEntity : class
        {
            return FindPrimaryKeyProperties<TEntity>(dbContext)
                .Select(v => GetPropertyValue(entity, v))
                .Where(v => v != null)
                .ToArray();
        }


        public static Dictionary<string, object> GetNonPrimaryKeyValues<TEntity>(DbContext dbContext, TEntity entity) where TEntity : class
        {
            var properties = GetProperties<TEntity>(dbContext);
            var result = new List<KeyValuePair<string, object>>(properties.Count);

            foreach (var property in properties.Where(v => !v.IsPrimaryKey()))
            {
                object value = GetPropertyValue(entity, property)!;
                result.Add(new KeyValuePair<string, object>(property.Name, value));
            }

            return new Dictionary<string, object>(result);
        }

        public static IReadOnlyList<IProperty> GetProperties<TEntity>(DbContext dbContext) where TEntity : class
        {
            var type = typeof(TEntity);

            if (cache.ContainsKey(type))
                return cache[type];

            var entityType = dbContext.Model.FindEntityType(typeof(TEntity));

            if (entityType == null)
                return Array.Empty<IProperty>();

            var items = entityType.GetProperties().ToSafeArray();

            cache.TryAdd(type, items);
            return items;
        }

        public static IReadOnlyList<IProperty> FindPrimaryKeyProperties<TEntity>(DbContext dbContext) where TEntity : class
        {
            return GetProperties<TEntity>(dbContext).ToSafeArray(v => v.IsPrimaryKey());
        }

        public static object? GetPropertyValue<TEntity>(TEntity entity, IProperty property) where TEntity : class
        {
            return property.PropertyInfo.GetValue(entity, null);
        }
    }
}
