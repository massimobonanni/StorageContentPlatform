using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Models.StatisticsController
{
    public class IndexViewModel
    {
        public DateTime ToFilter { get; set; }
        public DateTime FromFilter { get; set; }
        
        public IEnumerable<StatisticData> Statistics { get; set; }
    }
}
