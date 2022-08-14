using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Encore.Testing.Helpers;

namespace Encore.Types;

public static class TypeHelper
{
    private static readonly Type[] GenericArrayTypes = {
        typeof(IEnumerable<>),
        typeof(ICollection<>),
        typeof(IList<>)
    };

    /// <summary>
    /// Responsible for getting the parameter types from the constructor for the given 'type'
    /// </summary>
    [return: NotNull]
    public static Type[] GetTypeDependencies(Type type, bool onlyInterfaces = true)
    {
        var constructor = ConstructorLookup.Get(type);

        if (constructor == null)
            return Array.Empty<Type>();

        return constructor.GetParameters().Safe()
            .Select(v => UnwrapGenerics(v.ParameterType))
            .Union(GetDependenciesByAttribute(type))
            .Where(v => WhereFilter(v, onlyInterfaces))
            .Distinct()
            .ToSafeArray();
    }

    [return: NotNull]
    private static Type[] GetDependenciesByAttribute(Type type)
    {
        var attribute = type.GetCustomAttributes<RegisterAttribute>().FirstOrDefault();
        return attribute?.GetDependencies() ?? Array.Empty<Type>();
    }

    private static bool WhereFilter(Type type, bool onlyInterfaces)
    {
        if (onlyInterfaces && !type.IsInterface)
            return false;

        if (onlyInterfaces && type.IsInterface)
            return !ServiceCollectionExtensions.Excluding.Contains(type);

        return true;
    }

    private static Type UnwrapGenerics([NotNull] Type type)
    {
        if (!type.IsGenericType)
            return type;

        var genericType = type.GetGenericTypeDefinition();

        if (GenericArrayTypes.Contains(genericType))
            return type.GetGenericArguments().First();

        return type;
    }
}