using Microsoft.Extensions.DependencyInjection;
using System;

namespace Encore
{
    /// <summary>
    /// Register as a 'Collection' of types with 'Transient' Lifetime.
    /// Collections allow more than one concrete implementation to be registered
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionAttribute : RegisterAttribute
    {
        public CollectionAttribute()
        {
            base.Life = ServiceLifetime.Transient;
            base.IsCollection = true;
        }
    }
}