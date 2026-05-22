using Microsoft.AspNetCore.Mvc;
using Bom.Lib;

namespace Bom.UI.Controllers
{
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