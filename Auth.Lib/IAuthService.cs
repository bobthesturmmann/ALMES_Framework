using System.Threading.Tasks;

namespace Auth.Lib
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password, bool rememberMe);
        Task LogoutAsync();
    }
}