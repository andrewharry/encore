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
        [return: NotNull]
        public static Type[] GetFilteredInterfaces(this Type type)
        {
            return type
                .GetInterfaces()
                .Safe()
                .Except(ServiceCollectionExtensions.Excluding)
                .ToSafeArray();
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