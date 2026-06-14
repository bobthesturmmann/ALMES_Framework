using Bom.Lib;
using Core.Bom.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Bom.UI.Areas.Bom.Controllers
{
    [Area("Bom")]
    [Authorize]
    [Authorize(Policy = "ModuleControl")]
    public class RecipeManagementController(BomManager bomManager) : Controller
    {
        private readonly BomManager _bomManager = bomManager;

        [HttpGet]
        [Route("Bom/RecipeManagement")]
        public IActionResult Index()
        {
            string? referer = Request.Headers.Referer.ToString();

            ViewBag.ReturnUrl = (!string.IsNullOrEmpty(referer) && referer.Contains("/Bom") && !referer.Contains("RecipeManagement"))
                ? referer
                : "/Bom/RecipeList";

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

        [HttpPost]
        [Route("Bom/RecipeManagement/SaveBulkChanges")]
        public IActionResult SaveBulkChanges([FromBody] BulkRecipeSaveDto payload)
        {
            try
            {
                if (payload == null || payload.AnaUrunRef <= 0)
                {
                    return Json(new { isSuccess = false, message = "Geçersiz istek verisi veya Ana Ürün referansı eksik!" });
                }

                string firmaNo = "";
                string donemNo = "";

                if (payload.IsDeleteAll)
                {
                    _bomManager.ExecuteBulkRecipeLine(firmaNo, donemNo, 4, 0, payload.AnaUrunRef, payload.AnaMiktar, payload.AnaBirimRef, 0, 0, 0, 0);
                    return Json(new { isSuccess = true, message = "Reçete ve tüm bileşenleri Logo veritabanından tamamen temizlendi! Ürün beyaza döndü." });
                }

                _bomManager.UpdateMainProductInfo(firmaNo, donemNo, payload.AnaUrunRef, payload.AnaMiktar, payload.AnaBirimRef);

                foreach (var line in payload.Lines)
                {
                    int tasiyiciIslemTipi = 1;

                    if (line.Status == "deleted")
                    {
                        tasiyiciIslemTipi = 3;
                    }
                    else if (line.Status == "original")
                    {
                        tasiyiciIslemTipi = 2;
                    }
                    else if (line.Status == "new")
                    {
                        tasiyiciIslemTipi = 1;
                    }

                    _bomManager.ExecuteBulkRecipeLine(
                        firmaNo: firmaNo,
                        donemNo: donemNo,
                        islemTipi: tasiyiciIslemTipi,
                        satirNo: line.SatirNo,
                        anaUrunRef: payload.AnaUrunRef,
                        anaMiktar: payload.AnaMiktar,
                        anaBirimRef: payload.AnaBirimRef,
                        altUrunRef: line.AltUrunRef,
                        altMiktar: line.AltMiktar,
                        altBirimRef: line.AltBirimRef,
                        lostFactor: 0.0m
                    );
                }

                return Json(new { isSuccess = true, message = "Tüm değişiklikler başarıyla analiz edildi ve Logo'ya toplu olarak işlendi!" });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = "Sistem hatası: " + ex.Message });
            }
        }

        [HttpGet]
        [Route("Bom/RecipeManagement/SearchProducts")]
        public IActionResult SearchProducts(string searchCode, string selectionType)
        {
            try
            {
                var items = _bomManager.SearchLogoItems(searchCode ?? "", selectionType ?? "sub");

                var resultList = items.Select(p => {
                    bool isMain = string.Equals(selectionType, "main", StringComparison.OrdinalIgnoreCase);
                    return new
                    {
                        productCode = isMain ? p.MainProductCode : p.SubProductCode,
                        productName = isMain ? p.MainProductName : p.SubProductName,
                        productType = p.ProductType,
                        unit = isMain ? p.MainUnit : p.SubUnit,
                        productRef = isMain ? p.MainProductRef : p.SubProductRef,
                        unitRef = p.AltBirimRef,
                        isRecipeExists = p.IsRecipeExists,
                        quantity = isMain ? p.MainQuantity : p.SubQuantity
                    };
                }).ToList();

                return Json(new { isSuccess = true, data = resultList });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("Bom/RecipeManagement/GetRecipeLines")]
        public IActionResult GetRecipeLines(string mainProductCode)
        {
            try
            {
                if (string.IsNullOrEmpty(mainProductCode))
                    return Json(new { isSuccess = false, message = "Ana ürün kodu bulunamadı!" });

                var targetLines = _bomManager.PrepareBomListByProduct(mainProductCode);

                var resultList = targetLines.Select(l => new {
                    subProductCode = l.SubProductCode,
                    subProductName = l.SubProductName,
                    subProductType = l.ProductType,
                    subQuantity = l.SubQuantity,
                    subUnit = l.SubUnit,
                    altUrunRef = l.SubProductRef,
                    altBirimRef = l.AltBirimRef
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