using Encore.Testing.Services;
using Encore.Types;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;

namespace Encore.Testing
{
    public abstract class TestWithRegistry : TestWithHooks
    {
        protected readonly Dictionary<Type, object> substitutes = new (1000);

        /// <summary>
        /// Register class instance with container
        /// </summary>
        public void RegisterInstance<TClass>(TClass instance) where TClass : class
        {
            Registry.Register(instance);
        }

        /// <summary>
        /// Register class with container
        /// </summary>
        protected void RegisterWithDependencies<TClass>() where TClass : class
        {
            RegisterByType(typeof(TClass));
        }

        protected void RegisterByType(Type type)
        {
            if (Registry.IsRegistered(type))
                return;

            InfiniteLoop();

            var types = TypeDependencies.GetDependencies(SutAssembly, type, interfacesOnly:false);

            foreach (var next in types)
            {
                if (substitutes.ContainsKey(next.Dependency))
                    continue;

                if (UseLoggerSubstitute(next.Dependency))
                    continue;

                next.DerivedTypes.Each(RegisterByType);
            }

            if (Registry.RegisterByAttributes(type))
                return;
            
            var interfaces = type.GetFilteredInterfaces();
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
                var interfaces = type.GetFilteredInterfaces();
                interfaces.Each(v => RegisterMock(v));
                return null;
            }

            if (substitutes.ContainsKey(type))
                return substitutes[type];

            if (UseLoggerSubstitute(type))
                return substitutes[typeof(ILogger)];

            var proxy = SubstituteHelper.ForType(type);

            Registry.RegisterInstance(type, proxy);
            substitutes.Add(type, proxy);
            return proxy;
        }

        private bool UseLoggerSubstitute(Type type) => type.Implements(typeof(ILogger)) && substitutes.ContainsKey(typeof(ILogger));
        
        protected override void OnPostCleanup()
        {
            substitutes.Clear();
            base.OnPostCleanup();
        }
    }
}