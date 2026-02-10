using Microsoft.Extensions.Logging;
using StorageContentPlatform.ManagementFunctions.Entities;
using StorageContentPlatform.ManagementFunctions.Interfaces;
using System;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Services
{
    /// <summary>
    /// Manages the workflow of reading, analyzing, and persisting inventory manifests.
    /// </summary>
    /// <remarks>
    /// This service orchestrates the complete manifest processing pipeline by coordinating
    /// with the inventory persistence service and analyzer to read manifest files, perform
    /// statistical analysis, and persist the results.
    /// </remarks>
    public class ManifestManagementService : IManifestManagementService
    {
        private readonly IInventoryPersistanceService persistanceService;
        private readonly IInventoryAnalyzer inventoryAnalyzer;
        private readonly ILogger<ManifestManagementService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestManagementService"/> class.
        /// </summary>
        /// <param name="persistanceService">The service responsible for reading and saving inventory data.</param>
        /// <param name="inventoryAnalyzer">The analyzer used to process and generate statistics from inventory manifests.</param>
        /// <param name="logger">The logger instance for recording processing events and errors.</param>
        public ManifestManagementService(
            IInventoryPersistanceService persistanceService,
            IInventoryAnalyzer inventoryAnalyzer,
            ILogger<ManifestManagementService> logger)
        {
            this.persistanceService = persistanceService;
            this.inventoryAnalyzer = inventoryAnalyzer;
            this.logger = logger;
        }

        /// <inheritdoc/>
        /// <summary>
        /// Processes an inventory manifest from a blob URL by reading, analyzing, and persisting the results.
        /// </summary>
        /// <param name="manifestBlobUrl">The URL of the blob containing the inventory manifest to process.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a
        /// <see cref="ManifestProcessingResult"/> that indicates success or failure with relevant details.
        /// </returns>
        /// <remarks>
        /// The processing workflow includes the following steps:
        /// <list type="number">
        /// <item>Validates the manifest blob URL is not null or empty</item>
        /// <item>Reads the inventory manifest from the specified blob URL</item>
        /// <item>Analyzes the manifest to generate statistical information</item>
        /// <item>Persists the generated statistics to storage</item>
        /// </list>
        /// </remarks>
        /// <exception cref="Exception">
        /// Any exceptions during processing are caught and returned as a failure result with error details.
        /// </exception>
        public async Task<ManifestProcessingResult> ProcessManifestAsync(string manifestBlobUrl)
        {
            if (string.IsNullOrEmpty(manifestBlobUrl))
            {
                logger.LogError("ManifestBlobUrl is null or empty");
                return ManifestProcessingResult.CreateFailure("ManifestBlobUrl is null or empty");
            }

            try
            {
                logger.LogInformation($"Reading inventory manifest from {manifestBlobUrl}");
                var inventoryManifest = await persistanceService.ReadInventoryManifestFile(manifestBlobUrl);

                if (inventoryManifest == null)
                {
                    logger.LogError($"Inventory manifest not found at {manifestBlobUrl}");
                    return ManifestProcessingResult.CreateFailure($"Inventory manifest not found at {manifestBlobUrl}");
                }

                logger.LogInformation($"Analyzing inventory manifest started at {inventoryManifest.InventoryStartTime}");
                var inventoryStatistics = await inventoryAnalyzer.AnalyzeAsync(inventoryManifest);

                if (inventoryStatistics == null)
                {
                    logger.LogError($"Failed to analyze inventory {manifestBlobUrl}");
                    return ManifestProcessingResult.CreateFailure($"Failed to analyze inventory {manifestBlobUrl}");
                }

                logger.LogInformation($"Analyzed {inventoryStatistics.ObjectCount} objects from {manifestBlobUrl}");
                var saveResult = await persistanceService.SaveAsync(inventoryStatistics);

                if (!saveResult)
                {
                    logger.LogWarning($"Failed to save statistics for {manifestBlobUrl}");
                    return ManifestProcessingResult.CreateFailure($"Failed to save statistics for {manifestBlobUrl}");
                }

                logger.LogInformation($"Successfully processed manifest {manifestBlobUrl}");
                return ManifestProcessingResult.CreateSuccess(inventoryStatistics);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error processing manifest {manifestBlobUrl}");
                return ManifestProcessingResult.CreateFailure($"Error processing manifest: {ex.Message}");
            }
        }
    }
}