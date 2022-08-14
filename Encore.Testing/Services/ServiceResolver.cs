using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Encore.Testing.Services
{
    public class ServiceResolver : IServiceResolver
    {
        public IServiceProvider Container { get; }

        public ServiceResolver(IServiceProvider container)
        {
            this.Container = container;
        }

        public T Resolve<T>() where T : class
        {
            return Container.GetRequiredService<T>();
        }

        public T? TryResolve<T>() where T : class
        {
            return Container.GetService<T>();
        }

        public object? TryResolve(Type type)
        {
            return Container.GetService(type);
        }

        public object Resolve(Type type)
        {
            return Container.GetRequiredService(type);
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return Container.GetServices<T>().ToSafeArray();
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            return Container.GetServices(type).Cast<object>().ToSafeArray();
        }

        public void Dispose()
        {
            /* Does Nothing */
        }
    }
}
