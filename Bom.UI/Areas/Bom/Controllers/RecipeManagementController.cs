using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bom.UI.Areas.Bom.Controllers
{
    [Area("Bom")]
    [Authorize]
    [Authorize(Policy = "ModuleControl")]
    public class RecipeManagementController : Controller
    {
        [Route("Bom/RecipeManagement")]
        public IActionResult Index()
        {
            return View();
        }
    }
}