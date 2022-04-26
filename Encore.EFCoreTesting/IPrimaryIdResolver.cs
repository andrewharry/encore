namespace Encore.EFCoreTesting
{
    public interface IPrimaryIdResolver
    {
        object Resolve<TEntity>(TEntity item) where TEntity : class;
    }
}