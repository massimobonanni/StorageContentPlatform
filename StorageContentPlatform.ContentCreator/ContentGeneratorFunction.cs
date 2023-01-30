using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageContentPlatform.ContentCreator.Interfaces;

namespace StorageContentPlatform.ContentCreator
{
    public class ContentGeneratorFunction
    {
        private readonly IConfiguration configuration;
        private readonly IContentGenerator contentGenerator;
        
        public ContentGeneratorFunction(IConfiguration configuration,
            IContentGenerator contentGenerator)
        {
            this.configuration = configuration;
            this.contentGenerator = contentGenerator;
        }
        
        [FunctionName(nameof(ContentGeneratorFunction))]
        public async Task Run([TimerTrigger("%ContentGeneratorTimer%")]TimerInfo myTimer, 
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            await this.contentGenerator.GenerateContentsAsync();
        }
    }
}
