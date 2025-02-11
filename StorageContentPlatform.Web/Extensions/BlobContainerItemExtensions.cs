using Azure.Storage.Blobs.Models;

namespace Azure.Storage.Blobs.Models
{
    public static class BlobContainerItemExtensions
    {
        public static bool HasMetadata(this BlobContainerItem item, string metadataName)
        {
            if (item.Properties == null || item.Properties.Metadata == null)
                return false;
            return item.Properties.Metadata.TryGetValue(metadataName, out string? _);
        }

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
