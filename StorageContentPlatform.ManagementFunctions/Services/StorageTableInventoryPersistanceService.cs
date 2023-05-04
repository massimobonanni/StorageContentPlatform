using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
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

        private class StatisticEntity : TableEntity
        {
            internal const string StatisticsPartitionKey = "STATISTICS";
            public StatisticEntity() : base()
            {
                this.RowKey = Guid.NewGuid().ToString();
                this.PartitionKey = StatisticsPartitionKey;
            }

            public StatisticEntity(InventoryStatistics statistics) : base()
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
                var cloudStorageAccount = CloudStorageAccount.Parse(this.configurationValues.StatisticsStorageConnectionString);
                var tableClient = cloudStorageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference(this.configurationValues.StatisticsTableName);
                
                TableOperation operation = TableOperation.Insert(entity);
                var insertResult = await table.ExecuteAsync(operation, default, default, default);
                result = insertResult.HttpStatusCode == 200 || insertResult.HttpStatusCode == 204;
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
