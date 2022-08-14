using Encore.Helpers;
using Encore.Scope;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Encore
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterByAttributes(this IServiceCollection collection, Assembly application)
        {
            var types = AssemblyHelper.SearchByRegisterAttribute<RegisterAttribute>(application);

            if (types.IsNullOrEmpty())
                return;

            Register(collection, types);
        }

        public static void RegisterByAttributes(this IServiceCollection collection, Type[] types)
        {
            var items = AssemblyHelper.SearchByRegisterAttribute<RegisterAttribute>(types);

            if (items.IsNullOrEmpty())
                return;

            Register(collection, items);
        }

        public static readonly Type[] Excluding = {
            typeof(IDisposable),
            typeof(IEnumerable),
            typeof(IEnumerable<>),
            typeof(ICloneable),
            typeof(ISerializable),
            typeof(IScopedService),
            typeof(ILogger),
            typeof(ILogger<>),
        };

        public static void Register(IServiceCollection serviceCollection, List<(Type, RegisterAttribute)> items)
        {
            foreach (var (type, registerAttribute) in items)
            {
                if (registerAttribute == null)
                    continue;

                serviceCollection.TryRegister(type, registerAttribute.Life);

                if (registerAttribute.Interface != null)
                {
                    RegisterWithInterface(serviceCollection, registerAttribute, type, registerAttribute.Interface);
                    continue;
                }

                var interfaces = type.GetInterfaces(false).Except(Excluding).ToSafeArray();

                foreach (var @interface in interfaces)
                {
                    RegisterWithInterface(serviceCollection, registerAttribute, type, @interface);
                }
            }
        }

        private static bool RegisterWithInterface(IServiceCollection serviceCollection, RegisterAttribute register, Type type, Type @interface)
        {
            if (@interface == null)
                return false;

            if (register.IsCollection)            
                return serviceCollection.Register(@interface, type, register.Life);            

            return serviceCollection.TryRegister(@interface, type, register.Life);
        }

        public static bool Register<T>(this IServiceCollection serviceCollection, ServiceLifetime lifetime) where T : class
        {
            return Register(serviceCollection, typeof(T), lifetime);
        }

        public static bool Register(this IServiceCollection serviceCollection, Type type, ServiceLifetime lifetime)
        {
            return Register(serviceCollection, type, type, lifetime);
        }

        public static bool Register(this IServiceCollection serviceCollection, Type @interface, Type type, ServiceLifetime lifetime)
        {
            var count = serviceCollection.Count;
            serviceCollection.Add(new ServiceDescriptor(@interface, type, lifetime));
            return count < serviceCollection.Count;
        }

        public static bool Register(this IServiceCollection serviceCollection, Type type, object instance)
        {
            serviceCollection.Add(ServiceDescriptor.Singleton(type, sp => instance));
            return true;
        }

        public static bool Register<TInterface>(this IServiceCollection serviceCollection, TInterface instance) where TInterface : class
        {
            serviceCollection.Add(ServiceDescriptor.Singleton<TInterface>(instance));
            return true;
        }

        public static bool TryRegister(this IServiceCollection serviceCollection, Type type, ServiceLifetime lifetime)
        {
            return TryRegister(serviceCollection, type, type, lifetime);
        }

        public static bool TryRegister<TInterface>(this IServiceCollection serviceCollection, ServiceLifetime lifetime) where TInterface : class
        {
            return TryRegister(serviceCollection, typeof(TInterface), lifetime);
        }

        public static bool TryRegister<TInterface, TClass>(this IServiceCollection serviceCollection, ServiceLifetime lifetime) where TInterface : class where TClass : TInterface
        {
            return TryRegister(serviceCollection, typeof(TInterface), typeof(TClass), lifetime);
        }

        public static bool TryRegister(this IServiceCollection serviceCollection, Type @interface, Type type, ServiceLifetime lifetime)
        {
            serviceCollection.TryAdd(new ServiceDescriptor(type, type, lifetime));
            var count = serviceCollection.Count;
            serviceCollection.TryAdd(new ServiceDescriptor(@interface, type, lifetime));
            return count < serviceCollection.Count;
        }

        public static bool Register<TInterface, TClass>(this IServiceCollection serviceCollection, ServiceLifetime lifetime) where TInterface : class where TClass : TInterface
        {
            return Register(serviceCollection, typeof(TInterface), typeof(TClass), lifetime);
        }

        /// <summary>
        /// Checks to see if the Type is already registered
        /// </summary>
        public static bool IsRegistered(this IServiceCollection serviceCollection, Type type)
        {
            return serviceCollection.Any(v => v.ServiceType == type);
        }

        /// <summary>
        /// Checks to see if the Type is already registered
        /// </summary>
        public static bool IsRegistered<T>(this IServiceCollection serviceCollection)
        {
            return IsRegistered(serviceCollection, typeof(T));
        }

        public static bool NotRegistered<T>(this IServiceCollection serviceCollection)
        {
            return !IsRegistered(serviceCollection, typeof(T));
        }
    }
}
