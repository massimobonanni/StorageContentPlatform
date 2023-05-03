using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageContentPlatform.ContentCreator.Interfaces;
using StorageContentPlatform.ContentCreator.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

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

            var strBuild = new StringBuilder();
            var cumulativeSize = 0;
            while (cumulativeSize / 1024.0 < this.configurationValues.ContentsSizeForGenerationInKb)
            {
                var size = 0;
                while (size / 1024.0 < this.configurationValues.BlobMinimumSizeInKb)
                {
                    var sentence = Faker.Lorem.Sentence(Faker.RandomNumber.Next(1, 1000));
                    size += System.Text.ASCIIEncoding.UTF8.GetByteCount(sentence);
                    strBuild.AppendLine(sentence);
                }

                var contentId= Guid.NewGuid();
                var contentName = @$"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{contentId}.txt";
                var metadata= MetadataGenerator.GenerateMetadata(contentId);

                this.logger.LogInformation("Saving content {ContentName} with size {Size} bytes", contentName, size);
                await persistanceProvider.SaveContentAsync(contentName, strBuild.ToString(),metadata);
                this.logger.LogInformation("Saved content {ContentName} with size {Size} bytes", contentName, size);

                strBuild.Clear();
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
