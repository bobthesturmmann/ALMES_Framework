using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using _Core.Shared.Lib;
using Core.Service;
using Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Portal.Controllers
{
    [Authorize(Roles = "SysAdmin")]
    public class SysAdminConnectionController(SqlEngine sqlEngine, IAppSettingsService appSettingsService) : Controller
    {
        private string GetMasterDb()
        {
            var settings = appSettingsService.GetGlobalSettings();
            return settings != null ? settings.GlobalConnectionString : string.Empty;
        }

        public IActionResult Index(int? id)
        {
            var globalSettings = appSettingsService.GetGlobalSettings();

            var viewModel = new SysAdminViewModel
            {
                GlobalSirketKodu = globalSettings.SirketKodu,
                GlobalDonemKodu = globalSettings.DonemKodu,
                GlobalConnectionString = globalSettings.GlobalConnectionString,
                ActiveModules = appSettingsService.GetActiveModules(),
                ShownModules = appSettingsService.GetShownModules()
            };

            string execCommand = "EXEC ALP_SYS_CONNECTIONS 1";
            viewModel.SavedConnections = sqlEngine.ExecuteRawQuery(GetMasterDb(), execCommand, row => new SysConnectionModel
            {
                Id = Convert.ToInt32(row["Id"]),
                ModuleName = row["ModuleName"].ToString()!,
                FirmaNo = row["FirmaNo"] == DBNull.Value ? string.Empty : row["FirmaNo"].ToString()!,
                DonemNo = row["DonemNo"] == DBNull.Value ? string.Empty : row["DonemNo"].ToString()!,
                ConnectionString = row["ConnectionString"].ToString()!
            });

            if (id.HasValue)
            {
                string editCommand = $"EXEC ALP_SYS_CONNECTIONS 2, @Id = {id.Value}";
                viewModel.CurrentModel = sqlEngine.ExecuteRawQuery(GetMasterDb(), editCommand, row => new SysConnectionModel
                {
                    Id = Convert.ToInt32(row["Id"]),
                    ModuleName = row["ModuleName"].ToString()!,
                    FirmaNo = row["FirmaNo"] == DBNull.Value ? string.Empty : row["FirmaNo"].ToString()!,
                    DonemNo = row["DonemNo"] == DBNull.Value ? string.Empty : row["DonemNo"].ToString()!,
                    ConnectionString = row["ConnectionString"].ToString()!
                }).FirstOrDefault() ?? new SysConnectionModel();
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SaveGlobalSettings(string globalSirketKodu, string globalDonemKodu, string globalConnectionString)
        {
            appSettingsService.UpdateGlobalSettings(globalSirketKodu, globalDonemKodu, globalConnectionString);
            return RedirectToAction("Index", "SysAdminConnection");
        }

        [HttpPost]
        public IActionResult SaveShownModules(SysAdminViewModel postModel)
        {
            var selectedList = postModel.SelectedShownModules ?? [];
            appSettingsService.UpdateShownModules(selectedList);

            return RedirectToAction("Index", "SysAdminConnection");
        }

        [HttpPost]
        public IActionResult SaveConnection(SysAdminViewModel postModel)
        {
            var model = postModel.CurrentModel;
            var mod = string.IsNullOrEmpty(model.ModuleName) ? "BOM" : model.ModuleName.Trim().ToUpper();
            string firParam = string.IsNullOrEmpty(model.FirmaNo) ? "NULL" : $"'{model.FirmaNo.Trim()}'";
            string donParam = string.IsNullOrEmpty(model.DonemNo) ? "NULL" : $"'{model.DonemNo.Trim()}'";
            var conn = (model.ConnectionString ?? string.Empty).Trim().Replace("'", "''");
            string idParam = model.Id > 0 ? model.Id.ToString() : "NULL";

            string execCommand = $"EXEC ALP_SYS_CONNECTIONS 3, @Id = {idParam}, @ModuleName='{mod}', @FirmaNo={firParam}, @DonemNo={donParam}, @ConnectionString='{conn}'";

            sqlEngine.ExecuteRawQuery<int>(GetMasterDb(), execCommand, row => 0);

            return RedirectToAction("Index", "SysAdminConnection");
        }

        public IActionResult Delete(int id)
        {
            string execCommand = $"EXEC ALP_SYS_CONNECTIONS 4, @Id = {id}";
            sqlEngine.ExecuteRawQuery<int>(GetMasterDb(), execCommand, row => 0);
            return RedirectToAction("Index", "SysAdminConnection");
        }
    }
}