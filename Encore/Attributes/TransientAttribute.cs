using Microsoft.Extensions.DependencyInjection;

namespace Encore
{
    /// <summary>
    /// Register as a Transient Class with the DI Container
    /// </summary>
    public class TransientAttribute : RegisterAttribute
    {
        public TransientAttribute()
        {
            base.Life = ServiceLifetime.Transient;
        }
    }
}