using Azure.Data.Tables;
using Newtonsoft.Json;
using StorageContentPlatform.Web.Entities;

namespace Azure.Data.Tables
{
    /// <summary>
    /// Provides extension methods for <see cref="TableEntity"/> to convert Azure Table Storage entities 
    /// to domain-specific data objects.
    /// </summary>
    public static class TableEntityExtensions
    {
        /// <summary>
        /// Converts an Azure Table Storage <see cref="TableEntity"/> to a <see cref="StatisticData"/> object.
        /// Maps all statistical properties including inventory times, object counts, sizes per storage tier,
        /// and deserializes the metadata list from JSON.
        /// </summary>
        /// <param name="entity">The table entity to convert.</param>
        /// <returns>A <see cref="StatisticData"/> object populated with values from the table entity.</returns>
        public static StatisticData ToStatisticData(this TableEntity entity)
        {
            var data = new StatisticData();

            data.InventoryCompletionTime = entity.GetDateTimeOffset("InventoryCompletionTime").GetValueOrDefault();
            data.InventoryStartTime = entity.GetDateTimeOffset("InventoryStartTime").GetValueOrDefault();
            data.ObjectCount = entity.GetInt64("ObjectCount").GetValueOrDefault();
            data.TotalObjectSize = entity.GetInt64("TotalObjectSize").GetValueOrDefault();
            data.ObjectInHotCount = entity.GetInt64("ObjectInHotCount").GetValueOrDefault();
            data.TotalObjectInHotSize = entity.GetInt64("TotalObjectInHotSize").GetValueOrDefault();
            data.ObjectInCoolCount = entity.GetInt64("ObjectInCoolCount").GetValueOrDefault();
            data.TotalObjectInCoolSize = entity.GetInt64("TotalObjectInCoolSize").GetValueOrDefault();
            data.ObjectInColdCount = entity.GetInt64("ObjectInColdCount").GetValueOrDefault();
            data.TotalObjectInColdSize = entity.GetInt64("TotalObjectInColdSize").GetValueOrDefault();
            data.ObjectInArchiveCount = entity.GetInt64("ObjectInArchiveCount").GetValueOrDefault();
            data.TotalObjectInArchiveSize = entity.GetInt64("TotalObjectInArchiveSize").GetValueOrDefault();
            data.Timestamp = entity.GetDateTimeOffset("Timestamp");

            if (entity.ContainsKey("MetadataList"))
            {
                data.MetadataList = JsonConvert.DeserializeObject<Dictionary<string, Metadata>>(entity.GetString("MetadataList"));
            }

            return data;
        }

    }
}
