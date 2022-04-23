namespace Encore.Scope
{
    public interface IScopedServiceResolver
    {
        TService GetRequiredService<TService>() where TService : IScopedService;
        TService GetRequiredService<TService>(Type type) where TService : class, IScopedService;
        TService GetRequiredService<TService>(Func<TService, bool> @where) where TService : class, IScopedService;
        TService? GetService<TService>(Type type) where TService : class, IScopedService;
        TService? GetService<TService>() where TService : class, IScopedService;
    }
}
