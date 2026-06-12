using Bom.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Bom.UI.Areas.Bom.Controllers
{
    [Area("Bom")]
    [Authorize]
    [Authorize(Policy = "ModuleControl")]
    public class RecipeManagementController : Controller
    {
        private readonly BomManager _bomManager;

        public RecipeManagementController(BomManager bomManager)
        {
            _bomManager = bomManager;
        }

        [Route("Bom/RecipeManagement")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("Bom/RecipeManagement/SaveLine")]
        public IActionResult SaveLine([FromBody] BomRecipeSaveDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return Json(new { isSuccess = false, message = "Gönderilen veri boş olamaz." });
                }

                if (dto.AnaUrunRef <= 0 || dto.AltUrunRef <= 0)
                {
                    return Json(new { isSuccess = false, message = "Ana ürün ve Alt ürün seçimi zorunludur." });
                }

                if (dto.SatirNo <= 0)
                {
                    return Json(new { isSuccess = false, message = "Geçerli bir satır numarası belirtilmelidir." });
                }

                var result = _bomManager.SaveRecipeLine(dto);

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = $"Sistem Hatası: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("Bom/RecipeManagement/SearchProducts")]
        public IActionResult SearchProducts(string searchCode, string selectionType)
        {
            try
            {
                var items = _bomManager.SearchLogoItems(searchCode ?? "", selectionType ?? "sub");

                var resultList = items.Select(p => new {
                    productCode = p.MainProductCode,
                    productName = p.MainProductName,
                    unit = p.MainUnit,
                    productRef = p.LineNo,
                    unitRef = 1
                }).ToList();

                return Json(new { isSuccess = true, data = resultList });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}