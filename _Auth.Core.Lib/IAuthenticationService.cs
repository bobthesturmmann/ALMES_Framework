using System.Threading.Tasks;
using _Core.Shared.Lib;

namespace _Auth.Core
{
    public interface IAuthenticationService
    {
        Task<CurrentUserContext?> LoginAsync(string username, string password);

        Task LogoutAsync();
    }
}