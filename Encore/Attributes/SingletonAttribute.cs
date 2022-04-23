using Microsoft.Extensions.DependencyInjection;

namespace Encore
{
    /// <summary>
    /// Register as a Singleton Class with the DI Container
    /// </summary>
    public class SingletonAttribute : RegisterAttribute
    {
        public SingletonAttribute()
        {
            base.Life = ServiceLifetime.Singleton;
        }
    }
}