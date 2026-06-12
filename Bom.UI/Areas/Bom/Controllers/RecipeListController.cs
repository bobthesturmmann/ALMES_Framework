using Bom.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Bom.UI.Areas.Bom.Controllers
{
    [Area("Bom")]
    [Authorize]
    [Authorize(Policy = "ModuleControl")]
    public class RecipeListController(BomManager bomManager) : Controller
    {
        [Route("Bom/RecipeList")]
        [Route("Bom/[controller]/[action]")]
        public IActionResult List(string productCode, bool isDetail = false, int page = 1, int pageSize = 20)
        {
            if (pageSize <= 0) pageSize = 20;

            if (!string.IsNullOrEmpty(productCode) && isDetail)
            {
                ViewBag.ActiveMode = 2;

                string refererUrl = Request.Headers.Referer.ToString() ?? string.Empty;
                ViewBag.ReturnUrl = (!string.IsNullOrEmpty(refererUrl) && refererUrl.Contains("/Bom", StringComparison.OrdinalIgnoreCase))
                                    ? refererUrl
                                    : "/Bom/RecipeList";

                var rawDetails = bomManager.PrepareBomListByProduct(productCode);

                if (rawDetails != null && rawDetails.Count > 0)
                {
                    ViewBag.MainProductCode = rawDetails[0].MainProductCode;
                    ViewBag.MainProductName = rawDetails[0].MainProductName;
                    ViewBag.MainQuantity = rawDetails[0].MainQuantity;
                    ViewBag.MainUnit = rawDetails[0].MainUnit;
                }
                else
                {
                    ViewBag.MainProductCode = productCode;
                }

                var detailResult = new PagedBomResult
                {
                    Items = rawDetails ?? [],
                    TotalCount = rawDetails?.Count ?? 0,
                    CurrentPage = 1,
                    PageSize = rawDetails?.Count ?? 20
                };
                ViewBag.PageSize = pageSize;
                return View("Index", detailResult);
            }
            else
            {
                ViewBag.ActiveMode = 1;
                var pagedResult = bomManager.PreparePagedBomResult(1, productCode, page, pageSize);
                ViewBag.PageSize = pageSize;
                return View("Index", pagedResult);
            }
        }
    }
}