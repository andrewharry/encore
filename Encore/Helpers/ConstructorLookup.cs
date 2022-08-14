using System.Reflection;
using System;
using System.Linq;

namespace Encore.Testing.Helpers;

public static class ConstructorLookup
{
    /// <summary>
    /// Responsible for selecting the best constructor for the given 'type'
    /// </summary>
    public static ConstructorInfo? Get(Type type)
    {
        foreach (var constructor in GetConstructors(type))
        {
            if (constructor.GetCustomAttribute<DoNotSelectAttribute>() != null)
                continue;

            return constructor;
        }

        return null;
    }

    private static ConstructorInfo[] GetConstructors(Type type)
    {
        return type.GetConstructors().Safe()
            .OrderByDescending(v => v.GetParameters()?.Length ?? 0)
            .ToSafeArray();
    }
}