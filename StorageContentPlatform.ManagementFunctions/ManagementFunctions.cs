// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.Functions.Worker;
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
        private readonly ILogger<ManagementFunctions> logger;

        public ManagementFunctions(IConfiguration configuration,
            IInventoryAnalyzer inventoryAnalyzer, IInventoryPersistanceService persistanceService,
            IPersistenceManagementService persistentManagementService, ILogger<ManagementFunctions> logger)
        {
            this.configuration = configuration;
            this.inventoryAnalyzer = inventoryAnalyzer;
            this.persistanceService = persistanceService;
            this.persistentManagementService = persistentManagementService;
            this.logger = logger;
        }

        [Function(nameof(ManageInventoryCompleted))]
        public async Task ManageInventoryCompleted([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            logger.LogInformation(eventGridEvent.Data.ToString());

            var data = eventGridEvent.Data.ToObjectFromJson<InventoryCompletedData>(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            logger.LogInformation($"Calling ReadInventoryManifestFile {data.ManifestBlobUrl}");
            var inventoryManifest = await this.persistanceService.ReadInventoryManifestFile(data.ManifestBlobUrl);
            logger.LogInformation($"Called ReadInventoryManifestFile {data.ManifestBlobUrl} started at {inventoryManifest?.InventoryStartTime}");
            if (inventoryManifest != null)
            {
                logger.LogInformation($"Calling AnalyzeAsync {data.ManifestBlobUrl}");
                var inventoryStatistics = await this.inventoryAnalyzer.AnalyzeAsync(inventoryManifest);
                logger.LogInformation($"Called AnalyzeAsync {data.ManifestBlobUrl} analyzed {inventoryStatistics?.ObjectCount} objects ");
                if (inventoryStatistics != null)
                {
                    logger.LogInformation($"Calling SaveAsync {data.ManifestBlobUrl}");
                    var saveResult = await this.persistanceService.SaveAsync(inventoryStatistics);
                    logger.LogInformation($"Called SaveAsync {data.ManifestBlobUrl} with {saveResult} result");
                }
                else
                {
                    logger.LogError($"Failed to analyze inventory {data.ManifestBlobUrl}");
                }
            }
            else
            {
                logger.LogError($"Inventory manifest {data.ManifestBlobUrl} not read");
            }
        }

        [Function(nameof(BlobDeletedCompleted))]
        public async Task BlobDeletedCompleted([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            logger.LogInformation(eventGridEvent.Data.ToString());

            var data = eventGridEvent.Data.ToObjectFromJson<BlobDeletedData>(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            var result= await this.persistentManagementService.UndeleteBlobAsync(data.url);

            logger.LogInformation($"UndeleteBlobAsync {data.url} with {result} result");
        }
    }
}
