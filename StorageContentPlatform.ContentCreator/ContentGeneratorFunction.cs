using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageContentPlatform.ContentCreator.Interfaces;

namespace StorageContentPlatform.ContentCreator
{
    public class ContentGeneratorFunction
    {
        private readonly IConfiguration configuration;
        private readonly IContentGenerator contentGenerator;
        private readonly ILogger<ContentGeneratorFunction> logger;
        
        public ContentGeneratorFunction(IConfiguration configuration,
            IContentGenerator contentGenerator,
            ILogger<ContentGeneratorFunction> logger)
        {
            this.configuration = configuration;
            this.contentGenerator = contentGenerator;
            this.logger = logger;
        }
        
        [Function(nameof(ContentGeneratorFunction))]
        public async Task Run([TimerTrigger("%ContentGeneratorTimer%")] TimerInfo myTimer)
        {
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            await this.contentGenerator.GenerateContentsAsync();
        }
    }
}
