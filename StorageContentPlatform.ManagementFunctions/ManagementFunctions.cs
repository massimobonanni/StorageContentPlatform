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
        private readonly IManifestManagementService manifestManagementService;
        private readonly IPersistenceManagementService persistentManagementService;
        private readonly ILogger<ManagementFunctions> logger;

        public ManagementFunctions(IConfiguration configuration,
            IManifestManagementService manifestManagementService,
            IPersistenceManagementService persistentManagementService, 
            ILogger<ManagementFunctions> logger)
        {
            this.configuration = configuration;
            this.manifestManagementService = manifestManagementService;
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

            logger.LogInformation($"Processing manifest {data.ManifestBlobUrl}");
            var result = await manifestManagementService.ProcessManifestAsync(data.ManifestBlobUrl);

            if (result.Success)
            {
                logger.LogInformation($"Successfully processed manifest {data.ManifestBlobUrl} with {result.Statistics.ObjectCount} objects");
            }
            else
            {
                logger.LogError($"Failed to process manifest {data.ManifestBlobUrl}: {result.ErrorMessage}");
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

                logger.LogInformation($"Processing manifest analysis request for {data.ManifestBlobUrl}");
                var result = await manifestManagementService.ProcessManifestAsync(data.ManifestBlobUrl);

                if (!result.Success)
                {
                    response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    await response.WriteStringAsync(result.ErrorMessage);
                    return response;
                }

                response.StatusCode = System.Net.HttpStatusCode.OK;
                await response.WriteAsJsonAsync(new
                {
                    Success = true,
                    ObjectCount = result.Statistics.ObjectCount,
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
