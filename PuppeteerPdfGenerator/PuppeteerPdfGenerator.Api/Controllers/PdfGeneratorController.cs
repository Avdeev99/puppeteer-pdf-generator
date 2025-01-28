using Microsoft.AspNetCore.Mvc;

namespace PuppeteerPdfGenerator.Api.Controllers
{
    public class PdfGeneratorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
