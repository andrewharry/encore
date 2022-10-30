using Encore.Testing.Services;
using Microsoft.Extensions.DependencyInjection;
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
            RegisterWithSubstitutes(typeof(T));
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

            var types = TypeDependencies.GetDependenciesByInterfaces(SutAssembly, type);

            if (types.IsNullOrEmpty())
                return;

            foreach (var next in types)
            {
                if (Registry.IsRegistered(next))
                    continue;

                RegisterMock(next);
            }
        }
    }
}
