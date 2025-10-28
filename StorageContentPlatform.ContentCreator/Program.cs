using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StorageContentPlatform.ContentCreator.Interfaces;
using StorageContentPlatform.ContentCreator.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddScoped<IContentGenerator, ContentGenerator>();
        services.AddScoped<IPersistanceProvider, StorageAccountProvider>();
    })
    .Build();

host.Run();
