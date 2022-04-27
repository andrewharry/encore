using Microsoft.Extensions.DependencyInjection;

namespace Encore.Testing.Services
{
    public class ServiceResolver : IServiceResolver
    {
        private readonly IServiceProvider container;

        public ServiceResolver(IServiceProvider container)
        {
            this.container = container;
        }

        public T Resolve<T>() where T : class
        {
            return container.GetRequiredService<T>();
        }

        public T? TryResolve<T>() where T : class
        {
            return container.GetService<T>();
        }

        public object? TryResolve(Type type)
        {
            return container.GetService(type);
        }

        public object Resolve(Type type)
        {
            return container.GetRequiredService(type);
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return container.GetServices<T>().ToSafeArray();
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            return container.GetServices(type).Cast<object>().ToSafeArray();
        }

        public void Dispose()
        {
            /* Does Nothing */
        }
    }
}
