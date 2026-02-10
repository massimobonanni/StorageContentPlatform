using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Models.ContentsController
{
    /// <summary>
    /// View model for displaying blob information in the contents controller.
    /// Contains the container name and blob content details for rendering in Razor Pages.
    /// </summary>
    public class BlobViewModel
    {
        /// <summary>
        /// Gets or sets the name of the container that holds the blob.
        /// </summary>
        public string? ContainerName { get; internal set; }

        /// <summary>
        /// Gets or sets the blob content including metadata and data.
        /// </summary>
        public BlobContent? Blob { get; internal set; }
    }
}
