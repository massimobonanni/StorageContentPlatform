
using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Models.ContentsController
{
    public class ContainerViewModel
    {
        public string? ContainerName { get; set; }
        public DateTime Date { get; set; }
        public IEnumerable<BlobInfo>? Blobs { get; set; }
    }
}
