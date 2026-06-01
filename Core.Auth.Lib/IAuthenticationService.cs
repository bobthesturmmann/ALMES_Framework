using System.Threading.Tasks;

namespace Core.Auth.Lib
{
    public interface IAuthenticationService
    {
        Task<bool> LoginAsync(string username, string password, bool rememberMe);

        Task LogoutAsync();
    }
}