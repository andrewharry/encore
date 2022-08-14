using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Encore
{
    /// <summary>
    /// Option - A simple class to Wrap T and provide easy null checking
    /// http://www.horsdal-consult.dk/search/label/Maybe%20Monad
    /// </summary>
    public class Option
    {
        private Option()
        {
        }

        public static Option<T> Null<T>()
        {
            return new Option<T>();
        }

        public static Option<T> Create<T>(T? value)
        {
            return new Option<T>(value!);
        }
    }

    /// <summary>
    /// Option - A simple class to Wrap T and provide easy null checking
    /// http://www.horsdal-consult.dk/search/label/Maybe%20Monad
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4035:Classes implementing \"IEquatable<T>\" should be sealed", Justification = "3rd party library")]
#pragma warning disable S3897 // Classes that provide "Equals(<T>)" should implement "IEquatable<T>"
    public class Option<T> : IEnumerable<T>
#pragma warning restore S3897 // Classes that provide "Equals(<T>)" should implement "IEquatable<T>"
    {
        /// <summary>
        /// Gets the option value (set to default value if <c>HasValue</c> is <c>false</c>).
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Returns a value indicating whether or not the option has a value.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// Returns a value indicating whether or not the option has no value.
        /// </summary>
        public bool NoValue => !HasValue;

        /// <summary>
        /// Initialises an option with no value set.
        /// </summary>
        public static readonly Option<T> None = new Option<T>();

#pragma warning disable CS8601 // Possible null reference assignment.
        private static readonly T DefaultValue = default;
#pragma warning restore CS8601 // Possible null reference assignment.

        /// <summary>
        /// Creates a new option with no value set.
        /// </summary>
        public Option()
        {
            Value = DefaultValue;
            HasValue = false;
        }

        /// <summary>
        /// Creates a new option with a value.
        /// </summary>
        public Option(T value)
        {
            Value = value;
            HasValue = value != null;
        }

        public IEnumerable<T> ToEnumerable()
        {
            return HasValue ? new[] { Value } : Array.Empty<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ToEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator Option<T>(T value)
        {
            return new Option<T>(value);
        }

        public static explicit operator T(Option<T> option)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            if (option == null || option.NoValue)
                return DefaultValue;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            return option.Value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Code Smell", "S3875:\"operator==\" should not be overloaded on reference types", Justification = "3rd party library")]
        public static bool operator ==(Option<T> a, Option<T> b)
        {
            // If both are null, or both are same instance, return true.
            if (a is null)
                return b is null;

            // If one is null, but not both, return false.
            if (b is null)
                return false;

            if (a.HasValue != b.HasValue)
                return false;

            return a.NoValue || a?.Value?.Equals(b.Value) == true;
        }

        public static bool operator !=(Option<T> first, Option<T> second)
        {
            return !(first == second);
        }

        public bool Equals(Option<T> other)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            if (other == null) return false;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            if (other.HasValue != HasValue)
                return false;
            return NoValue || Equals(Value, other.Value);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Option<T> opObj)
                return Equals(opObj);
            return false;
        }

        public override int GetHashCode()
        {
#pragma warning disable S3358 // Ternary operators should not be nested
            return NoValue ? 0
                 : Value is null ? -1
                 : Value.GetHashCode();
#pragma warning restore S3358 // Ternary operators should not be nested
        }

        public override string ToString()
        {
            return HasValue ? $"Value: {Value}" : "No Value";
        }
    }
}