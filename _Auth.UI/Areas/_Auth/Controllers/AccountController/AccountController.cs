using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using _Auth.Lib;
using _Core.Shared.Lib;
using _Auth.UI.Areas._Auth.Models;

namespace _Auth.UI.Areas._Auth.Controllers.AccountController
{
    [Area("_Auth")]
    public class AccountController : Controller
    {
        private readonly AuthManager _authManager;

        public AccountController(AuthManager authManager)
        {
            _authManager = authManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [Route("_Auth/Account/Login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userContext = await _authManager.ValidateUserAsync(model.Username, model.Password);

            if (userContext != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userContext.UserId.ToString()),
                    new Claim(ClaimTypes.Name, userContext.Username),
                    new Claim(ClaimTypes.Email, userContext.Email)
                };

                foreach (var role in userContext.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, AuthConstants.CookieScheme);

                await HttpContext.SignInAsync(AuthConstants.CookieScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı!");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(AuthConstants.CookieScheme);
            return RedirectToAction("Login");
        }
    }
}