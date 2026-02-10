using StorageContentPlatform.ManagementFunctions.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Interfaces
{
    /// <summary>
    /// Defines a service for persisting and retrieving Azure Storage inventory data.
    /// This interface provides operations for reading inventory manifest files from blob storage
    /// and saving inventory statistics to a persistent data store.
    /// </summary>
    public interface IInventoryPersistanceService
    {
        /// <summary>
        /// Reads and deserializes an inventory manifest file from Azure Blob Storage.
        /// </summary>
        /// <param name="manifestBlobUrl">The URL of the blob containing the inventory manifest JSON file.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains the deserialized <see cref="InventoryManifest"/> object with inventory metadata,
        /// file lists, and completion information.
        /// </returns>
        /// <remarks>
        /// The manifest file typically contains information about the inventory run including:
        /// completion times, destination container, inventory status, and a list of files included in the inventory.
        /// </remarks>
        Task<InventoryManifest> ReadInventoryManifestFile(string manifestBlobUrl);

        /// <summary>
        /// Persists inventory statistics to the data store.
        /// </summary>
        /// <param name="statistics">
        /// The <see cref="InventoryStatistics"/> object containing aggregated inventory data,
        /// including object counts, sizes by access tier (Hot, Cool, Cold, Archive), and metadata information.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a boolean value indicating whether the save operation was successful.
        /// Returns <c>true</c> if the statistics were saved successfully; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> SaveAsync(InventoryStatistics statistics);
    }
}
