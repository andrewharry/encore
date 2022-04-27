namespace Encore.Testing.Services
{
    public interface IResolveClass
    {
        T Resolve<T>() where T : class;

        T? TryResolve<T>() where T : class;

        object? TryResolve(Type type);
    }
}