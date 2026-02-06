using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageContentPlatform.ContentCreator.Interfaces;

namespace StorageContentPlatform.ContentCreator
{
    /// <summary>
    /// Azure Function that generates content on a scheduled timer trigger.
    /// This function runs based on the schedule defined in the ContentGeneratorTimer configuration setting.
    /// </summary>
    public class ContentGeneratorFunction
    {
        private readonly IConfiguration configuration;
        private readonly IContentGenerator contentGenerator;
        private readonly ILogger<ContentGeneratorFunction> logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentGeneratorFunction"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="contentGenerator">The service responsible for generating content.</param>
        /// <param name="logger">The logger for tracking function execution.</param>
        public ContentGeneratorFunction(IConfiguration configuration,
            IContentGenerator contentGenerator,
            ILogger<ContentGeneratorFunction> logger)
        {
            this.configuration = configuration;
            this.contentGenerator = contentGenerator;
            this.logger = logger;
        }
        
        /// <summary>
        /// Executes the content generation process on a timer trigger.
        /// The timer schedule is configured using the ContentGeneratorTimer application setting.
        /// </summary>
        /// <param name="myTimer">The timer trigger information containing schedule and execution details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Function(nameof(ContentGeneratorFunction))]
        public async Task Run([TimerTrigger("%ContentGeneratorTimer%")] TimerInfo myTimer)
        {
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            await this.contentGenerator.GenerateContentsAsync();
        }
    }
}
