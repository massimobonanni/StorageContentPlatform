using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StorageContentPlatform.Web.Interfaces;
using StorageContentPlatform.Web.Models.ContentsController;

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

        // GET: ContentController/Container?containerName=<container name>
        public async Task<ActionResult> Container(string containerName)
        {
            var model = new ContainerViewModel();
            model.ContainerName = containerName;
            model.Date = DateTime.Now;
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
