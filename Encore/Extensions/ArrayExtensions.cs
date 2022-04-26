using System.Diagnostics.CodeAnalysis;

namespace Encore
{
    public static class ArrayExtensions
    {
        public static bool NotNullOrEmpty<T>([NotNullWhen(true)] this T[]? items) => !IsNullOrEmpty(items);

        public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this T[]? items)
        {
            return items == null || items.Length == 0;
        }

        public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? items)
        {
            return items == null || !items.Any();
        }

        [return: NotNull]
        public static T[] ToSafeArray<T>(this T[]? input)
        {
            return input ?? Array.Empty<T>();
        }

        [return: NotNull]
        public static T[] ToSafeArray<T>([AllowNull] this T[]? input, Func<T, bool> filter)
        {
            return input.ToSafeArray().Where(filter).ToArray();
        }

        /// <summary>
        /// If the Array is Null it will return an Empty Array instead
        /// </summary>
        [return: NotNull]
        public static IEnumerable<T> Safe<T>([AllowNull] this IEnumerable<T>? items)
        {
            return items ?? Array.Empty<T>();
        }

        /// <summary>
        /// If the Array is Null it will return an Empty Array instead
        /// </summary>
        [return: NotNull]
        public static T[] ToSafeArray<T>([AllowNull] this IEnumerable<T>? items, Func<T, bool> where)
        {
            return items.Safe().Where(where).ToArray();
        }

        /// <summary>
        /// If the Array is Null it will return an Empty Array instead
        /// </summary>
        [return: NotNull]
        public static TDestination[] ToSafeArray<TSource, TDestination>([AllowNull] this IEnumerable<TSource> items, Func<TSource, TDestination> select) where TDestination : class
        {
            return items.Safe().Select(select).ToArray();
        }

        /// <summary>
        /// If the Array is Null it will return an Empty Array instead
        /// </summary>
        [return: NotNull]
        public static T[] ToSafeArray<T>([AllowNull] this IEnumerable<T>? items)
        {
            return items.Safe().ToArray();
        }

        /// <summary>
        /// Performs the Action for each item (Uses Safe Guard internally)
        /// </summary>
        public static void Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items.Safe())
                action(item);
        }
    }
}
