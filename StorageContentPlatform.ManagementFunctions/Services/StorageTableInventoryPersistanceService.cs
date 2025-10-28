using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Azure.Data.Tables;
using Azure;
using Newtonsoft.Json;
using StorageContentPlatform.ManagementFunctions.Entities;
using StorageContentPlatform.ManagementFunctions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Services
{
    public class StorageTableInventoryPersistanceService : IInventoryPersistanceService
    {
        private class Configuration
        {
            public string InventoryStorageConnectionString { get; set; }
            public string StatisticsStorageConnectionString { get; set; }
            public string StatisticsTableName { get; set; }
        }

        private class StatisticEntity : ITableEntity
        {
            internal const string StatisticsPartitionKey = "STATISTICS";
            
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }

            public StatisticEntity()
            {
                this.RowKey = Guid.NewGuid().ToString();
                this.PartitionKey = StatisticsPartitionKey;
            }

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

            public DateTimeOffset InventoryCompletionTime { get; set; }
            public DateTimeOffset InventoryStartTime { get; set; }
            public long ObjectCount { get; set; }
            public long TotalObjectSize { get; set; }
            public long ObjectInHotCount { get; set; }
            public long TotalObjectInHotSize { get; set; }
            public long ObjectInCoolCount { get; set; }
            public long TotalObjectInCoolSize { get; set; }
            public long ObjectInColdCount { get; set; }
            public long TotalObjectInColdSize { get; set; }
            public long ObjectInArchiveCount { get; set; }
            public long TotalObjectInArchiveSize { get; set; }
            public string MetadataListJson { get; set; }
        }

        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;

        public StorageTableInventoryPersistanceService(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.configurationValues = new Configuration();
        }

        private void LoadConfig()
        {
            this.configurationValues.StatisticsTableName = this.configuration.GetValue<string>("StatisticsTableName");
            this.configurationValues.StatisticsStorageConnectionString = this.configuration.GetValue<string>("StatisticsStorageConnectionString");
            this.configurationValues.InventoryStorageConnectionString = this.configuration.GetValue<string>("InventoryStorageConnectionString");
        }

        public async Task<bool> SaveAsync(InventoryStatistics statistics)
        {
            var result = true;
            LoadConfig();
            var entity = new StatisticEntity(statistics);
            try
            {
                var tableClient = new TableClient(this.configurationValues.StatisticsStorageConnectionString,
                    this.configurationValues.StatisticsTableName);
                await tableClient.CreateIfNotExistsAsync();
                var response = await tableClient.AddEntityAsync(entity);
                result = response.Status == 204;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        public async Task<InventoryManifest> ReadInventoryManifestFile(string manifestBlobUrl)
        {
            InventoryManifest result = null;
            LoadConfig();
            try
            {
                manifestBlobUrl.ExtractContainerAndBlobName(out var containername, out var blobName);
                var blobClient = new BlobClient(this.configurationValues.InventoryStorageConnectionString,
                    containername, blobName);
                var blobContent = await blobClient.DownloadContentAsync();
                result = blobContent?.Value?.Content?.ToObjectFromJson<InventoryManifest>(new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                result = null;
            }

            return result;
        }


    }
}
