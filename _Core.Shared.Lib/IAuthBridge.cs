using System.Collections.Generic;
using System.Threading.Tasks;

namespace _Core.Shared.Lib
{
    public interface IAuthBridge
    {
        Task<bool> SignInAsync(string username, string password, bool rememberMe);
        Task SignOutAsync();

        bool IsUserLoggedIn();
        int? GetCurrentUserId();
        string GetCurrentUsername();
        List<string> GetUserRoles();
    }
}