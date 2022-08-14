using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Encore
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns all of the Interfaces implemented by the type.
        /// </summary>
        /// <param name="includeInherited">Option to exclude the Base Type interfaces</param>
        [return: NotNull]
        public static IEnumerable<Type> GetInterfaces(this Type type, bool includeInherited)
        {
            if (includeInherited || type.BaseType == null)
                return type.GetInterfaces();
            return type.GetInterfaces().Except(type.BaseType.GetInterfaces()).Except(ServiceCollectionExtensions.Excluding);
        }

        /// <summary>
        /// Checks to see if the 'type' class implements the 'target' class
        /// </summary>
        public static bool Implements(this Type type, Type target)
        {
            if (type == null || target == null) return false;
            return type.GetInterfaces().Contains(target);
        }
    }
}