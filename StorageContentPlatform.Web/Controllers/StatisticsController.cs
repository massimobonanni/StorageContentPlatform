using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StorageContentPlatform.Web.Interfaces;
using StorageContentPlatform.Web.Models.StatisticsController;

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

    }
}
