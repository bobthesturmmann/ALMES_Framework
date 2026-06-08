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
        public IActionResult Index(string productCode, bool isDetail = false, int page = 1, int pageSize = 20)
        {
            if (pageSize <= 0) pageSize = 20;

            if (!string.IsNullOrEmpty(productCode) && isDetail)
            {
                ViewBag.ActiveMode = 2;

                string refererUrl = Request.Headers["Referer"].ToString();
                ViewBag.ReturnUrl = (!string.IsNullOrEmpty(refererUrl) && refererUrl.Contains("/Bom"))
                                    ? refererUrl
                                    : "/Bom";

                var rawDetails = _bomManager.PrepareBomListByProduct(productCode);

                if (rawDetails != null && rawDetails.Any())
                {
                    ViewBag.MainProductCode = rawDetails.First().MainProductCode;
                    ViewBag.MainProductName = rawDetails.First().MainProductName;
                    ViewBag.MainQuantity = rawDetails.First().MainQuantity;
                    ViewBag.MainUnit = rawDetails.First().MainUnit;
                }
                else
                {
                    ViewBag.MainProductCode = productCode;
                }

                var detailResult = new PagedBomResult { Items = rawDetails ?? new List<BomDto>(), TotalCount = rawDetails?.Count ?? 0, CurrentPage = 1, PageSize = rawDetails?.Count ?? 20 };
                ViewBag.PageSize = pageSize;
                return View("RecipeList", detailResult);
            }
            else
            {
                ViewBag.ActiveMode = 1;
                var pagedResult = _bomManager.PreparePagedBomResult(1, productCode, page, pageSize);
                ViewBag.PageSize = pageSize;
                return View("RecipeList", pagedResult);
            }
        }
    }
}