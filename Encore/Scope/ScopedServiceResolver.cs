using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Encore.Scope
{
    [Singleton]
    public class ScopedServiceResolver : IScopedServiceResolver
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<ScopedServiceResolver> logger;

        public ScopedServiceResolver(IServiceScopeFactory scopeFactory, ILogger<ScopedServiceResolver> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        public TInterface GetRequiredService<TInterface>() where TInterface : IScopedService
        {
            var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TInterface>();
            service.Scope = scope;
            return service;
        }

        public TInterface GetRequiredService<TInterface>(Type type) where TInterface : class, IScopedService
        {
            var @interface = typeof(TInterface);

            if (!type.Implements(@interface))
            {
                var msg = $"The class {type.Name} doesn't implement the interface:{@interface.Name}";
                logger.LogWarning(msg);
                throw new NotSupportedException(msg);
            }

            var scope = scopeFactory.CreateScope();

            if (scope.ServiceProvider.GetRequiredService(type) is TInterface service)
            {
                service.Scope = scope;
                return service;
            }

            var msg2 = $"There was an issue resolving the class {type.Name}";
            logger.LogWarning(msg2);
            throw new NotSupportedException(msg2);
        }

        public TService GetRequiredService<TService>(Func<TService, bool> @where) where TService : class, IScopedService
        {
            var scope = scopeFactory.CreateScope();
            var services = scope.ServiceProvider.GetServices<TService>().ToSafeArray();

            foreach (var service in services)
            {
                service.Scope = scope;
            }

            var match = services.FirstOrDefault(@where);

            if (match == null)
            {
                var msg = $"No suitable classes where found for the interface:{typeof(TService).Name}";
                logger.LogWarning(msg);
                throw new NotSupportedException(msg);
            }

            return match;
        }

        public TService? GetService<TService>(Type type) where TService : class, IScopedService
        {
            var interfaceType = typeof(TService);

            if (!type.Implements(interfaceType))
            {
                var msg = $"The class {type.Name} doesn't implement the interface:{interfaceType.Name}";
                logger.LogWarning(msg);
                return null;
            }

            var scope = scopeFactory.CreateScope();

            if (scope.ServiceProvider.GetService(type) is TService service)
            {
                service.Scope = scope;
                return service;
            }

            return null;
        }

        public TService? GetService<TService>() where TService : class, IScopedService
        {
            var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetService<TService>();

            if (service == null)
                return null;

            service.Scope = scope;
            return service;
        }
    }
}
