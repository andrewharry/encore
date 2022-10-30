using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    }
}
