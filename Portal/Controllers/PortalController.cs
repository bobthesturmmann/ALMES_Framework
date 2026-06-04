using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using _Core.Shared.Lib;

namespace Portal.Controllers
{
    public class PortalController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IModuleConnectionProvider _connectionProvider;

        public PortalController(IConfiguration configuration, IModuleConnectionProvider connectionProvider)
        {
            _configuration = configuration;
            _connectionProvider = connectionProvider;
        }

        public IActionResult Index()
        {
            var availableModules = new List<ResolvedModuleSettings>();
            var activeModules = _configuration.GetSection("ActiveModules").Get<List<string>>() ?? new List<string>();
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            bool isSysAdmin = User.IsInRole("SysAdmin");

            foreach (var moduleName in activeModules)
            {
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