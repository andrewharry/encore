using System;

namespace Encore
{
    /// <summary>
    /// Ref http://www.horsdal-consult.dk/2014/03/in-search-of-maybe-in-c-part-iii.html
    /// </summary>
    public static class OptionExtensions
    {
        public static TResult Match<T, TResult>(this Option<T> self, Func<T, TResult> some, Func<TResult> none)
        {
            return self.HasValue ? some.Invoke(self.Value) : none.Invoke();
        }

        public static string Match<T>(this Option<T> self, Func<T, string> some)
        {
            return self.HasValue ? some.Invoke(self.Value) : string.Empty;
        }

        public static TResult Match<T, TResult>(this Option<T> self, Func<T, TResult> some)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return self?.HasValue == true ? some.Invoke(self.Value) : default;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static void Match<T>(this Option<T> self, Action<T> some)
        {
            if (self.HasValue)
                some.Invoke(self.Value);
        }

        public static void Match<T>(this Option<T> self, Action<T> some, Action none)
        {
            if (self.HasValue && some.NotNullOrDefault())
                some.Invoke(self.Value);

            if (self.NoValue && none.NotNullOrDefault())
                none.Invoke();
        }

        public static Option<T> Then<T>(this Option<T> self, Func<T, Option<T>> some)
        {
            return self.HasValue ? some.Invoke(self.Value) : self;
        }

        public static Option<T> Else<T>(this Option<T> self, Func<Option<T>> some)
        {
            return self.NoValue ? some.Invoke() : self;
        }

        public static U Value<T, U>(this Option<T> self, Func<T, U> some, U none)
        {
            return self.Match(
              some: some,
              none: () => none
            );
        }

        public static U Unwrap<T, U>(this Option<T> self, Func<T, U> some, Func<U> none)
        {
            return self.Match(
              some: some,
              none: none
            );
        }

        public static Option<U> Map<T, U>(this Option<T> self, Func<T, U> selector)
        {
            return self.Match(
              some: v => Option.Create(selector(v)),
              none: () => Option<U>.None
            );
        }

        public static Option<T> Filter<T>(this Option<T> self, Func<T, bool> predicate)
        {
            return self.Match(
              some: v => predicate(v) ? self : Option<T>.None,
              none: () => Option<T>.None
            );
        }
    }
}