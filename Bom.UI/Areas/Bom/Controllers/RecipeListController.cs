using Bom.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bom.UI.Areas.Bom.Controllers
{
    [Area("Bom")]
    [Authorize]
    [Authorize(Policy = "ModuleControl")]
    public class RecipeListController(BomManager bomManager) : Controller
    {
        [Route("Bom/RecipeList")]
        [Route("Bom/[controller]/[action]")]
        public IActionResult List(string productCode, int page = 1, int pageSize = 20)
        {
            if (pageSize <= 0) pageSize = 20;

            var pagedResult = bomManager.PreparePagedBomResult(1, productCode, page, pageSize);
            ViewBag.PageSize = pageSize;

            return View("Index", pagedResult);
        }
    }
}