using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Azure.Data.Tables;
using Azure;
using Newtonsoft.Json;
using StorageContentPlatform.ManagementFunctions.Entities;
using StorageContentPlatform.ManagementFunctions.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Services
{
    /// <summary>
    /// Provides persistence operations for inventory data using Azure Table Storage.
    /// </summary>
    public class StorageTableInventoryPersistanceService : IInventoryPersistanceService
    {
        /// <summary>
        /// Internal configuration settings for the inventory persistence service.
        /// </summary>
        private class Configuration
        {
            /// <summary>
            /// Gets or sets the connection string for accessing the inventory storage account.
            /// </summary>
            public string InventoryStorageConnectionString { get; set; }
            
            /// <summary>
            /// Gets or sets the connection string for accessing the statistics storage account.
            /// </summary>
            public string StatisticsStorageConnectionString { get; set; }
            
            /// <summary>
            /// Gets or sets the name of the Azure Table used for storing statistics.
            /// </summary>
            public string StatisticsTableName { get; set; }
        }

        /// <summary>
        /// Represents a table entity for storing inventory statistics in Azure Table Storage.
        /// </summary>
        private class StatisticEntity : ITableEntity
        {
            /// <summary>
            /// The partition key value used for all statistics entities.
            /// </summary>
            internal const string StatisticsPartitionKey = "STATISTICS";
            
            /// <summary>
            /// Gets or sets the partition key for the table entity.
            /// </summary>
            public string PartitionKey { get; set; }
            
            /// <summary>
            /// Gets or sets the row key (unique identifier) for the table entity.
            /// </summary>
            public string RowKey { get; set; }
            
            /// <summary>
            /// Gets or sets the timestamp of the table entity.
            /// </summary>
            public DateTimeOffset? Timestamp { get; set; }
            
            /// <summary>
            /// Gets or sets the ETag for optimistic concurrency.
            /// </summary>
            public ETag ETag { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="StatisticEntity"/> class with a generated row key.
            /// </summary>
            public StatisticEntity()
            {
                this.RowKey = Guid.NewGuid().ToString();
                this.PartitionKey = StatisticsPartitionKey;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="StatisticEntity"/> class from an <see cref="InventoryStatistics"/> object.
            /// </summary>
            /// <param name="statistics">The inventory statistics to convert to a table entity.</param>
            public StatisticEntity(InventoryStatistics statistics)
            {
                this.RowKey = Guid.NewGuid().ToString();
                this.PartitionKey = StatisticsPartitionKey;
                this.InventoryCompletionTime = statistics.InventoryCompletionTime;
                this.InventoryStartTime = statistics.InventoryStartTime;
                this.ObjectCount = statistics.ObjectCount;
                this.TotalObjectSize = statistics.TotalObjectSize;
                this.TotalObjectInCoolSize = statistics.TotalObjectInCoolSize;
                this.ObjectInCoolCount = statistics.ObjectInCoolCount;
                this.TotalObjectInColdSize = statistics.TotalObjectInColdSize;
                this.ObjectInColdCount = statistics.ObjectInColdCount;
                this.TotalObjectInHotSize = statistics.TotalObjectInHotSize;
                this.ObjectInHotCount = statistics.ObjectInHotCount;
                this.TotalObjectInArchiveSize = statistics.TotalObjectInArchiveSize;
                this.ObjectInArchiveCount = statistics.ObjectInArchiveCount;
                this.MetadataListJson = statistics.MetadataList != null 
                    ? JsonConvert.SerializeObject(statistics.MetadataList) 
                    : null;
            }

            /// <summary>
            /// Gets or sets the time when the inventory process was completed.
            /// </summary>
            public DateTimeOffset InventoryCompletionTime { get; set; }
            
            /// <summary>
            /// Gets or sets the time when the inventory process was started.
            /// </summary>
            public DateTimeOffset InventoryStartTime { get; set; }
            
            /// <summary>
            /// Gets or sets the total count of objects in the inventory.
            /// </summary>
            public long ObjectCount { get; set; }
            
            /// <summary>
            /// Gets or sets the total size in bytes of all objects in the inventory.
            /// </summary>
            public long TotalObjectSize { get; set; }
            
            /// <summary>
            /// Gets or sets the count of objects in the Hot access tier.
            /// </summary>
            public long ObjectInHotCount { get; set; }
            
            /// <summary>
            /// Gets or sets the total size in bytes of objects in the Hot access tier.
            /// </summary>
            public long TotalObjectInHotSize { get; set; }
            
            /// <summary>
            /// Gets or sets the count of objects in the Cool access tier.
            /// </summary>
            public long ObjectInCoolCount { get; set; }
            
            /// <summary>
            /// Gets or sets the total size in bytes of objects in the Cool access tier.
            /// </summary>
            public long TotalObjectInCoolSize { get; set; }
            
            /// <summary>
            /// Gets or sets the count of objects in the Cold access tier.
            /// </summary>
            public long ObjectInColdCount { get; set; }
            
            /// <summary>
            /// Gets or sets the total size in bytes of objects in the Cold access tier.
            /// </summary>
            public long TotalObjectInColdSize { get; set; }
            
            /// <summary>
            /// Gets or sets the count of objects in the Archive access tier.
            /// </summary>
            public long ObjectInArchiveCount { get; set; }
            
            /// <summary>
            /// Gets or sets the total size in bytes of objects in the Archive access tier.
            /// </summary>
            public long TotalObjectInArchiveSize { get; set; }
            
            /// <summary>
            /// Gets or sets the JSON representation of the metadata list.
            /// </summary>
            public string MetadataListJson { get; set; }
        }

        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;
        private readonly ILogger<StorageTableInventoryPersistanceService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageTableInventoryPersistanceService"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration containing storage and table settings.</param>
        /// <param name="logger">The logger instance for diagnostic logging.</param>
        public StorageTableInventoryPersistanceService(IConfiguration configuration, ILogger<StorageTableInventoryPersistanceService> logger)
        {
            this.configuration = configuration;
            this.configurationValues = new Configuration();
            this.logger = logger;
        }

        /// <summary>
        /// Loads configuration values from the application settings into the internal configuration object.
        /// </summary>
        private void LoadConfig()
        {
            this.configurationValues.StatisticsTableName = this.configuration.GetValue<string>("StatisticsTableName");
            this.configurationValues.StatisticsStorageConnectionString = this.configuration.GetValue<string>("StatisticsStorageConnectionString");
            this.configurationValues.InventoryStorageConnectionString = this.configuration.GetValue<string>("InventoryStorageConnectionString");
        }

        /// <summary>
        /// Saves inventory statistics to Azure Table Storage.
        /// </summary>
        /// <param name="statistics">The inventory statistics to persist.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// true if the statistics were successfully saved; otherwise, false.
        /// </returns>
        public async Task<bool> SaveAsync(InventoryStatistics statistics)
        {
            var result = true;
            LoadConfig();
            var entity = new StatisticEntity(statistics);
            try
            {
                logger.LogInformation("Saving inventory statistics to table storage");
                var tableClient = new TableClient(this.configurationValues.StatisticsStorageConnectionString,
                    this.configurationValues.StatisticsTableName);
                await tableClient.CreateIfNotExistsAsync();
                var response = await tableClient.AddEntityAsync(entity);
                result = response.Status == 204;
                logger.LogInformation("Successfully saved inventory statistics with status: {Status}", response.Status);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save inventory statistics to table storage");
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Reads and deserializes an inventory manifest file from Azure Blob Storage.
        /// </summary>
        /// <param name="manifestBlobUrl">The URL of the blob containing the manifest file.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the deserialized <see cref="InventoryManifest"/> object, or null if an error occurs.
        /// </returns>
        public async Task<InventoryManifest> ReadInventoryManifestFile(string manifestBlobUrl)
        {
            InventoryManifest result = null;
            LoadConfig();
            try
            {
                logger.LogInformation("Reading inventory manifest from blob URL: {BlobUrl}", manifestBlobUrl);
                manifestBlobUrl.ExtractContainerAndBlobName(out var containername, out var blobName);
                var blobClient = new BlobClient(this.configurationValues.InventoryStorageConnectionString,
                    containername, blobName);
                var blobContent = await blobClient.DownloadContentAsync();
                result = blobContent?.Value?.Content?.ToObjectFromJson<InventoryManifest>(new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
                logger.LogInformation("Successfully read inventory manifest from blob");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read inventory manifest from blob URL: {BlobUrl}", manifestBlobUrl);
                result = null;
            }

            return result;
        }
    }
}
