// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;
using StorageContentPlatform.ManagementFunctions.Entities;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StorageContentPlatform.ManagementFunctions.Interfaces;
using System.IO;
using Azure.Storage.Blobs;
using System.Text.Json;

namespace StorageContentPlatform.ManagementFunctions
{
    public class ManagementFunctions
    {
        private readonly IConfiguration configuration;
        private readonly IInventoryAnalyzer inventoryAnalyzer;
        private readonly IInventoryPersistanceService persistanceService;
        private readonly IPersistenceManagementService persistentManagementService;

        public ManagementFunctions(IConfiguration configuration,
            IInventoryAnalyzer inventoryAnalyzer, IInventoryPersistanceService persistanceService,
            IPersistenceManagementService persistentManagementService)
        {
            this.configuration = configuration;
            this.inventoryAnalyzer = inventoryAnalyzer;
            this.persistanceService = persistanceService;
            this.persistentManagementService = persistentManagementService;
        }

        [FunctionName(nameof(ManageInventoryCompleted))]
        public async Task ManageInventoryCompleted([EventGridTrigger] EventGridEvent eventGridEvent,
            [Blob("{data.manifestBlobUrl}", FileAccess.Read, Connection = "InventoryStorageConnectionString")] Stream input,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());

            var data = eventGridEvent.Data.ToObjectFromJson<InventoryCompletedData>(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            log.LogInformation($"Calling ReadInventoryManifestFile {data.ManifestBlobUrl}");
            var inventoryManifest = await this.persistanceService.ReadInventoryManifestFile(data.ManifestBlobUrl);
            log.LogInformation($"Called ReadInventoryManifestFile {data.ManifestBlobUrl} started at {inventoryManifest?.InventoryStartTime}");
            if (inventoryManifest != null)
            {
                log.LogInformation($"Calling AnalyzeAsync {data.ManifestBlobUrl}");
                var inventoryStatistics = await this.inventoryAnalyzer.AnalyzeAsync(inventoryManifest);
                log.LogInformation($"Called AnalyzeAsync {data.ManifestBlobUrl} analyzed {inventoryStatistics?.ObjectCount} objects ");
                if (inventoryStatistics != null)
                {
                    log.LogInformation($"Calling SaveAsync {data.ManifestBlobUrl}");
                    var saveResult = await this.persistanceService.SaveAsync(inventoryStatistics);
                    log.LogInformation($"Called SaveAsync {data.ManifestBlobUrl} with {saveResult} result");
                }
                else
                {
                    log.LogError($"Failed to analyze inventory {data.ManifestBlobUrl}");
                }
            }
            else
            {
                log.LogError($"Inventory manifest {data.ManifestBlobUrl} not read");
            }
        }

        [FunctionName(nameof(BlobDeletedCompleted))]
        public async Task BlobDeletedCompleted([EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());

            var data = eventGridEvent.Data.ToObjectFromJson<BlobDeletedData>(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            var result= await this.persistentManagementService.UndeleteBlobAsync(data.url);

            log.LogInformation($"UndeleteBlobAsync {data.url} with {result} result");
        }
    }
}
