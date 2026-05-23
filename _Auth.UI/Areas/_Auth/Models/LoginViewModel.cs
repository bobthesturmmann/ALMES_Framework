using System.ComponentModel.DataAnnotations;

namespace _Auth.UI.Areas._Auth.Models
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