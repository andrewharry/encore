using Microsoft.Extensions.DependencyInjection;

namespace Encore
{
    /// <summary>
    /// Register as a 'Collection' of types with 'Singleton' Lifetime.
    /// Collections allow more than one concrete implementation to be registered
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonCollectionAttribute : RegisterAttribute
    {
        public SingletonCollectionAttribute()
        {
            base.Life = ServiceLifetime.Singleton;
            base.IsCollection = true;
        }
    }
}