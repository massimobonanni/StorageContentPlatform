using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageContentPlatform.ManagementFunctions.Interfaces
{
    /// <summary>
    /// Provides persistence management operations for blob storage.
    /// </summary>
    public interface IPersistenceManagementService
    {
        /// <summary>
        /// Restores a previously soft-deleted blob.
        /// </summary>
        /// <param name="blobUrl">The URL of the blob to undelete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains <c>true</c> if the blob was successfully undeleted; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> UndeleteBlobAsync(string blobUrl);
    }
}
