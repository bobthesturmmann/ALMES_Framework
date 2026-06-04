using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using _Core.Shared.Lib;
using Core.Service;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Portal.Controllers
{
    [Authorize(Roles = "SysAdmin")]
    public class SysAdminConnectionController : Controller
    {
        private readonly SqlEngine _sqlEngine;
        private readonly IConfiguration _configuration;

        public SysAdminConnectionController(SqlEngine sqlEngine, IConfiguration configuration)
        {
            _sqlEngine = sqlEngine;
            _configuration = configuration;
        }

        private string GetMasterDb() => _configuration.GetConnectionString("DefaultConnection")!;

        public IActionResult Index()
        {
            string execCommand = "EXEC ALP_SYS_CONNECTIONS 1";
            var list = _sqlEngine.ExecuteRawQuery(GetMasterDb(), execCommand, row => new SysConnectionModel
            {
                Id = Convert.ToInt32(row["Id"]),
                ModuleName = row["ModuleName"].ToString()!,
                FirmaNo = row["FirmaNo"] == DBNull.Value ? string.Empty : row["FirmaNo"].ToString()!,
                DonemNo = row["DonemNo"] == DBNull.Value ? string.Empty : row["DonemNo"].ToString()!,
                ConnectionString = row["ConnectionString"].ToString()!
            });

            return View(list);
        }

        [HttpGet]
        public IActionResult Save(int? id)
        {
            if (id.HasValue)
            {
                string execCommand = $"EXEC ALP_SYS_CONNECTIONS 2, @Id = {id.Value}";
                var record = _sqlEngine.ExecuteRawQuery(GetMasterDb(), execCommand, row => new SysConnectionModel
                {
                    Id = Convert.ToInt32(row["Id"]),
                    ModuleName = row["ModuleName"].ToString()!,
                    FirmaNo = row["FirmaNo"] == DBNull.Value ? string.Empty : row["FirmaNo"].ToString()!,
                    DonemNo = row["DonemNo"] == DBNull.Value ? string.Empty : row["DonemNo"].ToString()!,
                    ConnectionString = row["ConnectionString"].ToString()!
                }).FirstOrDefault();

                return View(record ?? new SysConnectionModel());
            }

            return View(new SysConnectionModel());
        }

        [HttpPost]
        public IActionResult Save(SysConnectionModel model)
        {
            var mod = string.IsNullOrEmpty(model.ModuleName) ? "BOM" : model.ModuleName.Trim().Replace("'", "''");
            string firParam = string.IsNullOrEmpty(model.FirmaNo) ? "NULL" : $"'{model.FirmaNo.Trim().Replace("'", "''")}'";
            string donParam = string.IsNullOrEmpty(model.DonemNo) ? "NULL" : $"'{model.DonemNo.Trim().Replace("'", "''")}'";
            var conn = (model.ConnectionString ?? string.Empty).Trim().Replace("'", "''");
            string idParam = model.Id > 0 ? model.Id.ToString() : "NULL";

            string execCommand = $"EXEC ALP_SYS_CONNECTIONS 3, @Id = {idParam}, @ModuleName='{mod}', @FirmaNo={firParam}, @DonemNo={donParam}, @ConnectionString='{conn}'";

            _sqlEngine.ExecuteRawQuery<int>(GetMasterDb(), execCommand, row => 0);

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            string execCommand = $"EXEC ALP_SYS_CONNECTIONS 4, @Id = {id}";
            _sqlEngine.ExecuteRawQuery<int>(GetMasterDb(), execCommand, row => 0);
            return RedirectToAction("Index");
        }
    }
}