using StorageContentPlatform.Web.Entities;

namespace StorageContentPlatform.Web.Models.StatisticsController
{
    /// <summary>
    /// View model for displaying detailed statistics information in the statistics controller.
    /// Contains date and statistical data for rendering in Razor Pages.
    /// </summary>
    public class DetailViewModel
    {
        /// <summary>
        /// Gets or sets the date associated with the statistical data.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the statistical data for Azure Blob Storage inventory and metrics.
        /// </summary>
        public StatisticData? StatisticData { get; set; }
    }
}
