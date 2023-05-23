
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StorageContentPlatform.Web.Entities
{
    public class StatisticData : ITableEntity
    {
        public DateTimeOffset InventoryCompletionTime { get; set; }
        public DateTimeOffset InventoryStartTime { get; set; }
        public long ObjectCount { get; set; }
        public long TotalObjectSize { get; set; }
        public long TotalObjectSizeInMBytes { get => (long)Math.Round(TotalObjectSize / 1048576.0, 0); }
        public long ObjectInHotCount { get; set; }
        public long TotalObjectInHotSize { get; set; }
        public long TotalObjectInHotSizeInMBytes { get => (long)Math.Round(TotalObjectInHotSize / 1048576.0, 0); }
        public long ObjectInCoolCount { get; set; }
        public long TotalObjectInCoolSize { get; set; }
        public long TotalObjectInCoolSizeInMBytes { get => (long)Math.Round(TotalObjectInCoolSize / 1048576.0, 0); }
        public long ObjectInColdCount { get; set; }
        public long TotalObjectInColdSize { get; set; }
        public long TotalObjectInColdSizeInMBytes { get => (long)Math.Round(TotalObjectInColdSize / 1048576.0, 0); }
        public long ObjectInArchiveCount { get; set; }
        public long TotalObjectInArchiveSize { get; set; }
        public long TotalObjectInArchiveSizeInMBytes { get => (long)Math.Round(TotalObjectInArchiveSize / 1048576.0, 0); }

        public string PartitionKey { get ; set ; }
        public string RowKey { get ; set ; }
        public DateTimeOffset? Timestamp { get; set ; }
        public ETag ETag { get ; set ; }
    }
}
