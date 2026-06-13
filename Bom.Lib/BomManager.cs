using System.Collections.Generic;
using System.Linq;
using Core.Bom.Lib;

namespace Bom.Lib
{
    public class BomManager
    {
        private readonly BomRepository _bomRepository;

        public BomManager(BomRepository bomRepository)
        {
            _bomRepository = bomRepository;
        }

        public List<BomDto> PrepareBomListForUI(int mode = 1, string searchCode = "")
        {
            var data = _bomRepository.GetAllRecipes(string.Empty, string.Empty, mode, searchCode);
            return MapToDto(data);
        }

        public List<BomDto> PrepareBomListByProduct(string productCode)
        {
            var data = _bomRepository.GetAllRecipes(string.Empty, string.Empty, 3, productCode);
            return MapToDto(data);
        }

        public PagedBomResult PreparePagedBomResult(int mode = 1, string searchCode = "", int page = 1, int pageSize = 20)
        {
            var rawData = _bomRepository.GetAllRecipes(string.Empty, string.Empty, mode, searchCode);
            var allDtos = MapToDto(rawData);

            var pagedItems = allDtos
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedBomResult
            {
                Items = pagedItems,
                TotalCount = allDtos.Count,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        private List<BomDto> MapToDto(List<BomViewEntity> data)
        {
            return data.Select(x => new BomDto
            {
                // Ana Ürün Bilgileri (Eğer UrunRef varsa)
                MainProductRef = x.UrunRef,
                MainProductCode = x.UrunKodu,
                MainProductName = x.UrunAdi,
                MainQuantity = x.UrunRef > 0 ? x.Miktar : 0m,
                MainUnit = x.UrunRef > 0 ? x.Birim : string.Empty,

                // Alt Bileşen Bilgileri (Eğer BilesenRef varsa)
                SubProductRef = x.BilesenRef,
                SubProductCode = x.BilesenKodu,
                SubProductName = x.BilesenAdi,
                SubQuantity = x.BilesenRef > 0 ? x.Miktar : 0m,
                SubUnit = x.BilesenRef > 0 ? x.Birim : string.Empty,

                AltBirimRef = x.BirimRef > 0 ? x.BirimRef : 1,

                IsRecipeExists = x.ReceteDurumu == "VAR",

                ProductType = x.UrunRef > 0 ? x.UrunTuru : x.BilesenTuru
            }).ToList();
        }

        public BomManageResponseDto SaveRecipeLine(BomRecipeSaveDto dto)
        {
            var repoResult = _bomRepository.ManageRecipeLine(
                dto.FirmaNo,
                dto.DonemNo,
                dto.IslemTipi,
                dto.SatirNo,
                dto.AnaUrunRef,
                dto.AnaMiktar,
                dto.AnaBirimRef,
                dto.AltUrunRef,
                dto.AltMiktar,
                dto.AltBirimRef,
                dto.LostFactor
            );

            if (repoResult != null && repoResult.IslemBasarili == 1)
            {
                return new BomManageResponseDto
                {
                    IsSuccess = true,
                    Message = "Reçete satırı başarıyla kaydedildi.",
                    AddedLineNo = repoResult.EklenenSatirNo,
                    MainProductRef = repoResult.AnaUrunRef,
                    SubProductRef = repoResult.AltUrunRef
                };
            }

            return new BomManageResponseDto
            {
                IsSuccess = false,
                Message = "Reçete satırı kaydedilirken SQL katmanında bir hata oluştu veya işlem başarısız."
            };
        }

        public List<BomDto> SearchLogoItems(string searchCode, string selectionType)
        {
            int targetMode = string.Equals(selectionType, "main", StringComparison.OrdinalIgnoreCase) ? 2 : 4;

            var rawData = _bomRepository.GetAllRecipes(string.Empty, string.Empty, targetMode, searchCode);
            return MapToDto(rawData);
        }

        public void ExecuteBulkRecipeLine(
            string firmaNo, string donemNo, int islemTipi, int satirNo,
            int anaUrunRef, decimal anaMiktar, int anaBirimRef,
            int altUrunRef, decimal altMiktar, int altBirimRef, decimal lostFactor)
        {
            _bomRepository.ManageRecipeLine(
                firmaNo, donemNo, islemTipi, satirNo,
                anaUrunRef, anaMiktar, anaBirimRef,
                altUrunRef, altMiktar, altBirimRef, lostFactor
            );
        }

        public void UpdateMainProductInfo(string firmaNo, string donemNo, int anaUrunRef, decimal anaMiktar, int anaBirimRef)
        {
            _bomRepository.UpdateMainProductProductionInfo(firmaNo, donemNo, anaUrunRef, anaMiktar, anaBirimRef);
        }
    }
}