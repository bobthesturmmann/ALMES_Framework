using Bom.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bom.UI.Areas.Bom.Controllers
{
    [Area("Bom")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly BomManager _bomManager;

        public HomeController(BomManager bomManager)
        {
            _bomManager = bomManager;
        }

        [Route("Bom")]
        [Route("Bom/[controller]/[action]")]
        public IActionResult Index()
        {
            var model = _bomManager.PrepareBomListForUI();

            return View(model);
        }
    }
}