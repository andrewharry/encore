using Microsoft.Extensions.DependencyInjection;

namespace Encore
{
    /// <summary>
    /// Register as a Collection of types
    /// </summary>
    public class SingletonCollectionAttribute : RegisterAttribute
    {
        public SingletonCollectionAttribute()
        {
            base.Life = ServiceLifetime.Singleton;
            base.IsCollection = true;
        }
    }
}