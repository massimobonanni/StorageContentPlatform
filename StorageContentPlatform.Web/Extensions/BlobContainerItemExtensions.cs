using Azure.Storage.Blobs.Models;

namespace Azure.Storage.Blobs.Models
{
    /// <summary>
    /// Provides extension methods for <see cref="BlobContainerItem"/> to simplify metadata operations.
    /// </summary>
    public static class BlobContainerItemExtensions
    {
        /// <summary>
        /// Determines whether the blob container item has a metadata entry with the specified name.
        /// </summary>
        /// <param name="item">The blob container item to check.</param>
        /// <param name="metadataName">The name of the metadata key to search for.</param>
        /// <returns><c>true</c> if the metadata key exists; otherwise, <c>false</c>.</returns>
        public static bool HasMetadata(this BlobContainerItem item, string metadataName)
        {
            if (item.Properties == null || item.Properties.Metadata == null)
                return false;
            return item.Properties.Metadata.TryGetValue(metadataName, out string? _);
        }

        /// <summary>
        /// Determines whether the blob container item has a metadata entry with the specified name 
        /// and its value is contained in the provided collection of values.
        /// </summary>
        /// <param name="item">The blob container item to check.</param>
        /// <param name="metadataName">The name of the metadata key to search for.</param>
        /// <param name="values">The collection of acceptable values to match against.</param>
        /// <returns><c>true</c> if the metadata key exists and its value is in the collection; otherwise, <c>false</c>.</returns>
        public static bool HasMetadataValues(this BlobContainerItem item,
            string metadataName, IEnumerable<string>? values)
        {
            if (values == null)
                return false;
            if (item.Properties == null || item.Properties.Metadata == null)
                return false;
            if (item.Properties.Metadata.TryGetValue(metadataName, out string? value))
            {
                return values.Contains(value);
            }

            return false;
        }
    }
}
