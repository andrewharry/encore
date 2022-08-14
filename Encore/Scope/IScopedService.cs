using System;

namespace Encore.Scope
{
    public interface IScopedService : IDisposable
    {
        IDisposable Scope { get; set; }
    }
}
