using Microsoft.Extensions.DependencyInjection;

namespace Encore.Testing.Services
{
    public interface IRegisterClass
    {
        void Register<TInterface, TClass>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TInterface : class where TClass : TInterface;
    }
}
