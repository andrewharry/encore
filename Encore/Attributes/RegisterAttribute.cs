using Microsoft.Extensions.DependencyInjection;

namespace Encore
{
    public class RegisterAttribute : Attribute
    {
        public ServiceLifetime Life { get; set; }

        /// <summary>
        /// Indicates that this is the *ONLY Interface* which will be registered with the DI Container
        /// </summary>
        public Type? Interface { get; set; }

        public bool IsCollection { get; set; } = false;

        /// <summary>
        /// Used for surfacing a hidden dependency (manually resolved inside the class or factory)
        /// </summary>
        public Type? Dependency { get; set; }

        /// <summary>
        /// Used for surfacing any hidden dependencies (manually resolved inside the class or factory)
        /// </summary>
        public Type[] Dependencies { get; set; } = Array.Empty<Type>();
}
}
