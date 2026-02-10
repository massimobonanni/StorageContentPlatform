namespace StorageContentPlatform.Web.Entities
{
    /// <summary>
    /// Represents information about an Azure Blob Storage container.
    /// Contains the container name, last modification date, and associated metadata.
    /// </summary>
    public class ContainerInfo
    {
        /// <summary>
        /// Gets or sets the name of the Azure Blob Storage container.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the container was last modified.
        /// </summary>
        public DateTimeOffset LastModified { get; set; }

        /// <summary>
        /// Gets or sets the container metadata as a formatted string of key-value pairs.
        /// </summary>
        public string? Metadata { get; set; }
    }
}
