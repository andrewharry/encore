using System.Reflection;

namespace Encore
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetInterfaces(this Type type, bool includeInherited)
        {
            if (includeInherited || type.BaseType == null)
                return type.GetInterfaces();
            return type.GetInterfaces().Except(type.BaseType.GetInterfaces());
        }

        public static bool Implements(this Type type, Type target)
        {
            if (type == null || target == null) return false;
            return type.GetInterfaces().Contains(target);
        }
    }
}