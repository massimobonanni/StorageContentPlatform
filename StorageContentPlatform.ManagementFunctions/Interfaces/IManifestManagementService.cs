using StorageContentPlatform.ManagementFunctions.Entities;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Interfaces
{
    /// <summary>
    /// Service for managing inventory manifest processing workflow.
    /// </summary>
    public interface IManifestManagementService
    {
        /// <summary>
        /// Processes an inventory manifest by reading, analyzing, and persisting the statistics.
        /// </summary>
        /// <param name="manifestBlobUrl">The URL of the manifest blob to process.</param>
        /// <returns>A result containing success status and optional statistics data.</returns>
        Task<ManifestProcessingResult> ProcessManifestAsync(string manifestBlobUrl);
    }
}