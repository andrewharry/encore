using Encore.Testing.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;

namespace Encore.Testing
{
    /// <summary>
    /// Supports Unit Tests where all of the dependencies are real (not mocked) except for the database (In Memory) and HttpClient
    /// </summary>
    public abstract class TestWithDependencies<T> : TestWithEfCore where T : class
    {
        /// <summary>
        /// Gets an instance of the System Under Test (the class being tested)
        /// </summary>
        [NotNull]
        public T? Sut { get; private set; }

        protected bool UseMockedHttpClient { get; set; } = true;

        protected override void OnSutRegistration()
        {
            Register<T>();
        }

        protected override void OnSutResolve()
        {
            Sut = Resolve<T>();
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            Registry.Register<IHostEnvironment>(new HostingEnvironment { EnvironmentName = Environments.Development });

            if (UseMockedHttpClient)
            {
                httpMockHandler = new HttpMockHandler();

                RegisterMock<IHttpClientFactory>()
                    .CreateClient(Arg.Any<string>())
                    .ReturnsForAnyArgs(new HttpClient(httpMockHandler));
            }
        }

        private HttpMockHandler? httpMockHandler;

        public void SetHttpResponseMessage(HttpResponseMessage httpMessage)
        {
            if (UseMockedHttpClient == false)
                throw new InvalidOperationException($"{nameof(UseMockedHttpClient)} is set to false");

            if (httpMessage == null)
                throw new ArgumentNullException(nameof(httpMessage));

            if (httpMockHandler == null)
                throw new ArgumentNullException(nameof(httpMockHandler));

            if (httpMockHandler != null)
                httpMockHandler.Response = httpMessage;
        }
    }
}
