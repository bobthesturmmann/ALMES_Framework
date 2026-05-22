using Microsoft.AspNetCore.Mvc;

namespace Runner.Controllers
{
    public class PortalController : Controller
    {
        private readonly IConfiguration _configuration;

        public PortalController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var availableModules = new List<string>();

            string currentExecutionPath = AppDomain.CurrentDomain.BaseDirectory;
            string modulesRootPath = Path.Combine(currentExecutionPath, "MODULES");

            if (!Directory.Exists(modulesRootPath))
            {
                string solutionPath = Path.GetFullPath(Path.Combine(currentExecutionPath, "..", "..", "..", ".."));
                if (!Directory.Exists(Path.Combine(solutionPath, "Server", "MODULES")))
                {
                    solutionPath = Path.GetFullPath(Path.Combine(currentExecutionPath, "..", "..", "..", "..", ".."));
                }
                modulesRootPath = Path.Combine(solutionPath, "Server", "MODULES");
            }

            var activeModules = _configuration.GetSection("ActiveModules").Get<List<string>>() ?? new List<string>();

            if (Directory.Exists(modulesRootPath))
            {
                var directories = Directory.GetDirectories(modulesRootPath);
                foreach (var dir in directories)
                {
                    string folderName = Path.GetFileName(dir);

                    if (folderName.Equals("Shared", StringComparison.OrdinalIgnoreCase)) continue;

                    if (activeModules.Contains(folderName))
                    {
                        availableModules.Add(folderName);
                    }
                }
            }

            return View(availableModules);
        }
    }
}