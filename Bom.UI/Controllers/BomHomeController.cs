using Microsoft.AspNetCore.Mvc;
using Bom.Lib;

namespace Bom.UI.Controllers
{
    [Area("BOM")]
    public class BomHomeController : Controller
    {
        private readonly BomManager _bomManager;

        public BomHomeController(BomManager bomManager)
        {
            _bomManager = bomManager;
        }

        [HttpGet("/BomHome/Index")]
        public IActionResult Index()
        {
            var model = _bomManager.PrepareBomListForUI();

            return View("Index", model);
        }
    }
}