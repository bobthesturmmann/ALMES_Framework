using System.ComponentModel.DataAnnotations;

namespace Auth.UI.Areas.Auth.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}