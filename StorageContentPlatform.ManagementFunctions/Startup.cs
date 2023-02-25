using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

using StorageContentPlatform.ManagementFunctions;
using StorageContentPlatform.ManagementFunctions.Interfaces;
using StorageContentPlatform.ManagementFunctions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: WebJobsStartup(typeof(Startup))]

namespace StorageContentPlatform.ManagementFunctions
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.AddScoped<IInventoryPersistanceService, StorageTableInventoryPersistanceService>();
            builder.Services.AddScoped<IInventoryAnalyzer, InventoryAnalyzer>();
            builder.Services.AddScoped<IPersistenceManagementService, StorageManagementService>();
        }
    }
}
