using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using _Core.Shared.Lib;
using System.Threading.Tasks;

namespace Auth.UI.Filters
{
    public class ModuleAuthorizationHandler : AuthorizationHandler<ModuleRequirement>, IAuthorizationHandler
    {
        private readonly IAppSettingsService _appSettingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ModuleAuthorizationHandler(IAppSettingsService appSettingsService, IHttpContextAccessor httpContextAccessor)
        {
            _appSettingsService = appSettingsService;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ModuleRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var endpoint = httpContext.GetEndpoint();
            var controllerActionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (controllerActionDescriptor != null)
            {
                string moduleName = controllerActionDescriptor.RouteValues["area"]?.ToUpper()
                                    ?? controllerActionDescriptor.ControllerName.ToUpper();

                if (moduleName == "AUTH" ||
                    controllerActionDescriptor.ControllerName.Equals("Account", StringComparison.OrdinalIgnoreCase) ||
                    moduleName == "PORTAL")
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                var shownModules = _appSettingsService.GetShownModules();

                if (shownModules != null && shownModules.Contains(moduleName))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            else
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}