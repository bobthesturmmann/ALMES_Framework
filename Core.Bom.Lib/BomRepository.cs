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

        public BomRepository(SqlEngine sqlEngine, IAuthBridge authBridge, IModuleConnectionProvider connectionProvider)
        {
            _sqlEngine = sqlEngine;
            _authBridge = authBridge;
            _connectionProvider = connectionProvider;
        }

        public List<BomViewEntity> GetAllRecipes(string firmaNo = "", string donemNo = "")
        {
            if (!_authBridge.IsUserLoggedIn())
            {
                throw new UnauthorizedAccessException("Çekirdek Güvenlik İhlali: Bu veriyi okumak için giriş yapmalısınız!");
            }

            string connectionString = _connectionProvider.GetConnectionString("BOM", firmaNo, donemNo);

            string execCommand = "EXEC ALP_BOM_RECETE_SORGULA 1, ''";

            return _sqlEngine.ExecuteRawQuery(
                connectionString,
                execCommand,
                row => new BomViewEntity
                {
                    RECETE_KODU = row["RECETE_KODU"].ToString()!,
                    MAMUL_KODU = row["MAMUL_KODU"].ToString()!,
                    RECETE_ADI = row["RECETE_ADI"].ToString()!
                }
            );
        }

        public List<BomViewEntity> GetRecipesByProduct(string productCode, string firmaNo = "", string donemNo = "")
        {
            if (!_authBridge.IsUserLoggedIn())
            {
                throw new UnauthorizedAccessException("Çekirdek Güvenlik İhlali: Bu veriyi okumak için giriş yapmalısınız!");
            }

            string connectionString = _connectionProvider.GetConnectionString("BOM", firmaNo, donemNo);
            var cleanProductCode = productCode.Trim().Replace("'", "''");

            string execCommand = $"EXEC ALP_BOM_RECETE_SORGULA 2, '{cleanProductCode}'";

            return _sqlEngine.ExecuteRawQuery(
                connectionString,
                execCommand,
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