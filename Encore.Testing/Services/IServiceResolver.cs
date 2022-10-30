using System;
using System.Collections.Generic;

namespace Encore.Testing.Services
{
    public interface IServiceResolver : IResolveClass, IDisposable
    {
        IServiceProvider Container { get; }

        IEnumerable<T> ResolveAll<T>() where T : class;

        object Resolve(Type type);

        IEnumerable<object> ResolveAll(Type type);
    }
}
