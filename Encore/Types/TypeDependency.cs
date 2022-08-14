using Encore.Helpers;
using System;
using System.Reflection;

namespace Encore.Types;

public class TypeDependency
{
    public TypeDependency(Type dependencyType, Assembly assembly)
    {
        Dependency = dependencyType;
        DerivedTypes = dependencyType.IsInterface
            ? AssemblyHelper.GetTypesByInterface(dependencyType, assembly)
            : Array.Empty<Type>();
    }

    public Type Dependency { get; set; }

    public Type[] DerivedTypes { get; set; }
}