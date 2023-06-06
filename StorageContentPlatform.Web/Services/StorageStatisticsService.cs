using Azure.Data.Tables;
using Azure.Storage.Blobs.Models;
using StorageContentPlatform.Web.Entities;
using StorageContentPlatform.Web.Interfaces;

namespace StorageContentPlatform.Web.Services
{
    public class StorageStatisticsService : IStatisticsService
    {
        private class Configuration
        {
            public string StorageConnectionString { get; set; }

            public string StatisticTableName { get; set; }
        }

        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;

        public StorageStatisticsService(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.configurationValues = new Configuration();
        }

        private void LoadConfig()
        {
            this.configurationValues.StorageConnectionString = this.configuration.GetValue<string>("StorageConnectionString");
            this.configurationValues.StatisticTableName = this.configuration.GetValue<string>("StatisticTableName");
        }

        public async Task<IEnumerable<StatisticData>> GetStatisticsAsync(DateTime to, DateTime from)
        {
            var result = new List<StatisticData>();
            LoadConfig();

            var filterString=$"InventoryStartTime ge datetime'{from.ToString("yyyy-MM-ddT00:00:00Z")}' and InventoryStartTime le datetime'{to.ToString("yyyy-MM-ddT23:59:59Z")}'";

            var tableServiceClient = new TableServiceClient(this.configurationValues.StorageConnectionString);
            var tableClient = tableServiceClient.GetTableClient(this.configurationValues.StatisticTableName);
            var queryResult = tableClient
                .QueryAsync<TableEntity>(filterString)
                .AsPages(default, 100);
            
            await foreach (var tablePage in queryResult)
            {
                foreach (var tableItem in tablePage.Values)
                {
                    result.Add(tableItem.ToStatisticData());
                }
            }
            
            return result.OrderByDescending(s => s.InventoryStartTime);
        }
    }
}
