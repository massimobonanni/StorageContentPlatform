using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Models.ContentsController
{
    public class IndexViewModel
    {
        public IEnumerable<ContainerInfo> Containers { get; set; }
    }
}
