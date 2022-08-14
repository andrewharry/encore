using NSubstitute;
using System;
using System.Reflection;

namespace Encore.Testing.Services;

public static class SubstituteHelper
{
    private static readonly MethodInfo SubstituteForType = typeof(SubstituteHelper).GetMethod(nameof(SubstituteHelper.For), BindingFlags.Public | BindingFlags.Static)!;

    public static object ForType(Type type)
    {
        return SubstituteForType.MakeGenericMethod(type).Invoke(null, null)!;
    }

    public static object? For<T>() where T : class
    {
        return Substitute.For<T>();
    }
}