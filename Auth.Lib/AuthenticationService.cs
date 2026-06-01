using Core.Auth.Lib;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Auth.Lib
{
    public class AuthenticationService : Core.Auth.Lib.IAuthenticationService
    {
        private readonly CoreAuthManager _coreAuthManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationService(CoreAuthManager coreAuthManager, IHttpContextAccessor httpContextAccessor)
        {
            _coreAuthManager = coreAuthManager;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        public async Task<bool> LoginAsync(string username, string password, bool rememberMe)
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

                string cookieScheme = "AlmesSecureCookie";
                var claimsIdentity = new ClaimsIdentity(claims, cookieScheme);

                await HttpContext.SignInAsync(cookieScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
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