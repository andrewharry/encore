using Encore.Testing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Encore.Testing
{
    public abstract class IntegrationTestAll : TestingBase, IDisposable
    {
        public virtual bool UseSubstituteHttpClient => true;

        private readonly List<object> tracking = new List<object>(1000);
        private readonly Dictionary<Type, object> substitutes = new Dictionary<Type, object>(1000);

        protected ILogger? logger;

        private bool initialised;
        private bool finalised;

        protected IntegrationTestAll()
        {
            Initialise();
        }

        [TestInitialize]
        public void Initialise()
        {
            if (initialised)
                return;

            initialised = true;
            SetupRegistry();

            OnSetup();
            OnPreInitialise();
            OnSutRegistration();

            SetupResolver();
            OnPostResolver();

            OnSutResolve();
            OnPostInitialise();
        }

        protected virtual void OnSetup()
        {
            logger = RegisterMock<ILogger>();
        }

        protected virtual void OnSutRegistration()
        {

        }

        protected virtual void OnPostResolver()
        {

        }

        protected virtual void OnSutResolve()
        {

        }

        /// <summary>
        /// Executes custom test initialisation before the standard initialisation.
        /// </summary>
        protected virtual void OnPreInitialise()
        {

        }

        /// <summary>
        /// Executes custom test initialisation after the standard initialisation.
        /// </summary>
        /// <remarks>
        /// Resolve the system-under-test instance here from the container (e.g. "sut = ServiceProvider.GetRequiredService<ISutType>()")
        /// </remarks>
        protected virtual void OnPostInitialise()
        {
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (finalised)
                return;

            finalised = true;

            OnPreCleanup();

            foreach (var item in tracking)
            {
                if (item == null)
                    continue;

                if (item is IDisposable disposable)
                    disposable.Dispose();
            }

            tracking.Clear();
            substitutes.Clear();
            OnPostCleanup();
            base.Dispose();
        }

        /// <summary>
        /// Executes custom test cleanup before the standard cleanup.
        /// </summary>
        protected virtual void OnPreCleanup()
        {
        }

        /// <summary>
        /// Executes custom test cleanup after the standard cleanup.
        /// </summary>
        protected virtual void OnPostCleanup()
        {
        }

        /// <summary>
        /// Register class instance with container
        /// </summary>
        public void Register<TClass>(TClass instance) where TClass : class
        {
            Registry.Register(instance);
        }

        public new void Register<TInterface, TClass>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TInterface : class where TClass : TInterface
        {
            RegisterClass(typeof(TClass));
            Registry.Register<TInterface, TClass>(lifetime);
        }

        /// <summary>
        /// Register class instance with container
        /// </summary>
        protected void Register<TClass>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TClass : class
        {
            RegisterClass(typeof(TClass));
            Registry.Register<TClass>(lifetime);
        }

        protected void RegisterClass(Type type)
        {
            if (Registry.IsRegistered(type))
                return;

            var types = TypeDependencies.GetDependencies(type);

            foreach (var next in types)
            {
                if (Registry.IsRegistered(next))
                    continue;

                if (next.BaseType != null)
                {
                    RegisterMock(next);
                    continue;
                }

                RegisterClass(next);
            }

            RegisterByAttributes(types);
        }

        protected TInterface RegisterMock<TInterface>() where TInterface : class
        {
            var type = typeof(TInterface);

            if (type == typeof(ILogger<>) && logger != null)
                return (TInterface)logger;

            if (type == typeof(ILogger) && logger != null)
                return (TInterface)logger;

            if (substitutes.ContainsKey(type))
                return (TInterface)substitutes[type];

            var proxy = Substitute.For<TInterface>();

            substitutes.Add(type, proxy);
            Registry.Register(proxy);
            return proxy;
        }

        protected object? RegisterMock(Type type)
        {
            if (!type.IsInterface)
            {
                var interfaces = type.GetInterfaces(includeInherited: false);
                interfaces.Each(v => RegisterMock(v));
                return null;
            }

            if (substitutes.ContainsKey(type))
                return substitutes[type];

            var proxy = Substitute.For(new[] { type }, null);

            Registry.RegisterInstance(type, proxy);
            substitutes.Add(type, proxy);
            return proxy;
        }

        /// <summary>
        /// Returns an Instance of T from the container
        /// </summary>
        public new T Resolve<T>() where T : class
        {
            T item = Resolver.Resolve<T>();

            tracking.Add(item);

            return item;
        }

        protected void Release(object instance)
        {
            if (instance is IDisposable disposable)
                disposable.Dispose();
        }


        protected internal void ClearReceivedLogCalls()
        {
            logger?.ClearReceivedCalls();
        }

        protected internal void ExpectLogCriticalWithException(int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Critical, Arg.Any<string>(), Arg.Any<Exception>());
        }

        protected internal void ExpectLogCritical(int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Critical, Arg.Any<string>());
        }

        protected internal void ExpectLogError(int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Error, Arg.Any<string>());
        }

        protected internal void ExpectLogError(string message, int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Error, message);
        }

        protected internal void ExpectLogError(string message, Exception exception, int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Error, message, exception);
        }

        protected internal void ExpectLogWarning(int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Warning, Arg.Any<string>());
        }

        protected internal void ExpectLogWarning(string message, int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Warning, message);
        }

        protected internal void ExpectLogInfo(int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Information, Arg.Any<string>());
        }

        protected internal void ExpectLogInfo(string message, int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Information, message);
        }

        protected internal void ExpectLogInfo(Func<string, bool> expected, int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Information, Arg.Is<string>(v => expected(v)));
        }

        protected internal void ExpectLogDebug(string message, int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Debug, message);
        }

        protected internal void ExpectLogDebug(int count = 1)
        {
            logger?.Received(count).Log(LogLevel.Debug, Arg.Any<string>());
        }

        protected internal void ExpectLogDebug(Func<string, bool> assert)
        {
            logger?.Received().Log(LogLevel.Debug, Arg.Is<string>(v => assert(v)));
        }

        public new void Dispose()
        {
            Cleanup();
            base.Dispose();
        }
    }
}
