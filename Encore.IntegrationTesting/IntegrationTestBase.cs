using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Encore.Testing.Services;
using Encore.Types;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;

namespace Encore.IntegrationTesting
{
    [TestCategory("Integration")]
    public abstract class IntegrationTestBase<TStartup> where TStartup : class
    {
        protected readonly Dictionary<Type, object> substitutes = new Dictionary<Type, object>(1000);

        public virtual EnvironmentTypes EnvironmentType => EnvironmentTypes.Integration;

        [NotNull] protected HttpClient? Client;
        [NotNull] protected TestServer? Server;
        [NotNull] private DataAccessHelper? dataAccess;
        protected DbContextResolver? dbContextResolver;

        public bool IsInitialised { get; set; }
        private Action<IWebHostBuilder>? configuration;
        private IServiceCollection? collection;

        [TestInitialize]
        public void ClassInitialise()
        {
            if (IsInitialised)
                return;

            IsInitialised = true;

            OnStartup();

            var builder = CreateWebBuilder();
            CreateServer(builder);
            Client = Server.CreateClient();

            Setup();
        }

        public abstract void OnStartup();

        public virtual Action<ConfigurationBuilder> OnConfiguration => (builder) => { };

        public abstract void OnShutdown();
        
        public virtual void Setup()
        {

        }

        public virtual void Registration(IServiceCollection collection)
        {

        }

        private WebApplicationFactory<TStartup> CreateWebBuilder()
        {
            var environment = EnvironmentType.ToString();
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
            var factory = new ApiTestFactory<TStartup>(OnConfiguration);

            return factory.WithWebHostBuilder(config => {
                config.ConfigureLogging(logger => logger.ClearProviders().AddConsole());
                config.UseEnvironment(environment);

                if (configuration != null)
                    configuration.Invoke(config);

                config.ConfigureServices(services => {
                    collection = services;
                    Registration(services);
                });
            });
        }

        private void CreateServer(WebApplicationFactory<TStartup> webBuilder)
        {
            Server = webBuilder.Server;
            var serviceResolver = new ServiceResolver(Server.Services);
            dbContextResolver = new DbContextResolver();
            var assembly = typeof(TStartup).Assembly;
            dbContextResolver.SetupResolver(assembly, serviceResolver, autoPopulate: true);
            dataAccess = new DataAccessHelper(serviceResolver, dbContextResolver);
        }

        protected void Configuration(Action<IWebHostBuilder> configuration)
        {
            if (this.configuration != null)
                throw new InvalidOperationException("Configuration should only be called once");
            
            this.configuration = configuration;
        }

        [TestCleanup]
        public void Cleanup()
        {
            OnShutdown();
            dbContextResolver?.Dispose();
            Server?.Dispose();
        }

        public T Resolve<T>() where T: class
        {
            return Server.Services.GetRequiredService<T>();
        }

        protected virtual TInterface RegisterMock<TInterface>() where TInterface : class
        {
            var type = typeof(TInterface);

            if (substitutes.ContainsKey(type))
                return (TInterface)substitutes[type];

            var proxy = Substitute.For<TInterface>();

            substitutes.Add(type, proxy);
            collection?.AddSingleton(proxy);
            return proxy;
        }

        protected object? RegisterMock(Type type)
        {
            if (type.IsClass)
            {
                var interfaces = type.GetInterfaces(includeInherited: false);
                interfaces.Each(v => RegisterMock(v));
                return null;
            }

            if (substitutes.ContainsKey(type))
                return substitutes[type];

            if (type.FullName == null && type.Implements(typeof(ILogger)))
                return substitutes[typeof(ILogger)];

            var proxy = CreateSubstitute(type);

            substitutes.Add(type, proxy);
            collection?.AddSingleton(proxy);
            return proxy;
        }

        private static readonly MethodInfo? SubstituteForType = typeof(InvokeNSubstitute).GetMethod(nameof(InvokeNSubstitute.For));

        protected object? CreateSubstitute(Type type)
        {
            return SubstituteForType?.MakeGenericMethod(type).Invoke(new InvokeNSubstitute(), null);
        }

        public async Task<string> GetApiAsString(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK)
        {
            var response = await CallApi(requestUrl, HttpMethodType.GET, expectedStatus);
            return await response.GetBody();
        }

        public Task<HttpResponseMessage> GetApi(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK)
        {
            return CallApi(requestUrl, HttpMethodType.GET, expectedStatus);
        }

        public Task<T> GetApi<T>(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK) where T: class
        {
            return CallApi<T>(requestUrl, HttpMethodType.GET, expectedStatus);
        }

        public Task<HttpResponseMessage> PostApi(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK)
        {
            return CallApi(requestUrl, HttpMethodType.POST, expectedStatus);
        }

        public Task<T> PostApi<T>(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK) where T : class
        {
            return CallApi<T>(requestUrl, HttpMethodType.POST, expectedStatus);
        }

        public async Task<T> CallApi<T>(string requestUrl, HttpMethodType method, HttpStatusCode expectedStatus) where T : class
        {
            var response = await CallApi(requestUrl, method, expectedStatus);
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(body);
        }

        public async Task<HttpResponseMessage> CallApi(string requestUrl, HttpMethodType method, HttpStatusCode expectedStatus)
        {
            var response = await CallApi(requestUrl, method);
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedStatus, response.StatusCode);
            return response;
        }

        public Task<HttpResponseMessage> CallApi(string requestUrl, HttpMethodType method, Action<HttpRequestMessage> configure = null)
        {
            return Server
                .CreateRequest(requestUrl)
                .And(config => { configure?.Invoke(config); })
                .SendAsync(method.ToString());
        }

        protected void SetItem<TEntity>(Func<TEntity, bool> where, TEntity item) where TEntity : class
        {
            dataAccess.SetItem(where, item);
        }

        protected void SetItems<TEntity>(IEnumerable<TEntity> items) where TEntity : class
        {
            dataAccess.SetItems(items.ToSafeArray());
        }

        protected void SetItems<TEntity>(params TEntity[] items) where TEntity : class
        {
            dataAccess.SetItems(items);
        }

        protected IEnumerable<TEntity> GetItems<TEntity>(Func<TEntity, bool> where) where TEntity : class
        {
            return dataAccess.GetItems(where);
        }

        protected TEntity? FirstOrDefault<TEntity>() where TEntity : class
        {
            return dataAccess.FirstOrDefault<TEntity>(v => true);
        }

        internal class InvokeNSubstitute
        {
            public object? For<T>() where T : class
            {
                return Substitute.For<T>();
            }
        }
    }
}
