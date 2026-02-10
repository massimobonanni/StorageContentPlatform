using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Models.StatisticsController
{
    /// <summary>
    /// View model for displaying the index page of the statistics controller.
    /// Contains date filters and a collection of statistical data for rendering in Razor Pages.
    /// </summary>
    public class IndexViewModel
    {
        /// <summary>
        /// Gets or sets the end date of the statistics filter range.
        /// </summary>
        public DateTime ToFilter { get; set; }

        /// <summary>
        /// Gets or sets the start date of the statistics filter range.
        /// </summary>
        public DateTime FromFilter { get; set; }

        /// <summary>
        /// Gets or sets the collection of Azure Blob Storage statistics data within the filtered date range.
        /// </summary>
        public IEnumerable<StatisticData>? Statistics { get; set; }
    }
}
