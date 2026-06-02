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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthBridge(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;
        private ClaimsPrincipal? User => HttpContext?.User;

        public bool IsUserLoggedIn() => User?.Identity?.IsAuthenticated ?? false;

        public int? GetCurrentUserId()
        {
            var idStr = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idStr, out var id) ? id : null;
        }

        public string GetCurrentUsername() => User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        public List<string> GetUserRoles() => User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

        public async Task<bool> SignInAsync(string username, string password, bool rememberMe) => throw new NotImplementedException("Giriş işlemleri için Auth.Lib katmanını kullanın.");
        public async Task SignOutAsync() => throw new NotImplementedException("Çıkış işlemleri için Auth.Lib katmanını kullanın.");
    }
}