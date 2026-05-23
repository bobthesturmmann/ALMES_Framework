using Bom.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bom.UI.Controllers
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

        public IActionResult Index()
        {
            var model = _bomManager.PrepareBomListForUI();

            return View("Index", model);
        }
    }
}