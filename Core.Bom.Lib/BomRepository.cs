using _Core.Shared.Lib;
using Core.Service;
using System;
using System.Collections.Generic;
using System.Data;

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

        private (string Firma, string Donem) ResolveCompanyAndPeriod(string firmaNo, string donemNo)
        {
            if (!string.IsNullOrEmpty(firmaNo) && !string.IsNullOrEmpty(donemNo))
            {
                return (firmaNo, donemNo);
            }

            var resolved = _connectionProvider.ResolveModuleSettings("BOM");

            return (
                string.IsNullOrEmpty(firmaNo) ? resolved.FirmaNo : firmaNo.Trim(),
                string.IsNullOrEmpty(donemNo) ? resolved.DonemNo : donemNo.Trim()
            );
        }

        public List<BomViewEntity> GetAllRecipes(string firmaNo, string donemNo, int mode = 1, string searchCode = "")
        {
            if (!_authBridge.IsUserLoggedIn()) { throw new UnauthorizedAccessException("..."); }

            var (finalFirma, finalDonem) = ResolveCompanyAndPeriod(firmaNo, donemNo);

            string connectionString = _connectionProvider.GetConnectionString("BOM", finalFirma, finalDonem);
            var cleanSearch = (searchCode ?? "").Trim().Replace("'", "''");

            string execCommand = $"EXEC ALP_BOM_GET_RECETE @FirmaNo = '{finalFirma.Trim()}', @IslemTipi = {mode}, @AnaUrunKodu = '{cleanSearch}'";

            return _sqlEngine.ExecuteRawQuery(connectionString, execCommand, row => MapRowToEntity(row));
        }

        public List<BomViewEntity> GetRecipesByProduct(string productCode, string firmaNo, string donemNo)
        {
            if (!_authBridge.IsUserLoggedIn()) { throw new UnauthorizedAccessException("..."); }

            var (finalFirma, finalDonem) = ResolveCompanyAndPeriod(firmaNo, donemNo);

            string connectionString = _connectionProvider.GetConnectionString("BOM", finalFirma, finalDonem);
            var cleanProductCode = productCode.Trim().Replace("'", "''");

            string execCommand = $"EXEC ALP_BOM_GET_RECETE @FirmaNo = '{finalFirma.Trim()}', @IslemTipi = 3, @AnaUrunKodu = '{cleanProductCode}'";

            return _sqlEngine.ExecuteRawQuery(connectionString, execCommand, row => MapRowToEntity(row));
        }

        public BomManageResultEntity ManageRecipeLine(
            string firmaNo, string donemNo, int islemTipi, int satirNo,
            int anaUrunRef, decimal anaMiktar, int anaBirimRef,
            int altUrunRef, decimal altMiktar, int altBirimRef, decimal lostFactor)
        {
            if (!_authBridge.IsUserLoggedIn()) { throw new UnauthorizedAccessException("..."); }

            var (finalFirma, finalDonem) = ResolveCompanyAndPeriod(firmaNo, donemNo);

            string connectionString = _connectionProvider.GetConnectionString("BOM", finalFirma, finalDonem);

            string execCommand = $@"
                EXEC [dbo].[ALP_BOM_MANAGE_RECETE] 
                    @FirmaNo = '{finalFirma.Trim()}', 
                    @IslemTipi = {islemTipi}, 
                    @SatirNo = {satirNo}, 
                    @AnaUrunRef = {anaUrunRef}, 
                    @AnaMiktar = {anaMiktar.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
                    @AnaBirimRef = {anaBirimRef}, 
                    @AltUrunRef = {altUrunRef}, 
                    @AltMiktar = {altMiktar.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
                    @AltBirimRef = {altBirimRef}, 
                    @LostFactor = {lostFactor.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            var rawResult = _sqlEngine.ExecuteRawQuery(connectionString, execCommand, row => row);

            return new BomManageResultEntity
            {
                IslemBasarili = 1,
                AnaUrunRef = anaUrunRef,
                AltUrunRef = altUrunRef,
                EklenenSatirNo = satirNo
            };
        }

        public void UpdateMainProductProductionInfo(string firmaNo, string donemNo, int anaUrunRef, decimal anaMiktar, int anaBirimRef)
        {
            var (finalFirma, finalDonem) = ResolveCompanyAndPeriod(firmaNo, donemNo);

            string connectionString = _connectionProvider.GetConnectionString("BOM", finalFirma, finalDonem);

            string query = $@"
                UPDATE [LG_{finalFirma.Trim()}_ITEMS] 
                SET QPRODAMNT = {anaMiktar.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
                    QPRODUOM = {anaBirimRef} 
                WHERE LOGICALREF = {anaUrunRef}";

            _sqlEngine.ExecuteRawQuery(connectionString, query, row => row);
        }

        private BomViewEntity MapRowToEntity(System.Data.IDataRecord row)
        {
            bool HasColumn(System.Data.IDataRecord dr, string colName)
            {
                for (int i = 0; i < dr.FieldCount; i++)
                    if (dr.GetName(i).Equals(colName, StringComparison.InvariantCultureIgnoreCase)) return true;
                return false;
            }

            var entity = new BomViewEntity();

            // --- ANA ÜRÜN (MOD 1 & 2) ---
            if (HasColumn(row, "UrunRef"))
                entity.UrunRef = row["UrunRef"] != DBNull.Value ? Convert.ToInt32(row["UrunRef"]) : 0;

            if (HasColumn(row, "UrunKodu"))
                entity.UrunKodu = row["UrunKodu"]?.ToString() ?? string.Empty;

            if (HasColumn(row, "UrunAdi"))
                entity.UrunAdi = row["UrunAdi"]?.ToString() ?? string.Empty;

            if (HasColumn(row, "UrunTuru"))
                entity.UrunTuru = row["UrunTuru"]?.ToString() ?? string.Empty;

            if (HasColumn(row, "ReceteDurumu"))
                entity.ReceteDurumu = row["ReceteDurumu"]?.ToString() ?? string.Empty;

            // --- ALT BİLEŞEN (MOD 3 & 4) ---
            if (HasColumn(row, "BilesenRef"))
                entity.BilesenRef = row["BilesenRef"] != DBNull.Value ? Convert.ToInt32(row["BilesenRef"]) : 0;

            if (HasColumn(row, "BilesenKodu"))
                entity.BilesenKodu = row["BilesenKodu"]?.ToString() ?? string.Empty;

            if (HasColumn(row, "BilesenAdi"))
                entity.BilesenAdi = row["BilesenAdi"]?.ToString() ?? string.Empty;

            if (HasColumn(row, "BilesenTuru"))
                entity.BilesenTuru = row["BilesenTuru"]?.ToString() ?? string.Empty;

            if (HasColumn(row, "BirimRef"))
                entity.BirimRef = row["BirimRef"] != DBNull.Value ? Convert.ToInt32(row["BirimRef"]) : 1;

            // --- ORTAK ALANLAR ---
            if (HasColumn(row, "Miktar"))
                entity.Miktar = row["Miktar"] != DBNull.Value ? Convert.ToDecimal(row["Miktar"]) : 0m;

            if (HasColumn(row, "Birim"))
                entity.Birim = row["Birim"]?.ToString() ?? string.Empty;

            return entity;
        }
    }
}