using Microsoft.Extensions.DependencyInjection;
using System;

namespace Encore.Testing.Services
{
    public interface IServiceRegister : IRegisterClass
    {
        IServiceCollection ServiceCollection { get; }

        bool IsRegistered<T>() where T : class;

        bool NotRegistered<T>() where T : class;

        bool IsRegistered(Type type);

        void RegisterInstance(Type type, object instance);

        void Register<T>(T instance) where T : class;

        void Register<T>(ServiceLifetime lifetime = ServiceLifetime.Transient) where T : class;

        void Register(Type type, ServiceLifetime lifetime = ServiceLifetime.Transient);

        void Register(Type @interface, Type type, ServiceLifetime lifetime = ServiceLifetime.Transient);

        void TryRegister<T>(ServiceLifetime lifetime = ServiceLifetime.Transient) where T : class;

        void TryRegister<TInterface, TClass>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TInterface : class where TClass : TInterface;

        void TryRegister(Type type, ServiceLifetime lifetime = ServiceLifetime.Transient);

        void TryRegister(Type @interface, Type type, ServiceLifetime lifetime = ServiceLifetime.Transient);
    }
}
