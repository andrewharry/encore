using Microsoft.Extensions.DependencyInjection;

namespace Encore
{
    /// <summary>
    /// Register the class as a 'Singleton' service with the DI Container.
    /// There can only be one concrete implementation registered
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonAttribute : RegisterAttribute
    {
        public SingletonAttribute()
        {
            base.Life = ServiceLifetime.Singleton;
        }
    }
}