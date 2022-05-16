using Encore;
using Encore.Tutorials.TutorialConsole.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => {
        services.RegisterByAttributes(Assembly.GetExecutingAssembly());
        services.AddDbContext<SchoolContext>();
        services.AddHostedService<Worker>();
     })
    .Build();


await host.RunAsync();