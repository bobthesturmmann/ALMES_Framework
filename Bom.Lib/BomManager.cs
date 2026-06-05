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

        public List<BomDto> PrepareBomListForUI(int mode = 1)
        {
            var data = _bomRepository.GetAllRecipes(string.Empty, string.Empty, mode);
            return MapToDto(data);
        }

        public List<BomDto> PrepareBomListByProduct(string productCode)
        {
            var data = _bomRepository.GetRecipesByProduct(productCode, string.Empty, string.Empty);
            return MapToDto(data);
        }

        private List<BomDto> MapToDto(List<BomViewEntity> data)
        {
            return data.Select(x => new BomDto
            {
                LineNo = x.SatirNo,
                MainProductCode = x.AnaUrunKodu,
                MainProductName = x.AnaUrunAdi,
                MainQuantity = x.AnaMiktar,
                MainUnit = x.AnaBirimi,
                MainUnitSet = x.AnaBirimSeti,
                SubProductCode = x.AltUrunKodu,
                SubProductName = x.AltUrunAdi,
                SubQuantity = x.AltMiktar,
                SubUnit = x.AltBirimi,
                SubUnitSet = x.AltBirimSeti
            }).ToList();
        }
    }
}