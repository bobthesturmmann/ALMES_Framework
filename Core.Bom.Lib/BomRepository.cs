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

            string currentUsername = _authBridge.GetCurrentUsername();

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