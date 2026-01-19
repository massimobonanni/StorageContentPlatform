using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StorageContentPlatform.ManagementFunctions.Interfaces;
using StorageContentPlatform.ManagementFunctions.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddScoped<IInventoryPersistanceService, StorageTableInventoryPersistanceService>();
        services.AddScoped<IInventoryAnalyzer, InventoryAnalyzer>();
        services.AddScoped<IPersistenceManagementService, StorageManagementService>();
        services.AddScoped<IManifestManagementService, ManifestManagementService>();
    })
    .Build();

host.Run();
