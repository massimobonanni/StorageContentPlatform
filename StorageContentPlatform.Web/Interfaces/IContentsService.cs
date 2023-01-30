using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Interfaces
{
    public interface IContentsService
    {
        Task<IEnumerable<ContainerInfo>> GetContainersAsync();

        Task<IEnumerable<BlobInfo>> GetBlobsAsync(string containerName, DateTime date);

        Task<BlobContent> GetBlobAsync(string containerName, string blobName);
    }
}
