using System;
using System.Collections.Generic;

namespace _Core.Shared.Lib
{
    public class CurrentUserContext
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public string Token { get; set; } = string.Empty;
        public DateTime AuthenticationTime { get; set; }

        public bool IsInRole(string role)
        {
            return Roles.Contains(role);
        }
    }
}