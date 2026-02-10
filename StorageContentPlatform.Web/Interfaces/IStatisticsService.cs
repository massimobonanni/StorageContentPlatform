using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Interfaces
{
    /// <summary>
    /// Defines the contract for services that retrieve Azure Blob Storage statistics.
    /// Provides methods to query statistical data within specified date ranges.
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// Retrieves statistics data from Azure Table Storage within the specified date range.
        /// </summary>
        /// <param name="to">The end date of the statistics period.</param>
        /// <param name="from">The start date of the statistics period.</param>
        /// <returns>A collection of <see cref="StatisticData"/> objects representing the statistics within the date range.</returns>
        Task<IEnumerable<StatisticData>> GetStatisticsAsync(DateTime to, DateTime from );
    }
}
