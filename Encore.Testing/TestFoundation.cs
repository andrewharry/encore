using Encore.Testing.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Encore.Testing
{
    public abstract class TestFoundation : IRegisterClass, IResolveClass
    {
        protected Assembly SutAssembly { get; set; } = Assembly.GetCallingAssembly();

        protected bool ValidateOnBuild { get; set; } = false;
        protected bool ValidateScopes { get; set; } = false;

        [NotNull]
        protected IServiceRegister? Registry { get; set; }

        [NotNull]
        protected IServiceResolver? Resolver { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private ServiceProvider serviceProvider;
        private ServiceCollection collection;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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

        public T? TryResolve<T>() where T : class
        {
            return Resolver.TryResolve<T>();
        }

        public object? TryResolve(Type type)
        {
            return Resolver.TryResolve(type);
        }

        protected static void InfiniteLoop()
        {
            #if DEBUG
            var stack = new StackTrace().GetFrames();

            if (stack.Length > 100)
            {
                Console.WriteLine("Potential Infinite Loop - Stackoverflow imminent");
                Debugger.Break();
            }
            #endif
        }

        public void Dispose()
        {
            Resolver?.Dispose();
            serviceProvider?.Dispose();
        }
    }
}