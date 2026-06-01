using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using _Core.Shared.Lib;

namespace Core.Auth.Lib
{
    public class AuthBridge : IAuthBridge
    {
        private readonly CoreAuthManager _coreAuthManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CookieScheme = "AlmesSecureCookie";

        public AuthBridge(CoreAuthManager coreAuthManager, IHttpContextAccessor httpContextAccessor)
        {
            _coreAuthManager = coreAuthManager;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;
        private ClaimsPrincipal? User => HttpContext?.User;

        public async Task<bool> SignInAsync(string username, string password, bool rememberMe)
        {
            if (HttpContext == null) return false;

            var coreUser = await _coreAuthManager.ValidateUserInDatabaseAsync(username, password);

            if (coreUser != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, coreUser.UserId.ToString()),
                    new Claim(ClaimTypes.Name, coreUser.Username),
                    new Claim(ClaimTypes.Email, coreUser.Email)
                };

                foreach (var role in coreUser.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieScheme);

                await HttpContext.SignInAsync(CookieScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

                return true;
            }

            return false;
        }

        public async Task SignOutAsync()
        {
            if (HttpContext != null)
            {
                await HttpContext.SignOutAsync(CookieScheme);
            }
        }

        public bool IsUserLoggedIn() => User?.Identity?.IsAuthenticated ?? false;

        public int? GetCurrentUserId()
        {
            var idStr = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idStr, out var id) ? id : null;
        }

        public string GetCurrentUsername() => User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        public List<string> GetUserRoles() => User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
    }
}