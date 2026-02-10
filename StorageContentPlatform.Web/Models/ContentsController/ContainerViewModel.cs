
using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Models.ContentsController
{
    /// <summary>
    /// View model for displaying container information with its blobs in the contents controller.
    /// Contains the container name, date filter, and collection of blobs for rendering in Razor Pages.
    /// </summary>
    public class ContainerViewModel
    {
        /// <summary>
        /// Gets or sets the name of the Azure Blob Storage container.
        /// </summary>
        public string? ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the date used to filter blobs within the container.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the collection of blob information items contained in the container.
        /// </summary>
        public IEnumerable<BlobInfo>? Blobs { get; set; }
    }
}
