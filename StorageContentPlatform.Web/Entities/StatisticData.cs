using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace StorageContentPlatform.Web.Entities
{
    /// <summary>
    /// Represents statistical data for Azure Blob Storage inventory.
    /// Contains aggregate metrics including object counts, sizes across different storage tiers, and custom metadata.
    /// </summary>
    public class StatisticData
    {
        /// <summary>
        /// Gets or sets the date and time when the inventory process completed.
        /// </summary>
        public DateTimeOffset InventoryCompletionTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the inventory process started.
        /// </summary>
        public DateTimeOffset InventoryStartTime { get; set; }

        /// <summary>
        /// Gets or sets the total number of objects (blobs) in the storage account.
        /// </summary>
        public long ObjectCount { get; set; }

        /// <summary>
        /// Gets or sets the total size of all objects in bytes.
        /// </summary>
        public long TotalObjectSize { get; set; }

        /// <summary>
        /// Gets the total size of all objects in megabytes (MB), calculated from <see cref="TotalObjectSize"/>.
        /// </summary>
        public long TotalObjectSizeInMBytes { get => (long)Math.Round(TotalObjectSize / 1048576.0, 0); }

        /// <summary>
        /// Gets or sets the number of objects in the Hot storage tier.
        /// </summary>
        public long ObjectInHotCount { get; set; }

        /// <summary>
        /// Gets or sets the total size of objects in the Hot tier in bytes.
        /// </summary>
        public long TotalObjectInHotSize { get; set; }

        /// <summary>
        /// Gets the total size of objects in the Hot tier in megabytes (MB), calculated from <see cref="TotalObjectInHotSize"/>.
        /// </summary>
        public long TotalObjectInHotSizeInMBytes { get => (long)Math.Round(TotalObjectInHotSize / 1048576.0, 0); }

        /// <summary>
        /// Gets or sets the number of objects in the Cool storage tier.
        /// </summary>
        public long ObjectInCoolCount { get; set; }

        /// <summary>
        /// Gets or sets the total size of objects in the Cool tier in bytes.
        /// </summary>
        public long TotalObjectInCoolSize { get; set; }

        /// <summary>
        /// Gets the total size of objects in the Cool tier in megabytes (MB), calculated from <see cref="TotalObjectInCoolSize"/>.
        /// </summary>
        public long TotalObjectInCoolSizeInMBytes { get => (long)Math.Round(TotalObjectInCoolSize / 1048576.0, 0); }

        /// <summary>
        /// Gets or sets the number of objects in the Cold storage tier.
        /// </summary>
        public long ObjectInColdCount { get; set; }

        /// <summary>
        /// Gets or sets the total size of objects in the Cold tier in bytes.
        /// </summary>
        public long TotalObjectInColdSize { get; set; }

        /// <summary>
        /// Gets the total size of objects in the Cold tier in megabytes (MB), calculated from <see cref="TotalObjectInColdSize"/>.
        /// </summary>
        public long TotalObjectInColdSizeInMBytes { get => (long)Math.Round(TotalObjectInColdSize / 1048576.0, 0); }

        /// <summary>
        /// Gets or sets the number of objects in the Archive storage tier.
        /// </summary>
        public long ObjectInArchiveCount { get; set; }

        /// <summary>
        /// Gets or sets the total size of objects in the Archive tier in bytes.
        /// </summary>
        public long TotalObjectInArchiveSize { get; set; }

        /// <summary>
        /// Gets the total size of objects in the Archive tier in megabytes (MB), calculated from <see cref="TotalObjectInArchiveSize"/>.
        /// </summary>
        public long TotalObjectInArchiveSizeInMBytes { get => (long)Math.Round(TotalObjectInArchiveSize / 1048576.0, 0); }

        /// <summary>
        /// Gets or sets the timestamp when this statistic data was recorded.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set ; }

        /// <summary>
        /// Gets or sets the dictionary of custom metadata associated with this statistical data.
        /// The key is the metadata category name, and the value is the <see cref="Metadata"/> object.
        /// </summary>
        public IDictionary<string, Metadata>? MetadataList { get; set; }
    }

    /// <summary>
    /// Represents metadata information with a label and associated counters.
    /// Used for custom categorization and counting within statistical data.
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// Gets or sets the label or display name for this metadata category.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of counter values for this metadata category.
        /// The key represents the counter name, and the value is the count.
        /// </summary>
        public IDictionary<string, long> Counters { get; set; } = new Dictionary<string, long>();
    }
}
