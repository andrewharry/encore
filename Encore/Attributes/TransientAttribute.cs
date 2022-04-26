using Microsoft.Extensions.DependencyInjection;

namespace Encore
{
    /// <summary>
    /// Register the class as a 'Transient' service with the DI Container
    /// There can only be one concrete implementation registered
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TransientAttribute : RegisterAttribute
    {
        public TransientAttribute()
        {
            base.Life = ServiceLifetime.Transient;
        }
    }
}