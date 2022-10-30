using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace Encore.IntegrationTesting
{
    public class ApiTestFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        public ApiTestFactory(Action<ConfigurationBuilder> options)        
        {
            Options = options;
        }

        public Action<ConfigurationBuilder> Options { get; }

        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = new ConfigurationBuilder();

            Options(builder);

            var root = builder.Build();

            return Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(action => action.AddConfiguration(root))
                .ConfigureWebHostDefaults(builder =>
                    builder.UseStartup<TStartup>());
        }
    }
}