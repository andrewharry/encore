using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Encore.Helpers
{
    public static class AssemblyHelper
    {
        private static IEnumerable<Assembly> LoadedAssemblies => AssemblyLoadContext.Default.Assemblies;
        private static readonly OneTime OnceOnly = new();
        
        [return: NotNull]
        public static Type[] GetTypesByInterface(Type interfaceType, Assembly assembly)
        {
            var types = GetTypes(assembly, assembly.GetPrefix());

            return types
                .Where(t => t.IsClass && t.GetInterfaces().Contains(interfaceType))
                .Select(v => v)
                .ToSafeArray();
        }

        [return: NotNull]
        public static Type[] GetTypesByBaseClass(Type baseClass, Assembly assembly)
        {
            var types = GetTypes(assembly, assembly.GetPrefix());

            return types
                .Where(t => t.IsClass && t.BaseType == baseClass)
                .Select(v => v)
                .ToSafeArray();
        }

        public static List<(Type, T)> SearchByClassAttribute<T>(Assembly current) where T: Attribute
        {
            var types = GetTypes(current, current.GetPrefix());
            return SearchByClassAttribute<T>(types);
        }

        public static List<(Type, T)> SearchByClassAttribute<T>(IEnumerable<Type> types) where T : Attribute
        {
            return (from type in types
                    where !type.IsInterface
                    where !type.IsAbstract
                    let attrs = type.GetCustomAttributes(typeof(T), false) as T[]
                    where attrs.NotNullOrEmpty()
                    select (type, attrs.FirstOrDefault())).ToList();
        }

        public static List<(Type, T[])> SearchByMethodAttribute<T>(Assembly current, BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance) where T : Attribute
        {
            var types = GetTypes(current, current.GetPrefix());
            return SearchByMethodAttribute<T>(types, flags);
        }

        public static List<(Type, T[])> SearchByMethodAttribute<T>(IEnumerable<Type> types, BindingFlags flags) where T : Attribute
        {
            var result = new List<(Type, T[])>();

            foreach (var type in types)
            {
                if (type.IsInterface || type.IsAbstract)
                    continue;

                var methods = type.GetMethods(flags);

                if (methods.IsNullOrEmpty())
                    continue;

#pragma warning disable CS8603 // Possible null reference return.
                var attrs = methods.SelectMany(v => v.GetCustomAttributes(typeof(T), false) as T[]).ToSafeArray();
#pragma warning restore CS8603 // Possible null reference return.

                if (attrs.IsNullOrEmpty())
                    continue;

                result.Add((type, attrs));
            }

            return result;
        }

        public static IEnumerable<Type> GetTypes(Assembly current, string assemblyPrefix)
        {
            GetAssemblies(current, assemblyPrefix);
            return Assemblies.SelectMany(v => v.Value.GetTypes());
        }

        /// <summary>
        /// Eagerly Loads all of the Assemblies in the bin folder where the Assembly filename starts with the given Prefix.
        /// i.e. All encore.*.dll where the prefix is 'encore'
        /// </summary>
        public static IEnumerable<Assembly> GetAssemblies(Assembly current, string prefix)
        {
            OnceOnly.Guard(() => LoadAssemblies(current, prefix));
            return Assemblies.Values;
        }

        public static readonly Dictionary<string, Assembly> Assemblies = new();

        private static void LoadAssemblies(Assembly current, string prefix)
        {
            AddAssembly(current, prefix);

            var existing = LoadedAssemblies
                .ToSafeArray(v => v.FullName?.StartsWith(prefix) == true);

            foreach (var exist in existing)
            {
                AddAssembly(exist, prefix);
            }

            var path = AppDomain.CurrentDomain.BaseDirectory;
            if (!path.Contains("\\bin"))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
                if (!Directory.Exists(path))
                    path = AppDomain.CurrentDomain.BaseDirectory;
            }

            foreach (var dll in Directory.GetFiles(path, prefix + "*.dll", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var file = new FileInfo(dll);
                    if (file.Exists)
                        AddAssembly(file, dll, prefix);
                }
                catch (FileLoadException fileException) {
                    Console.WriteLine(fileException.Message);
                }
                catch (BadImageFormatException badImageException) {
                    Console.WriteLine(badImageException.Message);
                }
            }
        }

        private static void AddAssembly(FileInfo file, string path, string prefix)
        {
            var name = file.Name.Replace(".dll", "");

            var match = LoadedAssemblies.FirstOrDefault(v => v.GetName().Name == name);
            AddAssembly(match != null ? match : Assembly.LoadFile(path), prefix);
        }

        private static void AddAssembly(Assembly assembly, string prefix)
        {
            if (Assemblies.ContainsKey(assembly.GetShortName()))
                return;

            Assemblies.Add(assembly.GetShortName(), assembly);

            foreach (var item in assembly.GetReferencedAssemblies())
                if (item.FullName.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                    AddAssembly(LoadAssembly(item), prefix);
        }

        private static Assembly LoadAssembly(AssemblyName assemblyName)
        {
            var match = LoadedAssemblies.FirstOrDefault(v => v.FullName == assemblyName.FullName);
            if (match != null)
                return match;

            return AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
        }
    }
}