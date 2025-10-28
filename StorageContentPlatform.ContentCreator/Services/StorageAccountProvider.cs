using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageContentPlatform.ContentCreator.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ContentCreator.Services
{
    public class StorageAccountProvider : IPersistanceProvider
    {
        private class Configuration
        {
            public string StorageConnectionString { get; set; }
            public string StorageContainerName { get; set; }
        }

        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;
        private readonly ILogger<StorageAccountProvider> logger;

        public StorageAccountProvider(IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger< StorageAccountProvider>();
            this.configuration = configuration;
            this.configurationValues = new Configuration();
        }

        public async Task<bool> SaveContentAsync(string contentName, string content,
            IDictionary<string, string> metadata = null)
        {
            var result = true;
            LoadConfig();

            BlobContainerClient container = new BlobContainerClient(
                this.configurationValues.StorageConnectionString, 
                this.configurationValues.StorageContainerName);

            await container.CreateIfNotExistsAsync();

            try
            {
                BlobClient blob = container.GetBlobClient(contentName);

                await blob.UploadAsync(BinaryData.FromString(content), overwrite: true);
                if (metadata != null && metadata.Any())
                    await blob.SetMetadataAsync(metadata);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error saving content {ContentName}", contentName);
                result = false;
            }

            return result;
        }

        private void LoadConfig()
        {
            this.configurationValues.StorageContainerName = this.configuration.GetValue<string>("StorageContainerName");
            this.configurationValues.StorageConnectionString = this.configuration.GetValue<string>("StorageConnectionString");
        }
    }
}
