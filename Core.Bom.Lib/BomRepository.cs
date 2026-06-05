using System;
using System.Collections.Generic;
using Core.Service;
using _Core.Shared.Lib;

namespace Core.Bom.Lib
{
    public class BomRepository
    {
        private readonly SqlEngine _sqlEngine;
        private readonly IAuthBridge _authBridge;
        private readonly IModuleConnectionProvider _connectionProvider;
        private readonly IAppSettingsService _appSettingsService;

        public BomRepository(
            SqlEngine sqlEngine,
            IAuthBridge authBridge,
            IModuleConnectionProvider connectionProvider,
            IAppSettingsService appSettingsService)
        {
            _sqlEngine = sqlEngine;
            _authBridge = authBridge;
            _connectionProvider = connectionProvider;
            _appSettingsService = appSettingsService;
        }

        public List<BomViewEntity> GetAllRecipes(string firmaNo, string donemNo, int mode = 1)
        {
            if (!_authBridge.IsUserLoggedIn()) { throw new UnauthorizedAccessException("..."); }

            if (string.IsNullOrEmpty(firmaNo))
            {
                var globalSettings = _appSettingsService.GetGlobalSettings();
                firmaNo = globalSettings.SirketKodu;
            }

            string connectionString = _connectionProvider.GetConnectionString("BOM", firmaNo, donemNo);
            int fNo = int.TryParse(firmaNo, out var f) ? f : 0;

            string execCommand = $"EXEC ALP_BOM_GET_RECETE @FirmaNo = {fNo}, @IslemTipi = {mode}";

            return _sqlEngine.ExecuteRawQuery(connectionString, execCommand, row => MapRowToEntity(row));
        }

        public List<BomViewEntity> GetRecipesByProduct(string productCode, string firmaNo, string donemNo)
        {
            if (!_authBridge.IsUserLoggedIn())
            {
                throw new UnauthorizedAccessException("Çekirdek Güvenlik İhlali: Bu veriyi okumak için giriş yapmalısınız!");
            }

            if (string.IsNullOrEmpty(firmaNo))
            {
                var globalSettings = _appSettingsService.GetGlobalSettings();
                firmaNo = globalSettings.SirketKodu;
            }

            string connectionString = _connectionProvider.GetConnectionString("BOM", firmaNo, donemNo);
            var cleanProductCode = productCode.Trim().Replace("'", "''");

            int fNo = int.TryParse(firmaNo, out var f) ? f : 0;

            string execCommand = $"EXEC ALP_BOM_GET_RECETE @FirmaNo = {fNo}, @IslemTipi = 3, @AnaUrunKodu = '{cleanProductCode}'";

            return _sqlEngine.ExecuteRawQuery(connectionString, execCommand, row => MapRowToEntity(row));
        }

        private BomViewEntity MapRowToEntity(System.Data.IDataRecord row)
        {
            return new BomViewEntity
            {
                SatirNo = row["SatirNo"] != DBNull.Value ? Convert.ToInt32(row["SatirNo"]) : 0,
                AnaUrunKodu = row["AnaUrunKodu"].ToString()!,
                AnaUrunAdi = row["AnaUrunAdi"].ToString()!,
                AnaMiktar = row["AnaMiktar"] != DBNull.Value ? Convert.ToDecimal(row["AnaMiktar"]) : 0,
                AnaBirimi = row["AnaBirimi"].ToString()!,
                AnaBirimSeti = row["AnaBirimSeti"].ToString()!,
                AltUrunKodu = row["AltUrunKodu"] != DBNull.Value ? row["AltUrunKodu"].ToString()! : string.Empty,
                AltUrunAdi = row["AltUrunAdi"] != DBNull.Value ? row["AltUrunAdi"].ToString()! : string.Empty,
                AltMiktar = row["AltMiktar"] != DBNull.Value ? Convert.ToDecimal(row["AltMiktar"]) : 0,
                AltBirimi = row["AltBirimi"] != DBNull.Value ? row["AltBirimi"].ToString()! : string.Empty,
                AltBirimSeti = row["AltBirimSeti"] != DBNull.Value ? row["AltBirimSeti"].ToString()! : string.Empty
            };
        }
    }
}