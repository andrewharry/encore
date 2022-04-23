namespace Encore.Testing.Services
{
    public interface IServiceResolver : IResolveClass, IDisposable
    {
        IEnumerable<T> ResolveAll<T>() where T : class;

        object Resolve(Type type);

        IEnumerable<object> ResolveAll(Type type);
    }
}
