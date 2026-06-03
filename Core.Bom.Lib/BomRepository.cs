using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Core.Service;
using _Core.Shared.Lib;

namespace Core.Bom.Lib
{
    public class BomRepository
    {
        private readonly SqlEngine _sqlEngine;
        private readonly IAuthBridge _authBridge;

        public BomRepository(SqlEngine sqlEngine, IAuthBridge authBridge)
        {
            _sqlEngine = sqlEngine;
            _authBridge = authBridge;
        }

        public List<BomViewEntity> GetAllRecipes()
        {
            if (!_authBridge.IsUserLoggedIn())
            {
                throw new UnauthorizedAccessException("Çekirdek Güvenlik İhlali: Bu veriyi okumak için giriş yapmalısınız!");
            }

            List<SqlParameter> parameters = [
                new SqlParameter("@IslemTipi", 1),
            ];

            return _sqlEngine.ExecuteProcedure(
                "BOM",
                "RECETE_SORGULA",
                row => new BomViewEntity
                {
                    RECETE_KODU = row["RECETE_KODU"].ToString()!,
                    MAMUL_KODU = row["MAMUL_KODU"].ToString()!,
                    RECETE_ADI = row["RECETE_ADI"].ToString()!
                },
                parameters
            );
        }

        public List<BomViewEntity> GetRecipesByProduct(string productCode)
        {
            if (!_authBridge.IsUserLoggedIn())
            {
                throw new UnauthorizedAccessException("Çekirdek Güvenlik İhlali: Bu veriyi okumak için giriş yapmalısınız!");
            }

            List<SqlParameter> parameters = [
                new SqlParameter("@IslemTipi", 2),
                new SqlParameter("@FiltreDegeri", productCode.Trim())
            ];

            return _sqlEngine.ExecuteProcedure(
                "BOM",
                "RECETE_SORGULA",
                row => new BomViewEntity
                {
                    RECETE_KODU = row["RECETE_KODU"].ToString()!,
                    MAMUL_KODU = row["MAMUL_KODU"].ToString()!,
                    RECETE_ADI = row["RECETE_ADI"].ToString()!
                },
                parameters
            );
        }
    }
}