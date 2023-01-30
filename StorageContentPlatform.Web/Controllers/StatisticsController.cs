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
        public async Task<ActionResult> Index()
        {
            var model = new IndexViewModel();
            model.ToFilter = DateTime.Now;
            model.FromFilter = model.ToFilter.AddDays(-30);
            model.Statistics = await this._statisticsService.GetStatisticsAsync(model.ToFilter, model.FromFilter);
            return View(model);
        }

        // GET: StatisticsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: StatisticsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StatisticsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: StatisticsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: StatisticsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: StatisticsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: StatisticsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
