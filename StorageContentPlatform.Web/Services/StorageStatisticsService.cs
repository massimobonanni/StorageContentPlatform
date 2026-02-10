using Azure.Data.Tables;
using Azure.Identity;
using Azure.Storage.Blobs.Models;
using StorageContentPlatform.Web.Entities;
using StorageContentPlatform.Web.Interfaces;

namespace StorageContentPlatform.Web.Services
{
    /// <summary>
    /// Service implementation for retrieving storage statistics from Azure Table Storage.
    /// Provides methods to query and retrieve statistical data within specified date ranges.
    /// </summary>
    public class StorageStatisticsService : IStatisticsService
    {
        /// <summary>
        /// Configuration settings for Azure Table Storage connection and table name.
        /// </summary>
        private class Configuration
        {
            /// <summary>
            /// Gets or sets the Azure Storage connection string.
            /// </summary>
            public string? StorageConnectionString { get; set; }

            /// <summary>
            /// Gets or sets the Azure Storage account name.
            /// </summary>
            public string? StorageAccountName { get; set; }

            /// <summary>
            /// Gets or sets the name of the statistics table.
            /// </summary>
            public string? StatisticTableName { get; set; }
        }

        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageStatisticsService"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration instance.</param>
        public StorageStatisticsService(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.configurationValues = new Configuration();
        }

        #region [ Public Method - IStatisticsService implementation ]

        /// <summary>
        /// Retrieves statistics data from Azure Table Storage within the specified date range.
        /// </summary>
        /// <param name="to">The end date of the statistics period.</param>
        /// <param name="from">The start date of the statistics period.</param>
        /// <returns>A collection of <see cref="StatisticData"/> objects ordered by inventory start time in descending order.</returns>
        public async Task<IEnumerable<StatisticData>> GetStatisticsAsync(DateTime to, DateTime from)
        {
            var result = new List<StatisticData>();
            LoadConfig();

            var filterString = $"InventoryStartTime ge datetime'{from.ToString("yyyy-MM-ddT00:00:00Z")}' and InventoryStartTime le datetime'{to.ToString("yyyy-MM-ddT23:59:59Z")}'";
            
            TableServiceClient tableServiceClient = CreateTebleClient
                ();
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

        #endregion [ Public Method - IStatisticsService implementation ]

        #region [ Private Methods ]
        private void LoadConfig()
        {
            this.configurationValues.StorageConnectionString = this.configuration.GetValue<string>("StorageConnectionString");
            this.configurationValues.StorageAccountName = this.configuration.GetValue<string>("StorageAccountName");
            this.configurationValues.StatisticTableName = this.configuration.GetValue<string>("StatisticTableName");
        }

        private TableServiceClient CreateTebleClient()
        {
            TableServiceClient? tableClient = null;
            if (!string.IsNullOrWhiteSpace(this.configurationValues.StorageConnectionString))
            {
                tableClient = new TableServiceClient(
                    this.configurationValues.StorageConnectionString);
            }
            else
            {
                tableClient= new TableServiceClient(
                    new Uri($"https://{this.configurationValues.StorageAccountName}.table.core.windows.net"),
                    new DefaultAzureCredential());
            }
            return tableClient;
        }

        #endregion [ Private Methods ]
    }
}
