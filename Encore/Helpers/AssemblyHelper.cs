using System.Reflection;
using System.Runtime.Loader;

namespace Encore.Helpers
{
    public class AssemblyHelper
    {
        private static readonly OneTime Once = new();

        public static Type[] GetMatchingTypes(Assembly current, Type type)
        {
            var matches = GetMatchingTypes(type, type.Assembly);

            if (matches.NotNullOrEmpty())
                return matches;

            var assembles = GetAssemblies(current, current.GetPrefix());
            return GetMatchingTypes(type, assembles);
        }

        public static Type[] GetMatchingTypes(Type type, Assembly assembly)
        {
            return GetMatchingTypes(type, new[] { assembly });
        }

        public static Type[] GetMatchingTypes(Type type, IEnumerable<Assembly> assemblies)
        {
            var matches = assemblies.SelectMany(t => t.GetTypes())
                .Where(t => t.IsClass && t.GetInterfaces().Contains(type))
                .Select(v => v).ToArray();

            return matches.ToSafeArray();
        }

        public static List<(Type, T)> GetCustomAttributeClasses<T>(Assembly current)
        {
            var types = GetTypes(current, current.GetPrefix());
            return GetCustomAttributeClasses<T>(types);
        }

        public static List<(Type, T)> GetCustomAttributeClasses<T>(IEnumerable<Type> types)
        {
            return (from type in types
                    where !type.IsInterface
                    where !type.IsAbstract
                    let attrs = type.GetCustomAttributes(typeof(T), false) as T[]
                    where attrs.NotNullOrEmpty()
                    select (type, attrs.FirstOrDefault())).ToList();
        }

        public static IEnumerable<Type> GetTypes(Assembly current, string assemblyPrefix)
        {
            GetAssemblies(current, assemblyPrefix);
            return Assemblies.IsNullOrEmpty() ? Type.EmptyTypes : Assemblies.SelectMany(v => v.Value.GetTypes());
        }

        public static IEnumerable<Assembly> GetAssemblies(Assembly current, string prefix)
        {
            Once.Guard(() => LoadAssemblies(current, prefix));
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
                catch (FileLoadException) { }
                catch (BadImageFormatException) { }
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

        private static IEnumerable<Assembly> LoadedAssemblies => AssemblyLoadContext.Default.Assemblies;
    }
}