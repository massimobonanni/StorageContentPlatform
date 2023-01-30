using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Interfaces
{
    public interface IStatisticsService
    {
        Task<IEnumerable<StatisticData>> GetStatisticsAsync(DateTime to, DateTime from );
    }
}
