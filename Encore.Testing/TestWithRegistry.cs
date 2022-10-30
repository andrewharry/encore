using Encore.Testing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Encore.Testing
{
    public abstract class TestWithRegistry : TestWithHooks, IDisposable
    {
        protected readonly Dictionary<Type, object> substitutes = new Dictionary<Type, object>(1000);

        /// <summary>
        /// Register class instance with container
        /// </summary>
        public void RegisterInstance<TClass>(TClass instance) where TClass : class
        {
            Registry.Register(instance);
        }

        /// <summary>
        /// Register class and interface with container
        /// </summary>
        public void RegisterWithDependencies<TInterface, TClass>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TInterface : class where TClass : TInterface
        {
            RegisterByType(typeof(TClass));
            Registry.TryRegister<TInterface, TClass>(lifetime);
        }

        /// <summary>
        /// Register class with container
        /// </summary>
        protected void RegisterWithDependencies<TClass>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TClass : class
        {
            RegisterByType(typeof(TClass));
        }

        protected void RegisterByType(Type type)
        {
            if (Registry.IsRegistered(type))
                return;

            var types = TypeDependencies.GetDependencies(SutAssembly, type);

            foreach (var next in types)
            {
                if (Registry.IsRegistered(next))
                    continue;

                RegisterByType(next);
            }

            var interfaces = type.GetInterfaces(includeInherited: false);
            interfaces.Each(v => Registry.TryRegister(v, type));
            Registry.TryRegister(type);
        }

        protected virtual TInterface RegisterMock<TInterface>() where TInterface : class
        {
            var type = typeof(TInterface);

            if (substitutes.ContainsKey(type))
                return (TInterface)substitutes[type];

            var proxy = Substitute.For<TInterface>();

            substitutes.Add(type, proxy);
            Registry.Register(proxy);
            return proxy;
        }

        protected object? RegisterMock(Type type)
        {
            if (type.IsClass)
            {
                var interfaces = type.GetInterfaces(includeInherited: false);
                interfaces.Each(v => RegisterMock(v));
                return null;
            }

            if (substitutes.ContainsKey(type))
                return substitutes[type];

            if (type.FullName == null && type.Implements(typeof(ILogger)))
                return substitutes[typeof(ILogger)];

            var proxy = CreateSubstitute(type);

            Registry.RegisterInstance(type, proxy);
            substitutes.Add(type, proxy);
            return proxy;
        }

        private static readonly MethodInfo? SubstituteForType = typeof(InvokeNSubstitute).GetMethod(nameof(InvokeNSubstitute.For));

        protected object? CreateSubstitute(Type type)
        {
            return SubstituteForType?.MakeGenericMethod(type).Invoke(new InvokeNSubstitute(), null);
        }

        protected override void OnPostCleanup()
        {
            substitutes.Clear();
            base.OnPostCleanup();
        }
    }

    internal class InvokeNSubstitute
    {
        public object? For<T>() where T : class
        {
            return Substitute.For<T>();
        }
    }
}
