using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Auth.Lib;
using Auth.UI.Areas.Auth.Models;

namespace Auth.UI.Areas.Auth.Controllers.AccountController
{
    [Area("Auth")]
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        public AccountController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("Auth/Account/Login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            bool isSuccess = await _authenticationService.LoginAsync(model.Username, model.Password, model.RememberMe);

            if (isSuccess)
            {
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
            await _authenticationService.LogoutAsync();
            return RedirectToAction("Login");
        }
    }
}