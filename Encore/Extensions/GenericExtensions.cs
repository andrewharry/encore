using System.Collections.Generic;

namespace Encore
{
    public static class GenericExtensions
    {
        public static bool NoValue<T>(this T? item) where T : struct
        {
            return !item.HasValue;
        }

        public static bool IsNullOrDefault<T>(this T item)
        {
            return EqualityComparer<T>.Default.Equals(item, default(T));
        }

        public static bool NotNullOrDefault<T>(this T item)
        {
            return !IsNullOrDefault(item);
        }
    }
}
