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

            return data.Select(x => new BomDto
            {
                RecipeCode = x.RECETE_KODU,
                ProductCode = x.MAMUL_KODU,
                RecipeName = x.RECETE_ADI
            }).ToList();
        }
    }
}