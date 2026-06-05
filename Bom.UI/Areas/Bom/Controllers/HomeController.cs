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
        public IActionResult Index(string? productCode)
        {
            List<BomDto> model;

            if (!string.IsNullOrEmpty(productCode))
            {
                model = _bomManager.PrepareBomListByProduct(productCode);
            }
            else
            {
                model = _bomManager.PrepareBomListForUI();
            }

            return View(model);
        }
    }
}