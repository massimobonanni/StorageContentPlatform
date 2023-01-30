using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StorageContentPlatform.ContentCreator.Interfaces;
using StorageContentPlatform.ContentCreator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: WebJobsStartup(typeof(StorageContentPlatform.ContentCreator.Startup))]

namespace StorageContentPlatform.ContentCreator
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.AddScoped<IContentGenerator, ContentGenerator>();
            builder.Services.AddScoped<IPersistanceProvider, StorageAccountProvider>();
        }
    }
}
