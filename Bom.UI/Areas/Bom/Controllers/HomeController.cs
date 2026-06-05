using Bom.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Bom.UI.Areas.Bom.Controllers
{
    [Area("Bom")]
    [Authorize]
    [Authorize(Policy = "ModuleControl")]
    public class HomeController : Controller
    {
        private readonly BomManager _bomManager;

        public HomeController(BomManager bomManager)
        {
            _bomManager = bomManager;
        }

        [Route("Bom")]
        [Route("Bom/[controller]/[action]")]
        public IActionResult Index(string productCode, int mode = 1)
        {
            List<BomDto> model;

            if (!string.IsNullOrEmpty(productCode))
            {
                mode = 3;
                model = _bomManager.PrepareBomListByProduct(productCode);
            }
            else
            {
                model = _bomManager.PrepareBomListForUI(mode);
            }

            ViewBag.ActiveMode = mode;
            return View(model);
        }
    }
}