using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Models.ContentsController
{
    /// <summary>
    /// View model for displaying the index page of the contents controller.
    /// Contains a collection of container information for rendering in Razor Pages.
    /// </summary>
    public class IndexViewModel
    {
        /// <summary>
        /// Gets or sets the collection of Azure Blob Storage containers with their metadata.
        /// </summary>
        public IEnumerable<ContainerInfo>? Containers { get; set; }
    }
}
