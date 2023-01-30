using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
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

        public StorageAccountProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.configurationValues = new Configuration();
        }

        public async Task<bool> SaveContentAsync(string contentName, string content)
        {
            var result = true;
            LoadConfig();

            BlobContainerClient container = new BlobContainerClient(this.configurationValues.StorageConnectionString, this.configurationValues.StorageContainerName);
            await container.CreateIfNotExistsAsync();
            try
            {
                BlobClient blob = container.GetBlobClient(contentName);

                await blob.UploadAsync(BinaryData.FromString(content), overwrite: true);
            }
            catch
            {
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
