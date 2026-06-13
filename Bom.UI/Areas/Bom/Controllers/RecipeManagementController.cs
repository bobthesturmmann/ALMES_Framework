using Bom.Lib;
using Core.Bom.Lib;
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

                var resultList = items.Select(p => new {
                    productCode = p.MainProductCode,
                    productName = p.MainProductName,
                    unit = p.MainUnit,
                    productRef = p.MainProductRef,
                    unitRef = 1,
                    isRecipeExists = p.IsRecipeExists
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
        public IActionResult GetRecipeLines(int mainProductRef, string mainProductCode)
        {
            try
            {
                if (string.IsNullOrEmpty(mainProductCode) && mainProductRef > 0)
                {
                    var searchResult = _bomManager.SearchLogoItems("", "main");
                    var found = searchResult.FirstOrDefault(x => x.MainProductRef == mainProductRef);
                    if (found != null) mainProductCode = found.MainProductCode;
                }

                if (string.IsNullOrEmpty(mainProductCode))
                {
                    return Json(new { isSuccess = false, message = "Ana ürün kodu bulunamadı!" });
                }

                var targetLines = _bomManager.PrepareBomListByProduct(mainProductCode);

                var resultList = targetLines.Select(l => new {
                    subProductCode = l.SubProductCode,
                    subProductName = l.SubProductName,
                    subQuantity = l.SubQuantity,
                    subUnit = l.SubUnit,
                    altUrunRef = l.SubProductRef,
                    altBirimRef = l.AltBirimRef <= 0 ? 1 : l.AltBirimRef
                }).ToList();

                return Json(new { isSuccess = true, data = resultList });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = "Reçete satırları yüklenirken hata: " + ex.Message });
            }
        }
    }
}