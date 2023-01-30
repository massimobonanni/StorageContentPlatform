using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Models.ContentsController
{
    public class BlobViewModel
    {
        public string ContainerName { get; internal set; }
        public BlobContent Blob { get; internal set; }
    }
}
