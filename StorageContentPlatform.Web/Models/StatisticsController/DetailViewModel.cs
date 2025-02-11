using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Models.StatisticsController
{
    public class DetailViewModel
    {
        public DateTime Date { get; set; }
        
        public StatisticData? StatisticData { get; set; }
    }
}
