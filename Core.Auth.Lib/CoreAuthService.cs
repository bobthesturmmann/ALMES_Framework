using Core.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using _Core.Shared.Lib;
using BCrypt.Net;

namespace Core.Auth.Lib
{
    public class CoreAuthService(SqlEngine sqlEngine, IModuleConnectionProvider connectionProvider)
    {
        private readonly SqlEngine _sqlEngine = sqlEngine;
        private readonly IModuleConnectionProvider _connectionProvider = connectionProvider;

        public CoreAuthUser? ValidateUserInDatabase(string username, string password, string firmaNo = "", string donemNo = "")
        {
            var cleanUsername = username.Trim().Replace("'", "''");
            var cleanPassword = password.Trim();

            string connectionString = _connectionProvider.GetConnectionString("AUTH", firmaNo, donemNo);
            string execCommand = $"EXEC ALP_AUTH_USERS 1, '{cleanUsername}'";

            List<CoreUserWithPassword> users = _sqlEngine.ExecuteRawQuery(
                connectionString,
                execCommand,
                (IDataRecord row) => new CoreUserWithPassword
                {
                    User = new CoreAuthUser
                    {
                        UserId = Convert.ToInt32(row["Id"]),
                        Username = row["Username"].ToString() ?? string.Empty,
                        Email = row["Email"].ToString() ?? string.Empty,
                        Roles = row["Roles"].ToString()?.Split(',').ToList() ?? ["Admin"],
                        CookieScheme = _Core.Shared.Lib.AuthConstants.CookieScheme
                    },
                    PasswordHash = row["PasswordHash"].ToString() ?? string.Empty
                }
            );

            if (users is { Count: > 0 })
            {
                var matchedUser = users.First();

                if (BCrypt.Net.BCrypt.Verify(cleanPassword, matchedUser.PasswordHash))
                {
                    return matchedUser.User;
                }
            }
            return null;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }

    public class CoreAuthUser
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
        public string CookieScheme { get; set; } = string.Empty;
    }

    public class CoreUserWithPassword
    {
        public CoreAuthUser User { get; set; } = null!;
        public string PasswordHash { get; set; } = string.Empty;
    }
}