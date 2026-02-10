namespace StorageContentPlatform.Web.Entities
{
    /// <summary>
    /// Represents the content and metadata of an Azure Blob Storage blob.
    /// Contains the blob name, its content as a string, and associated metadata.
    /// </summary>
    public class BlobContent
    {
        /// <summary>
        /// Gets or sets the name or path of the blob.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the actual content of the blob as a string.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the blob as key-value pairs.
        /// </summary>
        public IDictionary<string,string>? Metadata { get; set; }
    }
}
