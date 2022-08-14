using Encore.Testing.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

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
        public T? Sut { get; protected set; }

        protected bool UseMockedHttpClient { get; set; } = true;

        protected override void OnSutRegistration()
        {
            RegisterWithDependencies<T>();
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
                var httpClient = new HttpClient(httpMockHandler);

                httpClient.BaseAddress = new Uri("https://localhost/");

                RegisterMock<IHttpClientFactory>()
                    .CreateClient(Arg.Any<string>())
                    .ReturnsForAnyArgs(httpClient);

                Registry.Register<HttpClient>(httpClient);
            }
        }

        private HttpMockHandler? httpMockHandler;

        public void SetHttpResponseMessage(object response)
        {
            SetHttpResponseMessage(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(response)
            });
        }

        public void SetHttpResponseMessage(HttpResponseMessage httpMessage)
        {
            if (!UseMockedHttpClient)
                throw new InvalidOperationException($"{nameof(UseMockedHttpClient)} is set to false");

            if (httpMessage == null)
                throw new ArgumentNullException(nameof(httpMessage));

            if (httpMockHandler == null)
                throw new NullReferenceException(nameof(httpMockHandler));

            httpMockHandler.Response = httpMessage;
        }
    }
}
