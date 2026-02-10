namespace StorageContentPlatform.Web.Entities
{
    /// <summary>
    /// Represents metadata and properties of an Azure Blob Storage blob.
    /// Contains information about the blob's name, modification date, replication status, size, and storage tier.
    /// </summary>
    public class BlobInfo
    {
        /// <summary>
        /// Gets or sets the name or path of the blob.
        /// </summary>
        public string? Name { get; internal set; }

        /// <summary>
        /// Gets or sets the date and time when the blob was last modified.
        /// </summary>
        public DateTimeOffset? LastModified { get; internal set; }

        /// <summary>
        /// Gets or sets the object replication policy identifier for the blob.
        /// </summary>
        public string? ReplicationPolicyId { get; internal set; }

        /// <summary>
        /// Gets or sets the object replication rule identifier for the blob.
        /// </summary>
        public string? ReplicationRuleId { get; internal set; }

        /// <summary>
        /// Gets or sets the current replication status of the blob.
        /// </summary>
        public string? ReplicationStatus { get; internal set; }

        /// <summary>
        /// Gets or sets the size of the blob in bytes.
        /// </summary>
        public long? Size { get; internal set; }

        /// <summary>
        /// Gets or sets the Azure storage access tier of the blob (e.g., Hot, Cool, Cold, Archive).
        /// </summary>
        public string? Tier { get; internal set; }
    }
}
