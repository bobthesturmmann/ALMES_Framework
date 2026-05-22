using Core.Service;

namespace Core.Bom.Lib
{
    public class BomRepository
    {
        private readonly SqlEngine _sqlEngine;

        public BomRepository(SqlEngine sqlEngine)
        {
            _sqlEngine = sqlEngine;
        }

        public List<BomViewEntity> GetAllRecipes()
        {
            return _sqlEngine.ReadFromView(
                "BOM", 
                "RECETE_LISTESI", 
                row => new BomViewEntity
                {
                    RECETE_KODU = row["RECETE_KODU"].ToString()!,
                    MAMUL_KODU = row["MAMUL_KODU"].ToString()!,
                    RECETE_ADI = row["RECETE_ADI"].ToString()!
                }
            );
        }
    }
}