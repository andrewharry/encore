using Microsoft.Extensions.DependencyInjection;
using System;

namespace Encore
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAttribute : Attribute
    {
        public ServiceLifetime Life { get; set; }

        /// <summary>
        /// Indicates that this is the *ONLY Interface* which will be registered with the DI Container.
        /// Useful when the class has an inherited interface which would otherwise be ignored.
        /// </summary>
        public Type? Interface { get; set; }

        /// <summary>
        /// Indicates whether there are more than one implementation of the service
        /// </summary>
        public bool IsCollection { get; set; } = false;

        /// <summary>
        /// Used for surfacing a hidden dependency (manually resolved inside the class or factory)
        /// You can not use both the 'Dependency' and the 'Dependencies' at the same time
        /// </summary>
        public Type? Dependency { get; set; }

        /// <summary>
        /// Used for surfacing any hidden dependencies (manually resolved inside the class or factory).
        /// You can not use both the 'Dependency' and the 'Dependencies' at the same time
        /// </summary>
        public Type[] Dependencies { get; set; } = Array.Empty<Type>();
}
}
