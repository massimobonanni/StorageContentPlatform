using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageContentPlatform.ContentCreator.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StorageContentPlatform.ContentCreator.Services
{
    /// <summary>
    /// Provides storage functionality for persisting content to Azure Blob Storage.
    /// Implements <see cref="IPersistanceProvider"/> to save content with optional metadata.
    /// </summary>
    public class StorageAccountProvider : IPersistanceProvider
    {
        /// <summary>
        /// Internal configuration class that holds Azure Storage connection settings.
        /// </summary>
        private class Configuration
        {
            /// <summary>
            /// Gets or sets the Azure Storage account connection string.
            /// </summary>
            public string StorageConnectionString { get; set; }
            
            /// <summary>
            /// Gets or sets the name of the blob container where content will be stored.
            /// </summary>
            public string StorageContainerName { get; set; }
        }

        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;
        private readonly ILogger<StorageAccountProvider> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageAccountProvider"/> class.
        /// </summary>
        /// <param name="configuration">The configuration instance used to retrieve storage settings.</param>
        /// <param name="loggerFactory">The logger factory used to create a logger for this provider.</param>
        public StorageAccountProvider(IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<StorageAccountProvider>();
            this.configuration = configuration;
            this.configurationValues = new Configuration();
        }

        /// <summary>
        /// Asynchronously saves content to Azure Blob Storage with optional metadata.
        /// Creates the container if it doesn't exist before uploading the content.
        /// </summary>
        /// <param name="contentName">The name of the blob to create or overwrite.</param>
        /// <param name="content">The string content to save.</param>
        /// <param name="metadata">Optional metadata key-value pairs to associate with the blob.</param>
        /// <param name="token">A cancellation token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains <c>true</c> if the content was saved successfully; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> SaveContentAsync(string contentName, string content,
            IDictionary<string, string> metadata = null, CancellationToken token = default)
        {
            this.logger.LogInformation("Saving content to Azure Blob Storage. Content Name: {ContentName}, Metadata: {Metadata}",
                contentName, metadata.ToConcatenatedString());
            var result = true;
            LoadConfig();

            BlobContainerClient container = new BlobContainerClient(
                this.configurationValues.StorageConnectionString,
                this.configurationValues.StorageContainerName);

            await container.CreateIfNotExistsAsync();

            BlobClient blob = container.GetBlobClient(contentName);

            try
            {
                var uploadOptions = new BlobUploadOptions
                {
                    Metadata = metadata
                };

                await blob.UploadAsync(BinaryData.FromString(content), uploadOptions, token);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error saving {ContentName} - {Metadata}", contentName, metadata.ToConcatenatedString());
                result = false;
            }

            this.logger.LogInformation("Finished saving content to Azure Blob Storage. Content Name: {ContentName}, Metadata: {Metadata}, Result: {Result}",
                contentName, metadata.ToConcatenatedString(), result);
            return result;
        }

        /// <summary>
        /// Loads Azure Storage configuration values from the application configuration.
        /// Retrieves the storage container name and connection string from configuration settings.
        /// </summary>
        private void LoadConfig()
        {
            this.configurationValues.StorageContainerName = this.configuration.GetValue<string>("StorageContainerName");
            this.configurationValues.StorageConnectionString = this.configuration.GetValue<string>("StorageConnectionString");
        }
    }
}
