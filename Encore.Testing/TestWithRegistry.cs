using Encore.Testing.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

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
        public new void Register<TInterface, TClass>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TInterface : class where TClass : TInterface
        {
            RegisterByType(typeof(TClass));
            Registry.Register<TInterface, TClass>(lifetime);
        }

        /// <summary>
        /// Register class with container
        /// </summary>
        protected void Register<TClass>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TClass : class
        {
            RegisterByType(typeof(TClass));
            Registry.Register<TClass>(lifetime);
        }

        protected void RegisterByType(Type type)
        {
            if (Registry.IsRegistered(type))
                return;

            var types = TypeDependencies.GetDependencies(type);

            foreach (var next in types)
            {
                if (Registry.IsRegistered(next))
                    continue;

                if (next.BaseType != null)
                {
                    RegisterMock(next);
                    continue;
                }

                if (ShouldMock(next))
                {
                    RegisterMock(next);
                    continue;
                }

                RegisterByType(next);
            }

            RegisterByAttributes(types);
        }

        protected virtual bool ShouldMock(Type next)
        {
            return false;
        }

        protected virtual object? OnRegisterMock(Type type)
        {
            return null;
        }

        protected virtual TInterface RegisterMock<TInterface>() where TInterface : class
        {
            var type = typeof(TInterface);

            if (substitutes.ContainsKey(type))
                return (TInterface)substitutes[type];

            var overrideType = OnRegisterMock(type);

            if (overrideType != null)
                return (TInterface)overrideType;

            var proxy = Substitute.For<TInterface>();

            substitutes.Add(type, proxy);
            Registry.Register(proxy);
            return proxy;
        }

        protected object? RegisterMock(Type type)
        {
            var overrideType = OnRegisterMock(type);

            if (overrideType != null)
                return overrideType;

            if (!type.IsInterface)
            {
                var interfaces = type.GetInterfaces(includeInherited: false);
                interfaces.Each(v => RegisterMock(v));
                return null;
            }

            if (substitutes.ContainsKey(type))
                return substitutes[type];

            var proxy = Substitute.For(new[] { type }, null);

            Registry.RegisterInstance(type, proxy);
            substitutes.Add(type, proxy);
            return proxy;
        }

        protected override void OnPostCleanup()
        {
            substitutes.Clear();
            base.OnPostCleanup();
        }
    }
}
