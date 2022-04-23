using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Encore
{
    public static class AssemblyExtensions
    {
        [return: NotNull]
        public static string GetPrefix(this Assembly assembly)
        {
            return assembly.GetShortName().Split('.').FirstOrDefault() ?? string.Empty;
        }

        [return: NotNull]
        public static string GetShortName(this Assembly assembly)
        {
            return assembly.GetName().Name ?? string.Empty;
        }

        [return: NotNull]
        public static string GetVersion(this Assembly assembly)
        {
            return assembly.GetName().Version?.ToString() ?? string.Empty;
        }

        [return: NotNull]
        public static string GetNameAndVersion(this Assembly assembly)
        {
            var assemblyName = assembly.GetName();
            var name = assemblyName.Name;
            var version = assemblyName.Version?.ToString() ?? string.Empty;
            return $"{name}: {version}";
        }
    }
}
