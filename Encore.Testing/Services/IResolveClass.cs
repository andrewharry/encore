namespace Encore.Testing.Services
{
    public interface IResolveClass
    {
        T Resolve<T>() where T : class;
    }
}