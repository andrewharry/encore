using Encore.Testing.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Encore.Testing
{
    public abstract class TestingBase : IRegisterClass, IResolveClass
    {
        protected bool ValidateOnBuild { get; set; } = false;
        protected bool ValidateScopes { get; set; } = false;

        protected IServiceRegister Registry;
        protected IServiceResolver Resolver;

        private ServiceProvider serviceProvider;
        private ServiceCollection collection;

        protected virtual void SetupRegistry()
        {
            collection = new ServiceCollection();
            Registry = new ServiceRegister(collection);
        }

        protected virtual void SetupResolver()
        {
            serviceProvider = collection.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = ValidateOnBuild, ValidateScopes = ValidateScopes });
            Resolver = new ServiceResolver(serviceProvider);
        }

        protected void RegisterByAttributes(Type[] types)
        {
            collection.RegisterByAttributes(types);
        }

        public void Register<TInterface, TClass>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TInterface : class where TClass : TInterface
        {
            Registry.Register<TInterface, TClass>(lifetime);
        }

        public T Resolve<T>() where T : class
        {
            return Resolver.Resolve<T>();
        }

        public void Dispose()
        {
            Resolver?.Dispose();
            serviceProvider?.Dispose();
            serviceProvider = null;
        }
    }
}