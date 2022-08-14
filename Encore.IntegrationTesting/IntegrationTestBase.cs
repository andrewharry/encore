using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Encore.Testing.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using System.Net.Http.Json;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Encore.Constants;

namespace Encore.IntegrationTesting
{
    [TestCategory("Integration")]
    public abstract class IntegrationTestBase<TStartup> where TStartup : class
    {
        protected readonly Dictionary<Type, object> substitutes = new Dictionary<Type, object>(1000);

        public virtual EnvironmentTypes EnvironmentType => EnvironmentTypes.Integration;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [NotNull] protected HttpClient? Client;
        [NotNull] protected TestServer? Server;
        [NotNull] private DataAccessHelper? dataAccess;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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
                var interfaces = type.GetFilteredInterfaces();
                interfaces.Each(v => RegisterMock(v));
                return null;
            }

            if (substitutes.ContainsKey(type))
                return substitutes[type];

            if (type.FullName == null && type.Implements(typeof(ILogger)))
                return substitutes[typeof(ILogger)];

            var proxy = CreateSubstitute(type);

#pragma warning disable CS8604 // Possible null reference argument.
            substitutes.Add(type, proxy);
#pragma warning restore CS8604 // Possible null reference argument.
            collection?.AddSingleton(proxy);
            return proxy;
        }

#pragma warning disable S2743 // Static fields should not be used in generic types
        private static readonly MethodInfo? SubstituteForType = typeof(InvokeNSubstitute).GetMethod(nameof(InvokeNSubstitute.For));
#pragma warning restore S2743 // Static fields should not be used in generic types

        protected object? CreateSubstitute(Type type)
        {
            return SubstituteForType?.MakeGenericMethod(type).Invoke(new InvokeNSubstitute(), null);
        }

        public async Task<string> GetApiAsString(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK)
        {
            var response = await CallApi(requestUrl, HttpMethodType.GET, expectedStatus);
            return await response.GetBody();
        }

        public Task<HttpResponseMessage> GetApi(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK, object? body = null)
        {
            return CallApi(requestUrl, HttpMethodType.GET, expectedStatus, body);
        }

        public Task<T> GetApi<T>(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK, object? body = null) where T: class
        {
            return CallApi<T>(requestUrl, HttpMethodType.GET, expectedStatus, body);
        }

        public Task<HttpResponseMessage> PostApi(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK, object? body = null)
        {
            return CallApi(requestUrl, HttpMethodType.POST, expectedStatus, body);
        }

        public Task<T> PostApi<T>(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK, object? body = null) where T : class
        {
            return CallApi<T>(requestUrl, HttpMethodType.POST, expectedStatus, body);
        }

        public Task<HttpResponseMessage> PutApi(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK, object? body = null)
        {
            return CallApi(requestUrl, HttpMethodType.PUT, expectedStatus, body);
        }

        public Task<T> PutApi<T>(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK, object? body = null) where T : class
        {
            return CallApi<T>(requestUrl, HttpMethodType.PUT, expectedStatus, body);
        }

        public Task<HttpResponseMessage> DeleteApi(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK, object? body = null)
        {
            return CallApi(requestUrl, HttpMethodType.DELETE, expectedStatus, body);
        }

        public Task<T> DeleteApi<T>(string requestUrl, HttpStatusCode expectedStatus = HttpStatusCode.OK, object? body = null) where T : class
        {
            return CallApi<T>(requestUrl, HttpMethodType.DELETE, expectedStatus, body);
        }


        public async Task<T> CallApi<T>(string requestUrl, HttpMethodType method, HttpStatusCode expectedStatus, object? body = null) where T : class
        {
            var response = await CallApi(requestUrl, method, expectedStatus, body);
            var responseBody = await response.Content.ReadAsStringAsync();
#pragma warning disable CS8603 // Possible null reference return.
            return JsonConvert.DeserializeObject<T>(responseBody);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public async Task<HttpResponseMessage> CallApi(string requestUrl, HttpMethodType method, HttpStatusCode expectedStatus, object? body = null)
        {
            var response = await CallApiInternal(requestUrl, method, body);
            Assert.IsNotNull(response);
            Assert.AreEqual(expectedStatus, response.StatusCode);
            return response;
        }

        private async Task<HttpResponseMessage> CallApiInternal(string requestUrl, HttpMethodType method, object? body)
        {
            using var client = Server.CreateClient();
            var request = new HttpRequestMessage(Convert(method), requestUrl);

            if (body != null)
                request.Content = JsonContent.Create(body);

            OnPreSend(request);

            return await client.SendAsync(request);
        }

        public virtual void OnPreSend(HttpRequestMessage request) { }

        private static HttpMethod Convert(HttpMethodType method)
        {
            return method switch
            {
                HttpMethodType.GET => HttpMethod.Get,
                HttpMethodType.POST => HttpMethod.Post,
                HttpMethodType.PATCH => HttpMethod.Patch,
                HttpMethodType.PUT => HttpMethod.Put,
                HttpMethodType.DELETE => HttpMethod.Delete,
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
            };
        }

        protected Option<TEntity> SetItem<TEntity>(Func<TEntity, bool> where, TEntity item) where TEntity : class
        {
            return dataAccess.SetItem(where, item);
        }

        protected Option<TEntity> SetItem<TEntity>(TEntity item) where TEntity : class
        {
            return dataAccess.SetItem(item);
        }

        protected TEntity[] SetItems<TEntity>(IEnumerable<TEntity> items) where TEntity : class
        {
            return dataAccess.SetItems(items.ToSafeArray());
        }

        protected TEntity[] SetItems<TEntity>(params TEntity[] items) where TEntity : class
        {
            return dataAccess.SetItems(items);
        }

        protected IEnumerable<TEntity> GetItems<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            return dataAccess.GetItems(where);
        }

        public TResult FindMax<TEntity, TResult>(Expression<Func<TEntity, TResult>> selector) where TEntity : class where TResult : struct
        {
            return Transform<TEntity, TResult>(set => set.Max(selector));
        }

        public TResult FindMax<TEntity, TResult>(Expression<Func<TEntity, TResult?>> selector) where TEntity : class where TResult : struct
        {
            return Transform<TEntity, TResult?>(set => set.Max(selector)) ?? default(TResult);
        }

        public TResult? Transform<TEntity, TResult>(Func<DbSet<TEntity>, TResult?> transform) where TEntity : class
        {
            return dataAccess.Transform(transform);
        }

        protected TEntity? FirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            return dataAccess.FirstOrDefault(where);
        }

        protected TEntity? FirstOrDefault<TEntity>() where TEntity : class
        {
            return dataAccess.FirstOrDefault<TEntity>(v => true);
        }

        protected bool DeleteItem<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            return dataAccess.DeleteItem(where);
        }

        internal class InvokeNSubstitute
        {
            [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
            public object? For<T>() where T : class
            {
                return Substitute.For<T>();
            }
        }
    }
}
