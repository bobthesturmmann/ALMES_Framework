using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.Service;
using Microsoft.Extensions.Configuration;

namespace _Core.Shared.Lib
{
    public class ModuleConnectionProvider : IModuleConnectionProvider
    {
        private readonly SqlEngine _sqlEngine;
        private readonly IConfiguration _configuration;

        public ModuleConnectionProvider(SqlEngine sqlEngine, IConfiguration configuration)
        {
            _sqlEngine = sqlEngine;
            _configuration = configuration;
        }

        public string GetConnectionString(string moduleName, string firmaNo, string donemNo)
        {
            string masterConnectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string globalFirma = _configuration["AlmesSettings:SirketKodu"] ?? "000";
            string globalDonem = _configuration["AlmesSettings:DonemKodu"] ?? "00";

            string targetFirma = firmaNo;
            string targetDonem = donemNo;

            try
            {
                string execCommand = "EXEC ALP_SYS_CONNECTIONS 1";
                List<SysConnectionModel> allConnections = _sqlEngine.ExecuteRawQuery(masterConnectionString, execCommand, (IDataRecord row) => new SysConnectionModel
                {
                    Id = Convert.ToInt32(row["Id"]),
                    ModuleName = row["ModuleName"].ToString() ?? string.Empty,
                    FirmaNo = row["FirmaNo"] == DBNull.Value ? string.Empty : row["FirmaNo"].ToString()!,
                    DonemNo = row["DonemNo"] == DBNull.Value ? string.Empty : row["DonemNo"].ToString()!,
                    ConnectionString = row["ConnectionString"].ToString() ?? string.Empty
                });

                var specRecord = allConnections.FirstOrDefault(x => x.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(targetFirma))
                {
                    targetFirma = (specRecord != null && !string.IsNullOrEmpty(specRecord.FirmaNo)) ? specRecord.FirmaNo : globalFirma;
                }

                if (string.IsNullOrEmpty(targetDonem))
                {
                    targetDonem = (specRecord != null && !string.IsNullOrEmpty(specRecord.DonemNo)) ? specRecord.DonemNo : globalDonem;
                }

                if (specRecord != null && !string.IsNullOrEmpty(specRecord.ConnectionString))
                {
                    return specRecord.ConnectionString;
                }
            }
            catch
            {
                if (string.IsNullOrEmpty(targetFirma)) targetFirma = globalFirma;
                if (string.IsNullOrEmpty(targetDonem)) targetDonem = globalDonem;
            }

            if (moduleName.Equals("AUTH", StringComparison.OrdinalIgnoreCase) && targetFirma == globalFirma)
            {
                return masterConnectionString;
            }

            return masterConnectionString;
        }

        public ResolvedModuleSettings ResolveModuleSettings(string moduleName)
        {
            string masterConnectionString = _configuration.GetConnectionString("DefaultConnection")!;

            string finalFirma = _configuration["AlmesSettings:SirketKodu"] ?? "000";
            string finalDonem = _configuration["AlmesSettings:DonemKodu"] ?? "00";

            try
            {
                string execCommand = "EXEC ALP_SYS_CONNECTIONS 1";
                var allConnections = _sqlEngine.ExecuteRawQuery(masterConnectionString, execCommand, (IDataRecord row) => new SysConnectionModel
                {
                    Id = Convert.ToInt32(row["Id"]),
                    ModuleName = row["ModuleName"].ToString() ?? string.Empty,
                    FirmaNo = row["FirmaNo"] == DBNull.Value ? string.Empty : row["FirmaNo"].ToString()!,
                    DonemNo = row["DonemNo"] == DBNull.Value ? string.Empty : row["DonemNo"].ToString()!
                });

                var specRecord = allConnections.FirstOrDefault(x => x.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

                if (specRecord != null)
                {
                    if (!string.IsNullOrEmpty(specRecord.FirmaNo)) finalFirma = specRecord.FirmaNo.Trim();
                    if (!string.IsNullOrEmpty(specRecord.DonemNo)) finalDonem = specRecord.DonemNo.Trim();
                }
            }
            catch { }

            return new ResolvedModuleSettings
            {
                ModuleName = moduleName.ToUpper(),
                FirmaNo = finalFirma,
                DonemNo = finalDonem,
                ProcedurePrefix = $"ALP_{finalFirma}_{finalDonem}_"
            };
        }
    }
}