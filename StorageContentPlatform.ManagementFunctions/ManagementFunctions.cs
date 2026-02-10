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
    /// <summary>
    /// Azure Functions class for managing storage inventory and blob lifecycle events.
    /// Provides functions for processing inventory completion events, handling blob deletions, and manual inventory analysis.
    /// </summary>
    public class ManagementFunctions
    {
        private readonly IConfiguration configuration;
        private readonly IManifestManagementService manifestManagementService;
        private readonly IPersistenceManagementService persistentManagementService;
        private readonly ILogger<ManagementFunctions> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagementFunctions"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration instance.</param>
        /// <param name="manifestManagementService">Service for processing Azure Blob Storage inventory manifests.</param>
        /// <param name="persistentManagementService">Service for managing blob persistence and soft-delete operations.</param>
        /// <param name="logger">Logger instance for recording function execution details.</param>
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

        /// <summary>
        /// Azure Function triggered by EventGrid when a blob inventory operation completes.
        /// Processes the inventory manifest and extracts statistical data about the storage account.
        /// </summary>
        /// <param name="eventGridEvent">The EventGrid event containing inventory completion data with the manifest blob URL.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This function is triggered by the Microsoft.Storage.BlobInventoryPolicyCompleted event.
        /// It downloads and processes the manifest file to calculate storage statistics.
        /// </remarks>
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

        /// <summary>
        /// Azure Function triggered by EventGrid when a blob is deleted from storage.
        /// Automatically attempts to restore the deleted blob using soft-delete recovery.
        /// </summary>
        /// <param name="eventGridEvent">The EventGrid event containing blob deletion data with the blob URL.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This function is triggered by the Microsoft.Storage.BlobDeleted event.
        /// It uses the persistence management service to undelete the blob if soft-delete is enabled.
        /// </remarks>
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

        /// <summary>
        /// HTTP-triggered Azure Function for manual analysis of storage inventory manifests.
        /// Accepts a POST request with a manifest blob URL and returns inventory statistics.
        /// </summary>
        /// <param name="req">The HTTP request data containing the manifest blob URL in the request body.</param>
        /// <returns>
        /// An HTTP response containing:
        /// - 200 OK: Success with inventory statistics (ObjectCount)
        /// - 400 Bad Request: When ManifestBlobUrl is missing or invalid
        /// - 500 Internal Server Error: When manifest processing fails
        /// </returns>
        /// <remarks>
        /// This function provides an on-demand way to analyze inventory manifests without waiting for EventGrid triggers.
        /// The request body should contain a JSON object with a "ManifestBlobUrl" property.
        /// Requires Function-level authorization.
        /// </remarks>
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
