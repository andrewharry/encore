using System;

namespace Encore
{
    public static class RegisterAttributeExtensions
    {
        public static bool HasDependencies(this RegisterAttribute attribute)
        {
            if (attribute == null)
                return false;

            if (attribute.Dependency != null)
                return true;

            return attribute.Dependencies.NotNullOrEmpty();
        }

        public static Type[] GetDependencies(this RegisterAttribute attribute)
        {
            if (attribute == null)
                return Array.Empty<Type>();

            return attribute.Dependency != null
                ? new[] { attribute.Dependency } 
                : attribute.Dependencies.ToSafeArray();
        }
    }
}