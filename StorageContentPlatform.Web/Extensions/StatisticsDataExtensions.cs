using Newtonsoft.Json;
using StorageContentPlatform.Web.Entities;
using System.Text;

namespace StorageContentPlatform.Web.Entities
{
    /// <summary>
    /// Provides extension methods for <see cref="StatisticData"/> collections to support data export operations.
    /// </summary>
    public static class StatisticsDataExtensions
    {
        /// <summary>
        /// Generates a CSV (Comma-Separated Values) representation of the statistics data collection.
        /// Includes headers and formats each statistic record as a CSV row with inventory date, blob counts, 
        /// sizes per tier (Hot, Cool, Cold, Archive), and JSON-serialized metadata.
        /// </summary>
        /// <param name="data">The collection of <see cref="StatisticData"/> to convert to CSV format.</param>
        /// <returns>A CSV-formatted string with headers and data rows representing the statistics.</returns>
        public static string GenerateCSV(this IEnumerable<StatisticData> data)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("InventoryDate,TotalBlobs,TotalSizeBlobs,HotBlobs,HotSizeBlobs,CoolBlobs,CoolSizeBlobs,ColdBlobs,ColdSizeBlobs,ArchiveBlobs,ArchiveSizeBlobs,Metadata");

            foreach (var item in data)
            {
                var metadata = JsonConvert.SerializeObject(item.MetadataList);
                sb.AppendLine($"{item.InventoryCompletionTime:dd/MM/yyyy HH:mm:ss},{item.ObjectCount},{item.TotalObjectSizeInMBytes},{item.ObjectInHotCount},{item.TotalObjectInHotSizeInMBytes},{item.ObjectInCoolCount},{item.TotalObjectInCoolSizeInMBytes},{item.ObjectInColdCount},{item.TotalObjectInColdSizeInMBytes},{item.ObjectInArchiveCount},{item.TotalObjectInArchiveSizeInMBytes},{metadata}");
            }

            return sb.ToString();
        }
    }
}
