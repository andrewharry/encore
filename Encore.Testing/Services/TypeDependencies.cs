using System.Reflection;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Encore.Helpers;
using TypeDictionary = System.Collections.Generic.Dictionary<System.Type, System.Type[]>;

namespace Encore.Testing.Services
{
    /// <summary>
    /// Responsible for Inspecting a Type and determining which Types it depends on
    /// </summary>
    public class TypeDependencies
    {
        public static Type[] IgnoreInterfaces { get; set; } = Array.Empty<Type>();
        public static int CacheSize { get; set; } = 20000;

        private static TypeDictionary classCache = new TypeDictionary(CacheSize);
        private static TypeDictionary interfaceCache = new TypeDictionary(CacheSize);

        private static readonly Type genericArray = typeof(IEnumerable<>);
        private static Type[] empty = Array.Empty<Type>();

        public static Type[] GetDependencies(Type type, bool includeDependenciesByAttribute = true)
        {
            return GetTypesInternal(type, classCache, interfacesOnly: false, includeDependenciesByAttribute);
        }

        public static Type[] GetInterfaces(Type type)
        {
            return GetTypesInternal(type, interfaceCache, interfacesOnly: true, includeDependenciesByAttribute: false);
        }

        private static Type[] GetTypesInternal(Type type, TypeDictionary cache, bool interfacesOnly, bool includeDependenciesByAttribute)
        {
            if (type == null)
                return empty;

            #if DEBUG
            var stack = new StackTrace().GetFrames();

            if (stack.Length > 60)
            {
                Console.WriteLine("Potential Infinite Loop - Stackoverflow imminent");
                Debugger.Break();
            }
            #endif

            if (cache?.ContainsKey(type) == true)
                return cache[type];

            var constructor = GetConstructor(type);

            if (constructor == null)
                return empty;

            var parameters = constructor.GetParameters().ToSafeArray(v => v.ParameterType);
            var types = new List<Type>(parameters.Length);

            types.AddRange(parameters.Where(v => !v.IsGenericType));
            types.AddRange(parameters.Where(v => v.IsGenericType && v.GetGenericTypeDefinition() == genericArray).Select(v => v.GetGenericArguments()[0]));

            var interfaces = types.Where(v => v.IsInterface)
                .Except(IgnoreInterfaces)
                .ToSafeArray();

            if (interfacesOnly)
            {
                cache?.Add(type, interfaces);
                return interfaces;
            }

            if (includeDependenciesByAttribute)
                types.AddRange(GetDependenciesByAttribute(type));

            interfaces = interfaces
                .SelectMany(v => AssemblyHelper.GetMatchingTypes(Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), v))
                .ToSafeArray();

            var classes = types.Where(v => v.IsClass && !v.IsInterface);
            var values = interfaces.Union(classes).Distinct().ToSafeArray();

            cache?.Add(type, values);
            return values;
        }

        /// <summary>
        /// Responsible for selecting the best constructor for the given 'type'
        /// </summary>
        public static ConstructorInfo? GetConstructor(Type type)
        {
            var constructors = type.GetConstructors().Safe().OrderByDescending(v => v.GetParameters()?.Length ?? 0).ToSafeArray();

            foreach (var constructor in constructors)
            {
                if (constructor.GetCustomAttribute<DoNotSelectAttribute>() != null)
                    continue;

                return constructor;
            }

            return null;
        }

        public static Type[] GetDependenciesByAttribute(Type type)
        {
            var attribute = type.GetCustomAttributes<RegisterAttribute>().FirstOrDefault();
            return attribute?.GetDependencies() ?? empty;
        }
    }
}
