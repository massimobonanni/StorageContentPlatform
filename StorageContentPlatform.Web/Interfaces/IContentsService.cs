using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Interfaces
{
    /// <summary>
    /// Defines the contract for services that manage Azure Blob Storage contents.
    /// Provides methods to retrieve containers, blobs, and blob content.
    /// </summary>
    public interface IContentsService
    {
        /// <summary>
        /// Retrieves all blob containers from Azure Storage that match the configured container types.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A collection of <see cref="ContainerInfo"/> objects representing the filtered containers.</returns>
        Task<IEnumerable<ContainerInfo>> GetContainersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all blobs from a specified container that match the provided date prefix.
        /// </summary>
        /// <param name="containerName">The name of the container to search.</param>
        /// <param name="date">The date used to filter blobs by prefix (formatted as yyyyMMdd).</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A collection of <see cref="BlobInfo"/> objects representing the blobs.</returns>
        Task<IEnumerable<BlobInfo>> GetBlobsAsync(string containerName, DateTime date, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a specific blob from a container including its content and metadata.
        /// </summary>
        /// <param name="containerName">The name of the container containing the blob.</param>
        /// <param name="blobName">The name of the blob to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="BlobContent"/> object containing the blob's content and metadata.</returns>
        Task<BlobContent> GetBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default);
    }
}
