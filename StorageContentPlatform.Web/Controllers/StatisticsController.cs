using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StorageContentPlatform.Web.Interfaces;
using StorageContentPlatform.Web.Models.StatisticsController;
using System.Reflection;
using System.Text;

namespace StorageContentPlatform.Web.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly IStatisticsService _statisticsService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(IStatisticsService statisticsService, ILogger<StatisticsController> logger)
        {
            _statisticsService = statisticsService;
            _logger = logger;
        }

        // GET: StatisticsController
        public async Task<ActionResult> Index(int dayHistory = 30)
        {
            var model = new IndexViewModel();
            model.ToFilter = DateTime.Now;
            model.FromFilter = model.ToFilter.AddDays(-dayHistory);
            model.Statistics = await this._statisticsService.GetStatisticsAsync(model.ToFilter, model.FromFilter);
            return View(model);
        }

        // write an action to export a csv file with the statistics 
        // GET: StatisticsController/Export?dayHistory=30
        public async Task<ActionResult> Export(int dayHistory = 30)
        {
            var toFilter = DateTime.Now;
            var fromFilter = toFilter.AddDays(-dayHistory);
            var statistics = await this._statisticsService.GetStatisticsAsync(toFilter,fromFilter);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("InventoryDate,TotalBlobs,TotalSizeBlobs,HotBlobs,HotSizeBlobs,CoolBlobs,CoolSizeBlobs,ArchiveBlobs,ArchiveSizeBlobs");
            foreach (var item in statistics)
            {
                sb.AppendLine($"{item.InventoryCompletionTime:dd/MM/yyyy HH:mm:ss},{item.ObjectCount},{item.TotalObjectSizeInMBytes},{item.ObjectInHotCount},{item.TotalObjectInHotSizeInMBytes},{item.ObjectInCoolCount},{item.TotalObjectInCoolSizeInMBytes},{item.ObjectInArchiveCount},{item.TotalObjectInArchiveSizeInMBytes}");
            }
            return File(Encoding.ASCII.GetBytes(sb.ToString()), "text/csv", "statistics.csv");
        }

    }
}
