using Microsoft.Extensions.DependencyInjection;

namespace Encore
{
    /// <summary>
    /// Register as a Collection of types
    /// </summary>
    public class CollectionAttribute : RegisterAttribute
    {
        public CollectionAttribute()
        {
            base.Life = ServiceLifetime.Transient;
            base.IsCollection = true;
        }
    }
}