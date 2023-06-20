using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageContentPlatform.ContentCreator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using Utilities = StorageContentPlatform.ContentCreator.Utilities;

namespace StorageContentPlatform.ContentCreator.Services
{
    public class ContentGenerator : IContentGenerator
    {
        private class Configuration
        {
            public int BlobMinimumSizeInKb { get; set; }
            public int ContentsSizeForGenerationInKb { get; set; }
        }

        private readonly IPersistanceProvider persistanceProvider;
        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;
        private readonly ILogger<ContentGenerator> logger;

        public ContentGenerator(IPersistanceProvider persistanceProvider,
            IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.persistanceProvider = persistanceProvider;
            this.configuration = configuration;
            configurationValues = new Configuration();
            this.logger = loggerFactory.CreateLogger<ContentGenerator>();
        }

        public async Task<bool> GenerateContentsAsync()
        {
            var result = true;
            LoadConfig();

            var cumulativeSize = 0.0;
            while (cumulativeSize  < this.configurationValues.ContentsSizeForGenerationInKb)
            {
                var contentId= Guid.NewGuid();
                var contentName = @$"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{contentId}.txt";
                var content = Utilities.ContentGenerator.GenerateRandomContent(this.configurationValues.BlobMinimumSizeInKb, out var size);
                var metadata= Utilities.MetadataGenerator.GenerateMetadata(contentId);

                this.logger.LogInformation("Saving content {ContentName} with size {Size} Kb", contentName, size);
                await persistanceProvider.SaveContentAsync(contentName, content,metadata);
                this.logger.LogInformation("Saved content {ContentName} with size {Size} Kb", contentName, size);

                cumulativeSize += size;
            }

            return result;
        }

        private void LoadConfig()
        {
            this.logger.LogInformation("Loading configuration");
            this.configurationValues.BlobMinimumSizeInKb = this.configuration.GetValue<int>("BlobMinimumSizeInKb");
            this.configurationValues.ContentsSizeForGenerationInKb = this.configuration.GetValue<int>("ContentsSizeForGenerationInKb");
        }
    }
}
