using Microsoft.Extensions.DependencyInjection;
using System;

namespace Encore.Testing.Services
{
    /// <summary>
    /// Note: This class is not available after service registration.  Hence this class can't be injected
    /// </summary>
    public class ServiceRegister : IServiceRegister
    {
        public IServiceCollection ServiceCollection { get; private set; }

        public ServiceRegister(IServiceCollection serviceCollection)
        {
            this.ServiceCollection = serviceCollection;
        }

        public bool IsRegistered<T>() where T : class
        {
            return ServiceCollection.IsRegistered(typeof(T));
        }

        public bool NotRegistered<T>() where T : class
        {
            return !IsRegistered<T>();
        }

        public bool IsRegistered(Type type)
        {
            return ServiceCollection.IsRegistered(type);
        }

        public void RegisterInstance(Type type, object instance)
        {
            ServiceCollection.Register(type, instance);
        }

        public bool NotRegistered(Type type)
        {
            return !IsRegistered(type);
        }

        public void Register<T>(T instance) where T : class
        {
            ServiceCollection.Register(instance);
        }

        public void Register<T>(ServiceLifetime lifetime) where T : class
        {
            ServiceCollection.Register<T>(lifetime);
        }

        public void Register<TInterface, TClass>(ServiceLifetime lifetime) where TInterface : class where TClass : TInterface
        {
            ServiceCollection.Register<TInterface, TClass>(lifetime);
        }

        public void Register(Type type, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            ServiceCollection.Register(type, lifetime);
        }

        public void Register(Type @interface, Type type, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            ServiceCollection.Register(@interface, type, lifetime);
        }

        public void TryRegister<T>(ServiceLifetime lifetime) where T : class
        {
            ServiceCollection.TryRegister<T>(lifetime);
        }

        public void TryRegister<TInterface, TClass>(ServiceLifetime lifetime) where TInterface : class where TClass : TInterface
        {
            ServiceCollection.TryRegister<TInterface, TClass>(lifetime);
        }

        public void TryRegister(Type type, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            ServiceCollection.TryRegister(type, lifetime);
        }

        public void TryRegister(Type @interface, Type type, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            ServiceCollection.TryRegister(@interface, type, lifetime);
        }
    }
}
