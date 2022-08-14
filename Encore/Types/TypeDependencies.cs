using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TypeCache = System.Collections.Generic.Dictionary<System.Type, System.Type[]>;
using TypeDependencyCache = System.Collections.Generic.Dictionary<System.Type, Encore.Types.TypeDependency[]>;

namespace Encore.Types
{
    /// <summary>
    /// Responsible for Inspecting a Type and determining which interfaces and types it depends upon
    /// </summary>
    public class TypeDependencies
    {
        public static int CacheSize { get; set; } = 1000;

        private static readonly TypeCache typeCache = new (CacheSize);
        private static readonly TypeDependencyCache typeDependencyCache = new (CacheSize);

        [return: NotNull]
        public static Type[] GetTypes(Type type, bool interfacesOnly)
        {
            if (typeCache.ContainsKey(type))
                return typeCache[type].ToSafeArray();

            var result = TypeHelper.GetTypeDependencies(type, interfacesOnly);

            if (result.NotNullOrEmpty())
                typeCache.Add(type, result);

            return result;
        }

        [return: NotNull]
        public static TypeDependency[] GetDependencies(Assembly assembly, Type type, bool interfacesOnly)
        {
            if (typeDependencyCache.ContainsKey(type))
                return typeDependencyCache[type];
            
            var parameters = TypeHelper.GetTypeDependencies(type, interfacesOnly);
            var list = new List<TypeDependency>(parameters.Length);

            foreach (var parameter in parameters)
            {
                if (interfacesOnly) {
                    if (parameter.IsInterface)
                        list.Add(new TypeDependency(parameter, assembly));

                    continue;
                }
                
                list.Add(new TypeDependency(parameter, assembly));
            }

            var result = list.ToSafeArray();

            if (result.NotNullOrEmpty())
                typeDependencyCache.Add(type, result);

            return result;
        }
    }
}
