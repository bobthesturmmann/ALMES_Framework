using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using _Core.Shared.Lib;

namespace Portal.Controllers
{
    public class PortalController : Controller
    {
        private readonly IModuleConnectionProvider _connectionProvider;
        private readonly IAppSettingsService _appSettingsService;

        public PortalController(IModuleConnectionProvider connectionProvider, IAppSettingsService appSettingsService)
        {
            _connectionProvider = connectionProvider;
            _appSettingsService = appSettingsService;
        }

        public IActionResult Index()
        {
            var availableModules = new List<ResolvedModuleSettings>();

            var activeModules = _appSettingsService.GetActiveModules();

            var shownModules = _appSettingsService.GetShownModules();

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            bool isSysAdmin = User.IsInRole("SysAdmin");

            foreach (var moduleName in activeModules)
            {
                string currentMod = moduleName.ToUpper().Trim();

                if (!shownModules.Contains(currentMod))
                {
                    continue;
                }

                var uiAssemblyExists = loadedAssemblies.Any(a =>
                    a.GetName().Name != null &&
                    a.GetName().Name!.Equals($"{moduleName}.UI", StringComparison.OrdinalIgnoreCase));

                if (uiAssemblyExists)
                {
                    var resolved = _connectionProvider.ResolveModuleSettings(moduleName);

                    if (!isSysAdmin)
                    {
                        resolved.FirmaNo = string.Empty;
                        resolved.DonemNo = string.Empty;
                        resolved.ProcedurePrefix = string.Empty;
                    }

                    availableModules.Add(resolved);
                }
            }

            return View(availableModules);
        }
    }
}