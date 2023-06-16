using Newtonsoft.Json;
using StorageContentPlatform.Web.Entities;
using System.Text;

namespace StorageContentPlatform.Web.Entities
{
    public static class StatisticsDataExtensions
    {
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
