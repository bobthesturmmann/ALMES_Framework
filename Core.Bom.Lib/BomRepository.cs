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
            bool HasColumn(System.Data.IDataRecord dr, string columnName)
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
                return false;
            }

            var entity = new BomViewEntity
            {
                AnaUrunRef = HasColumn(row, "AnaUrunRef") && row["AnaUrunRef"] != DBNull.Value ? Convert.ToInt32(row["AnaUrunRef"]) : 0,
                SatirNo = HasColumn(row, "SatirNo") && row["SatirNo"] != DBNull.Value ? Convert.ToInt32(row["SatirNo"]) : 0,
                AnaUrunKodu = row["AnaUrunKodu"].ToString()!,
                AnaUrunAdi = row["AnaUrunAdi"].ToString()!,
                AnaMiktar = row["AnaMiktar"] != DBNull.Value ? Convert.ToDecimal(row["AnaMiktar"]) : 0,
                AnaBirimi = row["AnaBirimi"].ToString()!,
                AnaBirimSeti = HasColumn(row, "AnaBirimSeti") && row["AnaBirimSeti"] != DBNull.Value ? row["AnaBirimSeti"].ToString()! : string.Empty,
                AltUrunRef = HasColumn(row, "AltUrunRef") && row["AltUrunRef"] != DBNull.Value ? Convert.ToInt32(row["AltUrunRef"]) : 0,
                AltBirimRef = HasColumn(row, "AltBirimRef") && row["AltBirimRef"] != DBNull.Value ? Convert.ToInt32(row["AltBirimRef"]) : 0,
                AltUrunKodu = HasColumn(row, "AltUrunKodu") && row["AltUrunKodu"] != DBNull.Value ? row["AltUrunKodu"].ToString()! : string.Empty,
                AltUrunAdi = HasColumn(row, "AltUrunAdi") && row["AltUrunAdi"] != DBNull.Value ? row["AltUrunAdi"].ToString()! : string.Empty,
                AltMiktar = HasColumn(row, "AltMiktar") && row["AltMiktar"] != DBNull.Value ? Convert.ToDecimal(row["AltMiktar"]) : 0,
                AltBirimi = HasColumn(row, "AltBirimi") && row["AltBirimi"] != DBNull.Value ? row["AltBirimi"].ToString()! : string.Empty,
                AltBirimSeti = HasColumn(row, "AltBirimSeti") && row["AltBirimSeti"] != DBNull.Value ? row["AltBirimSeti"].ToString()! : string.Empty
            };

            return entity;
        }
    }
}