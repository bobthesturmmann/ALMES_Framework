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

        public List<BomDto> PrepareBomListForUI()
        {
            var data = _bomRepository.GetAllRecipes();
            return MapToDto(data);
        }

        public List<BomDto> PrepareBomListByProduct(string productCode)
        {
            var data = _bomRepository.GetRecipesByProduct(productCode);
            return MapToDto(data);
        }

        private List<BomDto> MapToDto(List<BomViewEntity> data)
        {
            return data.Select(x => new BomDto
            {
                RecipeCode = x.RECETE_KODU,
                ProductCode = x.MAMUL_KODU,
                RecipeName = x.RECETE_ADI
            }).ToList();
        }
    }
}