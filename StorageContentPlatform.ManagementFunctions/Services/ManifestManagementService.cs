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
    public class ManifestManagementService : IManifestManagementService
    {
        private readonly IInventoryPersistanceService persistanceService;
        private readonly IInventoryAnalyzer inventoryAnalyzer;
        private readonly ILogger<ManifestManagementService> logger;

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