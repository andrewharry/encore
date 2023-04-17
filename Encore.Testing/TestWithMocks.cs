using Encore.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Encore.Testing
{
    /// <summary>
    /// Supports the traditional Unit Test where all of the dependencies are mocked (NSubstitute)
    /// </summary>
    public abstract class TestWithMocks<T> : TestWithLogs where T : class
    {
        /// <summary>
        /// Gets an instance of the System Under Test (the class being tested)
        /// </summary>
        [NotNull]
        public T? Sut { get; private set; }

        protected override void OnSutRegistration()
        {
            var type = typeof(T);
            SutAssembly = type.Assembly;
            RegisterWithSubstitutes(type);
            Registry.Register<T>(ServiceLifetime.Transient);
        }

        protected override void OnSutResolve()
        {
            Sut = Resolve<T>();
        }

        protected void SubstituteBind<TInterface>(TInterface instance) where TInterface : class
        {
            Registry.Register<TInterface>(instance);
        }

        protected void SubstituteBind<TInterface, TClass>() where TInterface : class where TClass : TInterface
        {
            RegisterWithSubstitutes(typeof(TClass));
            Registry.Register<TInterface, TClass>(ServiceLifetime.Transient);
        }

        private void RegisterWithSubstitutes(Type type)
        {
            if (Registry.IsRegistered(type))
                return;

            var types = TypeDependencies.GetTypes(type, interfacesOnly:true);

            if (types.IsNullOrEmpty())
                return;

            foreach (var next in types)
            {
                if (next.Implements(typeof(ILogger)))
                    continue;

                if (Registry.IsRegistered(next))
                    continue;

                RegisterMock(next);
            }
        }
    }
}
