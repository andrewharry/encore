using System.Reflection;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Encore.Helpers;

namespace Encore.Testing.Services
{
    /// <summary>
    /// Responsible for Inspecting a Type and determining which Types it is dependent on
    /// </summary>
    public class TypeDependencies
    {
        private static readonly Type genericArray = typeof(IEnumerable<>);
        private static Type[] ignoreInterfaces = new[] { typeof(ILogger), typeof(ILogger<>) };
        public static Dictionary<Type, Type[]> ClassDependencyCache = new Dictionary<Type, Type[]>(20000);
        public static Dictionary<Type, Type[]> InterfaceDependencyCache = new Dictionary<Type, Type[]>(20000);

        public static Type[] GetDependencies(Type type, bool includeDependenciesByAttribute = true)
        {
            if (type == null)
                return Array.Empty<Type>();

#if DEBUG
            var stack = new StackTrace().GetFrames();

            if (stack.Length > 60)
            {
                Console.WriteLine("Potential Infinite Loop - Stackoverflow imminent");
                Debugger.Break();
            }
#endif

            if (ClassDependencyCache?.ContainsKey(type) == true)
                return ClassDependencyCache[type];

            var constructor = GetConstructor(type);
            var parameters = (constructor?.GetParameters() ?? Array.Empty<ParameterInfo>()).Select(v => v.ParameterType).ToSafeArray();
            var types = new List<Type>(parameters.Length);

            types.AddRange(parameters.Where(v => !v.IsGenericType));
            types.AddRange(parameters.Where(v => v.IsGenericType && v.GetGenericTypeDefinition() == genericArray).Select(v => v.GetGenericArguments()[0]));
            if (includeDependenciesByAttribute)
                types.AddRange(GetDependenciesByAttribute(type));

            var interfaces = types.Where(v => v.IsInterface)
                .Except(ignoreInterfaces)
                .SelectMany(v => AssemblyHelper.GetMatchingTypes(Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), v))
                .ToSafeArray();

            var classes = types.Where(v => v.IsClass && !v.IsInterface);
            var values = interfaces.Union(classes).Distinct().ToSafeArray();

            ClassDependencyCache?.Add(type, values);
            return values;
        }

        public static ConstructorInfo? GetConstructor(Type type)
        {
            foreach (var constructor in type.GetConstructors())
            {
                var doNotSelect = constructor.GetCustomAttribute<DoNotSelectAttribute>();

                if (doNotSelect != null)
                    continue;

                return constructor;
            }

            return null;
        }

        public static Type[] GetDependenciesByAttribute(Type type)
        {
            var attribute = type.GetCustomAttributes<RegisterAttribute>().FirstOrDefault();
            return attribute?.GetDependencies() ?? Array.Empty<Type>();
        }

        public static Type[] GetInterfaces(Type type)
        {
            if (InterfaceDependencyCache?.ContainsKey(type) == true)
                return InterfaceDependencyCache[type];

            var constructor = type.GetConstructors().FirstOrDefault();
            var parameters = (constructor?.GetParameters() ?? Array.Empty<ParameterInfo>()).Select(v => v.ParameterType).ToSafeArray();
            var types = new List<Type>(parameters.Length);

            types.AddRange(parameters.Where(v => !v.IsGenericType));
            types.AddRange(parameters.Where(v => v.IsGenericType && v.GetGenericTypeDefinition() == genericArray).Select(v => v.GetGenericArguments()[0]));

            var interfaces = types.ToSafeArray(v => v.IsInterface);
            InterfaceDependencyCache?.Add(type, interfaces);
            return interfaces;
        }
    }
}
