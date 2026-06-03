using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Core.Auth.Lib;

namespace Auth.Lib
{
    public class AuthService(CoreAuthService coreAuthManager, IHttpContextAccessor httpContextAccessor) : IAuthService
    {
        private readonly CoreAuthService _coreAuthManager = coreAuthManager;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        public async Task<bool> LoginAsync(string username, string password, bool rememberMe)
        {
            if (HttpContext == null) return false;

            var coreUser = _coreAuthManager.ValidateUserInDatabase(username, password);

            if (coreUser is not null)
            {
                List<Claim> claims = [
                    new(ClaimTypes.NameIdentifier, coreUser.UserId.ToString()),
                    new(ClaimTypes.Name, coreUser.Username),
                    new(ClaimTypes.Email, coreUser.Email)
                ];

                foreach (var role in coreUser.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, coreUser.CookieScheme);

                await HttpContext.SignInAsync(coreUser.CookieScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

                return true;
            }

            return false;
        }

        public async Task LogoutAsync()
        {
            if (HttpContext != null)
            {
                await HttpContext.SignOutAsync("AlmesSecureCookie");
            }
        }
    }
}