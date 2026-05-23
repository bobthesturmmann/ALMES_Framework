using System;
using System.Threading.Tasks;
using _Core.Shared.Lib;

namespace _Auth.Lib
{
    public class AuthManager
    {
        public AuthManager()
        {
        }

        public async Task<CurrentUserContext?> ValidateUserAsync(string username, string password)
        {
            if (username == "berat" && password == "Almes!2026.Auth")
            {
                await Task.Delay(100);

                return new CurrentUserContext
                {
                    UserId = 1,
                    Username = "berat",
                    Email = "berat@alfabilisim.com",
                    Roles = new System.Collections.Generic.List<string> { AuthConstants.RoleAdmin },
                    AuthenticationTime = DateTime.UtcNow
                };
            }

            return null;
        }
    }
}