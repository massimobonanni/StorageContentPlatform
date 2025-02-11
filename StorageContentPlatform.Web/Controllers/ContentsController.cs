using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StorageContentPlatform.Web.Interfaces;
using StorageContentPlatform.Web.Models.ContentsController;
using System.Globalization;

namespace StorageContentPlatform.Web.Controllers
{
    public class ContentsController : Controller
    {
        private readonly IContentsService contentsService;
        private readonly ILogger<ContentsController> logger;
        
        public ContentsController(IContentsService contentsService, ILogger<ContentsController> logger)
        {
            this.contentsService = contentsService;
            this.logger = logger;
        }
        
        // GET: ContentController
        public async Task<ActionResult> Index()
        {
            var model = new IndexViewModel();
            model.Containers = await this.contentsService.GetContainersAsync() ;
            return View(model);
        }

        // GET: ContentController/Container?containerName=<container name>&date=20230210
        public async Task<ActionResult> Container(string containerName,string? date=null)
        {
            var model = new ContainerViewModel();
            model.ContainerName = containerName;
            if (string.IsNullOrWhiteSpace(date))
                model.Date = DateTime.Now;
            else
            {
                if (DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, out DateTime parsedDate))
                    model.Date = parsedDate;
                else
                    model.Date = DateTime.Now;
            }
            model.Blobs = await this.contentsService.GetBlobsAsync(model.ContainerName, model.Date);
            return View(model);
        }

        // GET: ContentController/Blob?containerName=<container name>&blobName=<blob name>
        public async Task<ActionResult> Blob(string containerName, string blobName)
        {
            var model = new BlobViewModel();
            model.ContainerName = containerName;
            model.Blob = await this.contentsService.GetBlobAsync(containerName, blobName);
            return View(model);
        }

       
    }
}
