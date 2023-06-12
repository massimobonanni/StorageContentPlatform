using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Interfaces
{
    public interface IContentsService
    {
        Task<IEnumerable<ContainerInfo>> GetContainersAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<BlobInfo>> GetBlobsAsync(string containerName, DateTime date, CancellationToken cancellationToken = default);

        Task<BlobContent> GetBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default);
    }
}
