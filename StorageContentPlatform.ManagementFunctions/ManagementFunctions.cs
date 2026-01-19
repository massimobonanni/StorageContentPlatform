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
using Microsoft.Azure.Functions.Worker.Http;
using StorageContentPlatform.ManagementFunctions.Requests;

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

            if (string.IsNullOrEmpty(data?.ManifestBlobUrl))
            {
                logger.LogError("ManifestBlobUrl is null or empty in event data");
                return;
            }

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

            if (string.IsNullOrEmpty(data?.url))
            {
                logger.LogError("Blob url is null or empty in event data");
                return;
            }

            var result= await this.persistentManagementService.UndeleteBlobAsync(data.url);

            logger.LogInformation($"UndeleteBlobAsync {data.url} with {result} result");
        }

        [Function(nameof(AnalyzeInventory))]
        public async Task<HttpResponseData> AnalyzeInventory(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "inventory/analyze")] HttpRequestData req)
        {
            logger.LogInformation("AnalyzeInventory HTTP trigger function processed a request.");

            var response = req.CreateResponse();

            try
            {
                // Read request body to get manifest URL
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonSerializer.Deserialize<AnalyzeInventoryRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (string.IsNullOrEmpty(data?.ManifestBlobUrl))
                {
                    response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("ManifestBlobUrl is required");
                    return response;
                }

                logger.LogInformation($"Reading inventory manifest from {data.ManifestBlobUrl}");
                var inventoryManifest = await this.persistanceService.ReadInventoryManifestFile(data.ManifestBlobUrl);

                if (inventoryManifest == null)
                {
                    response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    await response.WriteStringAsync($"Inventory manifest not found at {data.ManifestBlobUrl}");
                    return response;
                }

                logger.LogInformation($"Analyzing inventory manifest started at {inventoryManifest.InventoryStartTime}");
                var inventoryStatistics = await this.inventoryAnalyzer.AnalyzeAsync(inventoryManifest);

                if (inventoryStatistics == null)
                {
                    response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    await response.WriteStringAsync("Failed to analyze inventory");
                    return response;
                }

                logger.LogInformation($"Analyzed {inventoryStatistics.ObjectCount} objects");
                var saveResult = await this.persistanceService.SaveAsync(inventoryStatistics);
                logger.LogInformation($"Save result: {saveResult}");

                response.StatusCode = System.Net.HttpStatusCode.OK;
                await response.WriteAsJsonAsync(new
                {
                    Success = saveResult,
                    ObjectCount = inventoryStatistics.ObjectCount,
                    Message = "Inventory analysis completed successfully"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing inventory analysis request");
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                await response.WriteStringAsync($"Error: {ex.Message}");
            }

            return response;
        }
    }
}
